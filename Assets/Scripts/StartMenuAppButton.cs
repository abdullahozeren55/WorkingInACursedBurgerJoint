using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class StartMenuAppButton : MonoBehaviour, IPointerClickHandler
{
    //WARNING: ONLY WORKS FOR MUSIC PLAYER BECAUSE I SET ITS PAGE ENUM TO MINIMIZED OR OPENED HERE!!!

    [Header("UI References")]
    public GameObject windowToOpen;
    public Image buttonImage;         // Butonun kendi görseli
    public Image buttonImageWorld;         // Butonun kendi görseli

    [Header("Sprites")]
    public Sprite normalSprite;       // Normal (Dýþa çýkýk)
    public Sprite pressedSprite;      // Basýlý (Ýçe göçük)

    private bool isOpen;

    private void Start()
    {
        isOpen = true;
        buttonImage.sprite = pressedSprite;
        buttonImageWorld.sprite = pressedSprite;
    }

    public void TurnOnOff(bool on)
    {
        buttonImage.gameObject.SetActive(on);
        buttonImageWorld.gameObject.SetActive(on);
    }

    // Týklama Algýlama
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        ToggleMenu(!isOpen);

        if (windowToOpen != null)
        {
            if (isOpen)
            {
                MonitorManager.Instance.OpenWindow(windowToOpen);
                MonitorManager.Instance.SetMusicPlayerPageState(1);
            }    
            else
            {
                windowToOpen.GetComponent<WindowController>().MinimizeWindow();
                MonitorManager.Instance.SetMusicPlayerPageState(2);
            }
                
        }
    }

    public void ToggleMenu(bool shouldOpen)
    {
        isOpen = shouldOpen;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        // 2. Görsel Deðiþtir
        if (buttonImage != null)
            buttonImage.sprite = isOpen ? pressedSprite : normalSprite;

        if (buttonImageWorld != null)
            buttonImageWorld.sprite = isOpen ? pressedSprite : normalSprite;
    }
}