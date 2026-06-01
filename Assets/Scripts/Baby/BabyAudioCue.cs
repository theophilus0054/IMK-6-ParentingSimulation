using UnityEngine;

/// <summary>
/// Audio system - Play audio berdasarkan symptoms dari BabyBehavior
/// UPDATED: Audio mengikuti fase gejala eksklusif dari BabyBehavior
/// - Whimpering: Demam
/// - Cough: Batuk / Batuk Berdahak
/// - Wheeze: Sesak Nafas
/// - Cry: fase nangis pneumonia atau health rendah di luar pneumonia
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class BabyAudioCue : MonoBehaviour
{
    private AudioSource audioSource;
    private BabyBehavior babyBehavior;

    [Header("Audio Clips")]
    public AudioClip normalCryClip;      // Critical state crying
    public AudioClip whimperingClip;     // Demam whimpering
    public AudioClip coughClip;          // Batuk & Batuk Berdahak
    public AudioClip wheezingClip;       // Sesak Nafas

    [Header("Audio Parameters")]
    [Range(0f, 1f)] public float audioVolume = 0.7f;
    [Range(0.5f, 2f)] public float pitchVariation = 0.1f;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        babyBehavior = GetComponent<BabyBehavior>();
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.currentState != GameManager.GameState.Playing)
            return;

        UpdateAudio();
    }

    private void UpdateAudio()
    {
        if (babyBehavior == null || audioSource == null) return;

        AudioClip expectedClip = null;
        string expectedAudioType = "";

        // Tentukan audio clip yang seharusnya dimainkan berdasarkan fase gejala.
        // Prioritas mengikuti logika BabyAnimator (Crying -> Demam -> Sesak -> Batuk)
        if (babyBehavior.ShouldPlayCryAnimation())
        {
            expectedClip = normalCryClip;
            expectedAudioType = "CRY";
        }
        else if (babyBehavior.HasSymptom(BabyBehavior.Symptom.Demam))
        {
            expectedClip = whimperingClip;
            expectedAudioType = "WHIMPER";
        }
        else if (babyBehavior.HasSymptom(BabyBehavior.Symptom.SesakNafas))
        {
            expectedClip = wheezingClip;
            expectedAudioType = "WHEEZE";
        }
        else if (babyBehavior.HasSymptom(BabyBehavior.Symptom.Batuk) || babyBehavior.HasSymptom(BabyBehavior.Symptom.BatukBerdahak))
        {
            expectedClip = coughClip;
            expectedAudioType = "COUGH";
        }

        // Handle Audio Playback and Stopping
        if (expectedClip != null)
        {
            // Jika klip yang dimainkan sekarang berbeda dengan klip yang seharusnya, GANTI!
            if (audioSource.clip != expectedClip)
            {
                audioSource.Stop();
                audioSource.clip = expectedClip;
                audioSource.volume = audioVolume;
                audioSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
                audioSource.Play();
                Debug.Log($"<color=magenta>[AUDIO {expectedAudioType}] Started {expectedClip.name}</color>");
            }
            // Jika klip yang sama sudah selesai dimainkan, putar ulang (loop manual dengan pitch bervariasi)
            else if (!audioSource.isPlaying)
            {
                audioSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
                audioSource.Play();
            }
        }
        else
        {
            // Jika tidak ada gejala aktif / animasi tidak relevan, PASTIKAN audio berhenti
            if (audioSource.isPlaying || audioSource.clip != null)
            {
                audioSource.Stop();
                audioSource.clip = null;
                Debug.Log($"<color=magenta>[AUDIO] Stopped explicitly.</color>");
            }
        }
    }
}
