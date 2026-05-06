using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Raycast : MonoBehaviour
{
    // public InputActionProperty triggerValue; // Input Action untuk trigger gesture

    // // Update is called once per frame
    // void Update()
    // {
    //     if (triggerValue.action.WasPressedThisFrame())
    //     {
    //         Ray ray = new Ray(transform.position, transform.forward);
    //         RaycastHit hit;

    //         if (Physics.Raycast(ray, out hit))
    //         {
    //             Debug.Log("Raycast mengenai: " + hit.transform.name);
    //         }
    //         else
    //         {
    //             Debug.Log("Raycast tidak mengenai apa pun.");
    //         }
    //     }
    // }
    [SerializeField] LayerMask layerMask; // Layer mask untuk menentukan layer yang akan dipertimbangkan dalam raycast

    void Update()
    {
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out RaycastHit hit, 20f, layerMask))
        {
            Debug.Log("Raycast mengenai: " + hit.transform.name);
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 20f, Color.red);
        }
         else
        {
            Debug.Log("Raycast tidak mengenai apa pun.");
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 20f, Color.green);
        }
    }
}
