using UnityEngine;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit; // Wajib untuk XR Interaction Events

public class TempDetector : MonoBehaviour
{
    public TemperatureDisplay tempDisplay;
    public BabyBehavior babyBehavior;
    
    // Coroutine untuk mengatur durasi tampilan UI
    private Coroutine activeTimer;

    /// <summary>
    /// Fungsi ini dipanggil melalui Event 'Activated' pada XR Ray Interactor
    /// </summary>
    public void ScanTemperature(ActivateEventArgs args)
    {
        GameObject hitObject = null;

        // Cek apakah interactor adalah Ray Interactor (tangan yang menembakkan laser)
        if (args.interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor rayInteractor)
        {
            if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
            {
                hitObject = hit.collider.gameObject;
                Debug.DrawLine(rayInteractor.transform.position, hit.point, Color.green, 2f);
            }
        }
        else
        {
            // Jika thermometer dipegang langsung (Direct Interactor), gunakan raycast manual ke arah depan thermometer
            // (Sumbu Z / biru dari object thermometer)
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 5f))
            {
                hitObject = hit.collider.gameObject;
                Debug.DrawLine(transform.position, hit.point, Color.green, 2f);
            }
            else
            {
                // Tampilkan garis merah di scene view jika meleset
                Debug.DrawRay(transform.position, transform.forward * 5f, Color.red, 2f);
            }
        }

        // Cek apakah yang ditabrak adalah bagian dari Bayi (menghindari error jika collider ada di tulang/anak object yang tidak ber-tag "Baby")
        BabyBehavior detectedBaby = null;
        if (hitObject != null)
        {
            detectedBaby = hitObject.GetComponentInParent<BabyBehavior>();
        }

        // Pastikan object tidak null dan merupakan bagian dari bayi (punya BabyBehavior atau tag Baby)
        if (detectedBaby != null || (hitObject != null && hitObject.CompareTag("Baby")))
        {
            // Jika babyBehavior belum di-assign di inspector, gunakan yang baru saja dideteksi
            if (babyBehavior == null)
            {
                babyBehavior = detectedBaby != null ? detectedBaby : FindFirstObjectByType<BabyBehavior>();
            }

            if (babyBehavior == null || tempDisplay == null)
            {
                Debug.LogError("[TempDetector] Gagal! tempDisplay atau babyBehavior masih KOSONG di inspector.");
                return;
            }

            // Hentikan timer lama jika sedang berjalan agar tidak tumpang tindih
            if (activeTimer != null) StopCoroutine(activeTimer);

            // Munculkan UI
            tempDisplay.activateTemperatureText(true);
            
            // Mengambil suhu bayi
            float babyTemp = babyBehavior.GetTemperature();
            tempDisplay.UpdateTemperature(babyTemp);

            // Jalankan timer untuk menyembunyikan UI setelah 7 detik
            activeTimer = StartCoroutine(DeactivateTemperatureTextAfterDelay(7.0f));
            
            Debug.Log("Suhu " + hitObject.name + " berhasil terdeteksi: " + babyTemp);
        }
        else
        {
            Debug.Log("Meleset! Raycast mengenai: " + (hitObject != null ? hitObject.name : "Udara Kosong") + ". Pastikan arah depan Thermometer pas ke tubuh bayi.");
        }
    }

    private IEnumerator DeactivateTemperatureTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        tempDisplay.deactivateTemperatureText(true);
        activeTimer = null;
    }
}