using UnityEngine;

/// <summary>
/// Audio cue system untuk berbagai gejala dan keadaan bayi
/// Menangani: tangisan, batuk, pilek, sesak nafas, dll
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class BabyAudioCue : MonoBehaviour
{
    private AudioSource audioSource;
    private BabyDisease babyDisease;

    [Header("Crying Audio")]
    public AudioClip normalCryClip;
    public AudioClip hungryCryClip;
    public AudioClip uncomfortableCryClip;

    [Header("Disease Audio")]
    public AudioClip sneezeClip; // Pilek/bersin
    public AudioClip coughClip; // Batuk biasa
    public AudioClip coughWithPhlegmClip; // Batuk berdahak
    public AudioClip wheezingClip; // Sesak nafas/suara terengah-engah

    [Header("Audio Parameters")]
    [Range(0f, 1f)] public float cryVolume = 0.8f;
    [Range(0f, 1f)] public float diseaseAudioVolume = 0.6f;
    [Range(0.5f, 2f)] public float pitchVariation = 0.1f;

    [Header("Timing")]
    public float coughInterval = 5f; // Batuk setiap 5 detik
    public float sneezeInterval = 8f; // Bersin setiap 8 detik
    public float wheezingInterval = 3f; // Sesak nafas setiap 3 detik

    private float lastCoughTime = 0f;
    private float lastSneezeTime = 0f;
    private float lastWheezeTime = 0f;
    private float lastCryTime = 0f;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        babyDisease = GetComponent<BabyDisease>();
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.currentState != GameManager.GameState.Playing) return;

        UpdateDiseaseAudio();
    }

    public void PlayCry(BabyBehavior.BabyState state)
    {
        if (Time.time - lastCryTime < 1f) return; // Cooldown 1 detik
        lastCryTime = Time.time;

        if (audioSource == null)
        {
            Debug.LogError("[BabyAudioCue] AudioSource tidak ditemukan!");
            return;
        }

        AudioClip clipToPlay = normalCryClip;

        switch (state)
        {
            case BabyBehavior.BabyState.Lapar:
                clipToPlay = hungryCryClip;
                break;
            case BabyBehavior.BabyState.TidakNyaman:
                clipToPlay = uncomfortableCryClip;
                break;
        }

        if (clipToPlay != null)
        {
            audioSource.volume = cryVolume;
            audioSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
            audioSource.PlayOneShot(clipToPlay);
        }
        else
        {
            Debug.LogWarning($"[BabyAudioCue] Cry clip untuk state {state} kosong!");
        }
    }

    private void UpdateDiseaseAudio()
    {
        if (babyDisease.currentDisease.type == BabyDisease.DiseaseType.None) return;

        // Pilek - bersin
        if (babyDisease.HasSymptom(BabyDisease.Symptom.Pilek))
        {
            if (Time.time - lastSneezeTime >= sneezeInterval)
            {
                PlayDiseaseAudio(sneezeClip);
                lastSneezeTime = Time.time;
            }
        }

        // Batuk biasa
        if (babyDisease.HasSymptom(BabyDisease.Symptom.Batuk) && 
            !babyDisease.HasSymptom(BabyDisease.Symptom.BatukBerdahak))
        {
            if (Time.time - lastCoughTime >= coughInterval)
            {
                PlayDiseaseAudio(coughClip);
                lastCoughTime = Time.time;
            }
        }

        // Batuk berdahak (lebih sering dan berat)
        if (babyDisease.HasSymptom(BabyDisease.Symptom.BatukBerdahak))
        {
            if (Time.time - lastCoughTime >= coughInterval * 0.5f) // 2x lebih sering
            {
                PlayDiseaseAudio(coughWithPhlegmClip);
                lastCoughTime = Time.time;
            }
        }

        // Sesak nafas
        if (babyDisease.HasSymptom(BabyDisease.Symptom.SesakNafas))
        {
            if (Time.time - lastWheezeTime >= wheezingInterval)
            {
                PlayDiseaseAudio(wheezingClip);
                lastWheezeTime = Time.time;
            }
        }
    }

    private void PlayDiseaseAudio(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.volume = diseaseAudioVolume;
            audioSource.pitch = 1f + Random.Range(-pitchVariation * 0.5f, pitchVariation * 0.5f);
            audioSource.PlayOneShot(clip);
        }
        else if (clip == null)
        {
            Debug.LogWarning("[BabyAudioCue] Audio clip kosong!");
        }
    }
}
