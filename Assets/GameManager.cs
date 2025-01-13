// GameManager.cs
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Text scoreText;
    [SerializeField] private Button retryButton;
    public static GameManager Instance { get; private set; }

    // Score management
    private int currentScore = 0;

    // Level management
    private int currentLevel = 1;
    private float[] fallSpeedLevels = {
        1.0f,   // Level 1
        0.85f,  // Level 2
        0.75f,  // Level 3
        0.65f,  // Level 4
        0.55f,  // Level 5
        0.45f,  // Level 6
        0.35f,  // Level 7
        0.25f,  // Level 8
        0.15f,  // Level 9
        0.1f    // Level 10 (Maximum)
    };

    // Game state
    private bool isGameOver = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        InitializeGame();
        retryButton.onClick.AddListener(RestartGame);
    }

    public void AddScore(int lineCount)
    {
        currentScore += lineCount * 100;
        UpdateScoreDisplay();
        CheckLevelUp();
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {currentScore}";
        }
    }

    private void CheckLevelUp()
    {
        // Level up every 1000 points
        int newLevel = (currentScore / 1000) + 1;
        newLevel = Mathf.Min(newLevel, fallSpeedLevels.Length);

        if (newLevel != currentLevel)
        {
            currentLevel = newLevel;
            UpdateGameSpeed();
        }
    }

    public float GetCurrentFallSpeed()
    {
        return fallSpeedLevels[Mathf.Min(currentLevel - 1, fallSpeedLevels.Length - 1)];
    }

    private void UpdateGameSpeed()
    {
        // Update fall speed in TetrisPuzzleSceneDirector
        var director = FindFirstObjectByType<TetrisPuzzleSceneDirector>();
        if (director != null)
        {
            director.UpdateFallSpeed(GetCurrentFallSpeed());
        }
    }

    public void GameOver()
    {
        if (!isGameOver)
        {
            isGameOver = true;
            gameOverPanel.SetActive(true);
        }
    }

    public void RestartGame()
    {
        InitializeGame();
        var director = FindFirstObjectByType<TetrisPuzzleSceneDirector>();
        if (director != null)
        {
            director.RestartGame();
        }
    }

    private void InitializeGame()
    {
        currentScore = 0;
        currentLevel = 1;
        isGameOver = false;
        UpdateScoreDisplay();
        gameOverPanel.SetActive(false);
    }
}
