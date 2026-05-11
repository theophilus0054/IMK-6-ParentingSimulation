using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum GamePhase { Neonatal, Toddler }
    public enum GameState { Menu, Playing, Paused, Feedback, GameOver }

    [Header("Game Progression")]
    public GamePhase currentPhase = GamePhase.Neonatal;
    public GameState currentState = GameState.Menu;
    public int currentDay = 1;
    
    [Header("Phase Configuration")]
    public int neonatalPhaseDays = 7;
    public int toddlerPhaseDays = 7;

    [Header("References")]
    public BabyBehavior babyBehavior;
    private BabyDisease babyDisease;
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
            babyBehavior = FindObjectOfType<BabyBehavior>();
            if (babyBehavior == null)
            {
                Debug.LogError("[GameManager] BabyBehavior tidak ditemukan di scene!");
                return;
            }
        }

        babyDisease = babyBehavior.GetComponent<BabyDisease>();
        saveLoadManager = GetComponent<SaveLoadManager>();

        if (saveLoadManager == null)
        {
            Debug.LogError("[GameManager] SaveLoadManager tidak ditemukan! Tambahkan component ini ke GameObject yang sama.");
        }
    }

    public void StartGame()
    {
        currentState = GameState.Playing;
        currentDay = 1;
        currentPhase = GamePhase.Neonatal;
        // uiManager.ShowGameplayHUD();
        StartDayLoop();
    }

    public void StartDayLoop()
    {
        if (babyBehavior == null || saveLoadManager == null)
        {
            Debug.LogError("[GameManager] BabyBehavior atau SaveLoadManager tidak siap!");
            return;
        }

        Debug.Log($"[GAMEDAY] Memulai Hari ke-{currentDay} di fase {currentPhase}");
        currentState = GameState.Playing;
        
        // Reset status bayi untuk hari baru (kecuali hari pertama)
        if (currentDay > 1)
        {
            saveLoadManager.LoadGame();
        }
    }

    public void EndDay()
    {
        if (babyBehavior == null || saveLoadManager == null)
        {
            Debug.LogError("[GameManager] Cannot end day - missing references!");
            return;
        }

        currentState = GameState.Feedback;
        
        // Cek apakah pemain lolos level hari ini (berdasarkan status bayi)
        bool passLevel = babyBehavior.IsBabySafe();
        if (passLevel)
        {
            Debug.Log($"[GAMEDAY] Hari {currentDay} sukses dilewati!");
            // uiManager.ShowFeedback(true);
            
            // Simpan progress
            saveLoadManager.SaveGame();
            
            currentDay++;
            int maxDays = GetMaxDaysForPhase();

            if (currentDay > maxDays)
            {
                AdvanceToNextPhase();
            }
            else
            {
                // Lanjut ke hari berikutnya
                Invoke(nameof(StartDayLoop), 3f); // Delay 3 detik sebelum hari baru
            }
        }
        else
        {
            Debug.Log($"[GAMEDAY] Kondisi bayi memburuk. Game Over.");
            // uiManager.ShowFeedback(false);
            GameOver();
        }
    }

    private int GetMaxDaysForPhase()
    {
        return currentPhase == GamePhase.Neonatal ? neonatalPhaseDays : toddlerPhaseDays;
    }

    private void AdvanceToNextPhase()
    {
        if (currentPhase == GamePhase.Neonatal)
        {
            currentPhase = GamePhase.Toddler;
            currentDay = 1;
            Debug.Log("[GAMEDAY] ===== MEMASUKI FASE TODDLER! =====");
            // TODO: Logika ganti model bayi ke CHR-003 bisa dipicu di sini
            // TODO: Increase difficulty untuk Toddler phase
            Invoke(nameof(StartDayLoop), 2f);
        }
        else
        {
            Debug.Log("[GAMEDAY] ===== GAME SELESAI! MENAMPILKAN FUN FACT =====");
            currentState = GameState.GameOver;
            // uiManager.ShowFunFact();
            // TODO: Show final feedback atau return to main menu
        }
    }

    public void PauseGame()
    {
        if (currentState == GameState.Playing)
        {
            currentState = GameState.Paused;
            Time.timeScale = 0f;
            Debug.Log("[GAMEDAY] Game Paused");
        }
    }

    public void ResumeGame()
    {
        if (currentState == GameState.Paused)
        {
            currentState = GameState.Playing;
            Time.timeScale = 1f;
            Debug.Log("[GAMEDAY] Game Resumed");
        }
    }

    public void GameOver()
    {
        currentState = GameState.GameOver;
        Time.timeScale = 0f; // Pause the game
        Debug.Log("[GAMEDAY] GAME OVER!");
        // uiManager.ShowGameOverMenu();
    }
}