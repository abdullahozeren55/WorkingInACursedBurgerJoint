using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.Events; // <-- 1. BU EKLENDÝ (UnityEvent için þart)

public class DesktopIcon : MonoBehaviour, IPointerClickHandler
{
    [Header("Görsel Parçalar")]
    public Image iconImage;
    public Image iconImageWorld;
    public Image textBackground;
    public Image textBackgroundWorld;
    private TMP_Text text;
    public TMP_Text textWorld;

    [Header("Renkler (Win95 Style)")]
    [SerializeField] private Color winBlue = new Color(0f, 0f, 0.5f, 1f);
    [SerializeField] private Color originalTextColor;
    [SerializeField] private Color selectedTextColor;
    private Color transparent = Color.clear;
    private Color white = Color.white;

    [Header("Events")]
    // 2. BU EKLENDÝ: Inspector'da çýkacak olan kutu bu.
    public UnityEvent onDoubleClick;

    private void Awake()
    {
        text = GetComponentInChildren<TMP_Text>();
    }

    private void Start()
    {
        DeselectVisuals();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Sadece Sol Týk
        if (eventData.button != PointerEventData.InputButton.Left) return;

        if (eventData.clickCount == 2)
        {
            // Sigorta: Gerçekten seçili miyim?
            bool amIReallySelected = false;
            if (MonitorManager.Instance != null)
            {
                amIReallySelected = (MonitorManager.Instance.CurrentSelectedIcon == this);
            }

            if (amIReallySelected)
            {
                // Seçiliyken 2. týk geldiyse -> EVENT'Ý ÇALIÞTIR
                OpenApplication();
            }
            else
            {
                // Arada seçim kaybolmuþ -> YENÝDEN SEÇ
                SelectMe();
            }
        }
        else
        {
            SelectMe();
        }
    }

    public void SelectMe()
    {
        // NOT: Burada "if (isSelected) return;" KULLANMIYORUZ.
        // Hýzlý týklamalarda senkron hatasý olmasýn diye her seferinde boyuyoruz.

        if (MonitorManager.Instance != null)
            MonitorManager.Instance.SelectIcon(this);

        // --- GÖRSELLERÝ YAK ---
        if (textBackground) textBackground.color = winBlue;
        if (textBackgroundWorld) textBackgroundWorld.color = winBlue;

        if (iconImage) iconImage.color = winBlue;
        if (iconImageWorld) iconImageWorld.color = winBlue;

        if (text) text.color = selectedTextColor;
        if (textWorld) textWorld.color = selectedTextColor;
    }

    public void DeselectVisuals()
    {

        // --- GÖRSELLERÝ SÖNDÜR ---
        if (textBackground) textBackground.color = transparent;
        if (textBackgroundWorld) textBackgroundWorld.color = transparent;

        if (iconImage) iconImage.color = white;
        if (iconImageWorld) iconImageWorld.color = white;

        if (text) text.color = originalTextColor;
        if (textWorld) textWorld.color = originalTextColor;
    }

    private void OpenApplication()
    {
        // Seçimleri temizle
        if (MonitorManager.Instance != null)
            MonitorManager.Instance.DeselectAll();

        // 3. BU EKLENDÝ: Inspector'da ne ayarladýysan onu çalýþtýrýr.
        onDoubleClick.Invoke();
    }
}