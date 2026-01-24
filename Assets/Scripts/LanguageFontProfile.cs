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
        public FontType type;
        public TMP_FontAsset font;

        [Header("Pixel Perfect Settings")]
        [Tooltip("Bu fontun referans boyutu nedir? (Örn: Latin 16, Japonca 24)")]
        public float basePixelSize;

        // verticalOffset SÝLÝNDÝ. Artýk Font Asset -> Face Info ayarlarýndan yapýlacak.

        [Header("Spacing Adjustments")]
        [Tooltip("Karakterlerin arasýný açmak için (Japonca/Çince genelde ihtiyaç duyar)")]
        public float characterSpacingOffset;

        // Diðer spacing'leri de silebilirsin kullanmýyorsan, þimdilik býraktým.
        public float wordSpacingOffset;
        public float lineSpacingOffset;
    }

    public List<FontData> fontSettings;

    public FontData GetFontData(FontType type)
    {
        foreach (var mapping in fontSettings)
        {
            if (mapping.type == type)
                return mapping;
        }
        return new FontData { basePixelSize = 16f };
    }
}