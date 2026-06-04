using UnityEngine;
using UnityEngine.UI;

public class UIToggleButtonText : MonoBehaviour
{
    [Header("Referensi Objek UI")]
    public GameObject buttonObject; // Masukkan objek Tombol ke sini
    public GameObject textObject;   // Masukkan objek Teks ke sini

    void Start()
    {
        // Kondisi awal saat game dimulai: Tombol muncul, Teks sembunyi
        if (buttonObject != null) buttonObject.SetActive(true);
        if (textObject != null) textObject.SetActive(false);
    }

    // Fungsi utama yang akan dipanggil saat diklik
    public void ToggleSate()
    {
        if (buttonObject == null || textObject == null) return;

        // Cek status tombol saat ini (sedang aktif atau tidak)
        bool isButtonActive = buttonObject.activeSelf;

        // Balikkan kondisinya
        buttonObject.SetActive(!isButtonActive); // Jika aktif jadi mati, jika mati jadi aktif
        textObject.SetActive(isButtonActive);    // Kebalikan dari status tombol
    }
}