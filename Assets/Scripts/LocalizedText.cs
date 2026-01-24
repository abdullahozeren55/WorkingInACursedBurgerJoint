using UnityEngine;
using TMPro;

public class LocalizedText : MonoBehaviour
{
    public string localizationKey;
    public FontType fontType = FontType.DialogueOutlined;

    private TMP_Text _textComp;

    // Hafýza Deðerleri
    private float _initialFontSize;
    private float _initialCharSpacing;
    private float _initialWordSpacing;
    private float _initialLineSpacing;

    private void Awake()
    {
        _textComp = GetComponent<TMP_Text>();
        if (_textComp != null)
        {
            _initialFontSize = _textComp.fontSize;
            _initialCharSpacing = _textComp.characterSpacing;
            _initialWordSpacing = _textComp.wordSpacing;
            _initialLineSpacing = _textComp.lineSpacing;
        }
    }

    private void Start()
    {
        UpdateContent();
    }

    private void OnEnable()
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged += UpdateContent;
            UpdateContent();
        }
    }

    private void OnDisable()
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged -= UpdateContent;
    }

    public void UpdateContent()
    {
        if (_textComp == null || LocalizationManager.Instance == null) return;

        // 1. Verileri Çek
        var targetData = LocalizationManager.Instance.GetFontDataForCurrentLanguage(fontType);
        var defaultData = LocalizationManager.Instance.GetDefaultFontData(fontType);

        // 2. Font Ata
        if (targetData.font != null && _textComp.font != targetData.font)
        {
            _textComp.font = targetData.font;
        }

        // 3. BOYUT HESABI (Scale)
        float defaultBaseSize = Mathf.Max(defaultData.basePixelSize, 0.1f);
        float scaleRatio = _initialFontSize / defaultBaseSize;

        _textComp.fontSize = targetData.basePixelSize * scaleRatio;

        // --- OFFSET HESABI SÝLÝNDÝ (Layout Group dostu oldu) ---

        // 4. METNÝ DÝREKT YAZ
        _textComp.text = LocalizationManager.Instance.GetText(localizationKey);

        // 5. SPACING HESABI (Karakter boþluðu kalabilir, Layout bozmaz)
        _textComp.characterSpacing = _initialCharSpacing + (targetData.characterSpacingOffset - defaultData.characterSpacingOffset);
        _textComp.wordSpacing = _initialWordSpacing + (targetData.wordSpacingOffset - defaultData.wordSpacingOffset);
        _textComp.lineSpacing = _initialLineSpacing + (targetData.lineSpacingOffset - defaultData.lineSpacingOffset);
    }
}