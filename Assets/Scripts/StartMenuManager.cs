using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using System.Collections;

public class StartMenuManager : MonoBehaviour
{
	[Header("Referensi UI")]
	public CanvasGroup panelCanvasGroup; // Untuk kontrol opacity panel
	public GameObject startPanelObject;

	[Header("Referensi Efek")]
	public Volume blurVolume;

	[Header("Referensi Pergerakan")]
	public Behaviour[] movementProviders;

	[Header("Pengaturan Transisi")]
	public float panelFadeSpeed = 2.0f; // Kecepatan panel hilang (Cepat)
	public float blurFadeSpeed = 0.5f;  // Kecepatan blur hilang (Lambat)

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
	}

	public void OnStartButtonClicked()
	{
		StartCoroutine(TransitionRoutine());
	}

	IEnumerator TransitionRoutine()
	{
		// 1. Fade Out Panel (Cepat)
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
}
