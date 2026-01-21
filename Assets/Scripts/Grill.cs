using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Grill : MonoBehaviour
{
    [Header("Slot System")]
    [SerializeField] private List<Transform> cookingSlots;
    private BurgerIngredient[] occupiedSlots;
    [SerializeField] private float positionRandomness = 0.0004f;

    [Header("Audio Settings (Advanced)")]
    private AudioSource grillAudioSource;
    [SerializeField] private float volumePerPatty = 0.3f;
    [SerializeField] private float volumePerBun = 0.15f; // Ekmek daha az ses çýkarsýn
    [SerializeField] private float maxVolume = 1.0f;
    [SerializeField] private float fadeTime = 2.0f; // Ses geçiþ süresi

    // Audio Logic Variables
    private int cookingPattyCount = 0;
    private int cookingBunCount = 0;
    private float targetVolume = 0f;
    private Coroutine audioFadeCoroutine;

    [Header("Smoke Visuals")]
    [SerializeField] private ParticleSystem smokeParticles;
    [SerializeField] private float minSmokeEmission = 5f;
    [SerializeField] private float maxSmokeEmission = 30f;

    [Header("Animation Settings")]
    [SerializeField] private float placementApexHeight = 0.2f; // Tepeden inme yüksekliði (User's Z offset request)
    [SerializeField] private float placementDuration = 0.5f;   // Yerleþme süresi (Biraz daha yavaþ ve tok)
    [SerializeField] private Ease placementEase = Ease.OutSine; // Yumuþak bitiþ

    private void Awake()
    {
        grillAudioSource = GetComponent<AudioSource>();

        if (cookingSlots != null)
            occupiedSlots = new BurgerIngredient[cookingSlots.Count];

        if (grillAudioSource != null)
        {
            grillAudioSource.loop = true;
            grillAudioSource.volume = 0f; // Baþlangýçta ses yok
            grillAudioSource.Play();
        }
    }

    private void Update()
    {
        // --- PÝÞÝRME DÖNGÜSÜ ---
        int currentlyCookingCount = 0; // Duman için sayýyoruz

        for (int i = 0; i < occupiedSlots.Length; i++)
        {
            BurgerIngredient item = occupiedSlots[i];

            if (item != null)
            {
                // Item'ýn piþme metodunu çalýþtýr (Time.deltaTime gönderiyoruz)
                // Metot geriye "Hala piþiyor mu?" (Yandý mý?) döner.
                bool isStillCooking = item.ProcessCooking(Time.deltaTime);

                if (isStillCooking)
                {
                    currentlyCookingCount++;
                }
            }
        }

        HandleSmoke(currentlyCookingCount);
    }

    // --- TRIGGER TARAFINDAN ÇAÐRILACAK ---
    public bool AttemptAddItem(Collider other)
    {
        BurgerIngredient item = other.GetComponent<BurgerIngredient>();

        if (item == null) return false;

        // 1. KONTROL: Veri uygunluðu
        if (!item.data.isCookable) return false;

        if (item.IsGrabbed) return false;

        // 2. KONTROL: Sahiplik Durumu (Zaten bir yerde mi?)
        if (item.currentGrill != null) return false; // Baþka ýzgarada mý?
        // item.currentBasket check'i yapmýyoruz çünkü class farklý ama 
        // genel mantýkta "OnTray" layerýndaysa iþlem yapma diyebiliriz:
        if (item.gameObject.layer == LayerMask.NameToLayer("OnTray")) return false;

        // 3. KONTROL: Boþ slot var mý?
        int emptyIndex = GetFirstEmptySlotIndex();
        if (emptyIndex == -1) return false;

        // Slotu rezerve et
        occupiedSlots[emptyIndex] = item;
        item.currentGrill = this;

        // Görsel yerleþim ve SES
        MoveItemToSlot(item, emptyIndex);

        // Izgara genel sesi (Loop) güncelle
        UpdateAudioCounts(item.data.ingredientType, true);

        return true;
    }

    // --- BURGER INGREDIENT TARAFINDAN ÇAÐRILACAK (OnGrab) ---
    public void RemoveItem(BurgerIngredient item)
    {
        int index = GetSlotIndexForItem(item);
        if (index != -1)
        {
            occupiedSlots[index] = null;
            item.currentGrill = null;

            // SES: Listeden çýkan elemanýn türüne göre sayacý azalt
            UpdateAudioCounts(item.data.ingredientType, false);
        }
    }

    // --- SES YÖNETÝMÝ (SoundManager Mantýðý) ---
    private void UpdateAudioCounts(BurgerIngredientData.IngredientType type, bool isAdding)
    {
        int change = isAdding ? 1 : -1;

        if (type == BurgerIngredientData.IngredientType.PATTY)
            cookingPattyCount += change;
        else if (type == BurgerIngredientData.IngredientType.BOTTOMBUN || type == BurgerIngredientData.IngredientType.TOPBUN)
            cookingBunCount += change;

        // Güvenlik (Negatife düþmesin)
        if (cookingPattyCount < 0) cookingPattyCount = 0;
        if (cookingBunCount < 0) cookingBunCount = 0;

        RecalculateTargetVolume();
    }

    private void RecalculateTargetVolume()
    {
        // Hedef sesi hesapla
        float rawVolume = (cookingPattyCount * volumePerPatty) + (cookingBunCount * volumePerBun);
        targetVolume = Mathf.Clamp(rawVolume, 0f, maxVolume);

        // Coroutine yönetimi (Çakýþmayý önle)
        if (audioFadeCoroutine != null) StopCoroutine(audioFadeCoroutine);
        audioFadeCoroutine = StartCoroutine(LerpAudioVolume());
    }

    private IEnumerator LerpAudioVolume()
    {
        if (grillAudioSource == null) yield break;

        float startVol = grillAudioSource.volume;
        float time = 0f;

        while (time < fadeTime)
        {
            time += Time.deltaTime;
            grillAudioSource.volume = Mathf.Lerp(startVol, targetVolume, time / fadeTime);
            yield return null;
        }

        grillAudioSource.volume = targetVolume;
    }

    // --- HELPER METHODS ---
    private void HandleSmoke(int cookingCount)
    {
        if (smokeParticles == null) return;

        var emission = smokeParticles.emission;

        if (cookingCount > 0)
        {
            if (!smokeParticles.isPlaying) smokeParticles.Play();
            float ratio = (float)cookingCount / cookingSlots.Count;
            emission.rateOverTime = Mathf.Lerp(minSmokeEmission, maxSmokeEmission, ratio);
        }
        else
        {
            smokeParticles.Stop();
        }
    }

    private void MoveItemToSlot(BurgerIngredient item, int slotIndex)
    {
        Transform targetSlot = cookingSlots[slotIndex];

        // 1. FÝZÝK KAPATMA
        Rigidbody rb = item.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
        }

        // 2. PARENTING
        item.transform.SetParent(targetSlot, true);
        item.SetOnTrayLayer();

        // 3. RANDOMIZE POZÝSYON (Doðallýk için sapma)
        // Belirlediðimiz 0.0004 aralýðýnda rastgele X ve Y üret
        float randomX = Random.Range(-positionRandomness, positionRandomness);
        float randomY = Random.Range(-positionRandomness, positionRandomness);
        Vector3 randomOffset = new Vector3(randomX, randomY, 0f);

        // 4. HEDEF VE APEX HESABI
        // Slotun merkezi (0,0,0) + Data Offset + RANDOM OFFSET
        Vector3 targetLocalPos = Vector3.zero + item.data.grillPositionOffset + randomOffset;
        Quaternion targetLocalRot = Quaternion.Euler(item.data.grillRotationOffset);

        // Z ekseni yukarý baktýðý için Apex'i forward yönünde kaldýrýyoruz
        Vector3 apexLocalPos = targetLocalPos + (Vector3.forward * placementApexHeight);

        // 5. SES (First Sizzle)
        if (item.data.cookingSound != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySoundFX(
                item.data.cookingSound,
                item.transform,
                item.data.cookingSoundVolume,
                item.data.cookingSoundMinPitch,
                item.data.cookingSoundMaxPitch
            );
        }

        // 6. ANÝMASYON (Hareket)
        Sequence seq = DOTween.Sequence();

        // Yay çizerek in (Apex -> Random Target)
        seq.Join(item.transform.DOLocalPath(new Vector3[] { apexLocalPos, targetLocalPos }, placementDuration, PathType.CatmullRom).SetEase(placementEase));

        // Dönerken in
        seq.Join(item.transform.DOLocalRotateQuaternion(targetLocalRot, placementDuration).SetEase(Ease.OutCubic));

        // 7. BÝTÝÞ VE SQUASH JUICE
        seq.OnComplete(() =>
        {
            // Kilidi aç
            if (item != null && item.currentGrill == this)
            {
                item.PlayPutOnSoundEffect();
                item.SetOnGrabableLayer();
            }

            // --- SQUASH EFFECT (SLOT ÜZERÝNDEN) ---
            targetSlot.DOKill(true);
            targetSlot.localScale = Vector3.one;

            // X ve Y geniþler, Z (Yükseklik) basýlýr
            targetSlot.DOScale(new Vector3(1.1f, 1.1f, 0.9f), 0.15f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    // Eski haline lastik gibi dön
                    targetSlot.DOScale(Vector3.one, 0.3f)
                        .SetEase(Ease.OutElastic);
                });
        });
    }

    private int GetFirstEmptySlotIndex()
    {
        for (int i = 0; i < occupiedSlots.Length; i++)
            if (occupiedSlots[i] == null) return i;
        return -1;
    }

    private int GetSlotIndexForItem(BurgerIngredient item)
    {
        for (int i = 0; i < occupiedSlots.Length; i++)
            if (occupiedSlots[i] == item) return i;
        return -1;
    }
}