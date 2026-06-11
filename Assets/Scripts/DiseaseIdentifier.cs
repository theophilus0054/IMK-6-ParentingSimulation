using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    public string feedbackResourcesFolder = "Feedback Frame";

    [Header("Posisi Panel")]
    public Transform playerCamera;
    public float feedbackDistance = 1.5f;
    public Vector3 feedbackOffset = new Vector3(0f, -0.1f, 0f);

    private bool demam;
    private bool batuk;
    private bool batukBerdahak;
    private bool sesakNapas;
    private bool pilek;
    private bool symptomsSubmittedAndValid;

    private Image feedbackImage;
    private Button feedbackCloseButton;
    private RectTransform feedbackCard;
    private Sprite correctTreatmentSprite;
    private Sprite overTreatmentSprite;
    private Sprite criticalIgnoredSprite;
    private Sprite chooseTreatmentSprite;
    private Sprite wrongSymptomsSprite;

    private float lastDemamClickTime = -1f;
    private float lastBatukClickTime = -1f;
    private float lastBatukBerdahakClickTime = -1f;
    private float lastSesakNapasClickTime = -1f;
    private float lastPilekClickTime = -1f;

    private const float ButtonClickCooldown = 0.15f;
    private const float FeedbackPanelScale = 0.00625f;
    private const float FeedbackFontSize = 5f;
    private static readonly Vector2 FeedbackPanelSize = new Vector2(160f, 120f);

    private void Start()
    {
        LoadFeedbackSprites();
        RegisterButtonListeners();
        RegisterSubmitButtonListeners();
        SetupFeedbackLayout();
        UpdateAllButtonColors();
        SetTreatmentButtonsInteractable(false);

        if (feedbackPanel != null)
        {
            feedbackPanel.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        UnregisterButtonListeners();
        UnregisterSubmitButtonListeners();
    }

    private void RegisterButtonListeners()
    {
        RegisterButtonListener(demamButton, ToggleDemam);
        RegisterButtonListener(batukButton, ToggleBatuk);
        RegisterButtonListener(batukBerdahakButton, ToggleBatukBerdahak);
        RegisterButtonListener(sesakNapasButton, ToggleSesakNapas);
        RegisterButtonListener(pilekButton, TogglePilek);
    }

    private void RegisterButtonListener(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveListener(action);
        button.onClick.AddListener(action);
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

        if (selfCareSubmitButton != null && selfCareSubmitButton.onClick.GetPersistentEventCount() == 0)
        {
            selfCareSubmitButton.onClick.RemoveListener(SubmitSelfCare);
            selfCareSubmitButton.onClick.AddListener(SubmitSelfCare);
        }

        if (doctorSubmitButton != null && doctorSubmitButton.onClick.GetPersistentEventCount() == 0)
        {
            doctorSubmitButton.onClick.RemoveListener(SubmitDoctor);
            doctorSubmitButton.onClick.AddListener(SubmitDoctor);
        }
    }

    private void UnregisterButtonListeners()
    {
        UnregisterButtonListener(demamButton, ToggleDemam);
        UnregisterButtonListener(batukButton, ToggleBatuk);
        UnregisterButtonListener(batukBerdahakButton, ToggleBatukBerdahak);
        UnregisterButtonListener(sesakNapasButton, ToggleSesakNapas);
        UnregisterButtonListener(pilekButton, TogglePilek);
    }

    private void UnregisterButtonListener(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button != null)
        {
            button.onClick.RemoveListener(action);
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
        symptomsSubmittedAndValid = false;
        SetTreatmentButtonsInteractable(false);
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
        symptomsSubmittedAndValid = jawabanUser != BabyBehavior.DiseaseState.None && jawabanUser == penyakitBayiSaatIni;
        SetTreatmentButtonsInteractable(symptomsSubmittedAndValid);

        Debug.Log("=== SUBMIT GEJALA ===");
        Debug.Log("State penyakit bayi saat submit: " + penyakitBayiSaatIni);
        Debug.Log("Jawaban user: " + jawabanUser);
        Debug.Log("Gejala valid: " + symptomsSubmittedAndValid);

        ShowFeedbackSprite(symptomsSubmittedAndValid ? chooseTreatmentSprite : wrongSymptomsSprite);
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

        if (!symptomsSubmittedAndValid || jawabanUser == BabyBehavior.DiseaseState.None || jawabanUser != penyakitBayiSaatIni)
        {
            symptomsSubmittedAndValid = false;
            SetTreatmentButtonsInteractable(false);
            ShowFeedbackSprite(wrongSymptomsSprite);
            return;
        }

        TreatmentChoice expectedChoice = GetExpectedTreatment(penyakitBayiSaatIni);

        Debug.Log("=== SUBMIT IDENTIFIKASI ===");
        Debug.Log("State penyakit bayi saat submit: " + penyakitBayiSaatIni);
        Debug.Log("Jawaban user: " + jawabanUser);
        Debug.Log("Tindakan user: " + choice);
        Debug.Log("Tindakan seharusnya: " + expectedChoice);

        if (penyakitBayiSaatIni == BabyBehavior.DiseaseState.CommonCold && choice == TreatmentChoice.Doctor)
        {
            ShowFeedbackSprite(overTreatmentSprite);
            return;
        }

        if (penyakitBayiSaatIni == BabyBehavior.DiseaseState.Pneumonia && choice == TreatmentChoice.SelfCare)
        {
            ShowFeedbackSprite(criticalIgnoredSprite);
            return;
        }

        if (expectedChoice != TreatmentChoice.None && choice == expectedChoice)
        {
            Debug.Log("<color=green>Identifikasi benar!</color>");
            ShowFeedbackSprite(correctTreatmentSprite);
        }
        else
        {
            Debug.Log("<color=red>Identifikasi salah.</color>");
            ShowFeedbackSprite(wrongSymptomsSprite);
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

    private void ShowFeedbackSprite(Sprite sprite)
    {
        SetupFeedbackLayout();

        if (feedbackPanel != null)
        {
            PlacePanelInFrontOfPlayer(GetFeedbackRootObject(), feedbackDistance, feedbackOffset);
            feedbackPanel.SetActive(true);
        }

        if (feedbackImage != null)
        {
            feedbackImage.enabled = sprite != null;
            feedbackImage.sprite = sprite;
            feedbackImage.preserveAspect = true;
        }

        SetTextFeedbackActive(sprite == null);
    }

    private void SetupFeedbackLayout()
    {
        if (feedbackPanel == null)
        {
            return;
        }

        feedbackImage = feedbackPanel.GetComponent<Image>();
        if (feedbackImage == null)
        {
            feedbackImage = feedbackPanel.AddComponent<Image>();
        }

        feedbackImage.color = Color.white;
        feedbackImage.raycastTarget = true;

        feedbackCloseButton = feedbackPanel.GetComponent<Button>();
        if (feedbackCloseButton == null)
        {
            feedbackCloseButton = feedbackPanel.AddComponent<Button>();
        }

        feedbackCloseButton.onClick.RemoveListener(CloseFeedbackPanel);
        feedbackCloseButton.onClick.AddListener(CloseFeedbackPanel);

        ConfigureFeedbackPanelScale();

        feedbackCard = EnsureRectChild(feedbackPanel.transform, "Feedback Card", new Vector2(0.08f, 0.14f), new Vector2(0.92f, 0.74f));
        Image cardImage = feedbackCard.GetComponent<Image>();
        if (cardImage == null)
        {
            cardImage = feedbackCard.gameObject.AddComponent<Image>();
        }

        cardImage.color = new Color(0.99f, 0.97f, 0.97f, 1f);
        cardImage.raycastTarget = false;

        feedbackTitleText = EnsureTextChild(feedbackPanel.transform, feedbackTitleText, "Feedback Title", new Vector2(0.005f, 0.078f), new Vector2(0.095f, 0.098f), FeedbackFontSize, FontStyles.Bold, TextAlignmentOptions.Center);
        feedbackRecommendationText = EnsureTextChild(feedbackCard, feedbackRecommendationText, "Feedback Recommendation", new Vector2(0.008f, 0.064f), new Vector2(0.092f, 0.09f), FeedbackFontSize, FontStyles.Bold, TextAlignmentOptions.Center);

        if (feedbackText == null)
        {
            feedbackText = EnsureTextChild(feedbackCard, feedbackText, "Feedback Body", new Vector2(0.008f, 0.012f), new Vector2(0.092f, 0.058f), FeedbackFontSize, FontStyles.Bold, TextAlignmentOptions.Center);
        }
        else
        {
            feedbackText.transform.SetParent(feedbackCard, false);
            ConfigureTextRect(feedbackText.GetComponent<RectTransform>(), new Vector2(0.08f, 0.12f), new Vector2(0.92f, 0.58f));
            ConfigureText(feedbackText, FeedbackFontSize, FontStyles.Bold, TextAlignmentOptions.Center);
        }

        SetTextFeedbackActive(false);
    }

    private void LoadFeedbackSprites()
    {
        correctTreatmentSprite = LoadFeedbackSprite("Frame 1");
        overTreatmentSprite = LoadFeedbackSprite("Frame 2");
        criticalIgnoredSprite = LoadFeedbackSprite("Frame 3");
        chooseTreatmentSprite = LoadFeedbackSprite("Frame 12");
        wrongSymptomsSprite = LoadFeedbackSprite("Frame 13");
    }

    private Sprite LoadFeedbackSprite(string spriteName)
    {
        if (string.IsNullOrEmpty(feedbackResourcesFolder))
        {
            return null;
        }

        Sprite sprite = Resources.Load<Sprite>(feedbackResourcesFolder + "/" + spriteName);
        if (sprite != null)
        {
            return sprite;
        }

        Texture2D texture = Resources.Load<Texture2D>(feedbackResourcesFolder + "/" + spriteName);
        if (texture == null)
        {
            Debug.LogWarning("[DiseaseIdentifier] Feedback sprite tidak ditemukan: " + feedbackResourcesFolder + "/" + spriteName);
            return null;
        }

        Sprite textureSprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
        textureSprite.name = texture.name;
        return textureSprite;
    }

    private void SetTextFeedbackActive(bool isActive)
    {
        if (feedbackCard != null)
        {
            feedbackCard.gameObject.SetActive(isActive);
        }

        if (feedbackTitleText != null)
        {
            feedbackTitleText.gameObject.SetActive(isActive);
        }
    }

    private void SetTreatmentButtonsInteractable(bool isInteractable)
    {
        if (selfCareSubmitButton != null)
        {
            selfCareSubmitButton.interactable = isInteractable;
        }

        if (doctorSubmitButton != null)
        {
            doctorSubmitButton.interactable = isInteractable;
        }
    }

    private GameObject GetFeedbackRootObject()
    {
        if (feedbackPanel == null)
        {
            return null;
        }

        Canvas canvas = feedbackPanel.GetComponentInParent<Canvas>();
        return canvas != null ? canvas.gameObject : feedbackPanel;
    }

    private void ConfigureFeedbackPanelScale()
    {
        RectTransform panelRect = feedbackPanel.GetComponent<RectTransform>();
        if (panelRect != null)
        {
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            panelRect.localScale = Vector3.one;
        }

        Canvas canvas = feedbackPanel.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            return;
        }

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        if (canvasRect == null)
        {
            return;
        }

        canvasRect.anchorMin = new Vector2(0.5f, 0.5f);
        canvasRect.anchorMax = new Vector2(0.5f, 0.5f);
        canvasRect.anchoredPosition = Vector2.zero;
        canvasRect.sizeDelta = FeedbackPanelSize;
        canvasRect.pivot = new Vector2(0.5f, 0.5f);
        canvasRect.localScale = new Vector3(FeedbackPanelScale, FeedbackPanelScale, 1f);
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
        text.fontSizeMin = fontSize;
        text.fontSizeMax = fontSize;
        text.enableAutoSizing = false;
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

        if (!demam && batuk && !batukBerdahak && !sesakNapas && pilek)
        {
            return BabyBehavior.DiseaseState.CommonCold;
        }

        if (demam && !batuk && batukBerdahak && sesakNapas && !pilek)
        {
            return BabyBehavior.DiseaseState.Pneumonia;
        }

        return BabyBehavior.DiseaseState.None;
    }

    public void ResetChecklist()
    {
        demam = false;
        batuk = false;
        batukBerdahak = false;
        sesakNapas = false;
        pilek = false;
        symptomsSubmittedAndValid = false;

        UpdateAllButtonColors();
        SetTreatmentButtonsInteractable(false);

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
