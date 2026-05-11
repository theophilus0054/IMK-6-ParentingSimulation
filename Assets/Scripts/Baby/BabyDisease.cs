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

    private BabyBehavior babyBehavior;

    private void Start()
    {
        babyBehavior = GetComponent<BabyBehavior>();
        currentDisease = new Disease { type = DiseaseType.None };

        if (babyBehavior == null)
        {
            Debug.LogError("[BabyDisease] BabyBehavior component tidak ditemukan!");
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

        // Gejala penyakit biasa
        currentDisease.symptoms.Add(Symptom.Pilek);
        if (Random.value > 0.4f) currentDisease.symptoms.Add(Symptom.Batuk);

        Debug.Log($"[DISEASE] Bayi terkena pilek biasa. Severity: {currentDisease.severityLevel:F1}");
    }

    public void InfectPneumonia()
    {
        currentDisease.type = DiseaseType.Pneumonia;
        currentDisease.severityLevel = Random.Range(pneumoniaMinSeverity, pneumoniaMaxSeverity);
        currentDisease.duration = pneumoniaDuration;
        currentDisease.elapsedTime = 0f;
        currentDisease.symptoms.Clear();

        // Gejala pneumonia (parah)
        currentDisease.symptoms.Add(Symptom.Pilek);
        currentDisease.symptoms.Add(Symptom.SesakNafas);
        currentDisease.symptoms.Add(Symptom.BatukBerdahak);
        currentDisease.symptoms.Add(Symptom.Demam);
        
        if (currentDisease.severityLevel > 75f)
        {
            currentDisease.symptoms.Add(Symptom.Pucat);
            currentDisease.symptoms.Add(Symptom.DadaCekung);
        }

        // Trigger demam
        babyBehavior.temperature = Mathf.Max(babyBehavior.temperature, 38.5f);

        Debug.Log($"[DISEASE] Bayi terkena Pneumonia! Severity: {currentDisease.severityLevel:F1}");
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
}
