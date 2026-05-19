using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

/// <summary>
/// Validator untuk diagnosis/diagnosis yang dipilih player
/// Membandingkan dengan gejala aktual bayi dan memberikan feedback
/// </summary>
public class DiagnosisValidator : MonoBehaviour
{
    public enum DiagnosisAccuracy { Perfect, Good, Fair, Poor, Wrong }

    [System.Serializable]
    public class SymptomFeedback
    {
        public string symptomName;
        public bool isActual;              // Apakah gejala ini benar-benar ada di bayi
        public bool playerSelected;        // Apakah player memilih gejala ini
        public bool isCorrect;             // Apakah pilihan player benar untuk gejala ini
        public string feedback;            // Feedback untuk gejala ini
    }

    [System.Serializable]
    public class DiagnosisResult
    {
        public DiagnosisAccuracy accuracy;
        public float accuracyScore;        // 0-100
        public int correctCount;           // Gejala yang benar dipilih
        public int missedCount;            // Gejala yang seharusnya dipilih tapi tidak
        public int falsePositiveCount;     // Gejala yang dipilih tapi tidak ada
        public List<SymptomFeedback> symptoms = new List<SymptomFeedback>();
        
        public string GetAccuracyDescription()
        {
            return accuracy switch
            {
                DiagnosisAccuracy.Perfect => "Sempurna! Diagnosis Anda sangat akurat.",
                DiagnosisAccuracy.Good => "Bagus! Diagnosis Anda cukup akurat.",
                DiagnosisAccuracy.Fair => "Cukup. Beberapa gejala terlewatkan.",
                DiagnosisAccuracy.Poor => "Kurang akurat. Banyak kesalahan dalam diagnosis.",
                DiagnosisAccuracy.Wrong => "Sangat salah. Diagnosis tidak sesuai kondisi bayi.",
                _ => "Tidak diketahui"
            };
        }
    }

    [Header("Validation Settings")]
    [Range(0f, 1f)] public float falsePositivePenalty = 0.1f;  // Penalty per false positive
    [Range(0f, 1f)] public float missedSymptomPenalty = 0.15f; // Penalty per missed symptom

    [Header("Accuracy Thresholds")]
    [Range(0, 100)] public float perfectThreshold = 90f;   // >= 90 = Perfect
    [Range(0, 100)] public float goodThreshold = 70f;      // >= 70 = Good
    [Range(0, 100)] public float fairThreshold = 50f;      // >= 50 = Fair
    [Range(0, 100)] public float poorThreshold = 20f;      // >= 20 = Poor

    [Header("References")]
    [SerializeField] private BabyDisease babyDisease;
    [SerializeField] private BabyBehavior babyBehavior;

    // Events
    public UnityEvent<DiagnosisResult> onDiagnosisSubmitted = new UnityEvent<DiagnosisResult>();

    private DiagnosisResult lastResult;

    private void Start()
    {
        // Auto-assign components jika belum di-assign
        if (babyDisease == null)
            babyDisease = FindObjectOfType<BabyBehavior>()?.GetComponent<BabyDisease>();
        
        if (babyBehavior == null)
            babyBehavior = FindObjectOfType<BabyBehavior>();

        if (babyDisease == null)
            Debug.LogError("[DiagnosisValidator] BabyDisease component tidak ditemukan!");
        if (babyBehavior == null)
            Debug.LogError("[DiagnosisValidator] BabyBehavior component tidak ditemukan!");
    }

    /// <summary>
    /// Submit diagnosis dengan list gejala yang dipilih player
    /// </summary>
    public DiagnosisResult SubmitDiagnosis(List<BabyDisease.Symptom> playerSelectedSymptoms)
    {
        if (babyDisease == null || babyBehavior == null)
        {
            Debug.LogError("[DiagnosisValidator] Missing references!");
            return null;
        }

        lastResult = ValidateDiagnosis(playerSelectedSymptoms, babyDisease.currentDisease.symptoms);
        
        Debug.Log($"[DiagnosisValidator] Diagnosis submitted:");
        Debug.Log($"  - Accuracy: {lastResult.accuracy} ({lastResult.accuracyScore:F1}%)");
        Debug.Log($"  - Correct: {lastResult.correctCount}, Missed: {lastResult.missedCount}, False Positive: {lastResult.falsePositiveCount}");

        onDiagnosisSubmitted.Invoke(lastResult);
        return lastResult;
    }

    /// <summary>
    /// Submit diagnosis dengan nama symptom (string)
    /// </summary>
    public DiagnosisResult SubmitDiagnosisByName(List<string> playerSelectedSymptomNames)
    {
        List<BabyDisease.Symptom> symptoms = new List<BabyDisease.Symptom>();
        
        foreach (string name in playerSelectedSymptomNames)
        {
            if (System.Enum.TryParse<BabyDisease.Symptom>(name, out BabyDisease.Symptom symptom))
            {
                symptoms.Add(symptom);
            }
        }

        return SubmitDiagnosis(symptoms);
    }

    /// <summary>
    /// Validasi diagnosis dan hitung score
    /// </summary>
    private DiagnosisResult ValidateDiagnosis(List<BabyDisease.Symptom> selected, List<BabyDisease.Symptom> actual)
    {
        DiagnosisResult result = new DiagnosisResult();

        // Hitung true positives, false positives, dan false negatives
        int truePositives = 0;
        int falsePositives = 0;
        int falseNegatives = 0;

        // Remove duplicates
        selected = new List<BabyDisease.Symptom>(new HashSet<BabyDisease.Symptom>(selected));
        actual = new List<BabyDisease.Symptom>(new HashSet<BabyDisease.Symptom>(actual));

        // Cek setiap symptom yang ada di database
        var allSymptoms = GetAllPossibleSymptoms();

        foreach (var symptom in allSymptoms)
        {
            if (symptom == BabyDisease.Symptom.None) continue;

            bool isActual = actual.Contains(symptom);
            bool playerSelected = selected.Contains(symptom);
            bool isCorrect = isActual == playerSelected;

            // Count metrics
            if (isActual && playerSelected)
                truePositives++;
            else if (!isActual && playerSelected)
                falsePositives++;
            else if (isActual && !playerSelected)
                falseNegatives++;

            // Create feedback
            SymptomFeedback feedback = new SymptomFeedback
            {
                symptomName = symptom.ToString(),
                isActual = isActual,
                playerSelected = playerSelected,
                isCorrect = isCorrect,
                feedback = GetSymptomFeedback(symptom, isActual, playerSelected)
            };

            result.symptoms.Add(feedback);
        }

        result.correctCount = truePositives;
        result.missedCount = falseNegatives;
        result.falsePositiveCount = falsePositives;

        // Hitung accuracy score dengan formula weighted
        float score = CalculateAccuracyScore(truePositives, falsePositives, falseNegatives, actual.Count);
        result.accuracyScore = Mathf.Clamp01(score) * 100f;

        // Tentukan accuracy level
        result.accuracy = GetAccuracyLevel(result.accuracyScore);

        return result;
    }

    /// <summary>
    /// Hitung accuracy score (0-1)
    /// Formula: TP / (TP + FP + FN) dengan penalty
    /// </summary>
    private float CalculateAccuracyScore(int truePositives, int falsePositives, int falseNegatives, int actualCount)
    {
        if (actualCount == 0)
        {
            // Jika tidak ada gejala, hanya false positive yang dihitung
            return falsePositives == 0 ? 1f : 0f;
        }

        float score = 0f;

        // Base score: true positives dari total actual
        float baseScore = (float)truePositives / actualCount;

        // Penalty untuk false positives
        float fpPenalty = falsePositives * falsePositivePenalty;

        // Penalty untuk missed symptoms
        float fnPenalty = falseNegatives * missedSymptomPenalty;

        score = baseScore - fpPenalty - fnPenalty;

        return score;
    }

    /// <summary>
    /// Tentukan accuracy level berdasarkan score
    /// </summary>
    private DiagnosisAccuracy GetAccuracyLevel(float score)
    {
        if (score >= perfectThreshold)
            return DiagnosisAccuracy.Perfect;
        else if (score >= goodThreshold)
            return DiagnosisAccuracy.Good;
        else if (score >= fairThreshold)
            return DiagnosisAccuracy.Fair;
        else if (score >= poorThreshold)
            return DiagnosisAccuracy.Poor;
        else
            return DiagnosisAccuracy.Wrong;
    }

    /// <summary>
    /// Dapatkan feedback untuk setiap symptom
    /// </summary>
    private string GetSymptomFeedback(BabyDisease.Symptom symptom, bool isActual, bool playerSelected)
    {
        if (isActual && playerSelected)
            return "✓ Benar - Gejala ini memang ada pada bayi";
        else if (isActual && !playerSelected)
            return "✗ Terlewatkan - Gejala ini seharusnya Anda pilih";
        else if (!isActual && playerSelected)
            return "✗ Salah - Gejala ini tidak ada pada bayi";
        else
            return "○ Tidak dipilih - Benar, gejala ini tidak ada";
    }

    /// <summary>
    /// Dapatkan semua symptom yang mungkin (dari enum)
    /// </summary>
    private List<BabyDisease.Symptom> GetAllPossibleSymptoms()
    {
        List<BabyDisease.Symptom> all = new List<BabyDisease.Symptom>();
        foreach (BabyDisease.Symptom symptom in System.Enum.GetValues(typeof(BabyDisease.Symptom)))
        {
            all.Add(symptom);
        }
        return all;
    }

    /// <summary>
    /// Dapatkan result diagnosis terakhir
    /// </summary>
    public DiagnosisResult GetLastResult()
    {
        return lastResult;
    }

    /// <summary>
    /// Print diagnosis result ke console (untuk debugging)
    /// </summary>
    public void PrintDiagnosisResult(DiagnosisResult result)
    {
        if (result == null)
        {
            Debug.LogWarning("[DiagnosisValidator] No result to print");
            return;
        }

        Debug.Log($"===== DIAGNOSIS RESULT =====");
        Debug.Log($"Accuracy: {result.accuracy} ({result.accuracyScore:F1}%)");
        Debug.Log($"Description: {result.GetAccuracyDescription()}");
        Debug.Log($"");
        Debug.Log($"Statistics:");
        Debug.Log($"  Correct: {result.correctCount}");
        Debug.Log($"  Missed: {result.missedCount}");
        Debug.Log($"  False Positive: {result.falsePositiveCount}");
        Debug.Log($"");
        Debug.Log($"Symptom Feedback:");

        foreach (var feedback in result.symptoms)
        {
            if (feedback.isActual || feedback.playerSelected)
            {
                string icon = feedback.isCorrect ? "✓" : "✗";
                Debug.Log($"  {icon} {feedback.symptomName}: {feedback.feedback}");
            }
        }

        Debug.Log($"===========================");
    }

#if UNITY_EDITOR
    /// <summary>
    /// Test submission dengan random symptoms (Editor only)
    /// </summary>
    public void EditorTestRandomDiagnosis()
    {
        if (babyDisease == null)
        {
            Debug.LogError("[DiagnosisValidator] BabyDisease not assigned!");
            return;
        }

        // Generate random diagnosis
        List<BabyDisease.Symptom> randomSymptoms = new List<BabyDisease.Symptom>();
        var allSymptoms = GetAllPossibleSymptoms();

        foreach (var symptom in allSymptoms)
        {
            if (symptom != BabyDisease.Symptom.None && Random.value > 0.5f)
            {
                randomSymptoms.Add(symptom);
            }
        }

        Debug.Log($"[DiagnosisValidator] Testing with random diagnosis: {randomSymptoms.Count} symptoms selected");
        var result = SubmitDiagnosis(randomSymptoms);
        PrintDiagnosisResult(result);
    }
#endif
}
