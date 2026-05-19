using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ScriptableObject untuk menyimpan database gejala (symptom)
/// Menyediakan struktur data terpusat untuk semua gejala penyakit
/// </summary>
public class SymptomDatabase : ScriptableObject
{
    [System.Serializable]
    public class SymptomData
    {
        public string id;                    // Unique identifier (e.g., "pilek", "batuk")
        public string displayName;           // Nama yang ditampilkan di UI
        [TextArea(2, 4)] public string description;  // Deskripsi gejala
        public float severity;               // 0-100: tingkat keparahan gejala
        public string visualEffect;          // Reference untuk efek visual di bayi
        public string audioEffect;           // Reference untuk efek suara
        public bool isVisible;              // Apakah gejala terlihat di UI
        
        // Validasi
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(id) && 
                   !string.IsNullOrWhiteSpace(displayName) && 
                   severity >= 0 && severity <= 100;
        }
    }

    [System.Serializable]
    public class DiseaseData
    {
        public string id;                                   // "common_cold", "pneumonia"
        public string displayName;                          // Nama penyakit
        [TextArea(2, 4)] public string description;         // Deskripsi penyakit
        public List<string> symptomIds = new List<string>(); // ID gejala yang terkait
        public float minSeverity;                           // Minimal severity penyakit
        public float maxSeverity;                           // Maksimal severity penyakit
        public float duration;                              // Durasi penyakit (detik)
        [Range(0f, 1f)] public float probability;           // Probabilitas penyakit muncul
        [Range(0f, 1f)] public float pneumoniaProgression;  // Chance berkembang ke pneumonia
        
        // Validasi
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(id) && 
                   !string.IsNullOrWhiteSpace(displayName) && 
                   minSeverity >= 0 && maxSeverity <= 100 && 
                   minSeverity <= maxSeverity && 
                   duration > 0 &&
                   symptomIds.Count > 0;
        }
    }

    [Header("Symptom Data")]
    public List<SymptomData> symptoms = new List<SymptomData>();

    [Header("Disease Data")]
    public List<DiseaseData> diseases = new List<DiseaseData>();

    /// <summary>
    /// Mencari symptom berdasarkan ID
    /// </summary>
    public SymptomData GetSymptomById(string id)
    {
        return symptoms.Find(s => s.id == id);
    }

    /// <summary>
    /// Mencari disease berdasarkan ID
    /// </summary>
    public DiseaseData GetDiseaseById(string id)
    {
        return diseases.Find(d => d.id == id);
    }

    /// <summary>
    /// Mendapatkan semua symptoms untuk disease tertentu
    /// </summary>
    public List<SymptomData> GetSymptomsForDisease(string diseaseId)
    {
        List<SymptomData> result = new List<SymptomData>();
        DiseaseData disease = GetDiseaseById(diseaseId);
        
        if (disease != null)
        {
            foreach (string symptomId in disease.symptomIds)
            {
                SymptomData symptom = GetSymptomById(symptomId);
                if (symptom != null)
                    result.Add(symptom);
            }
        }
        
        return result;
    }

    /// <summary>
    /// Validasi seluruh database
    /// </summary>
    public bool ValidateDatabase(out List<string> errors)
    {
        errors = new List<string>();
        bool isValid = true;

        // Validasi symptoms
        if (symptoms.Count == 0)
        {
            errors.Add("Database tidak memiliki symptom apapun");
            isValid = false;
        }

        foreach (var symptom in symptoms)
        {
            if (!symptom.IsValid())
            {
                errors.Add($"Symptom '{symptom.id}' tidak valid: field kosong atau severity di luar range");
                isValid = false;
            }
        }

        // Validasi diseases
        if (diseases.Count == 0)
        {
            errors.Add("Database tidak memiliki disease apapun");
            isValid = false;
        }

        foreach (var disease in diseases)
        {
            if (!disease.IsValid())
            {
                errors.Add($"Disease '{disease.id}' tidak valid: field kosong atau parameter tidak sesuai");
                isValid = false;
            }

            // Cek apakah symptom yang direferensi ada
            foreach (string symptomId in disease.symptomIds)
            {
                if (GetSymptomById(symptomId) == null)
                {
                    errors.Add($"Disease '{disease.id}' mereferensi symptom '{symptomId}' yang tidak ada");
                    isValid = false;
                }
            }
        }

        return isValid;
    }

#if UNITY_EDITOR
    /// <summary>
    /// Debug helper untuk menampilkan info database di console
    /// </summary>
    public void PrintDatabaseInfo()
    {
        Debug.Log($"=== SYMPTOM DATABASE INFO ===");
        Debug.Log($"Jumlah Symptoms: {symptoms.Count}");
        foreach (var symptom in symptoms)
        {
            Debug.Log($"  - {symptom.id} ({symptom.displayName}): severity={symptom.severity}");
        }

        Debug.Log($"Jumlah Diseases: {diseases.Count}");
        foreach (var disease in diseases)
        {
            Debug.Log($"  - {disease.id} ({disease.displayName}): {disease.symptomIds.Count} symptoms, probability={disease.probability}");
        }
    }
#endif
}
