using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Renderer))]
public class LocalizedMaterial : MonoBehaviour
{
    [System.Serializable]
    public struct LanguageMaterial
    {
        public LocalizationManager.GameLanguage language;
        public Material material;
    }

    [Header("Default Settings")]
    [Tooltip("Varsayýlan materyal (Genelde Ýngilizce). Dil listede yoksa bu kullanýlýr.")]
    public Material defaultMaterial;

    [Header("Language Specifics")]
    [Tooltip("Sadece özel materyal gerektiren dilleri buraya ekle (Örn: TR, ZH, JA).")]
    public List<LanguageMaterial> specializedMaterials;

    // MeshRenderer, SkinnedMeshRenderer vb. hepsini kapsar
    private Renderer _renderer;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
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
            // Açýlýþta hemen tetikle ki doðru baþlasýn
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
        if (_renderer == null || LocalizationManager.Instance == null) return;

        // 1. Mevcut dili öðren
        var currentLang = LocalizationManager.Instance.currentLanguage;

        // 2. Varsayýlaný hedef olarak belirle
        Material targetMat = defaultMaterial;

        // 3. Listede bu dil için özel bir materyal var mý?
        foreach (var mapping in specializedMaterials)
        {
            if (mapping.language == currentLang)
            {
                targetMat = mapping.material;
                break;
            }
        }

        // 4. Materyali Ata
        if (targetMat != null)
        {
            // ÖNEMLÝ NOT: 
            // .material kullanýrsan Unity hafýzada o materyalin kopyasýný (Instance) oluþturur.
            // .sharedMaterial kullanýrsan direkt asset'i kullanýr.
            // Dil deðiþimi gibi toplu ve statik iþlerde 'sharedMaterial' daha performanslýdýr ve memory leak önler.

            // Eðer objenin birden fazla materyali varsa (Element 0, Element 1...)
            // Bu kod sadece ÝLK materyali (Element 0) deðiþtirir.
            // Genelde posterler tek materyalli olduðu için bu yeterlidir.

            if (_renderer.sharedMaterial != targetMat)
            {
                _renderer.sharedMaterial = targetMat;
            }
        }
    }
}