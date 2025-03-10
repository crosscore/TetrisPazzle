// TetrisPuzzleSceneDirector.cs
using System.Collections.Generic;
using UnityEngine;

public class TetrisPuzzleSceneDirector : MonoBehaviour
{
    // Field dimensions
    const int FieldWidth = 10;
    const int FieldHeight = 20;

    [SerializeField] List<BlockController> prefabBlocks;
    [SerializeField] AudioClip seStopBlock;
    [SerializeField] AudioClip seClearLine;
    BlockController nextBlock;
    BlockController currentBlock;
    Transform[,] fieldTiles;

    private float currentFallSpeed = 1.0f;
    private bool isGameActive = true;

    AudioSource audioSource;


    void Start()
    {
        InitializeGame();
    }

    private void InitializeGame()
    {
        fieldTiles = new Transform[FieldWidth, FieldHeight];
        audioSource = GetComponent<AudioSource>();
        isGameActive = true;
        currentFallSpeed = 1.0f;

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
        // If the game is not active or no current block, do nothing
        if (!isGameActive || currentBlock == null) return;

        // If currentBlock.enabled == false, that means the block can no longer move
        if (!currentBlock.enabled)
        {
            Transform[] blockParts = currentBlock.GetAllBlockParts();

            // Check if it's game over
            if (IsGameOver(blockParts))
            {
                // Destroy the current block to avoid overlapping with existing blocks
                Destroy(currentBlock.gameObject);

                GameOver();
                return;
            }

            // Place block safely
            PlaceBlock(blockParts);
            audioSource.PlayOneShot(seStopBlock);

            // Clear lines and spawn next block
            CheckAndClearLines();
            SpawnBlock();
        }
    }

    // This method checks if blockParts are out of field or overlapping
    private bool IsGameOver(Transform[] blockParts)
    {
        foreach (Transform part in blockParts)
        {
            Vector2Int index = GetIndexPosition(part.position);
            if (!IsWithinField(index) || GetFieldTile(index) != null)
            {
                return true;
            }
        }
        return false;
    }

    // Place the block's parts into fieldTiles
    private void PlaceBlock(Transform[] blockParts)
    {
        foreach (Transform part in blockParts)
        {
            Vector2Int index = GetIndexPosition(part.position);
            fieldTiles[index.x, index.y] = part;
        }
    }

    Vector2Int GetIndexPosition(Vector3 worldPosition)
    {
        Vector2Int index = new Vector2Int();
        index.x = Mathf.RoundToInt(worldPosition.x - 0.5f) + FieldWidth / 2;
        index.y = Mathf.RoundToInt(worldPosition.y - 0.5f) + FieldHeight / 2;
        return index;
    }

    bool IsWithinField(Vector2Int index)
    {
        return index.x >= 0 && index.x < FieldWidth && index.y >= 0 && index.y < FieldHeight;
    }

    public bool IsMovable(Transform[] blockTransforms)
    {
        foreach (Transform item in blockTransforms)
        {
            Vector2Int index = GetIndexPosition(item.position);
            if (!IsWithinField(index) || GetFieldTile(index) != null)
            {
                return false;
            }
        }
        return true;
    }

    void SetupNextBlock()
    {
        int rnd = Random.Range(0, prefabBlocks.Count);
        Vector3 setupPosition = new Vector3(2.5f, 11f, 0);
        nextBlock = Instantiate(prefabBlocks[rnd], setupPosition, Quaternion.identity);
        nextBlock.Initialize(this, currentFallSpeed);
        nextBlock.enabled = false;
    }

    void SpawnBlock()
    {
        Vector3 initialPosition = new Vector3(0.5f, 8.5f, 0);
        currentBlock = nextBlock;
        currentBlock.transform.position = initialPosition;

        // Check game over at spawn
        if (IsGameOver(currentBlock.GetAllBlockParts()))
        {
            // Destroy the current block if game over occurs at spawn
            Destroy(currentBlock.gameObject);

            GameOver();
            return;
        }

        currentBlock.enabled = true;
        SetupNextBlock();
    }

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
        bool isPlaySE = false;
        for (int x = 0; x < FieldWidth; x++)
        {
            if (fieldTiles[x, y] != null)
            {
                Destroy(fieldTiles[x, y].gameObject);
                fieldTiles[x, y] = null;
                isPlaySE = true;
            }
        }
        if (isPlaySE)
        {
            audioSource.PlayOneShot(seClearLine);
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
