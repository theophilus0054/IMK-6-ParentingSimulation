using UnityEngine;
using UnityEngine.UI;
using TMPro; // Wajib untuk mengakses TextMeshPro

public class BabyHealthHUD : MonoBehaviour
{
    [Header("Baby Reference")]
    public BabyBehavior babyScript;

    [Header("UI Elements")]
    public Slider healthSlider;
    public TextMeshProUGUI healthText; // Slot untuk teks persentase

    void Start()
    {
        // Set Max and Min values to perfectly match the health range in BabyBehavior
        if (healthSlider != null) 
        {
            healthSlider.minValue = 0f;
            healthSlider.maxValue = 100f;
        }
    }

    void Update()
    {
        // Prevent errors if the baby object isn't assigned
        if (babyScript == null) return;

        // Update the slider
        if (healthSlider != null) 
        {
            healthSlider.value = babyScript.health;
        }

        // Update the text with a percentage symbol
        if (healthText != null)
        {
            // Mathf.RoundToInt membulatkan angka (contoh: 99.8 menjadi 100)
            int currentHealth = Mathf.RoundToInt(babyScript.health);
            healthText.text = currentHealth.ToString() + "%";
        }
    }
}