using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum GameState { Menu, Playing, Paused, Feedback, GameOver }

    [Header("Game State")]
    public GameState currentState = GameState.Menu;

    [Header("References")]
    public BabyBehavior babyBehavior;
    [SerializeField] private GameOverManager gameOverManager;
    private SaveLoadManager saveLoadManager;
    // public UIManager uiManager; // Hubungkan dengan script UI Anda

    private void Awake()
    {
        // Singleton pattern agar GameManager mudah diakses dari script lain
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Auto-assign components jika belum di-assign di Inspector
        if (babyBehavior == null)
        {
            babyBehavior = FindFirstObjectByType<BabyBehavior>();
            if (babyBehavior == null)
            {
                Debug.LogError("[GameManager] BabyBehavior tidak ditemukan di scene!");
                return;
            }
        }

        saveLoadManager = GetComponent<SaveLoadManager>();
        if (gameOverManager == null)
        {
            gameOverManager = FindFirstObjectByType<GameOverManager>();
        }

        if (saveLoadManager == null)
        {
            Debug.LogError("[GameManager] SaveLoadManager tidak ditemukan! Tambahkan component ini ke GameObject yang sama.");
        }
    }

    public void StartGame()
    {
        Time.timeScale = 1f;
        currentState = GameState.Playing;
        // uiManager.ShowGameplayHUD();
        StartSimulation();
    }

    public void StartSimulation()
    {
        if (babyBehavior == null)
        {
            Debug.LogError("[GameManager] BabyBehavior tidak siap!");
            return;
        }

        Debug.Log("[GAME] Simulasi dimulai.");
        currentState = GameState.Playing;
    }

    // Dipertahankan agar Button/UnityEvent lama yang masih memanggil StartDayLoop tidak error.
    public void StartDayLoop()
    {
        StartSimulation();
    }

    public void EndDay()
    {
        if (babyBehavior == null)
        {
            Debug.LogError("[GameManager] Cannot evaluate simulation - missing BabyBehavior!");
            return;
        }

        currentState = GameState.Feedback;

        if (babyBehavior.IsBabySafe())
        {
            Debug.Log("[GAME] Kondisi bayi aman.");
            // uiManager.ShowFeedback(true);

            if (saveLoadManager != null)
            {
                saveLoadManager.SaveGame();
            }

            currentState = GameState.Playing;
        }
        else
        {
            Debug.Log("[GAME] Kondisi bayi memburuk. Game Over.");
            // uiManager.ShowFeedback(false);
            GameOver();
        }
    }

    public void PauseGame()
    {
        if (currentState == GameState.Playing)
        {
            currentState = GameState.Paused;
            Time.timeScale = 0f;
            Debug.Log("[GAME] Game Paused");
        }
    }

    public void ResumeGame()
    {
        if (currentState == GameState.Paused)
        {
            currentState = GameState.Playing;
            Time.timeScale = 1f;
            Debug.Log("[GAME] Game Resumed");
        }
    }

    public void GameOver()
    {
        currentState = GameState.GameOver;
        if (gameOverManager != null)
        {
            gameOverManager.TriggerGameOver();
        }

        Time.timeScale = 0f; // Pause the game
        Debug.Log("[GAME] GAME OVER!");
        // uiManager.ShowGameOverMenu();
    }
}
