using UnityEngine;

public class BabyBehavior : MonoBehaviour
{
    public enum BabyState { Normal, Lapar, TidakNyaman, Demam, Crying }
    
    [Header("Current State")]
    public BabyState currentState = BabyState.Normal;

    [Header("Baby Status Parameters")]
    [Range(0, 100)] public float hunger = 100f;
    [Range(0, 100)] public float comfort = 100f;
    [Range(36.0f, 41.0f)] public float temperature = 36.5f;

    [Header("Thresholds")]
    public float thresholdNormal = 40f;
    public float feverThreshold = 37.5f;
    public float criticalThreshold = 15f;

    [Header("Decay Multipliers")]
    public float timeScale = 1.0f; 

    private BabyAnimator babyAnim;

    private void Start()
    {
        babyAnim = GetComponent<BabyAnimator>();
    }

    private void Update()
    {
        if (GameManager.Instance.currentState != GameManager.GameState.Playing) return;

        UpdateStatus();
        EvaluateState();
    }

    private void UpdateStatus()
    {
        // Penurunan nilai dasar seiring waktu
        hunger -= timeScale * Time.deltaTime;
        comfort -= timeScale * Time.deltaTime;

        // Cegah nilai tembus ke negatif
        hunger = Mathf.Clamp(hunger, 0, 100);
        comfort = Mathf.Clamp(comfort, 0, 100);
    }

    private void EvaluateState()
    {
        BabyState previousState = currentState;

        if (hunger < criticalThreshold || comfort < criticalThreshold)
            currentState = BabyState.Crying; // [cite: 187]
        else if (temperature >= feverThreshold)
            currentState = BabyState.Demam;
        else if (hunger < thresholdNormal)
            currentState = BabyState.Lapar;
        else if (comfort < thresholdNormal)
            currentState = BabyState.TidakNyaman;
        else
            currentState = BabyState.Normal;

        // Update animasi hanya jika state berubah
        if (currentState != previousState)
        {
            babyAnim.UpdateAnimatorState(currentState);
        }
    }

    // Fungsi ini dipanggil GameManager di akhir hari untuk cek apakah game over
    public bool IsBabySafe()
    {
        // Jika bayi dalam kondisi menangis terus atau demam parah di akhir hari, gagal
        return currentState != BabyState.Crying && temperature < 38.5f;
    }

    // --- Fungsi Interaksi dari Controller VR ---
    public void ReceiveFood()
    {
        hunger = 100f;
        Debug.Log("Pemain memberi susu.");
    }

    public void ChangeDiaper()
    {
        comfort = 100f;
        Debug.Log("Pemain mengganti popok.");
    }

    public void GiveMedicine()
    {
        temperature = 36.5f;
        Debug.Log("Pemain memberikan obat penurun panas.");
    }
}