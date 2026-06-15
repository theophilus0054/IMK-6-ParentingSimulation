using UnityEngine;


public class TagSocketInteractor : UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor
{
    [Header("Pengaturan Tag Custom")]
    [Tooltip("Masukkan nama Tag dari objek yang diizinkan masuk ke socket ini")]
    public string targetTag = "OximeterSocket"; // Ganti dengan tag default Anda jika mau

    // Fungsi ini mengecek apakah objek boleh mendekat (memunculkan efek hover/hologram)
    public override bool CanHover(UnityEngine.XR.Interaction.Toolkit.Interactables.IXRHoverInteractable interactable)
    {
        // Izinkan HANYA jika syarat bawaan XR terpenuhi DAN tag objeknya cocok
        return base.CanHover(interactable) && interactable.transform.CompareTag(targetTag);
    }

    // Fungsi ini mengecek apakah objek benar-benar boleh dilepas dan menempel (select/snap)
    public override bool CanSelect(UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable interactable)
    {
        return base.CanSelect(interactable) && interactable.transform.CompareTag(targetTag);
    }
}