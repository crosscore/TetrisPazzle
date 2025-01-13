// BlockController.cs
using UnityEngine;

public class BlockController : MonoBehaviour
{
    // Maximum time between block falls
    float fallTimeMax = 0.5f;
    float fallTimer = 0;
    // Reference to the game scene director
    [SerializeField] TetrisPuzzleSceneDirector gameSceneDirector;

    // Initialize block with game director and fall speed
    public void Initialize(TetrisPuzzleSceneDirector director, int fallTime)
    {
        gameSceneDirector = director;
        fallTimeMax = fallTime;
    }

    void Start()
    {
        fallTimer = fallTimeMax;
    }

    void Update()
    {
        fallTimer -= Time.deltaTime;
        Vector3 movePosition = Vector3.zero;

        // Handle left movement
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            movePosition = Vector3.left;
        }
        // Handle right movement
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            movePosition = Vector3.right;
        }
        // Handle downward movement
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            movePosition = Vector3.down;
        }
        // Handle rotation
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            // Get all child blocks before rotation
            Transform[] blockParts = GetAllBlockParts();

            // Perform rotation
            transform.Rotate(new Vector3(0, 0, 1), 90);

            // Check if any part of the block is outside the field
            if (!gameSceneDirector.IsMovable(blockParts))
            {
                transform.Rotate(new Vector3(0, 0, 1), -90);
            }
        }

        // Automatic falling
        if (fallTimer <= 0)
        {
            movePosition = Vector3.down;
            fallTimer = fallTimeMax;
        }

        // Apply movement
        transform.position += movePosition;

        // Check if movement is valid for all block parts
        if (!gameSceneDirector.IsMovable(GetAllBlockParts()))
        {
            transform.position -= movePosition;
            // Lock block in place if it can't move down
            if (movePosition == Vector3.down)
            {
                enabled = false;
            }
        }
    }

    // Get all block parts including children
    public Transform[] GetAllBlockParts()
    {
        // Get all child transforms
        Transform[] children = GetComponentsInChildren<Transform>();
        // Remove the parent transform from the array
        System.Collections.Generic.List<Transform> blockParts = new System.Collections.Generic.List<Transform>(children);
        blockParts.Remove(transform);
        return blockParts.ToArray();
    }
}
