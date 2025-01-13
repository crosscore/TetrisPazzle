// BlockController.cs
using UnityEngine;

public class BlockController : MonoBehaviour
{
    // Base fall time that will be modified by current speed
    private float baseFallTime = 0.5f;
    private float currentFallTime = 0.5f;
    private float fallTimer = 0;

    // Input delay settings remain unchanged
    float horizontalInputDelayMax = 0.8f;
    float verticalInputDelayMax = 0.02f;
    float horizontalInputTimer = 0;
    float verticalInputTimer = 0;

    // Wall kick offset checks
    readonly Vector3[] kickOffsets = new Vector3[]
    {
        Vector3.zero,           // Original position
        new Vector3(-1, 0, 0),  // Try left
        new Vector3(1, 0, 0),   // Try right
        new Vector3(-2, 0, 0),  // Try far left
        new Vector3(2, 0, 0)    // Try far right
    };

    [SerializeField] TetrisPuzzleSceneDirector gameSceneDirector;

    public void Initialize(TetrisPuzzleSceneDirector director, float fallSpeed)
    {
        gameSceneDirector = director;
        UpdateFallSpeed(fallSpeed);
    }

    public void UpdateFallSpeed(float speedMultiplier)
    {
        currentFallTime = baseFallTime * speedMultiplier;
    }

    void Start()
    {
        fallTimer = currentFallTime;
        horizontalInputTimer = horizontalInputDelayMax;
        verticalInputTimer = verticalInputDelayMax;
    }

    void Update()
    {
        fallTimer -= Time.deltaTime;
        horizontalInputTimer -= Time.deltaTime;
        verticalInputTimer -= Time.deltaTime;

        Vector3 movePosition = Vector3.zero;

        // Handle left movement
        if (Input.GetKeyDown(KeyCode.LeftArrow) ||
            (Input.GetKey(KeyCode.LeftArrow) && horizontalInputTimer <= 0))
        {
            movePosition = Vector3.left;
            horizontalInputTimer = horizontalInputDelayMax;
        }
        // Handle right movement
        else if (Input.GetKeyDown(KeyCode.RightArrow) ||
                (Input.GetKey(KeyCode.RightArrow) && horizontalInputTimer <= 0))
        {
            movePosition = Vector3.right;
            horizontalInputTimer = horizontalInputDelayMax;
        }
        // Handle downward movement - faster interval
        else if (Input.GetKeyDown(KeyCode.DownArrow) ||
                (Input.GetKey(KeyCode.DownArrow) && verticalInputTimer <= 0))
        {
            movePosition = Vector3.down;
            verticalInputTimer = verticalInputDelayMax;
        }
        // Handle rotation with wall kicks
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            TryRotateWithKick();
        }

        // Automatic falling
        if (fallTimer <= 0)
        {
            movePosition = Vector3.down;
            fallTimer = currentFallTime;
        }

        // Apply movement
        if (movePosition != Vector3.zero)
        {
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
    }

    // Attempt rotation with wall kicks
    void TryRotateWithKick()
    {
        Transform[] blockParts = GetAllBlockParts();
        Vector3 originalPosition = transform.position;

        // Store original rotation
        Quaternion originalRotation = transform.rotation;

        // Perform the rotation
        transform.Rotate(new Vector3(0, 0, 1), 90);

        // Try each kick offset
        foreach (Vector3 offset in kickOffsets)
        {
            transform.position = originalPosition + offset;

            if (gameSceneDirector.IsMovable(GetAllBlockParts()))
            {
                // Valid position found
                return;
            }
        }

        // If no valid position found, revert both rotation and position
        transform.rotation = originalRotation;
        transform.position = originalPosition;
    }

    public Transform[] GetAllBlockParts()
    {
        Transform[] children = GetComponentsInChildren<Transform>();
        System.Collections.Generic.List<Transform> blockParts = new System.Collections.Generic.List<Transform>(children);
        blockParts.Remove(transform);
        return blockParts.ToArray();
    }
}
