using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

/// <summary>
/// UNIFIED Baby Behavior + Disease Script
/// Mengelola:
/// - Vital Signs: Health, Temperature, Oxygen Level
/// - Disease Progression: None → CommonCold → Pneumonia
/// - Symptoms: Pilek, Batuk, SesakNafas, BatukBerdahak, Demam, Nangis
/// - State Triggering: Animations, Effects, Audio
///
/// Flow:
/// Rest Time -> chance terkena Common Cold -> satu kali cek chance:
/// - berhasil: berkembang ke Pneumonia
/// - gagal: tetap Common Cold sampai disembuhkan
/// Penyakit tidak sembuh otomatis berdasarkan durasi; durasi hanya dipakai untuk menyalakan chance transisi.
/// </summary>
public class BabyBehavior : MonoBehaviour
{
    [System.Serializable]
    public enum DiseaseState { None, CommonCold, Pneumonia }

    [System.Serializable]
    public enum Symptom { None, Pilek, Batuk, SesakNafas, BatukBerdahak, Demam, Nangis }

    // ============ VITAL SIGNS ============
    [Header("VITAL SIGNS")]
    [Range(0, 100)] public float health = 100f;
    [Range(36.0f, 41.0f)] public float temperature = 36.5f;
    [Range(0, 100)] public float oxygenLevel = 100f;

    // ============ DISEASE STATE ============
    [Header("DISEASE STATE")]
    public DiseaseState currentDisease = DiseaseState.None;
    public List<Symptom> activeSymptoms = new List<Symptom>();

    [Header("Disease Timers")]
    public float diseaseElapsedTime = 0f;
    [Tooltip("Matikan agar durasi penyakit dan durasi symptom tetap mengikuti detik nyata. Jika aktif, timeScale ikut mempercepat/memperlambat timer penyakit.")]
    public bool diseaseTimerUsesTimeScale = false;
    [Tooltip("Jika aktif, Common Cold bisa berubah menjadi Pneumonia sebelum durasi Common Cold selesai.")]
    public bool allowCommonColdProgression = false;
    [Min(0f)]
    [Tooltip("Common Cold melakukan satu kali cek peluang berubah ke Pneumonia setelah waktu ini.")]
    public float commonColdProgressionDelay = 30f;
    [Range(0f, 100f)]
    [Tooltip("Persentase peluang Common Cold berubah ke Pneumonia saat cek sekali. 30 berarti 30%.")]
    public float pneumoniaProgressionChancePercent = 30f;
    [HideInInspector, System.Obsolete("Use pneumoniaProgressionChancePercent. This field is kept only for older scripts.")]
    public float diseaseProgressionChance = 0.1f;

    private DiseaseState lastLoggedDisease = DiseaseState.None;
    private List<Symptom> lastLoggedSymptoms = new List<Symptom>();
    private DiseaseState lastDisease = DiseaseState.None; // track external changes
    private bool commonColdProgressionRollCompleted = false;

    private float lastPilekOnsetLog = -1f;           // Track kapan fase Pilek di-log
    private float lastBatukOnsetLog = -1f;           // Track kapan fase Batuk di-log
    private float lastBatukBerdahakOnsetLog = -1f;   // Track kapan fase BatukBerdahak di-log
    private float lastSesakOnsetLog = -1f;           // Track kapan fase SesakNafas di-log
    private float lastDemamOnsetLog = -1f;           // Track kapan fase Demam di-log
    private float lastCryOnsetLog = -1f;             // Track kapan fase Nangis di-log
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
    [Header("THRESHOLDS")]
    public float feverThreshold = 37.5f;
    public float lowOxygenThreshold = 40f;
    public float criticalHealthThreshold = 20f;

    [Header("Decay Rates")]
    public float timeScale = 1.0f;
    [Tooltip("Pengurangan health per detik saat Common Cold.")]
    public float commonColdHealthDecayRate = 0.1f;
    [Tooltip("Pengurangan health per detik saat Pneumonia.")]
    public float pneumoniaHealthDecayRate = 0.3f;
    [HideInInspector, System.Obsolete("Use commonColdHealthDecayRate and pneumoniaHealthDecayRate.")]
    public float healthDecayRate = 5f;
    public float oxygenDecayRate = 10f;             // Per second while pneumonia is active

    // ============ LEGACY DISEASE DURATIONS ============
    [HideInInspector, System.Obsolete("Common Cold no longer has an auto-cure duration. Use commonColdProgressionDelay for transition timing.")]
    public float commonColdDuration = 30f;
    [HideInInspector, System.Obsolete("Pneumonia no longer has an auto-cure duration.")]
    public float pneumoniaDuration = 60f;

    [Header("Symptom Durations (seconds)")]
    [FormerlySerializedAs("cc_onset_batuk")]
    [Min(0f)]
    [Tooltip("Common Cold: durasi fase Pilek sebelum lanjut ke Batuk.")]
    public float cc_pilek_duration = 10f;
    [FormerlySerializedAs("cc_onset_cry_delay")]
    [Min(0f)]
    [Tooltip("Common Cold: durasi fase Batuk sebelum lanjut ke Nangis.")]
    public float cc_batuk_duration = 10f;
    [Min(0f)]
    [Tooltip("Common Cold: durasi fase Nangis sebelum kembali ke Pilek.")]
    public float cc_cry_duration = 10f;

    [FormerlySerializedAs("pn_onset_batuk_berdahak")]
    [Min(0f)]
    [Tooltip("Pneumonia: durasi fase Pilek sebelum lanjut ke Batuk Berdahak.")]
    public float pn_pilek_duration = 10f;
    [Min(0f)]
    [Tooltip("Pneumonia: durasi fase Batuk Berdahak sebelum lanjut ke Sesak Nafas.")]
    public float pn_batuk_berdahak_duration = 10f;
    [FormerlySerializedAs("pn_onset_demam_delay")]
    [Min(0f)]
    [Tooltip("Pneumonia: durasi fase Sesak Nafas sebelum lanjut ke Demam.")]
    public float pn_sesak_duration = 10f;
    [FormerlySerializedAs("pn_onset_cry_delay")]
    [Min(0f)]
    [Tooltip("Pneumonia: durasi fase Demam sebelum lanjut ke Nangis.")]
    public float pn_demam_duration = 10f;
    [Min(0f)]
    [Tooltip("Pneumonia: durasi fase Nangis sebelum kembali ke Pilek.")]
    public float pn_cry_duration = 10f;

    // ============ RANDOM INFECTION ============
    [Header("Initial Rest Time")]
    [Tooltip("Durasi awal bayi tetap sehat sebelum penyakit biasa boleh muncul otomatis.")]
    public float initialRestDuration = 10f;
    [Tooltip("Jika aktif, bayi otomatis terkena penyakit biasa setelah rest time selesai.")]
    public bool autoInfectCommonColdAfterRest = true;

    [Header("Common Cold Transition")]
    [Range(0f, 100f)]
    [Tooltip("Jika auto infect mati, setelah rest time selesai peluang ini dicek setiap detik untuk masuk Common Cold.")]
    public float commonColdChancePerSecondPercent = 10f;
    [HideInInspector, System.Obsolete("Use commonColdChancePerSecondPercent.")]
    public float diseaseChancePerSecond = 0.001f;
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

        // Update disease first so vital signs use the latest symptom phase.
        UpdateDiseaseProgression();
        UpdateSymptoms();
        UpdateVitalSigns();
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
            // Fever acts as a multiplier (e.g. 1.0x normal, up to 1.5x during high fever) instead of massive flat damage
            float tempMultiplier = temperature >= feverThreshold ? 1.0f + ((temperature - feverThreshold) * 0.5f) : 1.0f;

            health -= timeScale * GetCurrentHealthDecayRate() * tempMultiplier * Time.deltaTime;
        }
        else if (temperature > 38f)
        {
            // Demam tanpa penyakit juga damage health (tapi scaling kecil berdasarkan decay rate user)
            health -= timeScale * (commonColdHealthDecayRate * (temperature - 38f)) * Time.deltaTime;
        }

        health = Mathf.Clamp(health, 0, 100f);
        if (health <= 0f)
        {
            TriggerGameOver();
            return;
        }

        // OXYGEN: Decreases selama Pneumonia aktif, recovers otherwise.
        if (currentDisease == DiseaseState.Pneumonia)
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
                    float roll = Random.Range(0f, 100f);
                    if (roll < commonColdChancePerSecondPercent)
                    {
                        Debug.Log($"<color=yellow>[TRANSITION] Roll {roll:F1}% < {commonColdChancePerSecondPercent:F1}%: bayi terkena Common Cold.</color>");
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
            diseaseElapsedTime += Time.deltaTime * (diseaseTimerUsesTimeScale ? timeScale : 1f);

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

            // Check progression once: CommonCold -> Pneumonia or stay CommonCold.
            if (allowCommonColdProgression &&
                currentDisease == DiseaseState.CommonCold &&
                !commonColdProgressionRollCompleted &&
                diseaseElapsedTime >= commonColdProgressionDelay)
            {
                commonColdProgressionRollCompleted = true;
                float roll = Random.Range(0f, 100f);

                if (roll < pneumoniaProgressionChancePercent)
                {
                    Debug.Log($"<color=red>[PROGRESSION] Roll {roll:F1}% < {pneumoniaProgressionChancePercent:F1}%: Common Cold berkembang menjadi Pneumonia.</color>");
                    ProgressToPneumonia();
                }
                else
                {
                    Debug.Log($"<color=yellow>[PROGRESSION] Roll {roll:F1}% >= {pneumoniaProgressionChancePercent:F1}%: bayi tetap Common Cold sampai durasi selesai.</color>");
                }
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

        // Add exactly one symptom phase based on configured durations.
        // The phase sequence loops until disease changes or player cures it.
        if (currentDisease == DiseaseState.CommonCold)
        {
            float cycleDuration = GetCommonColdCycleDuration();
            if (cycleDuration <= 0f)
            {
                return;
            }

            ResetPhaseLogsIfNewCycle(ref lastCommonColdCycleIndex, cycleDuration);

            float phaseTime = GetLoopedElapsedTime(cycleDuration);
            float pilekDuration = GetDuration(cc_pilek_duration);
            float batukDuration = GetDuration(cc_batuk_duration);
            float cryDuration = GetDuration(cc_cry_duration);
            float batukStart = pilekDuration;
            float cryStart = batukStart + batukDuration;

            // Phase 3: Nangis
            if (phaseTime >= cryStart)
            {
                activeSymptoms.Add(Symptom.Nangis);
                if (lastCryOnsetLog < 0)
                {
                    Debug.Log($"<color=yellow>[SYMPTOM PHASE] Common Cold Phase 3: Nangis selama {cryDuration:F1}s.</color>");
                    lastCryOnsetLog = phaseTime;
                }
            }
            // Phase 2: Batuk
            else if (phaseTime >= batukStart)
            {
                activeSymptoms.Add(Symptom.Batuk);
                if (lastBatukOnsetLog < 0)
                {
                    Debug.Log($"<color=yellow>[SYMPTOM PHASE] Common Cold Phase 2: Batuk selama {batukDuration:F1}s.</color>");
                    lastBatukOnsetLog = phaseTime;
                }
            }
            // Phase 1: Pilek
            else
            {
                activeSymptoms.Add(Symptom.Pilek);
                if (lastPilekOnsetLog < 0)
                {
                    Debug.Log($"<color=yellow>[SYMPTOM PHASE] Common Cold Phase 1: Pilek selama {pilekDuration:F1}s.</color>");
                    lastPilekOnsetLog = phaseTime;
                }
            }
        }
        else if (currentDisease == DiseaseState.Pneumonia)
        {
            float cycleDuration = GetPneumoniaCycleDuration();
            if (cycleDuration <= 0f)
            {
                return;
            }

            ResetPhaseLogsIfNewCycle(ref lastPneumoniaCycleIndex, cycleDuration);

            // Pneumonia phases: eksklusif agar animasi/audio/particle tidak saling tumpuk.
            float phaseTime = GetLoopedElapsedTime(cycleDuration);
            float pilekDuration = GetDuration(pn_pilek_duration);
            float batukBerdahakDuration = GetDuration(pn_batuk_berdahak_duration);
            float sesakDuration = GetDuration(pn_sesak_duration);
            float demamDuration = GetDuration(pn_demam_duration);
            float cryDuration = GetDuration(pn_cry_duration);
            float batukBerdahakStart = pilekDuration;
            float sesakStart = batukBerdahakStart + batukBerdahakDuration;
            float demamStart = sesakStart + sesakDuration;
            float cryStart = demamStart + demamDuration;

            // Phase 5: Nangis
            if (phaseTime >= cryStart)
            {
                activeSymptoms.Add(Symptom.Nangis);
                if (lastCryOnsetLog < 0)
                {
                    Debug.Log($"<color=yellow>[SYMPTOM PHASE] Pneumonia Phase 5: Nangis selama {cryDuration:F1}s.</color>");
                    lastCryOnsetLog = phaseTime;
                }
            }
            // Phase 4: Demam
            else if (phaseTime >= demamStart)
            {
                activeSymptoms.Add(Symptom.Demam);
                if (lastDemamOnsetLog < 0)
                {
                    Debug.Log($"<color=yellow>[SYMPTOM PHASE] Pneumonia Phase 4: Demam selama {demamDuration:F1}s.</color>");
                    lastDemamOnsetLog = phaseTime;
                }
            }
            // Phase 3: Sesak Nafas
            else if (phaseTime >= sesakStart)
            {
                activeSymptoms.Add(Symptom.SesakNafas);
                if (lastSesakOnsetLog < 0)
                {
                    Debug.Log($"<color=yellow>[SYMPTOM PHASE] Pneumonia Phase 3: Sesak Nafas selama {sesakDuration:F1}s.</color>");
                    lastSesakOnsetLog = phaseTime;
                }
            }
            // Phase 2: Batuk Berdahak
            else if (phaseTime >= batukBerdahakStart)
            {
                activeSymptoms.Add(Symptom.BatukBerdahak);
                if (lastBatukBerdahakOnsetLog < 0)
                {
                    Debug.Log($"<color=yellow>[SYMPTOM PHASE] Pneumonia Phase 2: Batuk Berdahak selama {batukBerdahakDuration:F1}s.</color>");
                    lastBatukBerdahakOnsetLog = phaseTime;
                }
            }
            // Phase 1: Pilek
            else
            {
                activeSymptoms.Add(Symptom.Pilek);
                if (lastPilekOnsetLog < 0)
                {
                    Debug.Log($"<color=yellow>[SYMPTOM PHASE] Pneumonia Phase 1: Pilek selama {pilekDuration:F1}s.</color>");
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
        temperature = 37f;

        // Reset phase tracking
        ResetOnsetLogs();
        lastCommonColdCycleIndex = -1;
        lastPneumoniaCycleIndex = -1;
        commonColdProgressionRollCompleted = false;

        Debug.Log($"\n<color=yellow>[INFECTION] Bayi terkena PENYAKIT BIASA</color> | Cek Pneumonia setelah: {commonColdProgressionDelay}s\n");
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
        temperature = 38.5f;

        // Reset phase tracking
        ResetOnsetLogs();
        lastCommonColdCycleIndex = -1;
        lastPneumoniaCycleIndex = -1;
        commonColdProgressionRollCompleted = true;

        Debug.Log($"\n<color=red>[INFECTION] Bayi terkena PNEUMONIA</color>\n");
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
            Debug.Log($"<color=green>[CURED] Penyakit sembuh setelah {diseaseElapsedTime:F1}s!</color>");
            currentDisease = DiseaseState.None;
            diseaseElapsedTime = 0f;
            activeSymptoms.Clear();
            temperature = 36.5f;
            ResetOnsetLogs();
            lastCommonColdCycleIndex = -1;
            lastPneumoniaCycleIndex = -1;
            commonColdProgressionRollCompleted = false;
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
        return GetDuration(cc_pilek_duration) + GetDuration(cc_batuk_duration);
    }

    public float GetCommonColdCycleDuration()
    {
        return GetCommonColdCryOnsetTime() + GetDuration(cc_cry_duration);
    }

    public bool IsCommonColdCryPhase()
    {
        float cycleDuration = GetCommonColdCycleDuration();
        return currentDisease == DiseaseState.CommonCold &&
               cycleDuration > 0f &&
               GetLoopedElapsedTime(cycleDuration) >= GetCommonColdCryOnsetTime();
    }

    public float GetPneumoniaDemamOnsetTime()
    {
        return GetDuration(pn_pilek_duration) +
               GetDuration(pn_batuk_berdahak_duration) +
               GetDuration(pn_sesak_duration);
    }

    public float GetPneumoniaCryOnsetTime()
    {
        return GetPneumoniaDemamOnsetTime() + GetDuration(pn_demam_duration);
    }

    public bool IsPneumoniaCryPhase()
    {
        float cycleDuration = GetPneumoniaCycleDuration();
        return currentDisease == DiseaseState.Pneumonia &&
               cycleDuration > 0f &&
               GetLoopedElapsedTime(cycleDuration) >= GetPneumoniaCryOnsetTime();
    }

    public bool IsPneumoniaSesakPhase()
    {
        if (currentDisease != DiseaseState.Pneumonia)
        {
            return false;
        }

        float phaseTime = GetLoopedElapsedTime(GetPneumoniaCycleDuration());
        float sesakStart = GetDuration(pn_pilek_duration) + GetDuration(pn_batuk_berdahak_duration);
        return phaseTime >= sesakStart && phaseTime < GetPneumoniaDemamOnsetTime();
    }

    public float GetPneumoniaCycleDuration()
    {
        return GetPneumoniaCryOnsetTime() + GetDuration(pn_cry_duration);
    }

    private float GetDuration(float duration)
    {
        return Mathf.Max(0f, duration);
    }

    private float GetCurrentHealthDecayRate()
    {
        if (currentDisease == DiseaseState.CommonCold)
        {
            return Mathf.Max(0f, commonColdHealthDecayRate);
        }

        if (currentDisease == DiseaseState.Pneumonia)
        {
            return Mathf.Max(0f, pneumoniaHealthDecayRate);
        }

        return 0f;
    }

    private void TriggerGameOver()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("[BabyBehavior] Health mencapai 0, tapi GameManager.Instance tidak ditemukan!");
            return;
        }

        if (GameManager.Instance.currentState != GameManager.GameState.GameOver)
        {
            Debug.Log("<color=red>[STATUS] Health mencapai 0. Game Over.</color>");
            GameManager.Instance.GameOver();
        }
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
