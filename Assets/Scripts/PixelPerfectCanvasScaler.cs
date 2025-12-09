using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasScaler))]
public class PixelPerfectCanvasScaler : MonoBehaviour
{
    [Header("Tasarým Yaptýðýn Çözünürlük")]
    public float referenceHeight = 360f; // 360p (Retro standart)

    [Header("Ayarlar")]
    public bool onlyIntegerScale = true;

    [Header("Özel Durumlar")]
    [Tooltip("Eðer bu iþaretliyse, Ayarlardaki UI Scale seçeneðini takmaz. Hep ekrana sýðan en büyük tam sayýyý alýr. (Örn: Telefon UI için)")]
    public bool forceMaxIntegerScale = false;

    private int currentScaleOffset = 0;
    private CanvasScaler _canvasScaler;

    private void Awake()
    {
        _canvasScaler = GetComponent<CanvasScaler>();
        // Modu garantiye al
        _canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
    }

    private void Start()
    {
        // Baþlangýçta MenuManager'dan veya hafýzadan güncel offset'i çekmek isteyebilirsin
        // Þimdilik varsayýlan çalýþýyor.
        UpdateScale();

        if (MenuManager.Instance != null)
        {
            MenuManager.Instance.RegisterScaler(this);
        }
    }

    private void OnDestroy()
    {
        if (MenuManager.Instance != null)
        {
            MenuManager.Instance.UnregisterScaler(this);
        }
    }

    public void UpdateScale(int scaleOffset = -1)
    {
        // Eðer dýþarýdan bir deðer gelirse onu kaydet (Ayarlardan gelmiþtir)
        if (scaleOffset != -1) currentScaleOffset = scaleOffset;

        if (_canvasScaler == null) return;
        if (Screen.height == 0) return;

        float screenHeight = Screen.height;
        float scaleFactor = 1f;

        if (onlyIntegerScale)
        {
            // 1. Maksimum mümkün olan tam sayýyý bul (Örn: 1080p -> 3x)
            int maxScale = Mathf.FloorToInt(screenHeight / referenceHeight);
            if (maxScale < 1) maxScale = 1;

            // --- KRÝTÝK DEÐÝÞÝKLÝK BURADA ---
            // Eðer forceMaxIntegerScale seçiliyse, kullanýcýnýn offset ayarýný (currentScaleOffset) YOK SAYIYORUZ.
            int effectiveOffset = forceMaxIntegerScale ? 0 : currentScaleOffset;

            // 2. Hedef scale'i belirle
            int targetScale = maxScale - effectiveOffset;

            // 3. Güvenlik: 1'in altýna düþmesin
            if (targetScale < 1) targetScale = 1;

            scaleFactor = targetScale;
        }
        else
        {
            scaleFactor = screenHeight / referenceHeight;
        }

        _canvasScaler.scaleFactor = scaleFactor;
    }
}