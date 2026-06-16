using UnityEngine;
using UnityEngine.Rendering;
using System.Diagnostics;

public class VRGameRestarter : MonoBehaviour
{
    [Header("Game Over Panel (Sprite)")]
    [Tooltip("The GameObject containing your Game Over Sprite Image and Button.")]
    [SerializeField] private GameObject gameOverPanel;
    
    [Tooltip("How close to the player's face should the sprite spawn (in meters)?")]
    [SerializeField] private float distanceFromFace = 1.0f;
    
    [Tooltip("Slightly adjust height or side placement (Y = -0.1f lowers it slightly below eye level).")]
    [SerializeField] private Vector3 positioningOffset = new Vector3(0f, -0.1f, 0f);

    [Header("Blur / Global Volume")]
    [SerializeField] private Volume globalVolume;

    [Header("VR Components")]
    [Tooltip("Drag your Main VR Camera here. If left empty, it will automatically find 'Camera.main'.")]
    [SerializeField] private Transform vrCamera;

    [Header("Movement / Gameplay Lock")]
    [Tooltip("Drag your Player Movement, Look, or Shooting scripts here to disable them on death.")]
    [SerializeField] private MonoBehaviour[] scriptsToDisable;

    private bool isGameOver = false;

    private void Start()
    {
        // Reset states on start
        isGameOver = false;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // Auto-detect the main VR camera if not assigned
        if (vrCamera == null && Camera.main != null)
        {
            vrCamera = Camera.main.transform;
        }
    }

    private void Update()
    {
        // Constantly check if your GameManager switched to the GameOver state
        if (!isGameOver && GameManager.Instance != null && GameManager.Instance.currentState == GameManager.GameState.GameOver)
        {
            TriggerGameOver();
        }
    }

    public void TriggerGameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        // 1. Activate the Blur Volume
        if (globalVolume != null)
        {
            globalVolume.enabled = true;
            globalVolume.weight = 1f;
        }

        // 2. Position and show the Sprite Panel right in front of the player's face
        if (gameOverPanel != null && vrCamera != null)
        {
            Vector3 spawnPosition = vrCamera.position + (vrCamera.forward * distanceFromFace);
            spawnPosition += vrCamera.TransformVector(positioningOffset);
            
            gameOverPanel.transform.position = spawnPosition;

            // Rotate the flat sprite to face the player perfectly
            gameOverPanel.transform.LookAt(vrCamera);
            gameOverPanel.transform.Rotate(0f, 180f, 0f); // 180 flip so the sprite faces forward

            gameOverPanel.SetActive(true);
        }

        // 3. Disable movement and actions so the player cannot move around
        if (scriptsToDisable != null)
        {
            foreach (var script in scriptsToDisable)
            {
                if (script != null) script.enabled = false;
            }
        }
    }

    /// <summary>
    /// Hook this up to your Panel Button's OnClick() event.
    /// </summary>
    public void RestartGameEntirely()
    {
        if (Application.isEditor)
        {
            UnityEngine.Debug.LogWarning("Unity Editor: Reloading scene instead of closing app.");
            
            // Bersihkan semua object DontDestroyOnLoad agar tidak ada reference yang missing
            // menggunakan DestroyImmediate agar object langsung terhapus sebelum scene baru di-load,
            // sehingga Singleton baru tidak mendeteksi duplikat dan menghancurkan dirinya sendiri.
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            foreach (GameObject go in allObjects)
            {
                if (go.transform.parent == null && go.scene.name == "DontDestroyOnLoad")
                {
                    DestroyImmediate(go);
                }
            }

            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
            return;
        }

        try
        {
            // Fetch the exact file path of this running build executable
            string exePath = Process.GetCurrentProcess().MainModule.FileName;
            // Launch a totally fresh, clean instance of the game
            Process.Start(exePath);
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError($"Failed to hard restart game executable: {e.Message}");
            return;
        }

        // Instantly terminate this old instance
        Application.Quit();
    }
}