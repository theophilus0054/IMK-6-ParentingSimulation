using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject gameOverPanel;

    [Header("Blur / Global Volume")]
    [SerializeField] private Volume globalVolume;

    [Header("Optional: objek yang ingin dimatikan saat game over")]
    [SerializeField] private MonoBehaviour[] disableScripts;
    [SerializeField] private GameObject[] hideObjects;

    private bool isGameOver = false;

    private void Start()
    {
        SetGameOverState(false);
    }

    public void TriggerGameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        // Aktifkan blur
        if (globalVolume != null)
            globalVolume.enabled = true;

        // Tampilkan panel game over
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        // Nonaktifkan gameplay jika perlu
        if (disableScripts != null)
        {
            foreach (var s in disableScripts)
                if (s != null) s.enabled = false;
        }

        if (hideObjects != null)
        {
            foreach (var go in hideObjects)
                if (go != null) go.SetActive(false);
        }

        // Pause game
        Time.timeScale = 0f;
    }

    public void ReturnToGame()
    {
        if (!isGameOver) return;

        isGameOver = false;

        // Hilangkan panel
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Matikan blur
        if (globalVolume != null)
            globalVolume.enabled = false;

        // Aktifkan lagi script gameplay
        if (disableScripts != null)
        {
            foreach (var s in disableScripts)
                if (s != null) s.enabled = true;
        }

        if (hideObjects != null)
        {
            foreach (var go in hideObjects)
                if (go != null) go.SetActive(true);
        }

        // Lanjut game
        Time.timeScale = 1f;
    }

    private void SetGameOverState(bool state)
    {
        isGameOver = state;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(state);

        if (globalVolume != null)
            globalVolume.enabled = state;

        Time.timeScale = state ? 0f : 1f;
    }

    public void RestartScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}