using UnityEngine;
using TMPro;

public class LocalizedText : MonoBehaviour
{
    public string localizationKey;
    public FontType fontType = FontType.DialogueOutlined;

    private TMP_Text _textComp;
    private RectTransform _rectTransform;

    // --- ORÝJÝNAL (INSPECTOR) DEÐERLERÝ HAFIZASI ---
    private float _initialFontSize;
    private Vector2 _initialAnchoredPosition;
    private float _initialCharSpacing;
    private float _initialWordSpacing;
    private float _initialLineSpacing;

    private void Awake()
    {
        _textComp = GetComponent<TMP_Text>();
        _rectTransform = GetComponent<RectTransform>();

        // 1. Oyun baþladýðý an, Inspector'da senin elle girdiðin deðerleri "Referans" olarak kaydet.
        if (_textComp != null)
        {
            _initialFontSize = _textComp.fontSize;
            _initialCharSpacing = _textComp.characterSpacing;
            _initialWordSpacing = _textComp.wordSpacing;
            _initialLineSpacing = _textComp.lineSpacing;
        }

        if (_rectTransform != null)
        {
            _initialAnchoredPosition = _rectTransform.anchoredPosition;
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
            // Enable olduðunda hemen güncelle ki UI açýlýr açýlmaz doðru görünsün
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

        // A. Metni Güncelle
        _textComp.text = LocalizationManager.Instance.GetText(localizationKey);

        // B. Font Verilerini Çek
        // 1. Hedef Dilin Verisi (Örn: Japonca)
        var targetData = LocalizationManager.Instance.GetFontDataForCurrentLanguage(fontType);

        // 2. Referans Dilin Verisi (Örn: Ýngilizce/Latin) - Oran hesabý için þart!
        var defaultData = LocalizationManager.Instance.GetDefaultFontData(fontType);

        // C. Fontu Ata
        if (targetData.font != null && _textComp.font != targetData.font)
        {
            _textComp.font = targetData.font;
        }

        // --- MATEMATÝK KISMI ---

        // 1. BOYUT HESAPLAMA (Relative Scaling)
        // Soru: Inspector'da boyutu kaç katýna çýkarmýþým?
        // Formül: (InspectorBoyutu / LatinBaseBoyutu)
        float defaultBaseSize = Mathf.Max(defaultData.basePixelSize, 0.1f); // 0'a bölünme hatasý önlemi
        float scaleRatio = _initialFontSize / defaultBaseSize;

        // Cevap: Yeni fontun base boyutunu ayný oranda büyüt.
        _textComp.fontSize = targetData.basePixelSize * scaleRatio;


        // 2. KONUM HESAPLAMA (Delta Offset)
        // Eðer bu obje bir LayoutGroup altýndaysa pozisyonu deðiþtirmek çalýþmayabilir ama denemekten zarar gelmez.
        if (_rectTransform != null)
        {
            // Fark: (Hedef Dilin Kaymasý - Latin Dilin Kaymasý)
            Vector2 offsetDelta = targetData.positionOffset - defaultData.positionOffset;

            // Sonuç: Orijinal konum + Fark
            _rectTransform.anchoredPosition = _initialAnchoredPosition + offsetDelta;
        }


        // 3. SPACING HESAPLAMA (Additive)
        _textComp.characterSpacing = _initialCharSpacing + (targetData.characterSpacingOffset - defaultData.characterSpacingOffset);
        _textComp.wordSpacing = _initialWordSpacing + (targetData.wordSpacingOffset - defaultData.wordSpacingOffset);
        _textComp.lineSpacing = _initialLineSpacing + (targetData.lineSpacingOffset - defaultData.lineSpacingOffset);
    }
}