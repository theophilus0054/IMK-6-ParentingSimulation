using UnityEngine;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit; // Wajib untuk XR Interaction Events

public class TempDetector : MonoBehaviour
{
    public TemperatureDisplay tempDisplay;
    
    // Coroutine untuk mengatur durasi tampilan UI
    private Coroutine activeTimer;

    /// <summary>
    /// Fungsi ini dipanggil melalui Event 'Activated' pada XR Ray Interactor
    /// </summary>
    public void ScanTemperature(ActivateEventArgs args)
    {
        // Mengambil referensi object yang sedang ditunjuk oleh Raycast
        // Menggunakan 'interactableObject' langsung untuk menghindari error IXRInteractable
        var hitObject = args.interactableObject;

        // Pastikan object tidak null dan memiliki tag "Baby"
        if (hitObject != null && hitObject.transform.CompareTag("Baby"))
        {
            // Hentikan timer lama jika sedang berjalan agar tidak tumpang tindih
            if (activeTimer != null) StopCoroutine(activeTimer);

            // Munculkan UI
            tempDisplay.activateTemperatureText(true);
            
            // Logika random suhu untuk prototype
            // Sesuai suhu tubuh normal manusia (36.5 - 37.5)
            float randomTemp = Random.Range(36.5f, 37.5f); 
            tempDisplay.UpdateTemperature(randomTemp);

            // Jalankan timer untuk menyembunyikan UI setelah 7 detik
            activeTimer = StartCoroutine(DeactivateTemperatureTextAfterDelay(7.0f));
            
            Debug.Log("Suhu " + hitObject.transform.name + " berhasil terdeteksi: " + randomTemp);
        }
        else
        {
            Debug.Log("Raycast mengenai: " + (hitObject != null ? hitObject.transform.name : "Nothing") + ". Bukan target Baby.");
        }
    }

    private IEnumerator DeactivateTemperatureTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        tempDisplay.deactivateTemperatureText(true);
        activeTimer = null;
    }
}