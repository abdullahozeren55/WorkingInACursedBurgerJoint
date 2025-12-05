using UnityEngine;
using UnityEngine.UI; // Button komponentine eriþmek için
using UnityEngine.EventSystems; // Mouse olaylarýný yakalamak için

public class UIButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
{
    [Header("Ses Dosyalarý")]
    public AudioClip hoverSound; // Üzerine gelince (Hover)
    public AudioClip clickSound; // Týklayýnca (Click)

    [Header("Ayarlar")]
    public float hoverVolume = 1f;
    public float hoverMinPitch = 0.85f;
    public float hoverMaxPitch = 1.15f;
    [Space]
    public float clickVolume = 1f;
    public float clickMinPitch = 0.85f;
    public float clickMaxPitch = 1.15f;

    private Selectable _selectable;

    private void Awake()
    {
        _selectable = GetComponent<Selectable>();
    }

    // Mouse üzerine geldiðinde çalýþýr
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Eðer buton pasifse (Interactable = false) ses çalmasýn
        if (_selectable != null && !_selectable.interactable) return;

        if (hoverSound != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayUISoundFX(hoverSound, hoverVolume, hoverMinPitch, hoverMaxPitch);
        }
    }

    // Mouse týklandýðýnda (Bastýðýn an) çalýþýr
    public void OnPointerDown(PointerEventData eventData)
    {
        if (_selectable != null && !_selectable.interactable) return;

        if (clickSound != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayUISoundFX(clickSound, clickVolume, clickMinPitch, clickMaxPitch);
        }
    }
}