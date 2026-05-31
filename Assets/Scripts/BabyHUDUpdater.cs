using UnityEngine;
using UnityEngine.UI; // Wajib untuk mengakses komponen UI

public class BabyHUDUpdater : MonoBehaviour
{
    [Header("Referensi Karakter Bayi")]
    public BabyBehavior babyScript; // Tarik objek bayi ke sini

    [Header("Referensi UI Bar (Gunakan UI Slider)")]
    public Slider healthBar;
    public Slider oxygenBar;

    void Start()
    {
        // Pastikan nilai maksimal bar adalah 100 agar sesuai dengan script BabyBehavior
        if (healthBar != null) healthBar.maxValue = 100f;
        if (oxygenBar != null) oxygenBar.maxValue = 100f;
    }

    void Update()
    {
        // Cegah error jika objek bayi belum dimasukkan
        if (babyScript == null) return;

        // Memperbarui UI setiap frame agar pergerakannya mulus
        if (healthBar != null) 
        {
            healthBar.value = babyScript.health;
        }

        if (oxygenBar != null) 
        {
            oxygenBar.value = babyScript.oxygenLevel;
        }
    }
}