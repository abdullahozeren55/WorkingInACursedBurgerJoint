using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.Events; // Týklama olayý için

public class StartMenuItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Görsel Parçalar")]
    public Image background;          // Arkadaki þerit
    public TextMeshProUGUI labelText; // Yazý
    public StartMenuController startMenuController;

    [Header("Win95 Renkleri")]
    public Color winBlue = new Color(0f, 0f, 0.5f, 1f); // Lacivert
    private Color transparent = Color.clear;             // Þeffaf (Normal hali)
    private Color white = Color.white;
    private Color black = Color.black;

    [Header("Týklanýnca Ne Olacak?")]
    public UnityEvent onClick;

    private void Start()
    {
        // Baþlangýçta temizle
        ResetVisuals();
    }

    // Mouse Üzerine Gelince (Hover)
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (background != null) background.color = winBlue;
        if (labelText != null) labelText.color = white;
    }

    // Mouse Gidince (Normal)
    public void OnPointerExit(PointerEventData eventData)
    {
        ResetVisuals();
    }

    // Týklanýnca
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        startMenuController.CloseMenu();

        // 2. Asýl iþlevi çalýþtýr (Bilgisayarý kapat, pencere aç vs.)
        onClick.Invoke();

        // 3. Görseli sýfýrla (Menü kapanýrken mavi kalmasýn)
        ResetVisuals();
    }

    private void ResetVisuals()
    {
        if (background != null) background.color = transparent;
        if (labelText != null) labelText.color = black;
    }

    // Obje kapanýp açýldýðýnda takýlý kalmasýn diye
    private void OnDisable()
    {
        ResetVisuals();
    }
}