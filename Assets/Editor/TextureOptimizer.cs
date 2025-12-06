using UnityEngine;
using UnityEditor; // Editor kütüphanesi şart

public class TextureOptimizer : EditorWindow
{
    // Üst menüye buton ekliyoruz
    [MenuItem("Tools/X Burger/Texture Optimizer 🛠️")]
    public static void ShowWindow()
    {
        GetWindow<TextureOptimizer>("Texture Optimizer");
    }

    // Ayarlar
    int maxTextureSizeCap = 512; // En büyük texture kaç olsun? (PSX için 512 ideal)
    bool setPointFilter = true;  // Point filter olsun mu?
    bool enableMipMaps = true;   // Mipmap açılsın mı?

    void OnGUI()
    {
        GUILayout.Label("PSX Doku Optimize Edici", EditorStyles.boldLabel);

        maxTextureSizeCap = EditorGUILayout.IntField("Max Size Limiti:", maxTextureSizeCap);
        setPointFilter = EditorGUILayout.Toggle("Filter: Point Yap", setPointFilter);
        enableMipMaps = EditorGUILayout.Toggle("Generate Mip Maps", enableMipMaps);

        GUILayout.Space(20);

        if (GUILayout.Button("SEÇİLİ Texture'ları Optimize Et"))
        {
            OptimizeSelectedTextures();
        }

        GUILayout.Label("UYARI: Bu işlem geri alınamaz!", EditorStyles.miniLabel);
    }

    void OptimizeSelectedTextures()
    {
        // Project panelinde seçili olan objeleri al
        Object[] textures = Selection.objects;

        int count = 0;

        foreach (Object obj in textures)
        {
            // Sadece Texture olanları işle
            if (obj is Texture2D)
            {
                string path = AssetDatabase.GetAssetPath(obj);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

                if (importer != null)
                {
                    // 1. Filter Mode Ayarı
                    if (setPointFilter)
                    {
                        importer.filterMode = FilterMode.Point;
                    }

                    // 2. Mipmap Ayarı
                    if (enableMipMaps)
                    {
                        importer.mipmapEnabled = true;
                        // Olique Texture sorun çıkarmasın diye Box veya Kaiser seçebilirsin
                        // importer.mipmapFilter = TextureImporterMipFilter.BoxFilter; 
                    }

                    // 3. Max Size Ayarı (Zeka burada)
                    // Texture'ın orijinal boyutunu alalım ama importer üzerinden almak zor.
                    // Unity importer max size ayarını "En yakın 2'nin kuvveti" olarak ayarlar.

                    // Şöyle bir mantık kuruyoruz: 
                    // Direkt olarak senin belirlediğin Cap'i (512) basıyoruz.
                    // Unity zaten texture 64x64 ise ve sen MaxSize 512 dersen, onu 512'ye büyütmez.
                    // 64 olarak bırakır. Sadece 512'den büyükse küçültür.

                    importer.maxTextureSize = maxTextureSizeCap;

                    // Ayarları kaydet
                    importer.SaveAndReimport();
                    count++;
                }
            }
        }

        Debug.Log($"Tamamlandı! {count} adet texture PSX formatına getirildi.");
    }
}