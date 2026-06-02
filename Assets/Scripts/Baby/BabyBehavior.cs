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

    private float lastPilekOnsetLog = -1f;           // Track kapan Pilek onset di-log
    private float lastBatukOnsetLog = -1f;           // Track kapan Batuk onset di-log
    private float lastBatukBerdahakOnsetLog = -1f;   // Track kapan BatukBerdahak onset di-log
    private float lastSesakOnsetLog = -1f;           // Track kapan SesakNafas onset di-log
    private float lastDemamOnsetLog = -1f;           // Track kapan Demam onset di-log
    private float lastCryOnsetLog = -1f;             // Track kapan Cry onset di-log
    private int lastCommonColdCycleIndex = -1;
    private int lastPneumoniaCycleIndex = -1;

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
    public float cc_onset_batuk = 10f;
    [Tooltip("Common Cold: Nangis onset delay setelah Batuk (s) - jadi total onset = cc_onset_batuk + ini")]
    public float cc_onset_cry_delay = 10f;
    [Tooltip("Common Cold: Durasi Nangis sebelum kembali ke Pilek (s)")]
    public float cc_cry_duration = 10f;

    [Tooltip("Pneumonia: Pilek onset (s)")]
    public float pn_onset_pilek = 0f;
    [Tooltip("Pneumonia: Batuk berdahak onset (s)")]
    public float pn_onset_batuk_berdahak = 10f;
    [Tooltip("Pneumonia: Sesak nafas onset (s)")]
    public float pn_onset_sesak = 20f;
    [Tooltip("Pneumonia: Demam onset delay setelah Sesak (s) - jadi total onset = pn_onset_sesak + ini")]
    public float pn_onset_demam_delay = 10f;  // 10 detik setelah Sesak Nafas
    [Tooltip("Pneumonia: Nangis onset delay setelah Demam (s) - jadi total onset = demam onset + ini")]
    public float pn_onset_cry_delay = 10f;    // 10 detik setelah Demam
    [Tooltip("Pneumonia: Durasi Nangis sebelum kembali ke Pilek (s)")]
    public float pn_cry_duration = 10f;

    private float cc_onset_cry_calculated = -1f;
    private float pn_onset_demam_calculated = -1f;  // Calculated saat SesakNafas onset
    private float pn_onset_cry_calculated = -1f;    // Calculated saat Demam onset

    // ============ RANDOM INFECTION ============
    [Header("Initial Rest Time")]
    [Tooltip("Durasi awal bayi tetap sehat sebelum penyakit biasa boleh muncul otomatis.")]
    public float initialRestDuration = 10f;
    [Tooltip("Jika aktif, bayi otomatis terkena penyakit biasa setelah rest time selesai.")]
    public bool autoInfectCommonColdAfterRest = true;

    [Header("Random Infection")]
    public float diseaseChancePerSecond = 0.001f;   // Random chance to catch cold each second
    private float initialRestStartedAt = -1f;
    private bool initialRestAutoInfectionTriggered = false;
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
            // Fever acts as a multiplier (e.g. 1.0x normal, up to 1.5x during high fever) instead of massive flat damage
            float tempMultiplier = temperature >= feverThreshold ? 1.0f + ((temperature - feverThreshold) * 0.5f) : 1.0f;

            health -= timeScale * (healthDecayRate * diseaseSeverity * tempMultiplier) * Time.deltaTime;
        }
        else if (temperature > 38f)
        {
            // Demam tanpa penyakit juga damage health (tapi scaling kecil berdasarkan decay rate user)
            health -= timeScale * (healthDecayRate * (temperature - 38f)) * Time.deltaTime;
        }

        health = Mathf.Clamp(health, 0, 100f);

        // OXYGEN: Decreases hanya saat fase Sesak Nafas, recovers otherwise
        if (HasSymptom(Symptom.SesakNafas) || IsPneumoniaSesakPhase())
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

        // Rest time awal sebelum penyakit biasa muncul.
        if (currentDisease == DiseaseState.None)
        {
            if (initialRestStartedAt < 0f)
            {
                initialRestStartedAt = Time.time;
                nextDiseaseCheckTime = Time.time + 1f;
                Debug.Log($"[REST] Bayi mulai rest time awal selama {initialRestDuration:F1}s sebelum penyakit biasa muncul.");
            }

            if (IsInitialRestTimeComplete())
            {
                if (autoInfectCommonColdAfterRest && !initialRestAutoInfectionTriggered)
                {
                    initialRestAutoInfectionTriggered = true;
                    InfectCommonCold();
                }
                else if (!autoInfectCommonColdAfterRest && Time.time > nextDiseaseCheckTime)
                {
                    if (Random.value < diseaseChancePerSecond)
                    {
                        InfectCommonCold();
                    }
                    nextDiseaseCheckTime = Time.time + 1f;
                }
            }
            else
            {
                nextDiseaseCheckTime = Time.time + 1f;
            }
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
            float cycleDuration = GetCommonColdCycleDuration();
            ResetPhaseLogsIfNewCycle(ref lastCommonColdCycleIndex, cycleDuration);

            float phaseTime = GetLoopedElapsedTime(cycleDuration);
            float cryOnset = GetCommonColdCryOnsetTime();

            // Phase 3: Nangis, Batuk tetap jadi gejala klinisnya.
            if (phaseTime >= cryOnset)
            {
                activeSymptoms.Add(Symptom.Batuk);
                if (lastCryOnsetLog < 0)
                {
                    Debug.Log($"<color=yellow>[SYMPTOM ONSET] Common Cold Phase 3: Nangis muncul di T={phaseTime:F1}s. Gejala Batuk dihentikan secara visual/audio.</color>");
                    lastCryOnsetLog = phaseTime;
                }
            }
            // Phase 2: Batuk
            else if (phaseTime >= cc_onset_batuk)
            {
                activeSymptoms.Add(Symptom.Batuk);
                if (lastBatukOnsetLog < 0)
                {
                    Debug.Log($"<color=yellow>[SYMPTOM ONSET] Common Cold Phase 2: Batuk muncul di T={phaseTime:F1}s. Gejala sebelumnya dihentikan.</color>");
                    lastBatukOnsetLog = phaseTime;
                }
            }
            // Phase 1: Pilek
            else if (phaseTime >= cc_onset_pilek)
            {
                activeSymptoms.Add(Symptom.Pilek);
                if (lastPilekOnsetLog < 0)
                {
                    Debug.Log($"<color=yellow>[SYMPTOM ONSET] Common Cold Phase 1: Pilek muncul di T={phaseTime:F1}s</color>");
                    lastPilekOnsetLog = phaseTime;
                }
            }
        }
        else if (currentDisease == DiseaseState.Pneumonia)
        {
            float cycleDuration = GetPneumoniaCycleDuration();
            ResetPhaseLogsIfNewCycle(ref lastPneumoniaCycleIndex, cycleDuration);

            // Pneumonia phases: eksklusif agar animasi/audio/particle tidak saling tumpuk.
            float phaseTime = GetLoopedElapsedTime(cycleDuration);
            float demamOnset = GetPneumoniaDemamOnsetTime();
            float cryOnset = GetPneumoniaCryOnsetTime();

            // Phase 5: Nangis (T = 40s+), demam tetap jadi gejala klinisnya.
            if (phaseTime >= cryOnset)
            {
                activeSymptoms.Add(Symptom.Demam);
                if (lastCryOnsetLog < 0)
                {
                    Debug.Log($"<color=yellow>[SYMPTOM ONSET] Phase 5: Nangis muncul di T={phaseTime:F1}s setelah Demam 10 detik. Efek Demam dihentikan.</color>");
                    lastCryOnsetLog = phaseTime;
                }
            }
            // Phase 4: Demam (T = 30s - 40s)
            else if (phaseTime >= demamOnset)
            {
                activeSymptoms.Add(Symptom.Demam);
                if (lastDemamOnsetLog < 0)
                {
                    Debug.Log($"<color=yellow>[SYMPTOM ONSET] Phase 4: Demam muncul di T={phaseTime:F1}s. Gejala sebelumnya dihentikan.</color>");
                    lastDemamOnsetLog = phaseTime;
                }
            }
            // Phase 3: Sesak Nafas (T = 20s - 30s)
            else if (phaseTime >= pn_onset_sesak)
            {
                activeSymptoms.Add(Symptom.SesakNafas);
                if (lastSesakOnsetLog < 0)
                {
                    Debug.Log($"<color=yellow>[SYMPTOM ONSET] Phase 3: Sesak Nafas muncul di T={phaseTime:F1}s. Gejala sebelumnya dihentikan.</color>");
                    lastSesakOnsetLog = phaseTime;
                }
            }
            // Phase 2: Batuk Berdahak (T = 10s - 20s)
            else if (phaseTime >= pn_onset_batuk_berdahak)
            {
                activeSymptoms.Add(Symptom.BatukBerdahak);
                if (lastBatukBerdahakOnsetLog < 0)
                {
                    Debug.Log($"<color=yellow>[SYMPTOM ONSET] Phase 2: Batuk Berdahak muncul di T={phaseTime:F1}s. Gejala sebelumnya dihentikan.</color>");
                    lastBatukBerdahakOnsetLog = phaseTime;
                }
            }
            // Phase 1: Pilek (T = 0s - 10s)
            else if (phaseTime >= pn_onset_pilek)
            {
                activeSymptoms.Add(Symptom.Pilek);
                if (lastPilekOnsetLog < 0)
                {
                    Debug.Log($"<color=yellow>[SYMPTOM ONSET] Phase 1: Pilek muncul di T={phaseTime:F1}s</color>");
                    lastPilekOnsetLog = phaseTime;
                }
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

        // Reset onset tracking
        ResetOnsetLogs();
        cc_onset_cry_calculated = -1f;
        pn_onset_demam_calculated = -1f;
        pn_onset_cry_calculated = -1f;
        lastCommonColdCycleIndex = -1;
        lastPneumoniaCycleIndex = -1;

        Debug.Log($"\n<color=yellow>[INFECTION] Bayi terkena PENYAKIT BIASA</color> | Durasi: {commonColdDuration}s\n");
        lastDisease = currentDisease;
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

        // Reset onset tracking untuk log fresh & demam delay
        ResetOnsetLogs();
        cc_onset_cry_calculated = -1f;
        pn_onset_demam_calculated = -1f;
        pn_onset_cry_calculated = -1f;
        lastCommonColdCycleIndex = -1;
        lastPneumoniaCycleIndex = -1;

        Debug.Log($"\n<color=red>[INFECTION] Bayi terkena PNEUMONIA</color> | Durasi: {pneumoniaDuration}s\n");
        lastDisease = currentDisease;
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
            ResetOnsetLogs();
            lastCommonColdCycleIndex = -1;
            lastPneumoniaCycleIndex = -1;
            lastDisease = currentDisease;
        }
    }

    // ============ HELPER METHODS ============
    private bool IsInitialRestTimeComplete()
    {
        if (initialRestDuration <= 0f)
        {
            return true;
        }

        return initialRestStartedAt >= 0f && Time.time - initialRestStartedAt >= initialRestDuration;
    }

    private float GetLoopedElapsedTime(float cycleDuration)
    {
        if (cycleDuration <= 0f)
        {
            return diseaseElapsedTime;
        }

        return Mathf.Repeat(diseaseElapsedTime, cycleDuration);
    }

    private void ResetPhaseLogsIfNewCycle(ref int lastCycleIndex, float cycleDuration)
    {
        int currentCycleIndex = cycleDuration > 0f ? Mathf.FloorToInt(diseaseElapsedTime / cycleDuration) : 0;
        if (currentCycleIndex == lastCycleIndex)
        {
            return;
        }

        ResetOnsetLogs();
        lastCycleIndex = currentCycleIndex;
    }

    private void ResetOnsetLogs()
    {
        lastPilekOnsetLog = -1f;
        lastBatukOnsetLog = -1f;
        lastBatukBerdahakOnsetLog = -1f;
        lastSesakOnsetLog = -1f;
        lastDemamOnsetLog = -1f;
        lastCryOnsetLog = -1f;
    }

    public bool HasSymptom(Symptom symptom)
    {
        return activeSymptoms.Contains(symptom);
    }

    public float GetCommonColdCryOnsetTime()
    {
        if (cc_onset_cry_calculated < 0f)
        {
            cc_onset_cry_calculated = cc_onset_batuk + cc_onset_cry_delay;
        }

        return cc_onset_cry_calculated;
    }

    public float GetCommonColdCycleDuration()
    {
        return GetCommonColdCryOnsetTime() + cc_cry_duration;
    }

    public bool IsCommonColdCryPhase()
    {
        return currentDisease == DiseaseState.CommonCold &&
               GetLoopedElapsedTime(GetCommonColdCycleDuration()) >= GetCommonColdCryOnsetTime();
    }

    public float GetPneumoniaDemamOnsetTime()
    {
        if (pn_onset_demam_calculated < 0f)
        {
            pn_onset_demam_calculated = pn_onset_sesak + pn_onset_demam_delay;
        }

        return pn_onset_demam_calculated;
    }

    public float GetPneumoniaCryOnsetTime()
    {
        if (pn_onset_cry_calculated < 0f)
        {
            pn_onset_cry_calculated = GetPneumoniaDemamOnsetTime() + pn_onset_cry_delay;
        }

        return pn_onset_cry_calculated;
    }

    public bool IsPneumoniaCryPhase()
    {
        return currentDisease == DiseaseState.Pneumonia &&
               GetLoopedElapsedTime(GetPneumoniaCycleDuration()) >= GetPneumoniaCryOnsetTime();
    }

    public bool IsPneumoniaSesakPhase()
    {
        if (currentDisease != DiseaseState.Pneumonia)
        {
            return false;
        }

        float phaseTime = GetLoopedElapsedTime(GetPneumoniaCycleDuration());
        return phaseTime >= pn_onset_sesak && phaseTime < GetPneumoniaDemamOnsetTime();
    }

    public float GetPneumoniaCycleDuration()
    {
        return GetPneumoniaCryOnsetTime() + pn_cry_duration;
    }

    public bool ShouldPlayCryAnimation()
    {
        if (IsCommonColdCryPhase() || IsPneumoniaCryPhase())
        {
            return true;
        }

        // Saat sakit, cry dikunci sampai fase nangis agar flow tidak meloncat.
        return currentDisease == DiseaseState.None && health < criticalHealthThreshold;
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
