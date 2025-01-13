// TetrisPuzzleSceneDirector.cs
using System.Collections.Generic;
using UnityEngine;

public class TetrisPuzzleSceneDirector : MonoBehaviour
{
    // Field dimensions
    const int FieldWidth = 10;
    const int FieldHeight = 20;

    // Block prefabs collection
    [SerializeField] List<BlockController> prefabBlocks;
    // Next block to spawn
    BlockController nextBlock;
    // Currently active block
    BlockController currentBlock;
    // Grid of placed blocks
    Transform[,] fieldTiles;

    // Game state and speed management
    private float currentFallSpeed = 1.0f;
    private bool isGameActive = true;

    void Start()
    {
        InitializeGame();
    }

    private void InitializeGame()
    {
        // Initialize the playing field
        fieldTiles = new Transform[FieldWidth, FieldHeight];
        isGameActive = true;
        currentFallSpeed = 1.0f;

        // Clear existing blocks if any
        var existingBlocks = FindObjectsByType<BlockController>(FindObjectsSortMode.None);
        foreach (var block in existingBlocks)
        {
            Destroy(block.gameObject);
        }

        SetupNextBlock();
        SpawnBlock();
    }

    void Update()
    {
        if (!isGameActive) return;

        // Continue if current block is still active
        if (currentBlock.enabled) return;

        // Place the block in the field grid
        Transform[] blockParts = currentBlock.GetAllBlockParts();
        foreach (Transform item in blockParts)
        {
            Vector2Int index = GetIndexPosition(item.position);
            // Check if position is within bounds
            if (IsWithinField(index))
            {
                fieldTiles[index.x, index.y] = item;

                // Check for game over - if block is placed at or above the top of the field
                if (index.y >= FieldHeight - 2)
                {
                    GameOver();
                    return;
                }
            }
            else
            {
                GameOver();
                return;
            }
        }

        // Check and clear any completed lines
        CheckAndClearLines();

        // Spawn next block if game is still active
        if (isGameActive)
        {
            SpawnBlock();
        }
    }

    // Convert world position to grid index
    Vector2Int GetIndexPosition(Vector3 worldPosition)
    {
        Vector2Int index = new Vector2Int();
        index.x = Mathf.RoundToInt(worldPosition.x - 0.5f) + FieldWidth / 2;
        index.y = Mathf.RoundToInt(worldPosition.y - 0.5f) + FieldHeight / 2;
        return index;
    }

    // Check if position is within field boundaries
    bool IsWithinField(Vector2Int index)
    {
        return index.x >= 0 && index.x < FieldWidth && index.y >= 0 && index.y < FieldHeight;
    }

    // Check if movement is valid
    public bool IsMovable(Transform[] blockTransforms)
    {
        foreach (Transform item in blockTransforms)
        {
            Vector2Int index = GetIndexPosition(item.position);

            // Check boundaries
            if (!IsWithinField(index))
            {
                return false;
            }

            // Check collision with existing blocks
            if (GetFieldTile(index) != null)
            {
                return false;
            }
        }
        return true;
    }

    // Generate the next block
    void SetupNextBlock()
    {
        int rnd = Random.Range(0, prefabBlocks.Count);
        Vector3 setupPosition = new Vector3(2.5f, 11f, 0);
        nextBlock = Instantiate(prefabBlocks[rnd], setupPosition, Quaternion.identity);
        nextBlock.Initialize(this, currentFallSpeed);
        nextBlock.enabled = false;
    }

    // Spawn block into play field
    void SpawnBlock()
    {
        Vector3 spawnPosition = new Vector3(0.5f, 8.5f, 0);
        currentBlock = nextBlock;
        currentBlock.transform.position = spawnPosition;
        currentBlock.enabled = true;
        SetupNextBlock();
    }

    // Get tile at specified grid position
    Transform GetFieldTile(Vector2Int index)
    {
        if (!IsWithinField(index))
        {
            return null;
        }
        return fieldTiles[index.x, index.y];
    }

    // Check and clear completed lines
    void CheckAndClearLines()
    {
        int linesCleared = 0;

        for (int y = 0; y < FieldHeight; y++)
        {
            if (IsLineFilled(y))
            {
                ClearLine(y);
                DropBlocksAboveLine(y);
                y--; // Recheck the same line after dropping blocks
                linesCleared++;
            }
        }

        if (linesCleared > 0)
        {
            GameManager.Instance.AddScore(linesCleared);
        }
    }

    // Check if line is completely filled
    bool IsLineFilled(int y)
    {
        for (int x = 0; x < FieldWidth; x++)
        {
            if (fieldTiles[x, y] == null)
            {
                return false;
            }
        }
        return true;
    }

    // Clear a single line
    void ClearLine(int y)
    {
        for (int x = 0; x < FieldWidth; x++)
        {
            if (fieldTiles[x, y] != null)
            {
                Destroy(fieldTiles[x, y].gameObject);
                fieldTiles[x, y] = null;
            }
        }
    }

    // Drop blocks above the cleared line
    void DropBlocksAboveLine(int clearedLine)
    {
        for (int y = clearedLine + 1; y < FieldHeight; y++)
        {
            for (int x = 0; x < FieldWidth; x++)
            {
                if (fieldTiles[x, y] != null)
                {
                    // Move block down in the grid
                    fieldTiles[x, y - 1] = fieldTiles[x, y];
                    fieldTiles[x, y] = null;

                    // Update visual position
                    fieldTiles[x, y - 1].position += Vector3.down;
                }
            }
        }
    }

    // Game state management methods
    private void GameOver()
    {
        isGameActive = false;
        GameManager.Instance.GameOver();
    }

    public void RestartGame()
    {
        InitializeGame();
    }

    public void UpdateFallSpeed(float newSpeed)
    {
        currentFallSpeed = newSpeed;
        if (currentBlock != null)
        {
            currentBlock.UpdateFallSpeed(currentFallSpeed);
        }
        if (nextBlock != null)
        {
            nextBlock.UpdateFallSpeed(currentFallSpeed);
        }
    }
}
