using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Týklamayý algýlamak için þart

public class WindowController : MonoBehaviour, IPointerClickHandler
{
    [Header("Görsel Ayarlar")]
    public Image headerBackground;    // Mavi/Gri olacak þerit (Title Bar)
    public Image headerBackgroundWorld;    // Mavi/Gri olacak þerit (Title Bar)
    public Sprite activeSprite;       // Lacivert (Aktif) Sprite
    public Sprite passiveSprite;      // Gri (Pasif) Sprite
    [Space]
    public StartMenuAppButton startMenuAppButton;

    [Header("Etkileþim Engelleyici")]
    public GameObject contentBlocker;

    // Pencere baþlýðý yazýsýnýn rengini de deðiþtirmek istersen:
    // public TMPro.TextMeshProUGUI headerText;
    // public Color activeTextColor = Color.white;
    // public Color passiveTextColor = Color.grey;

    // Pencerenin herhangi bir yerine týklandýðýnda çalýþýr
    public void OnPointerClick(PointerEventData eventData)
    {
        // Týklanýnca Manager'a "Beni öne al" de
        MonitorManager.Instance.FocusWindow(this);
    }

    // Manager tarafýndan çaðrýlýr: Görseli güncelle
    public void SetState(bool isActive)
    {
        if (headerBackground != null)
        {
            headerBackground.sprite = isActive ? activeSprite : passiveSprite;
        }

        if (headerBackgroundWorld != null)
        {
            headerBackgroundWorld.sprite = isActive ? activeSprite : passiveSprite;
        }

        if (contentBlocker != null)
        {
            contentBlocker.SetActive(!isActive);
        }

        if (startMenuAppButton != null)
        {
            startMenuAppButton.ToggleMenu(isActive);
        }

        // Eðer metin rengi deðiþecekse:
        // if (headerText != null) headerText.color = isActive ? activeTextColor : passiveTextColor;
    }

    // Kapatma butonuna baðlayacaðýn fonksiyon
    public void CloseWindow()
    {

        if (startMenuAppButton != null)
        {
            startMenuAppButton.TurnOnOff(false);
        }

        // Kapanmadan önce Manager'a haber ver ki bir sonrakini seçsin
        MonitorManager.Instance.OnWindowClosed(this);
        TurnOnOff(false);
    }

    public void MinimizeWindow()
    {
        if (startMenuAppButton != null)
        {
            startMenuAppButton.TurnOnOff(true);

            startMenuAppButton.ToggleMenu(false);
        }

        MonitorManager.Instance.OnWindowClosed(this);
        TurnOnOff(false);
    }

    public void SetLast()
    {
        headerBackground.transform.SetAsLastSibling();
        headerBackgroundWorld.transform.SetAsLastSibling();

        if (startMenuAppButton != null)
        {
            startMenuAppButton.TurnOnOff(true);
            startMenuAppButton.ToggleMenu(true);
        }
    }

    public void TurnOnOff(bool on)
    {
        headerBackground.gameObject.SetActive(on);
        headerBackgroundWorld.gameObject.SetActive(on);
    }
}