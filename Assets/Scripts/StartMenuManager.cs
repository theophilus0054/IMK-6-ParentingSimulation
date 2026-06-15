using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class StartMenuManager : MonoBehaviour
{
	[Header("Referensi UI")]
	public CanvasGroup panelCanvasGroup;
	public GameObject startPanelObject;
	public GameObject tutorialPanelObject;
	public GameObject healthbarCanvasObject; 

	[Header("Tutorial PNG")]
	[Tooltip("Isi dengan frame tutorial sebelum frame akhir. Frame terakhir di array akan memakai tombol Mengerti untuk pindah ke frame10.")]
	public Sprite[] tutorialFrameSprites;
	[Tooltip("Isi dengan frame10. Tombol Lanjut di frame ini akan memulai game seperti understandButton lama.")]
	public Sprite finalTutorialSprite;
	[Tooltip("Fallback auto-load dari Resources/TutorialFrames. Mendukung frame1..frame7 atau Frame 4..Frame 7, dan Frame 10.")]
	public string tutorialResourcesFolder = "TutorialFrames";

	[Header("Start Menu PNG")]
	[Tooltip("Sprite menu awal dengan tombol START dan tanda seru. Auto-load Frame 9 jika kosong.")]
	public Sprite startMenuSprite;
	[Tooltip("Sprite info pertama. Auto-load Frame 11 jika kosong.")]
	public Sprite startInfoSprite;
	[Tooltip("Sprite info tambahan. Jika kosong, tombol atas di Frame 11 akan kembali ke start menu.")]
	public Sprite startInfoDetailSprite;

	[Header("Referensi Efek")]
	public Volume blurVolume;

	[Header("Referensi Pergerakan")]
	public Behaviour[] movementProviders;

	[Header("Pengaturan Transisi")]
	public float panelFadeSpeed = 2.0f;
	public float blurFadeSpeed = 0.5f;

	private const float TutorialCanvasScale = 0.00625f;
	private const float TutorialButtonCooldown = 0.35f;
	private readonly TutorialSlide[] fallbackSlides =
	{
		new TutorialSlide("Misi Utama", "Tugasmu adalah mengobservasi gejala penyakit bayi\ndan mengambil keputusan medis yang tepat."),
		new TutorialSlide("Observasi & Alat Medis", "Perhatikan fisik dan suara bayi. Gunakan alat bantu\nseperti Thermometer untuk mengecek tanda-tanda vitalnya."),
		new TutorialSlide("Buku Panduan", "Buka Buku Panduan di menumu. Cocokkan hasil observasi\n(suhu, napas, dll.) dengan informasi gejala penyakit."),
		new TutorialSlide("Waktunya Bertindak!", "Pilih tindakan penanganan yang tepat:\n[Self-care]: Untuk gejala ringan dan aman.\n[Hubungi Dokter]: Jika muncul tanda bahaya (Red Flags).")
	};

	private CanvasGroup tutorialCanvasGroup;
	private Image tutorialFrameImage;
	private TMP_Text fallbackTitleText;
	private TMP_Text fallbackSlideTitleText;
	private TMP_Text fallbackBodyText;
	private GameObject startMenuRuntimeObject;
	private Image startMenuImage;
	private Button startMenuStartButton;
	private Button startMenuInfoButton;
	private Button startMenuInfoTopButton;
	private Button previousButton;
	private Button nextButton;
	private Button understandButton;
	private int currentTutorialIndex;
	private bool showingIntroFrame;
	private bool transitionStarted;
	private float tutorialInputUnlockTime;
	private float lastTutorialButtonClickTime = -1f;

	private int LastTutorialIndex
	{
		get { return Mathf.Max(GetTutorialFrameCount(), fallbackSlides.Length) - 1; }
	}

	private int UnderstandFrameIndex
	{
		get { return LastTutorialIndex; }
	}

void Start()
    {
        Debug.Log("TESTING: Fungsi Start GameManager Berhasil Dipanggil!");

        // --- TAMBAHKAN BARIS INI ---
        // Hentikan waktu game di latar belakang
        Time.timeScale = 0f; 
        
        // Pastikan state GameManager tidak Playing (misalnya Paused)
        if (GameManager.Instance != null)
        {
            // Sesuaikan dengan nama state pause di GameManager-mu
            // GameManager.Instance.currentState = GameManager.GameState.Paused; 
        }
        // ---------------------------

		if (healthbarCanvasObject != null)
        {
            healthbarCanvasObject.SetActive(false); // Sembunyikan Healthbar
        }

        foreach (var move in movementProviders)
        {
            if (move != null)
            {
                move.enabled = false;
            }
        }

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

		LoadTutorialSpritesFromResourcesIfNeeded();
		LoadStartMenuSpritesFromResourcesIfNeeded();
		CreateStartMenuSpritePanelIfNeeded();
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
		LoadTutorialSpritesFromResourcesIfNeeded();
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
		showingIntroFrame = finalTutorialSprite != null;
		tutorialInputUnlockTime = Time.unscaledTime + 0.2f;
		lastTutorialButtonClickTime = -1f;
		UpdateTutorialPage();
	}

	private void ShowStartInfoFrame()
	{
		SetStartMenuSprite(startInfoSprite);
		SetStartMenuButtonsActive(false, false, true);
	}

	private void ShowStartInfoDetailFrame()
	{
		if (startInfoDetailSprite != null)
		{
			SetStartMenuSprite(startInfoDetailSprite);
			SetStartMenuButtonsActive(false, false, true);
			return;
		}

		RestoreStartMenuFrame();
	}

	private void RestoreStartMenuFrame()
	{
		SetStartMenuSprite(startMenuSprite);
		SetStartMenuButtonsActive(true, true, false);
	}

	private void PreviousTutorialPage()
	{
		if (!CanAcceptTutorialButtonClick())
		{
			return;
		}

		if (showingIntroFrame)
		{
			return;
		}

		if (currentTutorialIndex <= 0)
		{
			return;
		}

		currentTutorialIndex--;
		UpdateTutorialPage();
	}

	private void NextTutorialPage()
	{
		if (!CanAcceptTutorialButtonClick())
		{
			return;
		}

		if (showingIntroFrame)
		{
			if (Time.unscaledTime < tutorialInputUnlockTime)
			{
				return;
			}

			Debug.Log("[StartMenuManager] Intro tutorial continue clicked. Showing tutorial frames.");
			showingIntroFrame = false;
			currentTutorialIndex = 0;
			UpdateTutorialPage();
			return;
		}

		if (currentTutorialIndex >= LastTutorialIndex)
		{
			return;
		}

		currentTutorialIndex++;
		UpdateTutorialPage();
	}

	private void OnUnderstandButtonClicked()
	{
		if (!CanAcceptTutorialButtonClick())
		{
			return;
		}

		if (showingIntroFrame)
		{
			if (Time.unscaledTime < tutorialInputUnlockTime)
			{
				return;
			}

			Debug.Log("[StartMenuManager] Intro tutorial continue clicked. Showing tutorial frames.");
			showingIntroFrame = false;
			currentTutorialIndex = 0;
			UpdateTutorialPage();
			return;
		}

		if (currentTutorialIndex != UnderstandFrameIndex)
		{
			return;
		}

		Debug.Log("[StartMenuManager] Tutorial understand clicked. Starting game.");
		FinishTutorial();
	}

	private bool CanAcceptTutorialButtonClick()
	{
		if (Time.unscaledTime - lastTutorialButtonClickTime < TutorialButtonCooldown)
		{
			return false;
		}

		lastTutorialButtonClickTime = Time.unscaledTime;
		return true;
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
		Sprite currentSprite = GetCurrentSprite();
		bool useSpriteTutorial = currentSprite != null;

		if (tutorialFrameImage != null)
		{
			tutorialFrameImage.enabled = useSpriteTutorial;
			tutorialFrameImage.sprite = currentSprite;
			tutorialFrameImage.preserveAspect = true;
		}

		SetFallbackTextActive(!useSpriteTutorial);
		if (!useSpriteTutorial)
		{
			UpdateFallbackText();
		}

		bool onUnderstandFrame = !showingIntroFrame && (currentTutorialIndex == UnderstandFrameIndex || currentTutorialIndex >= LastTutorialIndex);
		bool canGoPrevious = !showingIntroFrame && currentTutorialIndex > 0;
		bool canGoNext = !showingIntroFrame && currentTutorialIndex < LastTutorialIndex && !onUnderstandFrame;

		if (previousButton != null)
		{
			previousButton.gameObject.SetActive(canGoPrevious);
		}

		if (nextButton != null)
		{
			nextButton.gameObject.SetActive(canGoNext);
		}

		if (understandButton != null)
		{
			understandButton.gameObject.SetActive(showingIntroFrame || onUnderstandFrame);
		}
	}

	private Sprite GetCurrentSprite()
	{
		if (showingIntroFrame)
		{
			return finalTutorialSprite;
		}

		if (tutorialFrameSprites == null || currentTutorialIndex < 0 || currentTutorialIndex >= tutorialFrameSprites.Length)
		{
			return null;
		}

		return tutorialFrameSprites[currentTutorialIndex];
	}

	IEnumerator TransitionRoutine()
	{
		Time.timeScale = 1f;

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

		foreach (var move in movementProviders)
		{
			if (move != null)
			{
				move.enabled = true;
			}
		}

		if (healthbarCanvasObject != null)
        {
            healthbarCanvasObject.SetActive(true); // Munculkan Healthbar
        }

		Debug.Log("Game Started!");
	}

	private void LoadTutorialSpritesFromResourcesIfNeeded()
	{
		if (string.IsNullOrEmpty(tutorialResourcesFolder))
		{
			return;
		}

		Sprite[] explicitTutorialFrames = LoadSequentialTutorialFrames(4, 11);
		if (HasAllSprites(explicitTutorialFrames))
		{
			tutorialFrameSprites = explicitTutorialFrames;
		}
		else
		{
			bool hasAnyTutorialSprite = tutorialFrameSprites != null && tutorialFrameSprites.Length > 0 && tutorialFrameSprites[0] != null;
			if (!hasAnyTutorialSprite)
			{
				tutorialFrameSprites = Resources.LoadAll<Sprite>(tutorialResourcesFolder);
				System.Array.Sort(tutorialFrameSprites, CompareTutorialSprites);

				if (tutorialFrameSprites.Length == 0)
				{
					Texture2D[] tutorialTextures = Resources.LoadAll<Texture2D>(tutorialResourcesFolder);
					System.Array.Sort(tutorialTextures, CompareTutorialTextures);
					tutorialFrameSprites = CreateSpritesFromTextures(tutorialTextures);
				}

				if (tutorialFrameSprites.Length == 0)
				{
					tutorialFrameSprites = new Sprite[fallbackSlides.Length];
					for (int i = 0; i < tutorialFrameSprites.Length; i++)
					{
						tutorialFrameSprites[i] = LoadSpriteByAnyName("frame" + (i + 1), "Frame " + (i + 1));
					}
				}
			}
		}

		if (finalTutorialSprite == null)
		{
			finalTutorialSprite = LoadSpriteByAnyName("story", "Story");
		}

		if (tutorialFrameSprites != null && tutorialFrameSprites.Length > 0)
		{
			tutorialFrameSprites = RemoveFinalFrameFromTutorialFrames(tutorialFrameSprites);
		}
	}

	private void LoadStartMenuSpritesFromResourcesIfNeeded()
	{
		if (string.IsNullOrEmpty(tutorialResourcesFolder))
		{
			return;
		}

		if (startMenuSprite == null)
		{
			startMenuSprite = LoadSpriteByAnyName("start", "Start");
		}

		if (startInfoSprite == null)
		{
			startMenuSprite = LoadSpriteByAnyName("about", "About");
		}

		if (startInfoDetailSprite == null)
		{
			startInfoDetailSprite = null;
		}
	}

	private Sprite[] LoadSequentialTutorialFrames(int firstFrameNumber, int lastFrameNumber)
	{
		int frameCount = lastFrameNumber - firstFrameNumber + 1;
		Sprite[] sprites = new Sprite[frameCount];

		for (int i = 0; i < frameCount; i++)
		{
			int frameNumber = firstFrameNumber + i;
			sprites[i] = LoadSpriteByAnyName("frame" + frameNumber, "Frame " + frameNumber);
		}

		return sprites;
	}

	private bool HasAllSprites(Sprite[] sprites)
	{
		if (sprites == null || sprites.Length == 0)
		{
			return false;
		}

		for (int i = 0; i < sprites.Length; i++)
		{
			if (sprites[i] == null)
			{
				return false;
			}
		}

		return true;
	}

	private Sprite LoadSpriteByAnyName(params string[] spriteNames)
	{
		for (int i = 0; i < spriteNames.Length; i++)
		{
			Sprite sprite = Resources.Load<Sprite>(tutorialResourcesFolder + "/" + spriteNames[i]);
			if (sprite != null)
			{
				return sprite;
			}

			Texture2D texture = Resources.Load<Texture2D>(tutorialResourcesFolder + "/" + spriteNames[i]);
			if (texture != null)
			{
				return CreateSpriteFromTexture(texture);
			}
		}

		return null;
	}

	private Sprite[] CreateSpritesFromTextures(Texture2D[] textures)
	{
		Sprite[] sprites = new Sprite[textures.Length];
		for (int i = 0; i < textures.Length; i++)
		{
			sprites[i] = CreateSpriteFromTexture(textures[i]);
		}

		return sprites;
	}

	private Sprite CreateSpriteFromTexture(Texture2D texture)
	{
		if (texture == null)
		{
			return null;
		}

		Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
		sprite.name = texture.name;
		return sprite;
	}

	private Sprite[] RemoveFinalFrameFromTutorialFrames(Sprite[] sprites)
	{
		int finalFrameNumber = ExtractFirstNumber(finalTutorialSprite != null ? finalTutorialSprite.name : "Story");
		int count = 0;

		for (int i = 0; i < sprites.Length; i++)
		{
			if (sprites[i] != null && ExtractFirstNumber(sprites[i].name) != finalFrameNumber)
			{
				count++;
			}
		}

		Sprite[] filteredSprites = new Sprite[count];
		int writeIndex = 0;
		for (int i = 0; i < sprites.Length; i++)
		{
			if (sprites[i] != null && ExtractFirstNumber(sprites[i].name) != finalFrameNumber)
			{
				filteredSprites[writeIndex] = sprites[i];
				writeIndex++;
			}
		}

		return filteredSprites;
	}

	private int CompareTutorialSprites(Sprite left, Sprite right)
	{
		int leftNumber = ExtractFirstNumber(left != null ? left.name : string.Empty);
		int rightNumber = ExtractFirstNumber(right != null ? right.name : string.Empty);
		return leftNumber.CompareTo(rightNumber);
	}

	private int CompareTutorialTextures(Texture2D left, Texture2D right)
	{
		int leftNumber = ExtractFirstNumber(left != null ? left.name : string.Empty);
		int rightNumber = ExtractFirstNumber(right != null ? right.name : string.Empty);
		return leftNumber.CompareTo(rightNumber);
	}

	private int ExtractFirstNumber(string value)
	{
		int result = 0;
		bool foundNumber = false;

		for (int i = 0; i < value.Length; i++)
		{
			char character = value[i];
			if (character >= '0' && character <= '9')
			{
				result = (result * 10) + (character - '0');
				foundNumber = true;
			}
			else if (foundNumber)
			{
				break;
			}
		}

		return foundNumber ? result : int.MaxValue;
	}

	private int GetTutorialFrameCount()
	{
		if (tutorialFrameSprites == null || tutorialFrameSprites.Length == 0)
		{
			return 0;
		}

		int lastFilledIndex = -1;
		for (int i = 0; i < tutorialFrameSprites.Length; i++)
		{
			if (tutorialFrameSprites[i] != null)
			{
				lastFilledIndex = i;
			}
		}

		return lastFilledIndex + 1;
	}

	private void CreateTutorialPanelIfNeeded()
	{
		if (tutorialPanelObject != null)
		{
			CacheTutorialReferences();
			return;
		}

		Transform parent = GetTutorialParent();

		tutorialPanelObject = new GameObject("PanelTutorial", typeof(RectTransform), typeof(CanvasGroup));
		tutorialPanelObject.transform.SetParent(parent, false);

		RectTransform panelRect = tutorialPanelObject.GetComponent<RectTransform>();
		ConfigureWorldPanelRect(panelRect);

		tutorialCanvasGroup = tutorialPanelObject.GetComponent<CanvasGroup>();

		tutorialFrameImage = CreateImage(tutorialPanelObject.transform, "Tutorial Frame", Vector2.zero, Vector2.one);

		RectTransform fallbackCard = CreateRect(tutorialPanelObject.transform, "Tutorial Fallback Card", new Vector2(0.09f, 0.28f), new Vector2(0.91f, 0.72f));
		Image fallbackCardImage = fallbackCard.gameObject.AddComponent<Image>();
		fallbackCardImage.color = new Color(0.99f, 0.97f, 0.97f, 1f);
		fallbackCardImage.raycastTarget = false;

		fallbackTitleText = CreateText(tutorialPanelObject.transform, "Tutorial Title", new Vector2(0.1f, 0.82f), new Vector2(0.9f, 0.95f), 10f, FontStyles.Bold, TextAlignmentOptions.Center);
		fallbackSlideTitleText = CreateText(fallbackCard, "Tutorial Slide Title", new Vector2(0.08f, 0.66f), new Vector2(0.92f, 0.9f), 7f, FontStyles.Bold, TextAlignmentOptions.Center);
		fallbackSlideTitleText.color = new Color(0.25f, 0.49f, 0.66f, 1f);
		fallbackBodyText = CreateText(fallbackCard, "Tutorial Body", new Vector2(0.08f, 0.18f), new Vector2(0.92f, 0.62f), 5f, FontStyles.Bold, TextAlignmentOptions.Center);

		previousButton = CreateTransparentButton(tutorialPanelObject.transform, "Tutorial Previous", new Vector2(0.03f, 0.36f), new Vector2(0.18f, 0.62f));
		previousButton.onClick.AddListener(PreviousTutorialPage);

		nextButton = CreateTransparentButton(tutorialPanelObject.transform, "Tutorial Next", new Vector2(0.82f, 0.36f), new Vector2(0.97f, 0.62f));
		nextButton.onClick.AddListener(NextTutorialPage);

		understandButton = CreateTransparentButton(tutorialPanelObject.transform, "Tutorial Understand Or Continue", new Vector2(0.25f, 0.02f), new Vector2(0.75f, 0.28f));
		understandButton.onClick.AddListener(OnUnderstandButtonClicked);
	}

	private void CreateStartMenuSpritePanelIfNeeded()
	{
		if (startPanelObject == null)
		{
			return;
		}

		Transform parent = startPanelObject.GetComponent<Canvas>() != null ? startPanelObject.transform : startPanelObject.transform.parent;
		if (parent == null)
		{
			parent = startPanelObject.transform;
		}

		if (startMenuRuntimeObject == null)
		{
			Transform existing = parent.Find("Start Menu Sprite Runtime");
			startMenuRuntimeObject = existing != null ? existing.gameObject : new GameObject("Start Menu Sprite Runtime", typeof(RectTransform), typeof(Image));
			startMenuRuntimeObject.transform.SetParent(parent, false);
		}

		RectTransform menuRect = startMenuRuntimeObject.GetComponent<RectTransform>();
		if (menuRect == null)
		{
			menuRect = startMenuRuntimeObject.AddComponent<RectTransform>();
		}

		ConfigureWorldPanelRect(menuRect);

		startMenuImage = startMenuRuntimeObject.GetComponent<Image>();
		if (startMenuImage == null)
		{
			startMenuImage = startMenuRuntimeObject.AddComponent<Image>();
		}

		startMenuImage.color = Color.white;
		startMenuImage.raycastTarget = false;
		startMenuImage.preserveAspect = true;

		startMenuStartButton = EnsureTransparentButton(startMenuRuntimeObject.transform, "Start Sprite Button", new Vector2(0.34f, 0.25f), new Vector2(0.66f, 0.43f), OnStartButtonClicked);
		startMenuInfoButton = EnsureTransparentButton(startMenuRuntimeObject.transform, "Start Info Button", new Vector2(0.44f, 0.78f), new Vector2(0.56f, 0.92f), ShowStartInfoFrame);
		startMenuInfoTopButton = EnsureTransparentButton(startMenuRuntimeObject.transform, "Start Info Top Button", new Vector2(0.86f, 0.84f), new Vector2(0.98f, 0.98f), ShowStartInfoDetailFrame);

		if (startMenuSprite != null)
		{
			RestoreStartMenuFrame();
			HideStartPanelLegacyContent();
		}
		else
		{
			startMenuRuntimeObject.SetActive(false);
			Debug.LogWarning("[StartMenuManager] Start menu sprite belum ditemukan. Tambahkan Frame 9.png ke Resources/TutorialFrames atau assign Start Menu Sprite di Inspector.");
		}
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

	private void HideStartPanelLegacyContent()
	{
		if (startPanelObject == null)
		{
			return;
		}

		foreach (Transform child in startPanelObject.transform)
		{
			if (startMenuRuntimeObject != null && child == startMenuRuntimeObject.transform)
			{
				continue;
			}

			if (tutorialPanelObject != null && child == tutorialPanelObject.transform)
			{
				continue;
			}

			child.gameObject.SetActive(false);
		}
	}

	private void CacheTutorialReferences()
	{
		tutorialCanvasGroup = tutorialPanelObject.GetComponent<CanvasGroup>();
		if (tutorialCanvasGroup == null)
		{
			tutorialCanvasGroup = tutorialPanelObject.AddComponent<CanvasGroup>();
		}

		Transform frameTransform = tutorialPanelObject.transform.Find("Tutorial Frame");
		Transform previousTransform = tutorialPanelObject.transform.Find("Tutorial Previous");
		Transform nextTransform = tutorialPanelObject.transform.Find("Tutorial Next");
		Transform understandTransform = tutorialPanelObject.transform.Find("Tutorial Understand Or Continue");

		tutorialFrameImage = frameTransform != null ? frameTransform.GetComponent<Image>() : null;
		previousButton = previousTransform != null ? previousTransform.GetComponent<Button>() : null;
		nextButton = nextTransform != null ? nextTransform.GetComponent<Button>() : null;
		understandButton = understandTransform != null ? understandTransform.GetComponent<Button>() : null;
	}

	private Image CreateImage(Transform parent, string objectName, Vector2 anchorMin, Vector2 anchorMax)
	{
		GameObject imageObject = new GameObject(objectName, typeof(RectTransform), typeof(Image));
		imageObject.transform.SetParent(parent, false);
		RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
		Stretch(rectTransform, anchorMin, anchorMax);

		Image image = imageObject.GetComponent<Image>();
		image.color = Color.white;
		image.raycastTarget = false;
		return image;
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

	private Button CreateTransparentButton(Transform parent, string objectName, Vector2 anchorMin, Vector2 anchorMax)
	{
		GameObject buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Button));
		buttonObject.transform.SetParent(parent, false);

		RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
		Stretch(rectTransform, anchorMin, anchorMax);

		Image image = buttonObject.GetComponent<Image>();
		image.color = new Color(1f, 1f, 1f, 0.01f);
		image.raycastTarget = true;

		Button button = buttonObject.GetComponent<Button>();
		button.targetGraphic = image;
		return button;
	}

	private Button EnsureTransparentButton(Transform parent, string objectName, Vector2 anchorMin, Vector2 anchorMax, UnityEngine.Events.UnityAction action)
	{
		Transform existing = parent.Find(objectName);
		Button button;

		if (existing == null)
		{
			button = CreateTransparentButton(parent, objectName, anchorMin, anchorMax);
		}
		else
		{
			button = existing.GetComponent<Button>();
			if (button == null)
			{
				button = existing.gameObject.AddComponent<Button>();
			}

			RectTransform rectTransform = existing.GetComponent<RectTransform>();
			if (rectTransform == null)
			{
				rectTransform = existing.gameObject.AddComponent<RectTransform>();
			}

			Stretch(rectTransform, anchorMin, anchorMax);

			Image image = existing.GetComponent<Image>();
			if (image == null)
			{
				image = existing.gameObject.AddComponent<Image>();
			}

			image.color = new Color(1f, 1f, 1f, 0.01f);
			image.raycastTarget = true;
			button.targetGraphic = image;
		}

		button.onClick.RemoveListener(action);
		button.onClick.AddListener(action);
		return button;
	}

	private void SetStartMenuSprite(Sprite sprite)
	{
		if (startMenuImage == null)
		{
			return;
		}

		startMenuImage.sprite = sprite != null ? sprite : startMenuSprite;
		startMenuImage.enabled = startMenuImage.sprite != null;
	}

	private void SetStartMenuButtonsActive(bool startActive, bool infoActive, bool topInfoActive)
	{
		if (startMenuStartButton != null)
		{
			startMenuStartButton.gameObject.SetActive(startActive);
		}

		if (startMenuInfoButton != null)
		{
			startMenuInfoButton.gameObject.SetActive(infoActive);
		}

		if (startMenuInfoTopButton != null)
		{
			startMenuInfoTopButton.gameObject.SetActive(topInfoActive);
		}
	}

	private void SetFallbackTextActive(bool isActive)
	{
		if (fallbackTitleText != null)
		{
			fallbackTitleText.gameObject.SetActive(isActive);
		}

		if (fallbackSlideTitleText != null)
		{
			fallbackSlideTitleText.transform.parent.gameObject.SetActive(isActive);
		}
	}

	private void UpdateFallbackText()
	{
		int index = Mathf.Clamp(currentTutorialIndex, 0, fallbackSlides.Length - 1);
		TutorialSlide slide = fallbackSlides[index];

		if (fallbackTitleText != null)
		{
			fallbackTitleText.text = "TUTORIAL";
		}

		if (fallbackSlideTitleText != null)
		{
			fallbackSlideTitleText.text = slide.title;
		}

		if (fallbackBodyText != null)
		{
			fallbackBodyText.text = slide.body;
		}
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
