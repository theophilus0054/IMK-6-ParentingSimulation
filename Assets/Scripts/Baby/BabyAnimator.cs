using UnityEngine;

[RequireComponent(typeof(Animator))]
public class BabyAnimator : MonoBehaviour
{
    private Animator animator;
    
    [Header("Visual Effects")]
    public ParticleSystem cryingTearsVFX; // Partikel Air Mata (VFX-001) [cite: 189]
    public ParticleSystem feverSweatVFX; 

    [Header("Facial Expressions (SkinnedMeshRenderer)")]
    public SkinnedMeshRenderer babyFaceRenderer;
    // Index blendshape biasanya didapat dari Blender (contoh: 0 = Senyum, 1 = Sedih/Menangis) [cite: 188]
    public int cryingBlendshapeIndex = 1; 

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void UpdateAnimatorState(BabyBehavior.BabyState state)
    {
        // Reset semua trigger animasi terlebih dahulu
        animator.SetBool("isCrying", false);
        animator.SetBool("isFever", false);
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

    private void SetFaceExpression(float weight)
    {
        if (babyFaceRenderer != null && babyFaceRenderer.sharedMesh.blendShapeCount > 0)
        {
            // Mengatur nilai blendshape wajah (0 - 100)
            babyFaceRenderer.SetBlendShapeWeight(cryingBlendshapeIndex, weight);
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
}