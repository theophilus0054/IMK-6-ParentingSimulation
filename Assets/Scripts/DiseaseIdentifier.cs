using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DiseaseIdentifier : MonoBehaviour
{
    [Header("Reference ke Baby")]
    public BabyBehavior babyBehavior;

    [Header("Button Gejala")]
    public Button demamButton;
    public Button batukButton;
    public Button batukBerdahakButton;
    public Button sesakNapasButton;
    public Button pilekButton;

    [Header("Warna Button")]
    public Color selectedButtonColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    public Color unselectedButtonColor = new Color(1f, 1f, 1f, 0f);

    [Header("Panel Feedback")]
    public GameObject feedbackPanel;
    public TMP_Text feedbackText;

    [Header("Posisi Panel")]
    public Transform playerCamera;
    public float feedbackDistance = 1.5f;
    public Vector3 feedbackOffset = new Vector3(0f, -0.1f, 0f);

    private bool demam;
    private bool batuk;
    private bool batukBerdahak;
    private bool sesakNapas;
    private bool pilek;
    private float lastDemamClickTime = -1f;
    private float lastBatukClickTime = -1f;
    private float lastBatukBerdahakClickTime = -1f;
    private float lastSesakNapasClickTime = -1f;
    private float lastPilekClickTime = -1f;

    private const float ButtonClickCooldown = 0.15f;

    private void Start()
    {
        // Feedback panel tidak muncul saat awal game
        if (feedbackPanel != null)
        {
            feedbackPanel.SetActive(false);
        }

        RegisterButtonListeners();
        UpdateAllButtonColors();
    }

    private void OnDestroy()
    {
        UnregisterButtonListeners();
    }

    private void RegisterButtonListeners()
    {
        if (demamButton != null)
        {
            demamButton.onClick.RemoveListener(ToggleDemam);
            demamButton.onClick.AddListener(ToggleDemam);
        }

        if (batukButton != null)
        {
            batukButton.onClick.RemoveListener(ToggleBatuk);
            batukButton.onClick.AddListener(ToggleBatuk);
        }

        if (batukBerdahakButton != null)
        {
            batukBerdahakButton.onClick.RemoveListener(ToggleBatukBerdahak);
            batukBerdahakButton.onClick.AddListener(ToggleBatukBerdahak);
        }

        if (sesakNapasButton != null)
        {
            sesakNapasButton.onClick.RemoveListener(ToggleSesakNapas);
            sesakNapasButton.onClick.AddListener(ToggleSesakNapas);
        }

        if (pilekButton != null)
        {
            pilekButton.onClick.RemoveListener(TogglePilek);
            pilekButton.onClick.AddListener(TogglePilek);
        }
    }

    private void UnregisterButtonListeners()
    {
        if (demamButton != null)
        {
            demamButton.onClick.RemoveListener(ToggleDemam);
        }

        if (batukButton != null)
        {
            batukButton.onClick.RemoveListener(ToggleBatuk);
        }

        if (batukBerdahakButton != null)
        {
            batukBerdahakButton.onClick.RemoveListener(ToggleBatukBerdahak);
        }

        if (sesakNapasButton != null)
        {
            sesakNapasButton.onClick.RemoveListener(ToggleSesakNapas);
        }

        if (pilekButton != null)
        {
            pilekButton.onClick.RemoveListener(TogglePilek);
        }
    }

    public void ToggleDemam()
    {
        ToggleGejala(ref demam, demamButton, "Demam", ref lastDemamClickTime);
    }

    public void ToggleBatuk()
    {
        ToggleGejala(ref batuk, batukButton, "Batuk", ref lastBatukClickTime);
    }

    public void ToggleBatukBerdahak()
    {
        ToggleGejala(ref batukBerdahak, batukBerdahakButton, "Batuk Berdahak", ref lastBatukBerdahakClickTime);
    }

    public void ToggleSesakNapas()
    {
        ToggleGejala(ref sesakNapas, sesakNapasButton, "Sesak Napas", ref lastSesakNapasClickTime);
    }

    public void TogglePilek()
    {
        ToggleGejala(ref pilek, pilekButton, "Pilek", ref lastPilekClickTime);
    }

    private void ToggleGejala(ref bool gejala, Button button, string label, ref float lastClickTime)
    {
        if (Time.unscaledTime - lastClickTime < ButtonClickCooldown)
        {
            return;
        }

        lastClickTime = Time.unscaledTime;
        gejala = !gejala;
        UpdateButtonColor(button, gejala);
        Debug.Log(label + ": " + gejala);
    }

    private void UpdateAllButtonColors()
    {
        UpdateButtonColor(demamButton, demam);
        UpdateButtonColor(batukButton, batuk);
        UpdateButtonColor(batukBerdahakButton, batukBerdahak);
        UpdateButtonColor(sesakNapasButton, sesakNapas);
        UpdateButtonColor(pilekButton, pilek);
    }

    private void UpdateButtonColor(Button button, bool isSelected)
    {
        if (button == null || button.image == null)
        {
            return;
        }

        button.image.color = isSelected ? selectedButtonColor : unselectedButtonColor;
        button.image.raycastTarget = true;
    }

    public void SubmitIdentifikasi()
    {
        if (babyBehavior == null)
        {
            Debug.LogError("[DiseaseIdentifier] BabyBehavior belum di-assign!");
            return;
        }

        BabyBehavior.DiseaseState penyakitBayiSaatIni = babyBehavior.currentDisease;
        BabyBehavior.DiseaseState jawabanUser = GetJawabanUserDariChecklist();

        Debug.Log("=== SUBMIT IDENTIFIKASI ===");
        Debug.Log("State penyakit bayi saat submit: " + penyakitBayiSaatIni);
        Debug.Log("Jawaban user: " + jawabanUser);

        bool jawabanBenar =
            jawabanUser == penyakitBayiSaatIni &&
            penyakitBayiSaatIni != BabyBehavior.DiseaseState.None;

        // Panel muncul setiap kali submit, baik benar maupun salah
        if (feedbackPanel != null)
        {
            PlacePanelInFrontOfPlayer(feedbackPanel, feedbackDistance, feedbackOffset);
            feedbackPanel.SetActive(true);
        }

        if (jawabanBenar)
        {
            Debug.Log("<color=green>Identifikasi benar!</color>");

            if (feedbackText != null)
            {
                feedbackText.text = "Identifikasi benar!";
            }
        }
        else
        {
            Debug.Log("<color=red>Identifikasi salah.</color>");

            if (feedbackText != null)
            {
                feedbackText.text = "Identifikasi salah. Coba perhatikan gejala bayi lagi.";
            }
        }
    }

    private void PlacePanelInFrontOfPlayer(GameObject panel, float distance, Vector3 offset)
    {
        Transform cameraTransform = playerCamera;

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        if (panel == null || cameraTransform == null)
        {
            return;
        }

        Vector3 panelPosition = cameraTransform.position + cameraTransform.forward * distance + cameraTransform.TransformVector(offset);
        panel.transform.position = panelPosition;
        panel.transform.rotation = Quaternion.LookRotation(panelPosition - cameraTransform.position, Vector3.up);
    }

    public void CloseFeedbackPanel()
    {
        if (feedbackPanel != null)
        {
            feedbackPanel.SetActive(false);
        }

        Debug.Log("Feedback panel ditutup.");
    }

    private BabyBehavior.DiseaseState GetJawabanUserDariChecklist()
    {
        Debug.Log("=== CHECKLIST USER ===");
        Debug.Log("Demam: " + demam);
        Debug.Log("Batuk: " + batuk);
        Debug.Log("Batuk Berdahak: " + batukBerdahak);
        Debug.Log("Sesak Napas: " + sesakNapas);
        Debug.Log("Pilek: " + pilek);

        // Common Cold = hanya Pilek + Batuk
        if (!demam && batuk && !batukBerdahak && !sesakNapas && pilek)
        {
            return BabyBehavior.DiseaseState.CommonCold;
        }

        // Pneumonia = Demam + Batuk Berdahak + Sesak Napas
        if (demam && !batuk && batukBerdahak && sesakNapas && !pilek)
        {
            return BabyBehavior.DiseaseState.Pneumonia;
        }

        // Kombinasi lain dianggap tidak valid
        return BabyBehavior.DiseaseState.None;
    }

    public void ResetChecklist()
    {
        demam = false;
        batuk = false;
        batukBerdahak = false;
        sesakNapas = false;
        pilek = false;

        UpdateAllButtonColors();

        if (feedbackPanel != null)
        {
            feedbackPanel.SetActive(false);
        }

        if (feedbackText != null)
        {
            feedbackText.text = "";
        }

        Debug.Log("Checklist di-reset.");
    }
}
