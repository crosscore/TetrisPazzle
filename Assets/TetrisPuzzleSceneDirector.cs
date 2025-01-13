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

    void Start()
    {
        // Initialize the playing field
        fieldTiles = new Transform[FieldWidth, FieldHeight];
        SetupNextBlock();
        SpawnBlock();
    }

    void Update()
    {
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
            }
            else
            {
                // Game over condition
                Debug.LogWarning("Block placed outside field - Game Over");
                // Add game over handling here
                return;
            }
        }
        SpawnBlock();
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
        nextBlock.Initialize(this, 1);
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
}
