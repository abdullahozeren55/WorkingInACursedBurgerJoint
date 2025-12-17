using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIGlowController : MonoBehaviour
{
    public enum GlowMode { Click, Hold }

    [Header("Ayarlar")]
    public GlowMode mode = GlowMode.Click;
    public float clickSpeed = 8f;
    public float chargeDuration = 0.5f;
    public float breathSpeed = 2f;
    public Vector2 breathRange = new Vector2(0.6f, 1.0f);

    [Header("Hedefler")]
    public Image targetImage;       // Child 1
    public GameObject keyboardParent; // Child 2
    public TMP_Text targetText;     // Child 2_Child

    // --- DEÐÝÞEN KISIM: ORÝJÝNAL MATERYALLERÝ SAKLA ---
    private Material _baseImageMat; // Image'in orijinal materyali
    private Material _baseTextMat;  // Text'in orijinal materyali

    private Material _imageMatInstance; // Anlýk kullanýlan kopya
    private Material _textMatInstance;  // Anlýk kullanýlan kopya

    private int _glowAmountID;
    private int _emissionMapID;
    private int _glowColorID;
    private int _faceColorID; // Bitmap font için

    private bool _isUsingImage = false;
    private Color _currentTextGlowColor;

    private void Awake()
    {
        _glowAmountID = Shader.PropertyToID("_GlowAmount");
        _emissionMapID = Shader.PropertyToID("_EmissionMap");
        _glowColorID = Shader.PropertyToID("_GlowColor");
        _faceColorID = Shader.PropertyToID("_FaceColor");

        // --- YEDEKLEME ÝÞLEMÝ ---
        // Oyun baþlarken editörde ne atadýysan onlarý "Referans" olarak saklýyoruz.
        if (targetImage != null) _baseImageMat = targetImage.material;
        if (targetText != null) _baseTextMat = targetText.fontSharedMaterial;
    }

    public void SetVisualData(bool useImage, Texture2D emissionTex, Color glowColor)
    {
        _isUsingImage = useImage;
        _currentTextGlowColor = glowColor;

        StopAllCoroutines();

        // Eski kopyalarý temizle
        if (_imageMatInstance) Destroy(_imageMatInstance);
        if (_textMatInstance) Destroy(_textMatInstance);

        if (useImage && targetImage != null)
        {
            // --- IMAGE MODU ---
            targetImage.gameObject.SetActive(true);
            if (keyboardParent) keyboardParent.SetActive(false);

            // DÝKKAT: Artýk targetImage.material'den deðil, _baseImageMat'ten kopya alýyoruz!
            if (_baseImageMat != null)
            {
                _imageMatInstance = Instantiate(_baseImageMat);
                targetImage.material = _imageMatInstance;

                if (emissionTex != null)
                    _imageMatInstance.SetTexture(_emissionMapID, emissionTex);

                _imageMatInstance.SetColor(_glowColorID, glowColor);
            }
        }
        else if (!useImage && targetText != null)
        {
            // --- TEXT MODU ---
            if (targetImage) targetImage.gameObject.SetActive(false);
            if (keyboardParent) keyboardParent.SetActive(true);
            targetText.gameObject.SetActive(true);

            // DÝKKAT: _baseTextMat'ten kopya alýyoruz!
            if (_baseTextMat != null)
            {
                _textMatInstance = Instantiate(_baseTextMat);
                targetText.fontSharedMaterial = _textMatInstance;

                // Bitmap Font Ayarlarý
                targetText.fontSharedMaterial.EnableKeyword("GLOW_ON");
                targetText.UpdateMeshPadding();
                targetText.fontSharedMaterial.SetColor(ShaderUtilities.ID_GlowColor, glowColor);
            }
        }

        StartCoroutine(AnimateGlow());
    }

    private IEnumerator AnimateGlow()
    {
        float timer = 0f;

        while (true)
        {
            float glowValue = 0f;

            if (mode == GlowMode.Click)
            {
                glowValue = Mathf.PingPong(Time.unscaledTime * clickSpeed, 1f);
            }
            else
            {
                if (timer < chargeDuration)
                {
                    timer += Time.unscaledDeltaTime;
                    float progress = timer / chargeDuration;
                    glowValue = Mathf.SmoothStep(0.2f, 1f, progress);
                }
                else
                {
                    float breathBase = breathRange.x;
                    float breathDiff = breathRange.y - breathRange.x;
                    float wave = (Mathf.Sin(Time.unscaledTime * breathSpeed) + 1f) / 2f;
                    glowValue = breathBase + (wave * breathDiff);
                }
            }

            ApplyGlow(glowValue);
            yield return null;
        }
    }

    private void ApplyGlow(float value)
    {
        if (_isUsingImage && _imageMatInstance != null)
        {
            // Image için 0-1 arasý iyidir, çünkü alttaki resim zaten görünüyor.
            _imageMatInstance.SetFloat(_glowAmountID, value);
        }
        else if (!_isUsingImage && _textMatInstance != null)
        {
            // --- TEXT FIX (KAYBOLMAYI ÖNLEME) ---
            // Gelen 'value' deðiþkeni 0 (sönük) ile 1 (parlak) arasýnda gidip geliyor.
            // Biz bunu Text için "En az 0.3 olsun" diye sýkýþtýrýyoruz (Lerp).

            float minVisibility = 0.3f; // %30'un altýna düþmesin (Bunu istersen 0.5 yap)
            float adjustedValue = Mathf.Lerp(minVisibility, 1f, value);

            // Artýk adjustedValue 0.3 ile 1.0 arasýnda.
            // HDR Rengi bu yeni deðerle çarpýyoruz.
            Color finalColor = _currentTextGlowColor * adjustedValue;

            _textMatInstance.SetColor(_faceColorID, finalColor);
        }
    }

    private void OnDisable() => StopAllCoroutines();

    private void OnDestroy()
    {
        if (_imageMatInstance) Destroy(_imageMatInstance);
        if (_textMatInstance) Destroy(_textMatInstance);
    }
}