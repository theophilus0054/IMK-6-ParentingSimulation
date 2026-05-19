using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Example script untuk menggunakan DiagnosisValidator
/// Tunjukkan cara submit diagnosis dan handle hasil validation
/// </summary>
public class DiagnosisExample : MonoBehaviour
{
    [SerializeField] private DiagnosisValidator diagnosisValidator;

    private void Start()
    {
        // Auto-find validator jika belum di-assign
        if (diagnosisValidator == null)
            diagnosisValidator = FindObjectOfType<DiagnosisValidator>();

        if (diagnosisValidator != null)
        {
            // Subscribe ke event result
            diagnosisValidator.onDiagnosisSubmitted.AddListener(OnDiagnosisResult);
        }
    }

    /// <summary>
    /// CONTOH 1: Submit diagnosis dengan enum list
    /// </summary>
    public void SubmitDiagnosisExample1()
    {
        // Pilihan gejala player
        List<BabyDisease.Symptom> playerDiagnosis = new List<BabyDisease.Symptom>
        {
            BabyDisease.Symptom.Pilek,
            BabyDisease.Symptom.Batuk,
            BabyDisease.Symptom.Demam
        };

        Debug.Log("[DiagnosisExample] Submitting diagnosis with 3 symptoms...");
        var result = diagnosisValidator.SubmitDiagnosis(playerDiagnosis);

        if (result != null)
        {
            diagnosisValidator.PrintDiagnosisResult(result);
        }
    }

    /// <summary>
    /// CONTOH 2: Submit diagnosis dengan string list
    /// </summary>
    public void SubmitDiagnosisExample2()
    {
        List<string> playerDiagnosis = new List<string>
        {
            "Pilek",
            "SesakNafas",
            "BatukBerdahak"
        };

        Debug.Log("[DiagnosisExample] Submitting diagnosis with string names...");
        var result = diagnosisValidator.SubmitDiagnosisByName(playerDiagnosis);

        if (result != null)
        {
            diagnosisValidator.PrintDiagnosisResult(result);
        }
    }

    /// <summary>
    /// Handle hasil diagnosis validation
    /// </summary>
    private void OnDiagnosisResult(DiagnosisValidator.DiagnosisResult result)
    {
        Debug.Log($"\n===== DIAGNOSIS FEEDBACK =====");
        Debug.Log($"Accuracy Level: {result.accuracy}");
        Debug.Log($"Score: {result.accuracyScore:F1}%");
        Debug.Log($"Message: {result.GetAccuracyDescription()}");
        Debug.Log($"");

        // Tampilkan detail per gejala
        Debug.Log("Symptom Details:");
        foreach (var feedback in result.symptoms)
        {
            if (feedback.isActual || feedback.playerSelected)
            {
                Debug.Log($"  - {feedback.symptomName}: {feedback.feedback}");
            }
        }

        Debug.Log($"==============================\n");

        // Custom logic berdasarkan accuracy
        switch (result.accuracy)
        {
            case DiagnosisValidator.DiagnosisAccuracy.Perfect:
                Debug.Log("🎉 Sempurna! Player mendapatkan reward penuh.");
                break;
            case DiagnosisValidator.DiagnosisAccuracy.Good:
                Debug.Log("✓ Bagus! Player mendapatkan reward partial.");
                break;
            case DiagnosisValidator.DiagnosisAccuracy.Fair:
                Debug.Log("◐ Cukup. Player mendapatkan reward minimal.");
                break;
            case DiagnosisValidator.DiagnosisAccuracy.Poor:
            case DiagnosisValidator.DiagnosisAccuracy.Wrong:
                Debug.Log("✗ Salah. Tidak ada reward.");
                break;
        }
    }
}
