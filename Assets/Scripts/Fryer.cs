using DG.Tweening;
using UnityEngine;

public class Fryer : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private Transform oilSurfaceTransform; // Hareket edecek Yað Objesi (Plane)
    [SerializeField] private MeshRenderer oilMeshRenderer;
    [SerializeField] private ParticleSystem bubbleParticles;
    [SerializeField] private ParticleSystem splashParticles;

    [Header("Turbulence Settings (Köpürme)")]
    [SerializeField] private float baseWeight = 0.2f;
    [SerializeField] private float weightPerBasket = 0.4f;
    [SerializeField] private float surgeAmount = 0.3f;
    [SerializeField] private float surgeDuration = 0.3f;
    [SerializeField] private float settleDuration = 0.5f;
    [SerializeField] private float minEmission = 5f;
    [SerializeField] private float maxEmission = 30f;

    [Header("Physics Settings (Seviye Yükselmesi)")]
    [SerializeField] private float risePerEmptyBasket = 0.0002f; // Boþ sepet ne kadar yükseltir?
    [SerializeField] private float risePerFullBasket = 0.0006f;  // Dolu sepet ne kadar yükseltir? (2 tanesi 0.0012 eder)
    [SerializeField] private float levelSurgeAmount = 0.0003f;   // Dalarken oluþan dalga yüksekliði (Taþma efekti)

    // Logic Variables
    private Material oilMat;
    private int fullBasketsCount = 0;  // Dolu sepet sayýsý
    private int emptyBasketsCount = 0; // Boþ sepet sayýsý

    private float currentWeight;       // Shader deðiþkeni
    private float initialLocalZ;       // Yaðýn baþlangýç yüksekliði

    // Tween References (Çakýþma önlemek için)
    private Tween turbulenceTween;
    private Tween levelTween;

    private void Awake()
    {
        if (oilMeshRenderer != null)
        {
            oilMat = oilMeshRenderer.material;
        }

        // Baþlangýç yüksekliðini kaydet (Referans noktasý)
        if (oilSurfaceTransform != null)
            initialLocalZ = oilSurfaceTransform.localPosition.z;
        else
            Debug.LogError("Fryer: Oil Surface Transform atanmadý! Yað yükselmeyecek.");

        currentWeight = baseWeight;
        UpdateShaderAndParticles();
    }

    private void Update()
    {
        UpdateShaderAndParticles();
    }

    // --- BASKET TARAFINDAN ÇAÐRILANLAR ---

    public void OnBasketDown(bool hasFood)
    {
        if (splashParticles != null) splashParticles.Play();

        // 1. Sayaçlarý Güncelle
        if (hasFood) fullBasketsCount++;
        else emptyBasketsCount++;

        // 2. Efektleri Tetikle (Surge = true, yani dalgalanma yap)
        AnimateTurbulence(hasFood); // Sadece yemek varsa köpürür
        AnimateOilLevel(true);      // Her türlü seviye yükselir (Dalga yaparak)
    }

    public void OnBasketUp(bool hasFood)
    {
        // 1. Sayaçlarý Güncelle
        if (hasFood) fullBasketsCount--;
        else emptyBasketsCount--;

        // Güvenlik (Eksiye düþmesin)
        if (fullBasketsCount < 0) fullBasketsCount = 0;
        if (emptyBasketsCount < 0) emptyBasketsCount = 0;

        // 2. Efektleri Tetikle (Surge = false, sakin dönüþ)
        AnimateTurbulence(hasFood);
        AnimateOilLevel(false);
    }

    // --- ANÝMASYON MANTIÐI ---

    // 1. KÖPÜRME / TURBULENCE (Eski Mantýk)
    private void AnimateTurbulence(bool triggerSurge)
    {
        // Sadece dolu sepetler köpürtür
        float targetVal = baseWeight + (fullBasketsCount * weightPerBasket);
        targetVal = Mathf.Clamp(targetVal, 0f, 1.5f);

        if (turbulenceTween != null && turbulenceTween.IsActive()) turbulenceTween.Kill();

        if (triggerSurge && fullBasketsCount > 0) // Dolu sepet girdiyse coþ
        {
            float surgeTarget = targetVal + surgeAmount;
            Sequence seq = DOTween.Sequence();
            seq.Append(DOTween.To(() => currentWeight, x => currentWeight = x, surgeTarget, surgeDuration).SetEase(Ease.OutCirc));
            seq.Append(DOTween.To(() => currentWeight, x => currentWeight = x, targetVal, settleDuration).SetEase(Ease.OutQuad));
            turbulenceTween = seq;
        }
        else // Sakin geçiþ (Boþ sepet girdiyse veya sepet çýktýysa)
        {
            turbulenceTween = DOTween.To(() => currentWeight, x => currentWeight = x, targetVal, settleDuration).SetEase(Ease.OutQuad);
        }
    }

    // 2. SEVÝYE YÜKSELMESÝ / LEVEL RISE (Yeni Fizik)
    private void AnimateOilLevel(bool isEntering)
    {
        if (oilSurfaceTransform == null) return;

        // Hedef Yükseklik Hesabý:
        // Baþlangýç + (Boþlar * Birim) + (Dolular * Birim)
        float totalRise = (emptyBasketsCount * risePerEmptyBasket) + (fullBasketsCount * risePerFullBasket);
        float targetZ = initialLocalZ + totalRise;

        // Eski hareketi öldür
        if (levelTween != null && levelTween.IsActive()) levelTween.Kill();

        if (isEntering)
        {
            // --- DALMA EFEKTÝ (DISPLACEMENT WAVE) ---
            // Sepet suya girdiðinde önce suyu hedef seviyenin de üstüne iter (Taþma hissi),
            // sonra dengeye oturur.
            float surgeZ = targetZ + levelSurgeAmount;

            Sequence seq = DOTween.Sequence();
            // Hýzla yüksel (Su itiliyor)
            seq.Append(oilSurfaceTransform.DOLocalMoveZ(surgeZ, surgeDuration).SetEase(Ease.OutBack));
            // Hedefe otur
            seq.Append(oilSurfaceTransform.DOLocalMoveZ(targetZ, settleDuration).SetEase(Ease.InOutSine));

            levelTween = seq;
        }
        else
        {
            // --- ÇIKMA EFEKTÝ ---
            // Sepet çýkýnca suyun boþluðu doldurmasý biraz daha yavaþtýr.
            // "InQuad" ile yavaþça baþlar, hýzla iner (Vakum etkisi gibi deðil, süzülme).
            levelTween = oilSurfaceTransform.DOLocalMoveZ(targetZ, settleDuration * 1.2f).SetEase(Ease.OutQuad);
        }
    }

    private void UpdateShaderAndParticles()
    {
        if (oilMat == null) return;

        oilMat.SetFloat("_EffectWeight", currentWeight);

        if (bubbleParticles != null)
        {
            var emission = bubbleParticles.emission;
            // Baloncuk sadece yemek varsa coþsun
            float t = Mathf.InverseLerp(baseWeight, baseWeight + (2 * weightPerBasket), currentWeight);
            emission.rateOverTime = Mathf.Lerp(minEmission, maxEmission, t);
        }
    }
}