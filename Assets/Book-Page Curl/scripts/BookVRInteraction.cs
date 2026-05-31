using UnityEngine;
using UnityEngine.EventSystems;

public class BookVRInteraction : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
	[Header("Referensi Buku")]
	public Book bookController;
	public bool isRightPage;

	// Terpanggil saat tombol Trigger di-klik pertama kali
	public void OnPointerDown(PointerEventData eventData)
	{
		ProcessVRPointer(eventData);
	}

	// Terpanggil saat Trigger ditahan dan controller digerakkan
	public void OnDrag(PointerEventData eventData)
	{
		ProcessVRPointer(eventData);
	}

	// Terpanggil saat Trigger dilepas
	public void OnPointerUp(PointerEventData eventData)
	{
		bookController.ReleasePage();
	}

	private void ProcessVRPointer(PointerEventData eventData)
	{
		// 1. Dapatkan posisi laser jatuhnya di dunia 3D
		Vector3 worldHitPos = eventData.pointerCurrentRaycast.worldPosition;

		// 2. Ubah posisi dunia 3D menjadi koordinat lokal Canvas/BookPanel
		Vector3 localPos = bookController.BookPanel.InverseTransformPoint(worldHitPos);

		// 3. Masukkan koordinat tersebut ke fungsi asli buku Anda
		if (isRightPage)
		{
			bookController.DragRightPageToPoint(localPos);
		}
		else
		{
			bookController.DragLeftPageToPoint(localPos);
		}
	}
}