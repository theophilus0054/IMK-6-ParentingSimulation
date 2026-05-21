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
    public ParticleSystem greenYellowParticle;

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
        animator.Play(stateName);
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

        if (babyBehavior.HasSymptom(BabyBehavior.Symptom.SesakNafas))
        {
            animState = fastBreathState;
        }
        else if (babyBehavior.HasSymptom(BabyBehavior.Symptom.Demam))
        {
            animState = rewelState;
        }
        else if (babyBehavior.HasSymptom(BabyBehavior.Symptom.BatukBerdahak) || 
                 babyBehavior.HasSymptom(BabyBehavior.Symptom.Batuk))
        {
            animState = coughState;
        }
        else if (babyBehavior.health < babyBehavior.criticalHealthThreshold)
        {
            animState = cryingState;
        }

        PlayAnimationState(animState);

        // Map symptoms ke effects
        bool hasPilek = babyBehavior.HasSymptom(BabyBehavior.Symptom.Pilek);
        bool hasBatukBerdahak = babyBehavior.HasSymptom(BabyBehavior.Symptom.BatukBerdahak);
        bool hasSesak = babyBehavior.HasSymptom(BabyBehavior.Symptom.SesakNafas);
        bool hasDemam = babyBehavior.HasSymptom(BabyBehavior.Symptom.Demam);

        // Pilek/Batuk Berdahak → Green/Yellow Particle
        if (hasPilek || hasBatukBerdahak)
        {
            if (greenYellowParticle != null && !greenYellowParticle.isPlaying)
            {
                greenYellowParticle.Play();
            }
            SetEffectActive(greenYellowParticle?.gameObject, true);
        }
        else if (greenYellowParticle != null && greenYellowParticle.isPlaying)
        {
            greenYellowParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            SetEffectActive(greenYellowParticle.gameObject, false);
        }

        // Sesak Nafas → Oximeter ON
        SetEffectActive(oximeterObject, hasSesak);

        // Demam → Thermometer ON
        SetEffectActive(thermometerObject, hasDemam);
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