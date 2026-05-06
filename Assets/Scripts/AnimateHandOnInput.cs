using UnityEngine;
using UnityEngine.InputSystem;

public class AnimateHandOnInput : MonoBehaviour
{
    public InputActionProperty triggerValue; // Input Action untuk trigger gesture
    public InputActionProperty gripValue;  // Input Action untuk grip gesture

    public Animator handAnimator; // Animator untuk mengontrol animasi tangan

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float trigger = triggerValue.action.ReadValue<float>(); // Membaca nilai trigger
        float grip = gripValue.action.ReadValue<float>(); // Membaca nilai grip

        handAnimator.SetFloat("Trigger", trigger); // Mengatur parameter Trigger di Animator
        handAnimator.SetFloat("Grip", grip); // Mengatur parameter Grip di Animator
    }
}
