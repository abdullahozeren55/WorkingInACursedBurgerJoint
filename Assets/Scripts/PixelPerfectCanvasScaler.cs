using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasScaler))]
public class PixelPerfectCanvasScaler : MonoBehaviour
{
    [Header("Tasarým Yaptýðýn Çözünürlük")]
    public float referenceHeight = 360f; // 360p (Retro standart)

    [Header("Ayarlar")]
    public bool onlyIntegerScale = true;


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
        // Gelmezse (-1 ise) hafýzadaki deðeri kullan (Start veya Refresh aný)
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

            // 2. Oyuncunun isteðine göre küçült (Offset kadar çýkar)
            // Örn: Max 3x. Oyuncu "Orta" (Offset 1) seçti -> 3 - 1 = 2x.
            int targetScale = maxScale - currentScaleOffset;

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