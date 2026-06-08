using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;

/// <summary>
/// Script untuk Oximeter.
/// Mengambil nilai oxygen dari BabyBehavior ketika dipasang (di-attach) ke XRSocketInteractor di tubuh bayi.
/// </summary>
[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class Oximeter : MonoBehaviour
{
    [Header("UI Reference")]
    [Tooltip("Text UI (TextMeshPro) yang ada pada layar Oximeter. Tarik object child text Anda ke kolom ini melalui Inspector.")]
    public TMP_Text textDisplay;

    [Header("Target & Configuration")]
    [Tooltip("Referensi ke script BabyBehavior. Akan dicari otomatis jika dibiarkan kosong.")]
    public BabyBehavior babyBehavior;
    
    [Tooltip("Tag dari XRSocketInteractor tempat oximeter ini dipasang (misal socket di jari bayi). Kosongkan jika socket apa saja boleh.")]
    public string targetSocketTag = "OximeterSocket";

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private bool isAttachedToTargetSocket = false;

    private void Awake()
    {
        // Mengambil referensi komponen XRGrabInteractable dari object ini
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grabInteractable != null)
        {
            // Mendaftarkan event ketika objek masuk (Select Entered) dan keluar (Select Exited) dari interactor
            grabInteractable.selectEntered.AddListener(OnSelectEntered);
            grabInteractable.selectExited.AddListener(OnSelectExited);
        }

        // Mencari BabyBehavior di scene jika belum di-assign melalui inspector
        if (babyBehavior == null)
        {
            babyBehavior = FindObjectOfType<BabyBehavior>();
        }

        // Set tampilan layar default ketika oximeter belum terpasang
        if (textDisplay != null)
        {
            textDisplay.text = "-- %";
            textDisplay.color = Color.white;
        }
    }

    private void OnDestroy()
    {
        // Membersihkan listener untuk mencegah memory leak
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnSelectEntered);
            grabInteractable.selectExited.RemoveListener(OnSelectExited);
        }
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        // Cek apakah interactor yang 'memegang' oximeter ini adalah sebuah XRSocketInteractor (bukan tangan player)
        if (args.interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor socket)
        {
            // Memastikan socket tersebut memiliki tag yang tepat (misalnya dipasang di jari, bukan di meja)
            if (string.IsNullOrEmpty(targetSocketTag) || socket.transform.CompareTag(targetSocketTag))
            {
                isAttachedToTargetSocket = true;
                Debug.Log("[Oximeter] Berhasil dipasang ke socket target!");
            }
        }
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        // Jika oximeter dilepas dari socket, matikan bacaan layarnya
        if (args.interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor)
        {
            isAttachedToTargetSocket = false;
            
            if (textDisplay != null)
            {
                textDisplay.text = "-- %";
                textDisplay.color = Color.white;
            }
            Debug.Log("[Oximeter] Dilepas dari socket.");
        }
    }

    private void Update()
    {
        // Update angka di layar secara real-time HANYA jika sedang terpasang di socket bayi
        if (isAttachedToTargetSocket && babyBehavior != null && textDisplay != null)
        {
            // Ambil nilai oksigen
            int currentOxygen = Mathf.RoundToInt(babyBehavior.oxygenLevel);
            
            // Tampilkan ke teks
            textDisplay.text = currentOxygen.ToString() + " %";

            // Opsional: Ganti warna indikator layar jika oksigen drop/kritis
            if (currentOxygen <= babyBehavior.lowOxygenThreshold)
            {
                textDisplay.color = Color.red; // Bahaya
            }
            else
            {
                textDisplay.color = Color.green; // Aman
            }
        }
    }
}
