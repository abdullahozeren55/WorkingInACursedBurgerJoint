using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewFontProfile", menuName = "Localization/Font Profile")]
public class LanguageFontProfile : ScriptableObject
{
    public LocalizationManager.GameLanguage language;

    [Serializable]
    public struct FontData
    {
        public FontType type;       // Hangi tür? (Header, Dialogue vs.)
        public TMP_FontAsset font;  // Font dosyasý

        [Header("Pixel Perfect Settings")]
        [Tooltip("Bu fontun '1x' boyutu nedir? (Örn: Latin için 16, Japonca için 24)")]
        public float basePixelSize;

        [Header("Offsets (Delta)")]
        [Tooltip("Bu dil seçildiðinde metin ne kadar kaysýn? (X, Y)")]
        public Vector2 positionOffset; // Inspector'daki konuma EKLENECEK deðer

        [Header("Spacing Adjustments")]
        [Tooltip("Karakter aralýðýna eklenecek fark")]
        public float characterSpacingOffset;

        [Tooltip("Kelime aralýðýna eklenecek fark")]
        public float wordSpacingOffset;

        [Tooltip("Satýr aralýðýna eklenecek fark")]
        public float lineSpacingOffset;
    }

    public List<FontData> fontSettings;

    // Helper: Ýstenen türdeki datayý bulur
    public FontData GetFontData(FontType type)
    {
        foreach (var mapping in fontSettings)
        {
            if (mapping.type == type)
                return mapping;
        }
        // Bulamazsa boþ döndür (Default deðerlerle)
        return new FontData { basePixelSize = 16f };
    }
}