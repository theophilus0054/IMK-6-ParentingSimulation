using UnityEngine;

[RequireComponent(typeof(Animator))]
public class BabyAnimator : MonoBehaviour
{
    private Animator animator;
    private BabyDisease babyDisease;
    private BabyBehavior babyBehavior;
    
    [Header("Visual Effects")]
    public ParticleSystem cryingTearsVFX; // Partikel Air Mata 
    public ParticleSystem feverSweatVFX; 

    [Header("Facial Expressions (SkinnedMeshRenderer)")]
    public SkinnedMeshRenderer babyFaceRenderer;
    // Index blendshape biasanya didapat dari Blender (contoh: 0 = Senyum, 1 = Sedih/Menangis)
    public int cryingBlendshapeIndex = 1;
    public int puledBlendshapeIndex = 2; // Untuk ekspresi pucat/sakit
    public int wheezingBlendshapeIndex = 3; // Untuk ekspresi sesak nafas

    [Header("Disease Visual Effects")]
    public SkinnedMeshRenderer bodyRenderer; // Untuk perubahan warna kulit (paleness)
    public SkinnedMeshRenderer chestRenderer; // Untuk chest indentation
    public int chestCavityBlendshapeIndex = 0; // Dada cekung
    
    [Header("Material Colors")]
    public Material babyNormalMaterial; // Material kulit normal
    private Material babyPaleMaterial; // Material kulit pucat (akan di-instantiate)
    public Color paleSkinColor = new Color(0.95f, 0.93f, 0.93f, 1f); // Warna pucat 

    private void Awake()
    {
        animator = GetComponent<Animator>();
        babyDisease = GetComponent<BabyDisease>();
        babyBehavior = GetComponent<BabyBehavior>();

        // Create a copy of the material for color modification
        if (babyNormalMaterial != null)
        {
            babyPaleMaterial = new Material(babyNormalMaterial);
            babyPaleMaterial.color = paleSkinColor;
        }
    }

    public void UpdateAnimatorState(BabyBehavior.BabyState state)
    {
        // Reset semua trigger animasi terlebih dahulu
        animator.SetBool("isCrying", false);
        animator.SetBool("isFever", false);
        animator.SetBool("isWheeling", false); // Sesak nafas
        SetFaceExpression(0f); // Reset wajah ke normal

        switch (state)
        {
            case BabyBehavior.BabyState.Normal:
                animator.Play("Idle_Sleep"); // Asumsi nama klip animasi Anda
                StopVFX(cryingTearsVFX);
                StopVFX(feverSweatVFX);
                break;

            case BabyBehavior.BabyState.Lapar:
            case BabyBehavior.BabyState.TidakNyaman:
                animator.Play("Restless_Moving");
                break;

            case BabyBehavior.BabyState.Demam:
                animator.SetBool("isFever", true);
                PlayVFX(feverSweatVFX);
                break;

            case BabyBehavior.BabyState.Crying:
                animator.SetBool("isCrying", true);
                SetFaceExpression(100f); // Set blendshape sedih/menangis ke maksimal
                PlayVFX(cryingTearsVFX);
                break;
        }

    }

    private void UpdateDiseaseVisuals()
    {
        if (babyDisease == null || babyDisease.currentDisease.type == BabyDisease.DiseaseType.None)
        {
            // Reset to normal
            if (bodyRenderer != null && babyNormalMaterial != null)
            {
                bodyRenderer.material = babyNormalMaterial;
            }
            animator.SetBool("isWheeling", false); // Reset wheeling state
            ResetChestIndentation();
            ResetWheezingExpression();
            return;
        }

        float severity = babyDisease.GetSeverity();

        // PUCAT - Perubahan warna kulit
        if (babyDisease.HasSymptom(BabyDisease.Symptom.Pucat) && bodyRenderer != null)
        {
            if (babyPaleMaterial != null)
            {
                bodyRenderer.material = babyPaleMaterial;
            }
        }

        // SESAK NAFAS - Dada cekung + ekspresi wajah
        if (babyDisease.HasSymptom(BabyDisease.Symptom.SesakNafas))
        {
            animator.SetBool("isWheeling", true);
            SetChestIndentation(Mathf.Clamp(severity * 0.5f, 0f, 100f));
            SetWheezingExpression(Mathf.Clamp(severity * 0.3f, 0f, 100f));
        }
        else
        {
            animator.SetBool("isWheeling", false);
        }

        // FACIAL EXPRESSIONS untuk penyakit (pucat/sakit)
        if (severity > 50f && babyDisease.HasSymptom(BabyDisease.Symptom.Demam))
        {
            SetPuledExpression(Mathf.Clamp(severity * 0.5f, 0f, 100f));
        }
    }

    private void SetFaceExpression(float weight)
    {
        if (babyFaceRenderer != null && babyFaceRenderer.sharedMesh.blendShapeCount > 0)
        {
            // Mengatur nilai blendshape wajah (0 - 100)
            babyFaceRenderer.SetBlendShapeWeight(cryingBlendshapeIndex, weight);
        }
    }

    private void SetPuledExpression(float weight)
    {
        if (babyFaceRenderer != null && babyFaceRenderer.sharedMesh.blendShapeCount > puledBlendshapeIndex)
        {
            babyFaceRenderer.SetBlendShapeWeight(puledBlendshapeIndex, weight);
        }
    }

    private void SetWheezingExpression(float weight)
    {
        if (babyFaceRenderer != null && babyFaceRenderer.sharedMesh.blendShapeCount > wheezingBlendshapeIndex)
        {
            babyFaceRenderer.SetBlendShapeWeight(wheezingBlendshapeIndex, weight);
        }
    }

    private void ResetWheezingExpression()
    {
        if (babyFaceRenderer != null && babyFaceRenderer.sharedMesh.blendShapeCount > wheezingBlendshapeIndex)
        {
            babyFaceRenderer.SetBlendShapeWeight(wheezingBlendshapeIndex, 0f);
        }
    }

    private void SetChestIndentation(float weight)
    {
        if (chestRenderer != null && chestRenderer.sharedMesh.blendShapeCount > 0)
        {
            chestRenderer.SetBlendShapeWeight(chestCavityBlendshapeIndex, weight);
        }
    }

    private void ResetChestIndentation()
    {
        if (chestRenderer != null && chestRenderer.sharedMesh.blendShapeCount > 0)
        {
            chestRenderer.SetBlendShapeWeight(chestCavityBlendshapeIndex, 0f);
        }
    }

    private void PlayVFX(ParticleSystem vfx)
    {
        if (vfx != null && !vfx.isPlaying)
        {
            vfx.Play();
        }
    }

    private void StopVFX(ParticleSystem vfx)
    {
        if (vfx != null && vfx.isPlaying)
        {
            vfx.Stop();
        }
    }

    private void Update()
    {
        // Update disease visuals setiap frame
        UpdateDiseaseVisuals();
    }
}