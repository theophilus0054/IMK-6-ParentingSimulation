using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject gameOverPanel;

    [Header("Blur / Global Volume")]
    [SerializeField] private Volume globalVolume;

    [Header("Posisi Panel")]
    [SerializeField] private Transform playerCamera;
    [SerializeField] private float panelDistance = 1.5f;
    [SerializeField] private Vector3 panelOffset = new Vector3(0f, -0.1f, 0f);

    [Header("Optional: objek yang ingin dimatikan saat game over")]
    [SerializeField] private MonoBehaviour[] disableScripts;
    [SerializeField] private GameObject[] hideObjects;

    private bool isGameOver = false;

    private void Start()
    {
        isGameOver = false;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (!isGameOver && GameManager.Instance != null && GameManager.Instance.currentState == GameManager.GameState.GameOver)
        {
            TriggerGameOver();
        }
    }

    public void TriggerGameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        // Aktifkan blur
        if (globalVolume != null)
        {
            globalVolume.enabled = true;
            globalVolume.weight = 1f;
        }

        // Tampilkan panel game over
        if (gameOverPanel != null)
        {
            PlacePanelInFrontOfPlayer(gameOverPanel);
            gameOverPanel.SetActive(true);
        }

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
            globalVolume.weight = 0f;

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

    private void PlacePanelInFrontOfPlayer(GameObject panel)
    {
        Transform cameraTransform = playerCamera;

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        if (panel == null || cameraTransform == null)
        {
            return;
        }

        Vector3 panelPosition = cameraTransform.position + cameraTransform.forward * panelDistance + cameraTransform.TransformVector(panelOffset);
        panel.transform.position = panelPosition;
        panel.transform.rotation = Quaternion.LookRotation(panelPosition - cameraTransform.position, Vector3.up);
    }

    public void RestartScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
