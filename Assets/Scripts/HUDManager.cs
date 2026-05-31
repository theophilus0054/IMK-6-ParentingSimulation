using UnityEngine;
using UnityEngine.InputSystem; // Wajib untuk membaca tombol VR modern

public class HUDManager : MonoBehaviour
{
    [Header("Referensi UI")]
    public GameObject healthbarCanvas; // Masukkan objek HUD_Healthbar ke sini

    [Header("Pengaturan Input")]
    [Tooltip("Pilih tombol untuk memunculkan HUD (Misal: XRI LeftHand/Primary Button)")]
    public InputActionReference toggleButton;

    // Status apakah game sudah dimulai (dikontrol oleh tombol Start nanti)
    private bool isGameStarted = false;

    void Start()
    {
        // Pastikan UI mati/hilang saat awal game
        if (healthbarCanvas != null)
        {
            healthbarCanvas.SetActive(false);
        }
    }

    void OnEnable()
    {
        if (toggleButton != null)
        {
            toggleButton.action.performed += ToggleHUD;
            toggleButton.action.Enable();
        }
    }

    void OnDisable()
    {
        if (toggleButton != null)
        {
            toggleButton.action.performed -= ToggleHUD;
            toggleButton.action.Disable();
        }
    }

    private void ToggleHUD(InputAction.CallbackContext context)
    {
        // Jika game belum mulai, pencetan tombol diabaikan
        if (!isGameStarted) return;

        // Nyalakan jika sedang mati, matikan jika sedang nyala
        if (healthbarCanvas != null)
        {
            healthbarCanvas.SetActive(!healthbarCanvas.activeSelf);
        }
    }

    // Fungsi ini akan dipanggil oleh Tombol Start di Menu Utama
    public void EnableHUD()
    {
        isGameStarted = true;
    }
}