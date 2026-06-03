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

    [Header("Button Keputusan")]
    public Button selfCareSubmitButton;
    public Button doctorSubmitButton;

    [Header("Warna Button")]
    public Color selectedButtonColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    public Color unselectedButtonColor = new Color(1f, 1f, 1f, 0f);

    [Header("Panel Feedback")]
    public GameObject feedbackPanel;
    public TMP_Text feedbackText;
    public TMP_Text feedbackTitleText;
    public TMP_Text feedbackRecommendationText;

    [Header("Posisi Panel")]
    public Transform playerCamera;
    public float feedbackDistance = 1.5f;
    public Vector3 feedbackOffset = new Vector3(0f, -0.1f, 0f);

    private bool demam;
    private bool batuk;
    private bool batukBerdahak;
    private bool sesakNapas;
    private bool pilek;
    private RectTransform feedbackCard;
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
        RegisterSubmitButtonListeners();
        SetupFeedbackLayout();
        UpdateAllButtonColors();
    }

    private void OnDestroy()
    {
        UnregisterButtonListeners();
        UnregisterSubmitButtonListeners();
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

    private void RegisterSubmitButtonListeners()
    {
        if (selfCareSubmitButton == null)
        {
            selfCareSubmitButton = FindButtonByName("Submit (1)");
        }

        if (doctorSubmitButton == null)
        {
            doctorSubmitButton = FindButtonByName("Submit (2)");
        }

        if (selfCareSubmitButton != null)
        {
            if (selfCareSubmitButton.onClick.GetPersistentEventCount() == 0)
            {
                selfCareSubmitButton.onClick.RemoveListener(SubmitSelfCare);
                selfCareSubmitButton.onClick.AddListener(SubmitSelfCare);
            }
        }

        if (doctorSubmitButton != null)
        {
            if (doctorSubmitButton.onClick.GetPersistentEventCount() == 0)
            {
                doctorSubmitButton.onClick.RemoveListener(SubmitDoctor);
                doctorSubmitButton.onClick.AddListener(SubmitDoctor);
            }
        }
    }

    private void UnregisterSubmitButtonListeners()
    {
        if (selfCareSubmitButton != null)
        {
            selfCareSubmitButton.onClick.RemoveListener(SubmitSelfCare);
        }

        if (doctorSubmitButton != null)
        {
            doctorSubmitButton.onClick.RemoveListener(SubmitDoctor);
        }
    }

    private Button FindButtonByName(string objectName)
    {
        Button[] buttons = Resources.FindObjectsOfTypeAll<Button>();
        foreach (Button button in buttons)
        {
            if (button != null && button.name == objectName)
            {
                return button;
            }
        }

        return null;
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
        ShowFeedback(
            "Pilih Tindakan",
            "pilih salah satu: Self-care atau bawa ke dokter",
            "Setelah mengamati gejala bayi, tentukan apakah kondisinya cukup dirawat mandiri atau membutuhkan penanganan dokter.",
            new Color(0.25f, 0.49f, 0.66f, 1f),
            false);
    }

    public void SubmitSelfCare()
    {
        SubmitTreatmentChoice(TreatmentChoice.SelfCare);
    }

    public void SubmitDoctor()
    {
        SubmitTreatmentChoice(TreatmentChoice.Doctor);
    }

    private void SubmitTreatmentChoice(TreatmentChoice choice)
    {
        if (babyBehavior == null)
        {
            Debug.LogError("[DiseaseIdentifier] BabyBehavior belum di-assign!");
            return;
        }

        BabyBehavior.DiseaseState penyakitBayiSaatIni = babyBehavior.currentDisease;
        BabyBehavior.DiseaseState jawabanUser = GetJawabanUserDariChecklist();
        TreatmentChoice expectedChoice = GetExpectedTreatment(penyakitBayiSaatIni);

        Debug.Log("=== SUBMIT IDENTIFIKASI ===");
        Debug.Log("State penyakit bayi saat submit: " + penyakitBayiSaatIni);
        Debug.Log("Jawaban user: " + jawabanUser);
        Debug.Log("Tindakan user: " + choice);
        Debug.Log("Tindakan seharusnya: " + expectedChoice);

        if (penyakitBayiSaatIni == BabyBehavior.DiseaseState.CommonCold && choice == TreatmentChoice.Doctor)
        {
            ShowFeedback(
                "Penanganan Berlebihan ❌",
                "seharusnya pilih : Self-care",
                "Bayi hanya mengalami demam ringan biasa. Membawanya ke rumah sakit justru berisiko memaparkannya pada virus lain.",
                new Color(1f, 0.3f, 0.08f, 1f),
                false);
            return;
        }

        if (penyakitBayiSaatIni == BabyBehavior.DiseaseState.Pneumonia && choice == TreatmentChoice.SelfCare)
        {
            ShowFeedback(
                "Kondisi Kritis Terabaikan ❌",
                "seharusnya pilih : bawa ke dokter",
                "Gejala seperti sesak napas dan demam tinggi adalah tanda bahaya (Red Flags). Perawatan mandiri di rumah tidak lagi cukup. Bayi harus segera mendapatkan penanganan medis dan bantuan darurat di rumah sakit!",
                new Color(1f, 0.3f, 0.08f, 1f),
                false);
            return;
        }

        if (expectedChoice != TreatmentChoice.None && choice == expectedChoice)
        {
            Debug.Log("<color=green>Identifikasi benar!</color>");
            ShowFeedback(
                "Keputusan Tepat ⭐",
                "Tindakanmu berhasil menyelamatkan kondisi bayi",
                "Tahukah kamu? Pneumonia adalah penyakit infeksi pembunuh nomor 1 pada anak balita di seluruh dunia! Penyakit ini menyumbang belasan persen dari total kematian anak di bawah usia 5 tahun setiap tahunnya. Kecepatan dan ketepatan observasimu sebagai orang tua sangat krusial untuk menyelamatkan nyawa mereka.",
                new Color(0.43f, 0.64f, 0.25f, 1f),
                true);
        }
        else
        {
            Debug.Log("<color=red>Identifikasi salah.</color>");
            ShowFeedback(
                "Observasi Belum Tepat ❌",
                "periksa lagi tanda-tanda bayi",
                "Kondisi bayi belum cukup jelas dari pilihanmu. Perhatikan kembali suhu, napas, batuk, pilek, dan tanda bahaya sebelum menentukan tindakan.",
                new Color(1f, 0.3f, 0.08f, 1f),
                false);
        }
    }

    private TreatmentChoice GetExpectedTreatment(BabyBehavior.DiseaseState disease)
    {
        if (disease == BabyBehavior.DiseaseState.CommonCold)
        {
            return TreatmentChoice.SelfCare;
        }

        if (disease == BabyBehavior.DiseaseState.Pneumonia)
        {
            return TreatmentChoice.Doctor;
        }

        return TreatmentChoice.None;
    }

    private void ShowFeedback(string title, string recommendation, string body, Color accentColor, bool isCorrect)
    {
        if (feedbackPanel != null)
        {
            SetupFeedbackLayout();
            PlacePanelInFrontOfPlayer(feedbackPanel, feedbackDistance, feedbackOffset);
            feedbackPanel.SetActive(true);
        }

        if (feedbackTitleText != null)
        {
            feedbackTitleText.text = title;
        }

        if (feedbackRecommendationText != null)
        {
            feedbackRecommendationText.text = recommendation;
            feedbackRecommendationText.color = accentColor;
        }

        if (feedbackText != null)
        {
            feedbackText.text = body;
        }
    }

    private void SetupFeedbackLayout()
    {
        if (feedbackPanel == null)
        {
            return;
        }

        Image panelImage = feedbackPanel.GetComponent<Image>();
        if (panelImage != null)
        {
            panelImage.color = new Color(0.78f, 0.94f, 0.95f, 1f);
            panelImage.raycastTarget = true;
        }

        feedbackCard = EnsureRectChild(feedbackPanel.transform, "Feedback Card", new Vector2(0.08f, 0.14f), new Vector2(0.92f, 0.74f));
        Image cardImage = feedbackCard.GetComponent<Image>();
        if (cardImage == null)
        {
            cardImage = feedbackCard.gameObject.AddComponent<Image>();
        }

        cardImage.color = new Color(0.99f, 0.97f, 0.97f, 1f);
        cardImage.raycastTarget = false;

        feedbackTitleText = EnsureTextChild(feedbackPanel.transform, feedbackTitleText, "Feedback Title", new Vector2(0.05f, 0.78f), new Vector2(0.95f, 0.98f), 38f, FontStyles.Bold, TextAlignmentOptions.Center);
        feedbackRecommendationText = EnsureTextChild(feedbackCard, feedbackRecommendationText, "Feedback Recommendation", new Vector2(0.08f, 0.64f), new Vector2(0.92f, 0.9f), 24f, FontStyles.Bold, TextAlignmentOptions.Center);

        if (feedbackText == null)
        {
            feedbackText = EnsureTextChild(feedbackCard, feedbackText, "Feedback Body", new Vector2(0.08f, 0.12f), new Vector2(0.92f, 0.58f), 24f, FontStyles.Bold, TextAlignmentOptions.Center);
        }
        else
        {
            feedbackText.transform.SetParent(feedbackCard, false);
            ConfigureTextRect(feedbackText.GetComponent<RectTransform>(), new Vector2(0.08f, 0.12f), new Vector2(0.92f, 0.58f));
            ConfigureText(feedbackText, 24f, FontStyles.Bold, TextAlignmentOptions.Center);
        }
    }

    private RectTransform EnsureRectChild(Transform parent, string objectName, Vector2 anchorMin, Vector2 anchorMax)
    {
        Transform child = parent.Find(objectName);
        RectTransform rectTransform;

        if (child == null)
        {
            GameObject childObject = new GameObject(objectName, typeof(RectTransform));
            childObject.transform.SetParent(parent, false);
            rectTransform = childObject.GetComponent<RectTransform>();
        }
        else
        {
            rectTransform = child.GetComponent<RectTransform>();
        }

        ConfigureTextRect(rectTransform, anchorMin, anchorMax);
        return rectTransform;
    }

    private TMP_Text EnsureTextChild(Transform parent, TMP_Text currentText, string objectName, Vector2 anchorMin, Vector2 anchorMax, float fontSize, FontStyles fontStyle, TextAlignmentOptions alignment)
    {
        TMP_Text text = currentText;

        if (text == null)
        {
            Transform existing = parent.Find(objectName);
            if (existing != null)
            {
                text = existing.GetComponent<TMP_Text>();
            }
        }

        if (text == null)
        {
            GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);
            text = textObject.GetComponent<TextMeshProUGUI>();
        }
        else
        {
            text.transform.SetParent(parent, false);
        }

        ConfigureTextRect(text.GetComponent<RectTransform>(), anchorMin, anchorMax);
        ConfigureText(text, fontSize, fontStyle, alignment);
        return text;
    }

    private void ConfigureTextRect(RectTransform rectTransform, Vector2 anchorMin, Vector2 anchorMax)
    {
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.localScale = Vector3.one;
    }

    private void ConfigureText(TMP_Text text, float fontSize, FontStyles fontStyle, TextAlignmentOptions alignment)
    {
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.alignment = alignment;
        text.color = Color.black;
        text.enableWordWrapping = true;
        text.raycastTarget = false;
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

    private enum TreatmentChoice
    {
        None,
        SelfCare,
        Doctor
    }
}
