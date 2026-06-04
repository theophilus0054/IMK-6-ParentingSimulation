using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using System.Collections;
using TMPro;

public class StartMenuManager : MonoBehaviour
{
	[Header("Referensi UI")]
	public CanvasGroup panelCanvasGroup; // Untuk kontrol opacity panel
	public GameObject startPanelObject;
	public GameObject tutorialPanelObject;

	[Header("Referensi Efek")]
	public Volume blurVolume;

	[Header("Referensi Pergerakan")]
	public Behaviour[] movementProviders;

	[Header("Pengaturan Transisi")]
	public float panelFadeSpeed = 2.0f; // Kecepatan panel hilang (Cepat)
	public float blurFadeSpeed = 0.5f;  // Kecepatan blur hilang (Lambat)

	private const float TutorialCanvasScale = 0.00625f;

	private readonly TutorialSlide[] tutorialSlides =
	{
		new TutorialSlide("Misi Utama", "Tugasmu adalah mengobservasi gejala penyakit bayi\ndan mengambil keputusan medis yang tepat."),
		new TutorialSlide("Observasi & Alat Medis", "Perhatikan fisik dan suara bayi. Gunakan alat bantu\nseperti Thermometer untuk mengecek tanda-tanda\nvitalnya."),
		new TutorialSlide("Buku Panduan", "Buka Buku Panduan di menumu. Cocokkan hasil\nobservasi (suhu, napas, dll.) dengan informasi gejala\npenyakit."),
		new TutorialSlide("Waktunya Bertindak!", "Pilih tindakan penanganan yang tepat:\n[Self-care]: Untuk gejala ringan dan aman.\n[Hubungi Dokter]: Jika muncul tanda bahaya (Red Flags).")
	};

	private CanvasGroup tutorialCanvasGroup;
	private TMP_Text tutorialTitleText;
	private TMP_Text tutorialSlideTitleText;
	private TMP_Text tutorialBodyText;
	private Button previousButton;
	private Button nextButton;
	private Button understandButton;
	private int currentTutorialIndex;
	private bool transitionStarted;

	void Start()
	{
		Debug.Log("TESTING: Fungsi Start GameManager Berhasil Dipanggil!"); // Tambahkan baris ini

																			// Setup awal: Gerakan mati, Blur penuh, Panel muncul
		foreach (var move in movementProviders) if (move != null) move.enabled = false;
		if (blurVolume != null)
		{
			blurVolume.enabled = true;
			blurVolume.weight = 1f;
		}

		if (startPanelObject != null)
		{
			startPanelObject.SetActive(true);
		}

		if (panelCanvasGroup != null)
		{
			panelCanvasGroup.alpha = 1f;
			panelCanvasGroup.interactable = true;
			panelCanvasGroup.blocksRaycasts = true;
		}

		CreateTutorialPanelIfNeeded();
		if (tutorialPanelObject != null)
		{
			tutorialPanelObject.SetActive(false);
		}
	}

	public void OnStartButtonClicked()
	{
		Debug.Log("[StartMenuManager] Start button clicked. Showing tutorial.");
		ShowTutorial();
	}

	private void ShowTutorial()
	{
		CreateTutorialPanelIfNeeded();

		SetStartPanelContentActive(false);

		if (tutorialPanelObject != null)
		{
			tutorialPanelObject.SetActive(true);
		}

		if (tutorialCanvasGroup != null)
		{
			tutorialCanvasGroup.alpha = 1f;
			tutorialCanvasGroup.interactable = true;
			tutorialCanvasGroup.blocksRaycasts = true;
		}

		currentTutorialIndex = 0;
		UpdateTutorialPage();
	}

	private void PreviousTutorialPage()
	{
		if (currentTutorialIndex <= 0)
		{
			return;
		}

		currentTutorialIndex--;
		UpdateTutorialPage();
	}

	private void NextTutorialPage()
	{
		if (currentTutorialIndex >= tutorialSlides.Length - 1)
		{
			return;
		}

		currentTutorialIndex++;
		UpdateTutorialPage();
	}

	private void FinishTutorial()
	{
		if (transitionStarted)
		{
			return;
		}

		transitionStarted = true;
		StartCoroutine(TransitionRoutine());
	}

	private void UpdateTutorialPage()
	{
		TutorialSlide slide = tutorialSlides[currentTutorialIndex];

		if (tutorialTitleText != null)
		{
			tutorialTitleText.text = "TUTORIAL";
		}

		if (tutorialSlideTitleText != null)
		{
			tutorialSlideTitleText.text = slide.title;
		}

		if (tutorialBodyText != null)
		{
			tutorialBodyText.text = slide.body;
		}

		if (previousButton != null)
		{
			previousButton.gameObject.SetActive(currentTutorialIndex > 0);
		}

		if (nextButton != null)
		{
			nextButton.gameObject.SetActive(currentTutorialIndex < tutorialSlides.Length - 1);
		}

		if (understandButton != null)
		{
			understandButton.gameObject.SetActive(currentTutorialIndex == tutorialSlides.Length - 1);
		}
	}

	IEnumerator TransitionRoutine()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.StartGame();
		}

		if (tutorialCanvasGroup != null)
		{
			tutorialCanvasGroup.interactable = false;
			tutorialCanvasGroup.blocksRaycasts = false;
		}

		if (panelCanvasGroup != null)
		{
			while (panelCanvasGroup.alpha > 0)
			{
				panelCanvasGroup.alpha -= Time.unscaledDeltaTime * panelFadeSpeed;
				yield return null;
			}

			panelCanvasGroup.alpha = 0f;
			panelCanvasGroup.interactable = false;
			panelCanvasGroup.blocksRaycasts = false;
		}

		if (startPanelObject != null)
		{
			startPanelObject.SetActive(false);
		}

		if (tutorialPanelObject != null)
		{
			tutorialPanelObject.SetActive(false);
		}

		// 2. Fade Out Blur (Perlahan/Lambat)
		if (blurVolume != null)
		{
			blurVolume.enabled = true;
			while (blurVolume.weight > 0)
			{
				blurVolume.weight -= Time.unscaledDeltaTime * blurFadeSpeed;
				yield return null;
			}

			blurVolume.weight = 0f;
		}

		// 3. Aktifkan pergerakan setelah semua bersih
		foreach (var move in movementProviders) if (move != null) move.enabled = true;

		Debug.Log("Game Started!");
	}

	private void CreateTutorialPanelIfNeeded()
	{
		if (tutorialPanelObject != null)
		{
			CacheTutorialReferences();
			return;
		}

		Transform parent = GetTutorialParent();

		tutorialPanelObject = new GameObject("PanelTutorial", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
		tutorialPanelObject.transform.SetParent(parent, false);

		RectTransform panelRect = tutorialPanelObject.GetComponent<RectTransform>();
		ConfigureWorldPanelRect(panelRect);

		Image panelImage = tutorialPanelObject.GetComponent<Image>();
		panelImage.color = new Color(0.78f, 0.94f, 0.95f, 1f);

		tutorialCanvasGroup = tutorialPanelObject.GetComponent<CanvasGroup>();

		tutorialTitleText = CreateText(tutorialPanelObject.transform, "Tutorial Title", new Vector2(0.1f, 0.82f), new Vector2(0.9f, 0.95f), 10f, FontStyles.Bold, TextAlignmentOptions.Center);

		RectTransform cardRect = CreateRect(tutorialPanelObject.transform, "Tutorial Card", new Vector2(0.09f, 0.28f), new Vector2(0.91f, 0.72f));
		Image cardImage = cardRect.gameObject.AddComponent<Image>();
		cardImage.color = new Color(0.99f, 0.97f, 0.97f, 1f);
		cardImage.raycastTarget = false;

		tutorialSlideTitleText = CreateText(cardRect, "Tutorial Slide Title", new Vector2(0.08f, 0.66f), new Vector2(0.92f, 0.9f), 7f, FontStyles.Bold, TextAlignmentOptions.Center);
		tutorialSlideTitleText.color = new Color(0.25f, 0.49f, 0.66f, 1f);
		tutorialBodyText = CreateText(cardRect, "Tutorial Body", new Vector2(0.08f, 0.18f), new Vector2(0.92f, 0.62f), 5f, FontStyles.Bold, TextAlignmentOptions.Center);

		previousButton = CreateTextButton(tutorialPanelObject.transform, "Tutorial Previous", "<", new Vector2(0.08f, 0.43f), new Vector2(0.16f, 0.55f), 10f);
		previousButton.onClick.AddListener(PreviousTutorialPage);

		nextButton = CreateTextButton(tutorialPanelObject.transform, "Tutorial Next", ">", new Vector2(0.84f, 0.43f), new Vector2(0.92f, 0.55f), 10f);
		nextButton.onClick.AddListener(NextTutorialPage);

		understandButton = CreateTextButton(cardRect, "Tutorial Understand", "Mengerti", new Vector2(0.36f, 0.03f), new Vector2(0.64f, 0.17f), 5f);
		understandButton.onClick.AddListener(FinishTutorial);
	}

	private void ConfigureWorldPanelRect(RectTransform rectTransform)
	{
		rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
		rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
		rectTransform.anchoredPosition = Vector2.zero;
		rectTransform.sizeDelta = new Vector2(160f, 120f);
		rectTransform.pivot = new Vector2(0.5f, 0.5f);
		rectTransform.localScale = new Vector3(-TutorialCanvasScale, TutorialCanvasScale, 1f);
	}

	private Transform GetTutorialParent()
	{
		if (startPanelObject == null)
		{
			return transform;
		}

		// In this scene PanelStart is the world-space Canvas itself. A UI panel created
		// beside it has no Canvas and will not render, so attach tutorial content to it.
		if (startPanelObject.GetComponent<Canvas>() != null)
		{
			return startPanelObject.transform;
		}

		return startPanelObject.transform.parent != null ? startPanelObject.transform.parent : transform;
	}

	private void SetStartPanelContentActive(bool isActive)
	{
		if (startPanelObject == null)
		{
			return;
		}

		if (tutorialPanelObject != null && tutorialPanelObject.transform.IsChildOf(startPanelObject.transform))
		{
			foreach (Transform child in startPanelObject.transform)
			{
				if (child.gameObject != tutorialPanelObject)
				{
					child.gameObject.SetActive(isActive);
				}
			}
			return;
		}

		startPanelObject.SetActive(isActive);
	}

	private void CacheTutorialReferences()
	{
		tutorialCanvasGroup = tutorialPanelObject.GetComponent<CanvasGroup>();
		if (tutorialCanvasGroup == null)
		{
			tutorialCanvasGroup = tutorialPanelObject.AddComponent<CanvasGroup>();
		}
	}

	private RectTransform CreateRect(Transform parent, string objectName, Vector2 anchorMin, Vector2 anchorMax)
	{
		GameObject childObject = new GameObject(objectName, typeof(RectTransform));
		childObject.transform.SetParent(parent, false);
		RectTransform rectTransform = childObject.GetComponent<RectTransform>();
		Stretch(rectTransform, anchorMin, anchorMax);
		return rectTransform;
	}

	private TMP_Text CreateText(Transform parent, string objectName, Vector2 anchorMin, Vector2 anchorMax, float fontSize, FontStyles fontStyle, TextAlignmentOptions alignment)
	{
		GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
		textObject.transform.SetParent(parent, false);
		RectTransform rectTransform = textObject.GetComponent<RectTransform>();
		Stretch(rectTransform, anchorMin, anchorMax);

		TMP_Text text = textObject.GetComponent<TMP_Text>();
		text.fontSize = fontSize;
		text.fontStyle = fontStyle;
		text.alignment = alignment;
		text.color = Color.black;
		text.enableWordWrapping = true;
		text.raycastTarget = false;
		return text;
	}

	private Button CreateTextButton(Transform parent, string objectName, string label, Vector2 anchorMin, Vector2 anchorMax, float fontSize)
	{
		GameObject buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Button));
		buttonObject.transform.SetParent(parent, false);

		RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
		Stretch(rectTransform, anchorMin, anchorMax);

		Image image = buttonObject.GetComponent<Image>();
		image.color = label == "Mengerti" ? new Color(0.82f, 0.82f, 0.82f, 1f) : new Color(1f, 1f, 1f, 0f);
		image.raycastTarget = true;

		Button button = buttonObject.GetComponent<Button>();

		TMP_Text text = CreateText(buttonObject.transform, objectName + " Text", Vector2.zero, Vector2.one, fontSize, FontStyles.Bold, TextAlignmentOptions.Center);
		text.text = label;

		return button;
	}

	private void Stretch(RectTransform rectTransform, Vector2 anchorMin, Vector2 anchorMax)
	{
		rectTransform.anchorMin = anchorMin;
		rectTransform.anchorMax = anchorMax;
		rectTransform.offsetMin = Vector2.zero;
		rectTransform.offsetMax = Vector2.zero;
		rectTransform.localScale = Vector3.one;
	}

	private struct TutorialSlide
	{
		public readonly string title;
		public readonly string body;

		public TutorialSlide(string title, string body)
		{
			this.title = title;
			this.body = body;
		}
	}
}