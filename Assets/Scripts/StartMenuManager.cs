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
		if (blurVolume != null) blurVolume.weight = 1f;
		panelCanvasGroup.alpha = 1f;
	}

	public void OnStartButtonClicked()
	{
		StartCoroutine(TransitionRoutine());
	}

	IEnumerator TransitionRoutine()
	{
		// 1. Fade Out Panel (Cepat)
		while (panelCanvasGroup.alpha > 0)
		{
			panelCanvasGroup.alpha -= Time.deltaTime * panelFadeSpeed;
			yield return null;
		}
		startPanelObject.SetActive(false);

		// 2. Fade Out Blur (Perlahan/Lambat)
		while (blurVolume.weight > 0)
		{
			blurVolume.weight -= Time.deltaTime * blurFadeSpeed;
			yield return null;
		}

		// 3. Aktifkan pergerakan setelah semua bersih
		foreach (var move in movementProviders) if (move != null) move.enabled = true;

		Debug.Log("Game Started!");
	}
}