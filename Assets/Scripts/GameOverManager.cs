using UnityEngine;
using UnityEngine.Rendering;
using System.Diagnostics; // Required for the full restart process
using System.IO;

public class GameOverManager : MonoBehaviour
{
    [Header("UI (Sprite Renderer Object)")]
    [SerializeField] private GameObject gameOverPanel;

    [Header("Blur / Global Volume")]
    [SerializeField] private Volume globalVolume;

    [Header("Posisi Panel")]
    [SerializeField] private Transform playerCamera;
    [SerializeField] private float panelDistance = 1.0f; // Kept slightly closer (1 meter) so it doesn't clip into walls
    [SerializeField] private Vector3 panelOffset = new Vector3(0f, -0.1f, 0f);

    [Header("Objek yang wajib dimatikan saat game over")]
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

        // 1. Enable VR Blur/Post-Processing
        if (globalVolume != null)
        {
            globalVolume.enabled = true;
            globalVolume.weight = 1f;
        }

        // 2. Position and display the Sprite Panel
        if (gameOverPanel != null)
        {
            PlaceSpriteInFrontOfPlayer(gameOverPanel);
            gameOverPanel.SetActive(true);
        }

        // 3. Disable gameplay scripts (movement, shooting, enemies) instead of freezing time
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

        // REMOVED Time.timeScale = 0f; to keep VR tracking and physical button raycasts working smoothly!
    }

    private void PlaceSpriteInFrontOfPlayer(GameObject panel)
    {
        Transform cameraTransform = playerCamera;

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        if (panel == null || cameraTransform == null) return;

        // Calculate position based on where the player is looking
        Vector3 panelPosition = cameraTransform.position + cameraTransform.forward * panelDistance + cameraTransform.TransformVector(panelOffset);
        panel.transform.position = panelPosition;

        // FIXED FOR SPRITES: Force the flat sprite to look directly at the player's eyes
        panel.transform.LookAt(cameraTransform);
        
        // Flip 180 degrees because Unity Sprites inherently face backwards when using LookAt
        panel.transform.Rotate(0, 180f, 0); 
    }

    /// <summary>
    /// This replaces your old RestartScene logic. It hard-restarts the whole app executable.
    /// Connect your panel's button click event to this function.
    /// </summary>
    public void RestartGameEntirely()
    {
        // If testing in the Unity Editor, just reload the scene so it doesn't close Unity
        if (Application.isEditor)
        {
            UnityEngine.Debug.LogWarning("Editor detected: Reloading scene instead of full app restart.");
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
            return;
        }

        try
        {
            // Get path to the build .exe file and launch a brand new instance
            string currentExePath = Process.GetCurrentProcess().MainModule.FileName;
            Process.Start(currentExePath);
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError($"Failed to restart game executable: {e.Message}");
            return;
        }

        // Kill this current frozen instance completely
        Application.Quit();
    }
}