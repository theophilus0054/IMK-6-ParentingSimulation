using UnityEngine;

[RequireComponent(typeof(Animator))]
public class BabyAnimator : MonoBehaviour
{
    private Animator animator;
    private BabyBehavior babyBehavior;
    private string currentAnimationState;

    private BabyBehavior.DiseaseState lastLoggedDisease = BabyBehavior.DiseaseState.None;

    [Header("Effect Objects")]
    public GameObject thermometerObject;
    public GameObject oximeterObject;

    [Header("Particle Systems")]
    public ParticleSystem pilekParticle;              // Untuk Pilek (ringan, hidung berair)
    public ParticleSystem coughSputumParticle;        // Untuk Batuk Berdahak (intensif, dahak hijau/kuning)

    [Header("Animator State Names")]
    [SerializeField] private string layBreathState = "Lay breath";
    [SerializeField] private string rewelState = "Rewel";
    [SerializeField] private string coughState = "Cough";
    [SerializeField] private string fastBreathState = "Fast breath";
    [SerializeField] private string cryingState = "Crying";

    private void Awake()
    {
        animator = GetComponent<Animator>();
        babyBehavior = GetComponent<BabyBehavior>();
    }

    public void PlayAnimationState(string stateName)
    {
        if (animator == null)
        {
            Debug.LogError("[ANIMATOR] Animator null!");
            return;
        }

        if (string.IsNullOrWhiteSpace(stateName))
        {
            Debug.LogError("[ANIMATOR] State name invalid!");
            return;
        }

        if (currentAnimationState == stateName)
        {
            return; // Sama dengan sebelumnya, jangan repeat
        }

        Debug.Log($"<color=cyan>[ANIM] Playing: <b>{stateName}</b></color>");
        animator.CrossFade(stateName, 0.25f, 0);
        currentAnimationState = stateName;
    }

    private void UpdateVisuals()
    {
        if (babyBehavior == null) return;

        // Log disease change
        if (babyBehavior.currentDisease != lastLoggedDisease)
        {
            lastLoggedDisease = babyBehavior.currentDisease;
            string symptomsStr = string.Join(", ", babyBehavior.activeSymptoms);
            Debug.Log($"[VISUALS] Disease: {babyBehavior.currentDisease} | Symptoms: {symptomsStr}");
        }

        // Map symptoms ke animation
        string animState = layBreathState; // Default

        // Prioritas tertinggi: Crying -> Demam (Rewel) -> Sesak (Fast Breath) -> Batuk
        if (babyBehavior.ShouldPlayCryAnimation())
        {
            animState = cryingState;
        }
        else if (babyBehavior.HasSymptom(BabyBehavior.Symptom.Demam))
        {
            animState = rewelState;
        }
        else if (babyBehavior.HasSymptom(BabyBehavior.Symptom.SesakNafas))
        {
            animState = fastBreathState;
        }
        else if (babyBehavior.HasSymptom(BabyBehavior.Symptom.BatukBerdahak) ||
                 babyBehavior.HasSymptom(BabyBehavior.Symptom.Batuk))
        {
            animState = coughState;
        }

        PlayAnimationState(animState);

        // Map symptoms ke effects
        bool hasPilek = babyBehavior.HasSymptom(BabyBehavior.Symptom.Pilek);
        bool hasBatukBerdahak = babyBehavior.HasSymptom(BabyBehavior.Symptom.BatukBerdahak);
        bool hasSesak = babyBehavior.HasSymptom(BabyBehavior.Symptom.SesakNafas);
        bool hasDemam = babyBehavior.HasSymptom(BabyBehavior.Symptom.Demam);

        // ========== PILEK PARTICLE ==========
        // Pilek → Hanya saat "Lay breath" (animasi normal)
        // Matikan pilek saat animasi lain (Cough, Rewel, Fast breath, Crying)
        bool isPilekAllowed = (currentAnimationState == layBreathState);

        if (hasPilek && isPilekAllowed && !hasBatukBerdahak)
        {
            SetEffectActive(pilekParticle?.gameObject, true);
            if (pilekParticle != null && !pilekParticle.isPlaying)
            {
                pilekParticle.Play();
                Debug.Log("[PARTICLE] Pilek ON (Lay breath state)");
            }
        }
        else if (pilekParticle != null && (pilekParticle.isPlaying || pilekParticle.gameObject.activeSelf))
        {
            pilekParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            SetEffectActive(pilekParticle.gameObject, false);
        }

        // ========== BATUK BERDAHAK PARTICLE ==========
        // Batuk Berdahak → Intensif, cepat, dari mulut (dahak tebal)
        bool isCoughAllowed = (currentAnimationState == coughState);

        if (hasBatukBerdahak && isCoughAllowed)
        {
            SetEffectActive(coughSputumParticle?.gameObject, true);
            if (coughSputumParticle != null && !coughSputumParticle.isPlaying)
            {
                coughSputumParticle.Play();
            }
        }
        else if (coughSputumParticle != null && (coughSputumParticle.isPlaying || coughSputumParticle.gameObject.activeSelf))
        {
            coughSputumParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            SetEffectActive(coughSputumParticle.gameObject, false);
        }

        // // Sesak Nafas → Oximeter ON
        // bool isSesakAllowed = (currentAnimationState == fastBreathState);
        // SetEffectActive(oximeterObject, hasSesak && isSesakAllowed);

        // // Demam → Thermometer ON
        // bool isDemamAllowed = (currentAnimationState == rewelState);
        // SetEffectActive(thermometerObject, hasDemam && isDemamAllowed);
    }

    private void SetEffectActive(GameObject target, bool isActive)
    {
        if (target == null) return;

        if (target.activeSelf != isActive)
        {
            target.SetActive(isActive);
            Debug.Log($"[EFFECT] {target.name}: {(isActive ? "<color=green>ON</color>" : "<color=red>OFF</color>")}");
        }
    }

    private void Update()
    {
        UpdateVisuals();
    }
}
