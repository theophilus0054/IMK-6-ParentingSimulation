using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum GamePhase { Neonatal, Toddler }
    public enum GameState { Menu, Playing, Paused, Feedback, GameOver }

    [Header("Game Progression")]
    public GamePhase currentPhase = GamePhase.Neonatal; // [cite: 157, 167]
    public GameState currentState = GameState.Menu;
    public int currentDay = 1;
    public int maxDaysPerPhase = 7; // Contoh: Setelah 7 hari sukses, naik ke Toddler

    [Header("References")]
    public BabyBehavior babyBehavior;
    // public UIManager uiManager; // Hubungkan dengan script UI Anda

    private void Awake()
    {
        // Singleton pattern agar GameManager mudah diakses dari script lain
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void StartGame()
    {
        currentState = GameState.Playing;
        currentDay = 1;
        // uiManager.ShowGameplayHUD();
        StartDayLoop();
    }

    public void StartDayLoop()
    {
        Debug.Log($"Memulai Hari ke-{currentDay} di fase {currentPhase}");
        // Memuat status bayi dari SaveLoadManager jika ada
        SaveLoadManager.Instance.LoadGame();
    }

    public void EndDay()
    {
        currentState = GameState.Feedback;
        // Cek apakah pemain lolos level hari ini (berdasarkan status bayi)
        bool passLevel = babyBehavior.IsBabySafe(); // [cite: 159, 169]

        if (passLevel)
        {
            Debug.Log("Hari sukses dilewati. Menampilkan Feedback Positif.");
            // uiManager.ShowFeedback(true);
            currentDay++;

            if (currentDay > maxDaysPerPhase)
            {
                AdvanceToNextPhase();
            }
            else
            {
                SaveLoadManager.Instance.SaveGame();
            }
        }
        else
        {
            Debug.Log("Kondisi bayi memburuk. Menampilkan Feedback Negatif / Game Over.");
            // uiManager.ShowFeedback(false); [cite: 161, 171]
            GameOver();
        }
    }

    private void AdvanceToNextPhase()
    {
        if (currentPhase == GamePhase.Neonatal)
        {
            currentPhase = GamePhase.Toddler; // [cite: 167]
            currentDay = 1;
            Debug.Log("Memasuki Fase Toddler!");
            // Logika ganti model bayi ke CHR-003 bisa dipicu di sini
        }
        else
        {
            Debug.Log("Game Selesai! Menampilkan Fun Fact.");
            // uiManager.ShowFunFact(); [cite: 178]
        }
    }

    public void GameOver()
    {
        currentState = GameState.GameOver;
        Debug.Log("Game Over.");
        // uiManager.ShowGameOverMenu();
    }
}