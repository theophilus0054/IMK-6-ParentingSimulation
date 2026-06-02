using UnityEngine;

public class BookPageUIManager : MonoBehaviour
{
    [Header("Referensi Buku")]
    public Book bookScript;

    [Header("Daftar UI Halaman / Spread")]
    public GameObject[] pagePanels;

    private void OnEnable()
    {
        if (bookScript != null)
            bookScript.OnFlip.AddListener(UpdatePageUI);
    }

    private void OnDisable()
    {
        if (bookScript != null)
            bookScript.OnFlip.RemoveListener(UpdatePageUI);
    }

    private void Start()
    {
        UpdatePageUI();
    }

    public void UpdatePageUI()
    {
        if (bookScript == null || pagePanels == null || pagePanels.Length == 0)
            return;

        // Matikan semua panel dulu
        for (int i = 0; i < pagePanels.Length; i++)
        {
            if (pagePanels[i] != null)
            {
                pagePanels[i].SetActive(false);

                CanvasGroup cg = pagePanels[i].GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    cg.interactable = false;
                    cg.blocksRaycasts = false;
                }
            }
        }

        // Karena currentPage naik 2: 0, 2, 4...
        // Maka spread index = 0, 1, 2...
        int spreadIndex = bookScript.currentPage / 2;

        if (spreadIndex >= 0 && spreadIndex < pagePanels.Length && pagePanels[spreadIndex] != null)
        {
            pagePanels[spreadIndex].SetActive(true);
            pagePanels[spreadIndex].transform.SetAsLastSibling();

            CanvasGroup cg = pagePanels[spreadIndex].GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.interactable = true;
                cg.blocksRaycasts = true;
            }
        }

        Debug.Log($"currentPage = {bookScript.currentPage}, spreadIndex = {bookScript.currentPage / 2}");
    }
}