using UnityEngine;

public class BlockController : MonoBehaviour
{
    // fall time
    float fallTimeMax = 0.5f;
    float fallTimer = 0;
    // scean director
    [SerializeField] TetrisPuzzleSceneDirector gameSceneDirector;

    // initialize the block
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

        // left arrow key
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            movePosition = Vector3.left;
        }
        // right arrow key
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            movePosition = Vector3.right;
        }
        // down arrow key
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            movePosition = Vector3.down;
        }
        // rotate the block
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            transform.Rotate(new Vector3(0, 0, 1), 90);
            if (!gameSceneDirector.IsMovable(new Transform[] { transform }))
            {
                transform.Rotate(new Vector3(0, 0, 1), -90);
            }
        }
        if (fallTimer <= 0)
        {
            movePosition = Vector3.down;
            fallTimer = fallTimeMax;
        }
        // move the block
        transform.position += movePosition;
        // if the block is not movable, return to the previous position
        if (!gameSceneDirector.IsMovable(new Transform[] { transform }))
        {
            transform.position -= movePosition;
            // if the block is not movable and the block is moving down, fix the block
            if (movePosition == Vector3.down)
            {
                enabled = false;
            }
        }
    }
}
