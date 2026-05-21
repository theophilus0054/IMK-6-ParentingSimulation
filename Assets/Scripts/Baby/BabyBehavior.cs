using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// UNIFIED Baby Behavior + Disease Script
/// Mengelola:
/// - Vital Signs: Health, Temperature, Oxygen Level
/// - Disease Progression: None → CommonCold → Pneumonia
/// - Symptoms: Pilek, Batuk, SesakNafas, BatukBerdahak, Demam
/// - State Triggering: Animations, Effects, Audio
/// 
/// Flow:
/// Normal → Penyakit Biasa (Pilek + Batuk) → tetap/develop ke Pneumonia (Pilek + Sesak + Batuk Berdahak + Demam)
/// </summary>
public class BabyBehavior : MonoBehaviour
{
    [System.Serializable]
    public enum DiseaseState { None, CommonCold, Pneumonia }

    [System.Serializable]
    public enum Symptom { None, Pilek, Batuk, SesakNafas, BatukBerdahak, Demam }

    // ============ VITAL SIGNS ============
    [Header("📊 VITAL SIGNS")]
    [Range(0, 100)] public float health = 100f;
    [Range(36.0f, 41.0f)] public float temperature = 36.5f;
    [Range(0, 100)] public float oxygenLevel = 100f;

    // ============ DISEASE STATE ============
    [Header("🦠 DISEASE STATE")]
    public DiseaseState currentDisease = DiseaseState.None;
    public List<Symptom> activeSymptoms = new List<Symptom>();
    
    [Header("Disease Timers")]
    public float diseaseElapsedTime = 0f;
    public float diseaseProgressionChance = 0.1f;  // Chance per second to progress to Pneumonia
    
    private DiseaseState lastLoggedDisease = DiseaseState.None;
    private List<Symptom> lastLoggedSymptoms = new List<Symptom>();
    private DiseaseState lastDisease = DiseaseState.None; // track external changes

    // Editor: handle changes made in Inspector so symptoms update immediately
    private void OnValidate()
    {
        // Only run in editor (both edit & play) to initialize when user changes enum via Inspector
        if (currentDisease != lastDisease)
        {
            if (currentDisease == DiseaseState.CommonCold)
            {
                InfectCommonCold();
            }
            else if (currentDisease == DiseaseState.Pneumonia)
            {
                InfectPneumonia();
            }
            else if (currentDisease == DiseaseState.None)
            {
                CureDisease();
            }

            // Refresh symptoms list for editor visibility
            UpdateSymptoms();
            lastDisease = currentDisease;
        }
    }

    // ============ THRESHOLDS & DECAY ============
    [Header("⚙️ THRESHOLDS")]
    public float feverThreshold = 37.5f;
    public float lowOxygenThreshold = 40f;
    public float criticalHealthThreshold = 20f;

    [Header("Decay Rates")]
    public float timeScale = 1.0f;
    public float healthDecayRate = 5f;              // Per second when sick
    public float oxygenDecayRate = 10f;             // Per second when sesak nafas

    // ============ DISEASE DURATIONS ============
    [Header("Disease Duration")]
    public float commonColdDuration = 30f;          // short for testing (30s)
    public float pneumoniaDuration = 60f;           // short for testing (60s)
    
    private float diseaseCureDuration = 0f;

    [Header("Symptom Onset Times (seconds)")]
    [Tooltip("Common Cold: Pilek onset (s)")]
    public float cc_onset_pilek = 0f;
    [Tooltip("Common Cold: Batuk onset (s)")]
    public float cc_onset_batuk = 3f;

    [Tooltip("Pneumonia: Pilek onset (s)")]
    public float pn_onset_pilek = 0f;
    [Tooltip("Pneumonia: Batuk berdahak onset (s)")]
    public float pn_onset_batuk_berdahak = 2f;
    [Tooltip("Pneumonia: Sesak nafas onset (s)")]
    public float pn_onset_sesak = 5f;
    
    // ============ RANDOM INFECTION ============
    [Header("Random Infection")]
    public float diseaseChancePerSecond = 0.001f;   // Random chance to catch cold each second
    private float nextDiseaseCheckTime = 0f;

    // ============ COMPONENTS ============
    private BabyAnimator babyAnim;
    private BabyAudioCue babyAudio;

    private void Start()
    {
        babyAnim = GetComponent<BabyAnimator>();
        babyAudio = GetComponent<BabyAudioCue>();

        if (babyAnim == null)
            Debug.LogError("[BabyBehavior] BabyAnimator component tidak ditemukan!");
        if (babyAudio == null)
            Debug.LogWarning("[BabyBehavior] BabyAudioCue component tidak ditemukan!");
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.currentState != GameManager.GameState.Playing)
            return;

        // Update vital signs & disease
        UpdateVitalSigns();
        UpdateDiseaseProgression();
        UpdateSymptoms();
        UpdateAnimationState();
        UpdateAudioCues();
    }

    // ============ VITAL SIGNS UPDATE ============
    private void UpdateVitalSigns()
    {
        float prevHealth = health;
        float prevOxygen = oxygenLevel;

        // HEALTH DECAY: Tergantung disease severity dan temperature
        if (currentDisease != DiseaseState.None)
        {
            float diseaseSeverity = GetDiseaseSeverity();
            float tempFactor = temperature >= feverThreshold ? (temperature - feverThreshold) * 2f : 0f;
            health -= timeScale * (healthDecayRate * diseaseSeverity + tempFactor) * Time.deltaTime;
        }
        else if (temperature > 38f)
        {
            // Demam tanpa penyakit juga damage health
            health -= timeScale * (temperature - 38f) * Time.deltaTime;
        }
        
        health = Mathf.Clamp(health, 0, 100f);

        // OXYGEN: Decreases jika Sesak Nafas, recovers otherwise
        if (HasSymptom(Symptom.SesakNafas))
        {
            oxygenLevel -= timeScale * oxygenDecayRate * Time.deltaTime;
        }
        else
        {
            oxygenLevel += timeScale * (oxygenDecayRate * 0.5f) * Time.deltaTime;
        }
        
        oxygenLevel = Mathf.Clamp(oxygenLevel, 0, 100f);

        // Log significant changes
        if (Mathf.FloorToInt(prevHealth) != Mathf.FloorToInt(health) && Mathf.FloorToInt(health) % 10 == 0)
        {
            Debug.Log($"[STATUS] Health: {health:F1}%");
        }
        if (Mathf.FloorToInt(prevOxygen) != Mathf.FloorToInt(oxygenLevel) && Mathf.FloorToInt(oxygenLevel) % 10 == 0)
        {
            Debug.Log($"[STATUS] Oxygen: {oxygenLevel:F1}%");
        }
    }

    // ============ DISEASE PROGRESSION ============
    private void UpdateDiseaseProgression()
    {
        // Detect external changes to currentDisease (e.g., via Inspector) and initialize properly
        if (currentDisease != lastDisease)
        {
            if (currentDisease == DiseaseState.CommonCold)
            {
                InfectCommonCold();
            }
            else if (currentDisease == DiseaseState.Pneumonia)
            {
                InfectPneumonia();
            }
            else if (currentDisease == DiseaseState.None)
            {
                CureDisease();
            }
            lastDisease = currentDisease;
        }

        // Random infection when healthy
        if (currentDisease == DiseaseState.None && Time.time > nextDiseaseCheckTime)
        {
            if (Random.value < diseaseChancePerSecond)
            {
                InfectCommonCold();
            }
            nextDiseaseCheckTime = Time.time + 1f;
        }

        // Disease timer update
        if (currentDisease != DiseaseState.None)
        {
            diseaseElapsedTime += Time.deltaTime * timeScale;

            // Temperature increases during disease
            if (currentDisease == DiseaseState.Pneumonia)
            {
                temperature += (temperature >= 38f ? 0.1f : 0.05f) * Time.deltaTime;
            }
            else
            {
                temperature += 0.02f * Time.deltaTime;
            }
            temperature = Mathf.Clamp(temperature, 36f, 41f);

            // Check progression: CommonCold → Pneumonia
            if (currentDisease == DiseaseState.CommonCold && diseaseElapsedTime > 30f)
            {
                if (Random.value < diseaseProgressionChance * Time.deltaTime)
                {
                    ProgressToPneumonia();
                }
            }

            // Check cure condition
            if (diseaseElapsedTime > diseaseCureDuration)
            {
                CureDisease();
            }
        }
        else
        {
            // Temperature recovers when healthy
            temperature = Mathf.Lerp(temperature, 36.5f, Time.deltaTime * 0.5f);
        }
    }

    // ============ SYMPTOM MANAGEMENT ============
    private void UpdateSymptoms()
    {
        activeSymptoms.Clear();

        // Add symptoms based on diseaseElapsedTime and configured onset times
        if (currentDisease == DiseaseState.CommonCold)
        {
            if (diseaseElapsedTime >= cc_onset_pilek) activeSymptoms.Add(Symptom.Pilek);
            if (diseaseElapsedTime >= cc_onset_batuk) activeSymptoms.Add(Symptom.Batuk);
        }
        else if (currentDisease == DiseaseState.Pneumonia)
        {
            if (diseaseElapsedTime >= pn_onset_pilek) activeSymptoms.Add(Symptom.Pilek);
            if (diseaseElapsedTime >= pn_onset_batuk_berdahak) activeSymptoms.Add(Symptom.BatukBerdahak);
            if (diseaseElapsedTime >= pn_onset_sesak) activeSymptoms.Add(Symptom.SesakNafas);

            // Demam jadi symptom jika suhu naik melewati threshold
            if (temperature >= feverThreshold)
            {
                activeSymptoms.Add(Symptom.Demam);
            }
        }

        // Log hanya jika symptoms berubah
        bool symptomsChanged = activeSymptoms.Count != lastLoggedSymptoms.Count;
        if (!symptomsChanged)
        {
            foreach (var sym in activeSymptoms)
            {
                if (!lastLoggedSymptoms.Contains(sym))
                {
                    symptomsChanged = true;
                    break;
                }
            }
        }

        if (symptomsChanged)
        {
            string symptoms = activeSymptoms.Count > 0 ? string.Join(", ", activeSymptoms) : "None";
            Debug.Log($"[SYMPTOMS] {symptoms}");
            lastLoggedSymptoms = new List<Symptom>(activeSymptoms);
        }
    }

    // ============ ANIMATION ROUTING ============
    private void UpdateAnimationState()
    {
        if (babyAnim == null) return;

        string animationState = "Lay breath"; // Default

        // Priority: disease symptoms first
        if (HasSymptom(Symptom.SesakNafas))
        {
            animationState = "Fast breath";
        }
        else if (HasSymptom(Symptom.Demam))
        {
            animationState = "Rewel";
        }
        else if (HasSymptom(Symptom.BatukBerdahak) || HasSymptom(Symptom.Batuk))
        {
            animationState = "Cough";
        }
        else if (health < criticalHealthThreshold)
        {
            animationState = "Crying";
        }

        babyAnim.PlayAnimationState(animationState);
    }

    // ============ AUDIO ROUTING ============
    private void UpdateAudioCues()
    {
        if (babyAudio == null) return;
        // Audio triggered otomatis di BabyAudioCue.Update() berdasarkan symptoms
    }

    // ============ DISEASE INFECTION METHODS ============
    public void InfectCommonCold()
    {
        // Initialize or re-initialize common cold parameters
        currentDisease = DiseaseState.CommonCold;
        diseaseElapsedTime = 0f;
        diseaseCureDuration = commonColdDuration;
        temperature = 37f;

        Debug.Log($"\n<color=yellow>[INFECTION] Bayi terkena PENYAKIT BIASA</color> | Durasi: {commonColdDuration}s\n");
        if (currentDisease != lastLoggedDisease)
        {
            lastLoggedDisease = currentDisease;
        }
    }

    public void InfectPneumonia()
    {
        currentDisease = DiseaseState.Pneumonia;
        diseaseElapsedTime = 0f;
        diseaseCureDuration = pneumoniaDuration;
        temperature = 38.5f;
        
        Debug.Log($"\n<color=red>[INFECTION] Bayi terkena PNEUMONIA</color> | Durasi: {pneumoniaDuration}s\n");
    }

    public void ProgressToPneumonia()
    {
        if (currentDisease == DiseaseState.CommonCold)
        {
            InfectPneumonia();
        }
    }

    public void CureDisease()
    {
        if (currentDisease != DiseaseState.None)
        {
            Debug.Log($"<color=green>[CURED] Penyakit sembuh!</color>");
            currentDisease = DiseaseState.None;
            diseaseElapsedTime = 0f;
            activeSymptoms.Clear();
            temperature = 36.5f;
        }
    }

    // ============ HELPER METHODS ============
    public bool HasSymptom(Symptom symptom)
    {
        return activeSymptoms.Contains(symptom);
    }

    public float GetDiseaseSeverity()
    {
        if (currentDisease == DiseaseState.CommonCold) return 0.4f;
        if (currentDisease == DiseaseState.Pneumonia) return 0.8f;
        return 0f;
    }

    public bool IsBabySafe()
    {
        return health > 30f && temperature < 38.5f && oxygenLevel > 25f;
    }

    // ============ PLAYER INTERACTIONS ============
    public void ProvideNourishment()
    {
        health = Mathf.Min(health + 30f, 100f);
        Debug.Log($"<color=green>[INTERACTION] Pemain memberi nutrisi → Health +30 (sekarang: {health:F1}%)</color>");
    }

    public void ProvideCare()
    {
        health = Mathf.Min(health + 20f, 100f);
        Debug.Log($"<color=green>[INTERACTION] Pemain memberikan perawatan → Health +20 (sekarang: {health:F1}%)</color>");
    }

    public void GiveMedicine()
    {
        temperature = 36.5f;
        health = Mathf.Min(health + 25f, 100f);
        CureDisease();
        Debug.Log($"<color=cyan>[INTERACTION] Pemain memberikan obat → Semua gejala hilang, Health +25</color>");
    }

    public void SupplementOxygen()
    {
        oxygenLevel = Mathf.Min(oxygenLevel + 40f, 100f);
        Debug.Log($"<color=cyan>[INTERACTION] Pemain memberikan oksigen → Oxygen +40 (sekarang: {oxygenLevel:F1}%)</color>");
    }

    // ============ DEBUG METHODS ============
    public void ForceCommonCold()
    {
        InfectCommonCold();
    }

    public void ForcePneumonia()
    {
        InfectPneumonia();
    }

    public void ForceHeal()
    {
        CureDisease();
    }
}