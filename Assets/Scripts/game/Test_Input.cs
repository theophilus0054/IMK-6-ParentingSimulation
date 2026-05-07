using UnityEngine;
using UnityEngine.InputSystem; // Tambahkan namespace sistem input baru

public class TestInput : MonoBehaviour
{
    public BabyBehavior babyBehavior;
    public GameManager gameManager;

    void Update()
    {
        // Pastikan keyboard fisik terdeteksi oleh sistem
        if (Keyboard.current == null) return;

        // Tekan tombol 'F' di keyboard untuk simulasi memberi susu
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            if (babyBehavior != null)
            {
                babyBehavior.ReceiveFood();
            }
        }

        // Tekan tombol 'D' untuk simulasi ganti popok
        if (Keyboard.current.dKey.wasPressedThisFrame)
        {
            if (babyBehavior != null)
            {
                babyBehavior.ChangeDiaper();
            }
        }

        // Tekan tombol 'Space' untuk simulasi mengakhiri hari
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (gameManager != null)
            {
                gameManager.EndDay();
            }
        }
    }
}