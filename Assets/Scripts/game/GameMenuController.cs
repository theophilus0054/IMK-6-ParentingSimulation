using UnityEngine;
using UnityEngine.SceneManagement; // Wajib untuk mengatur pindah Scene/Menu

public class GameMenuController : MonoBehaviour
{
    [Header("Pengaturan UI")]
    public GameObject pausePanel; // Opsional: Panel penanda kalau game sedang pause
    public string menuSceneName = "MainMenu"; // Nama Scene Main Menu Anda

    private bool isPaused = false;

    void Start()
    {
        // Pastikan saat awal game, waktu berjalan normal
        Time.timeScale = 1f;
    }

    // ============ 1. FUNGSI PAUSE / RESUME ============
    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            PauseGame();
        }
        else
        {
            ResumeGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f; // Menghentikan seluruh pergerakan/waktu di game

        if (pausePanel != null) pausePanel.SetActive(true);
        Debug.Log("Game DI-PAUSE");
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; // Mengembalikan waktu game menjadi normal

        if (pausePanel != null) pausePanel.SetActive(false);
        Debug.Log("Game DILANJUTKAN");
    }

    // ============ 2. FUNGSI KEMBALI KE MENU ============
    public void GoToMenu()
    {
        // Pastikan waktu dikembalikan normal sebelum pindah scene agar menu tidak membeku
        Time.timeScale = 1f; 
        
        // Membuka scene menu utama berdasarkan nama yang diisi di Inspector
        SceneManager.LoadScene(menuSceneName);
        Debug.Log("Kembali ke Main Menu");
    }

    // ============ 3. FUNGSI EXIT GAME ============
    public void ExitGame()
    {
        Debug.Log("Keluar dari Game (Fungsi ini aktif setelah game di-build)");
        
        // Perintah untuk menutup aplikasi game
        Application.Quit(); 
    }
} 