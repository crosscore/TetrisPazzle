using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TetrisPuzzleSceneDirector : MonoBehaviour
{
    // field size
    const int FieldWidth = 10;
    const int FieldHeight = 20;

    // prefab of blocks
    [SerializeField] List<BlockController> prefabBlocks;
    // next block
    BlockController nextBlock;
    // current block
    BlockController currentBlock;

    void Start()
    {
        // setup next block
        SetupNextBlock();
        // spawn block
        SpawnBlock();
    }

    void Update()
    {

    }
    // world position to field position
    Vector2Int GetIndexPosition(Vector3 worldPosition)
    {
        Vector2Int index = new Vector2Int();
        index.x = Mathf.RoundToInt(worldPosition.x - 0.5f) + FieldWidth / 2;
        index.y = Mathf.RoundToInt(worldPosition.y - 0.5f) + FieldHeight / 2;
        return index;
    }
    // 移動可能かどうか
    public bool IsMovable(Transform[] blockTransforms)
    {
        foreach (Transform item in blockTransforms)
        {
            Vector2Int index = GetIndexPosition(item.position);
            if (index.x < 0 || FieldWidth -1 < index.x || index.y < 0)
            {
                return false;
            }
        }
        return true;
    }

    // 次のブロックを生成
    void SetupNextBlock()
    {
        int rnd = Random.Range(0, prefabBlocks.Count);
        // setup position (out of the field)
        Vector3 setupPosition = new Vector3(2.5f, 11f, 0);
        // generate next block
        nextBlock = Instantiate(prefabBlocks[rnd], setupPosition, Quaternion.identity);
        // initialize the block
        nextBlock.Initialize(this, 1);
        // don't move the block
        nextBlock.enabled = false;
    }
    // ブロックをフィールドに表示
    void SpawnBlock()
    {
        // setup position (top center)
        Vector3 spawnPosition = new Vector3(0.5f, 8.5f, 0);
        // generate current block
        currentBlock = nextBlock;
        currentBlock.transform.position = spawnPosition;
        // initialize the block
        currentBlock.enabled = true;
        // setup next block
        SetupNextBlock();
    }
}
