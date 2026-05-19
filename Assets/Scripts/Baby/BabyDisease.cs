using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Sistem manajemen penyakit dan gejala bayi
/// Fokus: Penyakit Biasa (pilek, batuk) dan Pneumonia
/// </summary>
public class BabyDisease : MonoBehaviour
{
    public enum DiseaseType { None, CommonCold, Pneumonia }
    public enum Symptom { None, Pilek, Batuk, SesakNafas, BatukBerdahak, Demam, Pucat, DadaCekung }

    [System.Serializable]
    public class Disease
    {
        public DiseaseType type;
        public float severityLevel = 0f; // 0-100
        public List<Symptom> symptoms = new List<Symptom>();
        public float duration = 300f; // detik
        public float elapsedTime = 0f;
    }

    [Header("Disease Parameters")]
    public Disease currentDisease;
    [Range(0f, 1f)] public float diseaseChancePerUpdate = 0.001f; // Probabilitas penyakit muncul per frame

    [Header("Common Cold Properties")]
    public float commonColdDuration = 120f; // 2 menit
    public float commonColdMinSeverity = 30f;
    public float commonColdMaxSeverity = 60f;

    [Header("Pneumonia Properties")]
    public float pneumoniaDuration = 300f; // 5 menit
    public float pneumoniaMinSeverity = 60f;
    public float pneumoniaMaxSeverity = 100f;
    [Range(0f, 1f)] public float pneumoniaChance = 0.3f; // Chance untuk berkembang ke pneumonia

    [Header("Database Settings")]
    [SerializeField] private bool useSymptomDatabase = true;

    private BabyBehavior babyBehavior;
    private SymptomDatabase symptomDatabase;

    private void Start()
    {
        babyBehavior = GetComponent<BabyBehavior>();
        currentDisease = new Disease { type = DiseaseType.None };

        if (babyBehavior == null)
        {
            Debug.LogError("[BabyDisease] BabyBehavior component tidak ditemukan!");
        }

        // Load symptom database jika enabled
        if (useSymptomDatabase)
        {
            LoadSymptomDatabase();
        }
    }

    /// <summary>
    /// Load symptom database dari JSON
    /// </summary>
    private void LoadSymptomDatabase()
    {
        SymptomDatabaseLoader loader = SymptomDatabaseLoader.Instance;
        if (loader == null)
        {
            Debug.LogWarning("[BabyDisease] SymptomDatabaseLoader tidak ditemukan. Menggunakan fallback hardcoded values.");
            useSymptomDatabase = false;
            return;
        }

        symptomDatabase = loader.LoadDatabase();
        if (symptomDatabase == null)
        {
            Debug.LogWarning("[BabyDisease] Gagal load symptom database. Menggunakan fallback hardcoded values.");
            useSymptomDatabase = false;
            return;
        }

        Debug.Log("[BabyDisease] Symptom database berhasil dimuat!");

        // Update disease parameters dari database jika ada
        UpdateDiseaseParametersFromDatabase();
    }

    /// <summary>
    /// Update disease parameters dari database
    /// </summary>
    private void UpdateDiseaseParametersFromDatabase()
    {
        if (symptomDatabase == null) return;

        SymptomDatabase.DiseaseData commonCold = symptomDatabase.GetDiseaseById("common_cold");
        if (commonCold != null)
        {
            commonColdDuration = commonCold.duration;
            commonColdMinSeverity = commonCold.minSeverity;
            commonColdMaxSeverity = commonCold.maxSeverity;
            Debug.Log($"[BabyDisease] Updated Common Cold parameters from database");
        }

        SymptomDatabase.DiseaseData pneumonia = symptomDatabase.GetDiseaseById("pneumonia");
        if (pneumonia != null)
        {
            pneumoniaDuration = pneumonia.duration;
            pneumoniaMinSeverity = pneumonia.minSeverity;
            pneumoniaMaxSeverity = pneumonia.maxSeverity;
            pneumoniaChance = pneumonia.pneumoniaProgression;
            Debug.Log($"[BabyDisease] Updated Pneumonia parameters from database");
        }
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.currentState != GameManager.GameState.Playing) return;

        UpdateDisease();
        CheckForNewDisease();
    }

    private void UpdateDisease()
    {
        if (currentDisease.type == DiseaseType.None) return;

        currentDisease.elapsedTime += Time.deltaTime;

        // Update severity based on temperature
        if (babyBehavior.temperature >= 38f)
        {
            currentDisease.severityLevel = Mathf.Min(currentDisease.severityLevel + Time.deltaTime * 5f, 100f);
        }

        // Penyakit hilang setelah durasi habis
        if (currentDisease.elapsedTime >= currentDisease.duration)
        {
            CureDisease();
        }
    }

    private void CheckForNewDisease()
    {
        if (currentDisease.type != DiseaseType.None) return;

        // Random chance untuk kena penyakit
        if (Random.value < diseaseChancePerUpdate)
        {
            // Pilih penyakit random
            float rand = Random.value;
            if (rand < 0.7f)
            {
                InfectCommonCold();
            }
            else
            {
                InfectPneumonia();
            }
        }
    }

    public void InfectCommonCold()
    {
        currentDisease.type = DiseaseType.CommonCold;
        currentDisease.severityLevel = Random.Range(commonColdMinSeverity, commonColdMaxSeverity);
        currentDisease.duration = commonColdDuration;
        currentDisease.elapsedTime = 0f;
        currentDisease.symptoms.Clear();

        // Gejala penyakit biasa dari database jika tersedia
        if (useSymptomDatabase && symptomDatabase != null)
        {
            SymptomDatabase.DiseaseData diseaseData = symptomDatabase.GetDiseaseById("common_cold");
            if (diseaseData != null)
            {
                List<SymptomDatabase.SymptomData> dbSymptoms = symptomDatabase.GetSymptomsForDisease("common_cold");
                foreach (var symptomData in dbSymptoms)
                {
                    // Map database symptom IDs ke enum
                    Symptom sym = ParseSymptomFromId(symptomData.id);
                    if (sym != Symptom.None)
                    {
                        currentDisease.symptoms.Add(sym);
                    }
                }
                Debug.Log($"[DISEASE] Bayi terkena pilek biasa (dari database). Severity: {currentDisease.severityLevel:F1}, Symptoms: {currentDisease.symptoms.Count}");
            }
        }
        else
        {
            // Fallback ke hardcoded values
            currentDisease.symptoms.Add(Symptom.Pilek);
            if (Random.value > 0.4f) currentDisease.symptoms.Add(Symptom.Batuk);
            Debug.Log($"[DISEASE] Bayi terkena pilek biasa. Severity: {currentDisease.severityLevel:F1}");
        }
    }

    public void InfectPneumonia()
    {
        currentDisease.type = DiseaseType.Pneumonia;
        currentDisease.severityLevel = Random.Range(pneumoniaMinSeverity, pneumoniaMaxSeverity);
        currentDisease.duration = pneumoniaDuration;
        currentDisease.elapsedTime = 0f;
        currentDisease.symptoms.Clear();

        // Gejala pneumonia dari database jika tersedia
        if (useSymptomDatabase && symptomDatabase != null)
        {
            SymptomDatabase.DiseaseData diseaseData = symptomDatabase.GetDiseaseById("pneumonia");
            if (diseaseData != null)
            {
                List<SymptomDatabase.SymptomData> dbSymptoms = symptomDatabase.GetSymptomsForDisease("pneumonia");
                foreach (var symptomData in dbSymptoms)
                {
                    // Map database symptom IDs ke enum
                    Symptom sym = ParseSymptomFromId(symptomData.id);
                    if (sym != Symptom.None && currentDisease.severityLevel > 50f)
                    {
                        currentDisease.symptoms.Add(sym);
                    }
                }
                Debug.Log($"[DISEASE] Bayi terkena Pneumonia (dari database)! Severity: {currentDisease.severityLevel:F1}, Symptoms: {currentDisease.symptoms.Count}");
            }
        }
        else
        {
            // Fallback ke hardcoded values
            currentDisease.symptoms.Add(Symptom.Pilek);
            currentDisease.symptoms.Add(Symptom.SesakNafas);
            currentDisease.symptoms.Add(Symptom.BatukBerdahak);
            currentDisease.symptoms.Add(Symptom.Demam);
            
            if (currentDisease.severityLevel > 75f)
            {
                currentDisease.symptoms.Add(Symptom.Pucat);
                currentDisease.symptoms.Add(Symptom.DadaCekung);
            }
            Debug.Log($"[DISEASE] Bayi terkena Pneumonia! Severity: {currentDisease.severityLevel:F1}");
        }

        // Trigger demam
        babyBehavior.temperature = Mathf.Max(babyBehavior.temperature, 38.5f);
    }

    public void CureDisease()
    {
        Debug.Log($"[DISEASE] Bayi sembuh dari {currentDisease.type}");
        currentDisease.type = DiseaseType.None;
        currentDisease.symptoms.Clear();
        currentDisease.severityLevel = 0f;
        currentDisease.elapsedTime = 0f;
    }

    public bool HasSymptom(Symptom symptom)
    {
        return currentDisease.type != DiseaseType.None && currentDisease.symptoms.Contains(symptom);
    }

    public float GetSeverity()
    {
        return currentDisease.type != DiseaseType.None ? currentDisease.severityLevel : 0f;
    }

    /// <summary>
    /// Convert symptom ID dari database ke enum
    /// </summary>
    private Symptom ParseSymptomFromId(string symptomId)
    {
        return symptomId.ToLower() switch
        {
            "pilek" => Symptom.Pilek,
            "batuk" => Symptom.Batuk,
            "sesak_nafas" => Symptom.SesakNafas,
            "batuk_berdahak" => Symptom.BatukBerdahak,
            "demam" => Symptom.Demam,
            "pucat" => Symptom.Pucat,
            "dada_cekung" => Symptom.DadaCekung,
            _ => Symptom.None
        };
    }
}
