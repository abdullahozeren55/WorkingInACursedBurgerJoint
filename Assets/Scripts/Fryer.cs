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
    private int totalFoodItemsCount = 0;
    private int emptyBasketsCount = 0; // Boþ sepet sayýsý

    private const int BASKET_CAPACITY = 3;

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

    // Artýk sadece "Dolu mu?" diye deðil, "Kaç tane var?" diye soruyoruz
    public void OnBasketDown(int itemCount)
    {
        if (splashParticles != null) splashParticles.Play();

        if (itemCount > 0) totalFoodItemsCount += itemCount;
        else emptyBasketsCount++;

        // itemCount ne kadar çoksa Surge (Dalgalanma) o kadar þiddetli olsun
        float surgeMultiplier = itemCount > 0 ? (float)itemCount / BASKET_CAPACITY : 0.5f; // Boþsa yarým þiddet

        AnimateTurbulence(true, surgeMultiplier);
        AnimateOilLevel(true, surgeMultiplier);
    }

    public void OnBasketUp(int itemCount)
    {
        if (itemCount > 0) totalFoodItemsCount -= itemCount;
        else emptyBasketsCount--;

        if (totalFoodItemsCount < 0) totalFoodItemsCount = 0;
        if (emptyBasketsCount < 0) emptyBasketsCount = 0;

        AnimateTurbulence(false, 0f); // Çýkarken surge yok
        AnimateOilLevel(false, 0f);
    }

    // --- ANÝMASYON MANTIÐI ---

    // 1. KÖPÜRME / TURBULENCE (Eski Mantýk)
    private void AnimateTurbulence(bool triggerSurge, float surgeIntensity)
    {
        // HESAP: (Toplam Malzeme / 3) * TamSepetAðýrlýðý
        // Yani 1 malzeme varsa 1/3 etki, 3 malzeme varsa tam etki.
        float addedWeight = ((float)totalFoodItemsCount / BASKET_CAPACITY) * weightPerBasket;
        float targetVal = baseWeight + addedWeight;

        targetVal = Mathf.Clamp(targetVal, 0f, 1.5f);

        if (turbulenceTween != null && turbulenceTween.IsActive()) turbulenceTween.Kill();

        if (triggerSurge)
        {
            // Surge miktarý da giren malzeme sayýsýna göre artsýn
            float dynamicSurge = surgeAmount * surgeIntensity;
            float surgeTarget = targetVal + dynamicSurge;

            Sequence seq = DOTween.Sequence();
            seq.Append(DOTween.To(() => currentWeight, x => currentWeight = x, surgeTarget, surgeDuration).SetEase(Ease.OutCirc));
            seq.Append(DOTween.To(() => currentWeight, x => currentWeight = x, targetVal, settleDuration).SetEase(Ease.OutQuad));
            turbulenceTween = seq;
        }
        else
        {
            turbulenceTween = DOTween.To(() => currentWeight, x => currentWeight = x, targetVal, settleDuration).SetEase(Ease.OutQuad);
        }
    }

    private void AnimateOilLevel(bool isEntering, float surgeIntensity)
    {
        if (oilSurfaceTransform == null) return;

        // HESAP: Her malzeme için "Tam Sepet Yükselmesi / 3" kadar yüksel
        float risePerItem = risePerFullBasket / BASKET_CAPACITY;

        float totalRise = (emptyBasketsCount * risePerEmptyBasket) + (totalFoodItemsCount * risePerItem);
        float targetZ = initialLocalZ + totalRise;

        if (levelTween != null && levelTween.IsActive()) levelTween.Kill();

        if (isEntering)
        {
            // Dalma efekti de malzeme sayýsýna göre þiddetlensin
            float dynamicSurge = levelSurgeAmount * (0.5f + (surgeIntensity * 0.5f)); // Min %50 surge olsun
            float surgeZ = targetZ + dynamicSurge;

            Sequence seq = DOTween.Sequence();
            seq.Append(oilSurfaceTransform.DOLocalMoveZ(surgeZ, surgeDuration).SetEase(Ease.OutBack));
            seq.Append(oilSurfaceTransform.DOLocalMoveZ(targetZ, settleDuration).SetEase(Ease.InOutSine));
            levelTween = seq;
        }
        else
        {
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

            // Mevcut aðýrlýða göre partikül sayýsýný oranla
            // baseWeight -> minEmission
            // baseWeight + (2 * weightPerBasket) -> maxEmission (2 sepet dolusu malzeme varsayýmýyla maxladým)

            float maxExpectedWeight = baseWeight + (2 * weightPerBasket);
            float t = Mathf.InverseLerp(baseWeight, maxExpectedWeight, currentWeight);

            emission.rateOverTime = Mathf.Lerp(minEmission, maxEmission, t);
        }
    }
}