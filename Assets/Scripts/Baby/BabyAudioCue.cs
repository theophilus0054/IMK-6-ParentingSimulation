using UnityEngine;

/// <summary>
/// Audio system - Play audio berdasarkan symptoms dari BabyBehavior
/// - Whimpering: Demam
/// - Cough: Batuk / Batuk Berdahak
/// - Wheeze: Sesak Nafas
/// - Cry: Health rendah (critical)
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

    private float lastCryTime = 0f;
    private float lastWhimperTime = 0f;
    private float lastCoughTime = 0f;
    private float lastWheezeTime = 0f;

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
        if (babyBehavior == null) return;

        // Demam → Whimpering
        if (babyBehavior.HasSymptom(BabyBehavior.Symptom.Demam))
        {
            PlayWhimper();
        }

        // Batuk/BatukBerdahak → Coughing
        if (babyBehavior.HasSymptom(BabyBehavior.Symptom.Batuk))
        {
            PlayCough("Batuk");
        }
        else if (babyBehavior.HasSymptom(BabyBehavior.Symptom.BatukBerdahak))
        {
            PlayCough("BatukBerdahak");
        }

        // SesakNafas → Wheezing
        if (babyBehavior.HasSymptom(BabyBehavior.Symptom.SesakNafas))
        {
            PlayWheeze();
        }

        // Critical health → Crying
        if (babyBehavior.health < babyBehavior.criticalHealthThreshold)
        {
            PlayCry();
        }
    }

    private void PlayCry()
    {
        if (normalCryClip == null)
        {
            Debug.LogWarning("[AUDIO] Cry clip kosong!");
            return;
        }

        float clipLen = Mathf.Max(0.1f, normalCryClip.length);
        if (Time.time - lastCryTime < clipLen) return; // wait until previous clip finished
        lastCryTime = Time.time;
        PlayAudio(normalCryClip, "CRY");
    }

    private void PlayWhimper()
    {
        if (whimperingClip == null)
        {
            Debug.LogWarning("[AUDIO] Whimpering clip kosong!");
            return;
        }

        float clipLen = Mathf.Max(0.1f, whimperingClip.length);
        if (Time.time - lastWhimperTime < clipLen) return;
        lastWhimperTime = Time.time;
        PlayAudio(whimperingClip, "WHIMPER");
    }

    private void PlayCough(string coughType)
    {
        if (coughClip == null)
        {
            Debug.LogWarning("[AUDIO] Cough clip kosong!");
            return;
        }

        // Use clip length as cooldown to avoid overlap
        float clipLen = Mathf.Max(0.1f, coughClip.length);
        if (Time.time - lastCoughTime < clipLen) return;
        lastCoughTime = Time.time;
        PlayAudio(coughClip, $"COUGH ({coughType})");
    }

    private void PlayWheeze()
    {
        if (wheezingClip == null)
        {
            Debug.LogWarning("[AUDIO] Wheeze clip kosong!");
            return;
        }

        float clipLen = Mathf.Max(0.1f, wheezingClip.length);
        if (Time.time - lastWheezeTime < clipLen) return;
        lastWheezeTime = Time.time;
        PlayAudio(wheezingClip, "WHEEZE");
    }

    private void PlayAudio(AudioClip clip, string audioType = "Audio")
    {
        if (audioSource == null)
        {
            Debug.LogError("[AUDIO] AudioSource null!");
            return;
        }

        audioSource.volume = audioVolume;
        audioSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        audioSource.PlayOneShot(clip);
        Debug.Log($"<color=magenta>[AUDIO {audioType}] {clip.name} ({clip.length:F1}s)</color>");
    }
}
