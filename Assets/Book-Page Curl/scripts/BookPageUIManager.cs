using UnityEngine;

public class BookPageUIManager : MonoBehaviour
{
    [Header("Referensi Buku")]
    public Book bookScript; // Tarik script Book.cs Anda ke sini

    [Header("Daftar UI Halaman (Urutkan dari Hal 0, 1, 2...)")]
    public GameObject[] pagePanels; // Masukkan semua objek Page_X_UI ke sini Berurutan

    private int lastPage = -1;

    void Start()
    {
        // Jalankan pengecekan sekali di awal game
        UpdatePageUI();
    }

    void Update()
    {
        if (bookScript == null || pagePanels.Length == 0) return;

        // Jika pemain membalik halaman, perbarui tombol yang aktif
        if (bookScript.currentPage != lastPage)
        {
            UpdatePageUI();
            lastPage = bookScript.currentPage;
        }
    }

    void UpdatePageUI()
    {
        // 1. Matikan semua wadah UI terlebih dahulu agar bersih
        foreach (GameObject panel in pagePanels)
        {
            if (panel != null) panel.SetActive(false);
        }

        // Karena Book.cs menampilkan halaman Kiri dan Kanan sekaligus:
        // Sisi Kiri biasanya adalah indeks 'currentPage' atau 'currentPage - 1' bergantung posisi awal
        int leftPageIndex = bookScript.currentPage - 1; 
        int rightPageIndex = bookScript.currentPage;

        // 2. Aktifkan UI untuk halaman kiri jika ada di dalam daftar array
        if (leftPageIndex >= 0 && leftPageIndex < pagePanels.Length)
        {
            if (pagePanels[leftPageIndex] != null) 
                pagePanels[leftPageIndex].SetActive(true);
        }

        // 3. Aktifkan UI untuk halaman kanan jika ada di dalam daftar array
        if (rightPageIndex >= 0 && rightPageIndex < pagePanels.Length)
        {
            if (pagePanels[rightPageIndex] != null) 
                pagePanels[rightPageIndex].SetActive(true);
        }
    }
}