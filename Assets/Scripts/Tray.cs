using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tray : MonoBehaviour
{
    [SerializeField] private float startPointYHeight = 0.01f;
    [SerializeField] private float boxClosingSquashMinLimit = 0.16f;
    [SerializeField] private Transform burgerBoxTransform;
    [SerializeField] private Transform ingredientsParent;
    [SerializeField] private BoxCollider boxCollider;
    [SerializeField] private BoxCollider sauceCollider;

    [Header("Holograms")]
    [SerializeField] private GameObject onion;
    [SerializeField] private GameObject lettuce;
    [SerializeField] private GameObject tomato;
    [SerializeField] private GameObject pickle;
    [SerializeField] private GameObject patty;
    [SerializeField] private GameObject cheese;
    [SerializeField] private GameObject bottomBun;
    [SerializeField] private GameObject topBun;
    [SerializeField] private GameObject box;
    [SerializeField] private GameObject sauce;

    [Header("Sauces")]
    [SerializeField] private GameObject[] sauces; //0 ketchup, 1 mayo, 2 mustard, 3 bbq
    public AudioClip sauceOnTraySound;
    public float sauceOnTrayVolume = 1f;
    public float sauceOnTrayMinPitch = 0.8f;
    public float sauceOnTrayMaxPitch = 1.2f;

    private Vector3 currentLocationToPutBurgerIngredient;
    [HideInInspector] public Quaternion currentRotationToPutBurgerIngredient;
    private Vector3 hologramLocation;

    [HideInInspector] public List<BurgerIngredient> allBurgerIngredients = new List<BurgerIngredient>();
    private List<SauceBottle.SauceType> allSauces = new List<SauceBottle.SauceType>();
    private List<GameObject> allGO = new List<GameObject>();

    private float boxColliderStartZ;
    private float boxColliderStartCenterZ;

    private float sauceColliderStartZ;
    private float sauceColliderStartCenterZ;

    private bool burgerIsDone;

    [HideInInspector] public BurgerIngredient currentIngredient;
    [HideInInspector] public BurgerBox currentBox;

    private int onTrayLayer;
    private int grabableLayer;

    private void Awake()
    {
        currentIngredient = null;
        currentBox = null;

        boxColliderStartZ = boxCollider.size.z;
        boxColliderStartCenterZ = boxCollider.center.z;

        sauceColliderStartZ = sauceCollider.size.z;
        sauceColliderStartCenterZ = sauceCollider.center.z;

        burgerIsDone = false;

        ResetPosition();

        onTrayLayer = LayerMask.NameToLayer("OnTray");
        grabableLayer = LayerMask.NameToLayer("Grabable");

        GameManager.Instance.tray = this;
    }

    private void UpdateCurrentLocationToPutBurgerIngredient(float heightIncreaseAmount)
    {
        currentLocationToPutBurgerIngredient.y += heightIncreaseAmount;

        Vector3 newSize = boxCollider.size;
        newSize.z += heightIncreaseAmount/12;
        Vector3 newCenter = boxCollider.center;
        newCenter.z += heightIncreaseAmount / 24f;

        boxCollider.size = newSize;
        boxCollider.center = newCenter;


        newSize = sauceCollider.size;
        newSize.z += heightIncreaseAmount / 12;
        newCenter = sauceCollider.center;
        newCenter.z += heightIncreaseAmount / 24f;

        sauceCollider.size = newSize;
        sauceCollider.center = newCenter;
    }

    public void AddSauce(SauceBottle.SauceType type)
    {
        if (!allSauces.Contains(type) && !burgerIsDone)
        {
            GameObject go = Instantiate(type == SauceBottle.SauceType.Ketchup ? sauces[0] :
                        type == SauceBottle.SauceType.Mayo ? sauces[1] :
                        type == SauceBottle.SauceType.Mustard ? sauces[2] : sauces[3], sauce.transform.position, sauce.transform.rotation, transform);

            UpdateCurrentLocationToPutBurgerIngredient(2 * 0.004f);

            go.transform.parent = ingredientsParent;

            if (allBurgerIngredients.Count > 0)
                allBurgerIngredients[allBurgerIngredients.Count - 1].SetOnTrayLayer();

            allBurgerIngredients.Add(go.GetComponent<BurgerIngredient>());
            allSauces.Add(type);
            allGO.Add(go);

            SoundManager.Instance.PlaySoundFX(sauceOnTraySound, go.transform, sauceOnTrayVolume, sauceOnTrayMinPitch, sauceOnTrayMaxPitch);

            TurnOffAllHolograms();

            // Aslýnda AddSauce sonrasý hologramý kapalý tutmak daha mantýklý olabilir
            // çünkü sos sýkarken hologramýn içinden geçmesi garip durabilir.
            // Ama "Elimdeki malzeme hala geçerli mi?" diye baktýrmak istersen:
            RefreshHologram();
        }
            
    }

    public void RemoveSauce()
    {
        UpdateCurrentLocationToPutBurgerIngredient(-2 * 0.004f);
        allBurgerIngredients.RemoveAt(allBurgerIngredients.Count - 1);
        allSauces.RemoveAt(allSauces.Count - 1);
        allGO.RemoveAt(allGO.Count - 1);

        if (allBurgerIngredients.Count > 0)
            allBurgerIngredients[allBurgerIngredients.Count - 1].SetOnGrabableLayer();

        // Yükseklik düþtü, hologramý güncelle
        RefreshHologram();
    }

    public void ResetTray()
    {
        foreach (GameObject go in allGO)
        {
            Destroy(go);
        }

        foreach (BurgerIngredient burgerIngredient in allBurgerIngredients)
        {
            if (!burgerIngredient.data.isSauce)
                currentBox.allBurgerIngredientTypes.Add(burgerIngredient.data.ingredientType);
        }

        foreach(SauceBottle.SauceType sauceType in allSauces)
        {
            currentBox.allSauces.Add(sauceType);
        }

        allBurgerIngredients.Clear();
        allSauces.Clear();
        allGO.Clear();

        burgerIsDone = false;
        currentBox = null;
        currentIngredient = null;

        ResetPosition();
    }

    public void TurnOnSauceHologram(SauceBottle.SauceType type)
    {
        TurnOffAllHolograms();

        if (allBurgerIngredients.Count != 0 && !allSauces.Contains(type) && !burgerIsDone)
        {
            hologramLocation = currentLocationToPutBurgerIngredient;
            hologramLocation.y += 0.004f;      

            sauce.transform.position = hologramLocation;
            sauce.transform.rotation = Quaternion.Euler(90, Random.Range(0, 360), 0);

            if (currentIngredient != null && currentIngredient.data.isSauce)
            {
                currentIngredient.canAddToTray = true;

                currentRotationToPutBurgerIngredient = sauce.transform.rotation;
            }

            sauce.SetActive(true);
        }
        else
        {
            if (currentIngredient != null && currentIngredient.data.isSauce)
            {
                currentIngredient.canAddToTray = false;
            }
        }
            
    }

    public void TurnOnHologram(BurgerIngredientData.IngredientType type)
    {
        TurnOffAllHolograms();

        hologramLocation = currentLocationToPutBurgerIngredient;
        hologramLocation.y += currentIngredient.data.yHeight;

        currentRotationToPutBurgerIngredient = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        if (allBurgerIngredients.Count == 0)
        {
            if (type == BurgerIngredientData.IngredientType.BOTTOMBUN && currentIngredient.cookAmount == Cookable.CookAmount.REGULAR)
            {
                currentIngredient.canAddToTray = true;
                bottomBun.transform.position = hologramLocation;
                bottomBun.transform.rotation = currentRotationToPutBurgerIngredient;
                bottomBun.SetActive(true);
            }
            else
                currentIngredient.canAddToTray = false;
        }
        else if (!burgerIsDone)
        {
            if (type == BurgerIngredientData.IngredientType.PICKLE)
            {
                currentIngredient.canAddToTray = true;
                pickle.transform.position = hologramLocation;
                pickle.transform.rotation = currentRotationToPutBurgerIngredient;
                pickle.SetActive(true);
            }
            else if (type == BurgerIngredientData.IngredientType.LETTUCE)
            {
                currentIngredient.canAddToTray = true;
                lettuce.transform.position = hologramLocation;
                lettuce.transform.rotation = currentRotationToPutBurgerIngredient;
                lettuce.SetActive(true);
            }
            else if (type == BurgerIngredientData.IngredientType.ONION)
            {
                currentIngredient.canAddToTray = true;
                onion.transform.position = hologramLocation;
                onion.transform.rotation = currentRotationToPutBurgerIngredient;
                onion.SetActive(true);
            }
            else if (type == BurgerIngredientData.IngredientType.TOMATO)
            {
                currentIngredient.canAddToTray = true;
                tomato.transform.position = hologramLocation;
                tomato.transform.rotation = currentRotationToPutBurgerIngredient;
                tomato.SetActive(true);
            }
            else if (type == BurgerIngredientData.IngredientType.CHEESE)
            {
                currentIngredient.canAddToTray = true;
                cheese.transform.position = hologramLocation;
                cheese.transform.rotation = currentRotationToPutBurgerIngredient;
                cheese.SetActive(true);
            }
            else if (type == BurgerIngredientData.IngredientType.PATTY && currentIngredient.cookAmount == Cookable.CookAmount.REGULAR)
            {
                currentIngredient.canAddToTray = true;
                patty.transform.position = hologramLocation;
                patty.transform.rotation = currentRotationToPutBurgerIngredient;
                patty.SetActive(true);
            }
            else if (type == BurgerIngredientData.IngredientType.BOTTOMBUN && currentIngredient.cookAmount == Cookable.CookAmount.REGULAR)
            {
                currentIngredient.canAddToTray = true;
                bottomBun.transform.position = hologramLocation;
                bottomBun.transform.rotation = currentRotationToPutBurgerIngredient;
                bottomBun.SetActive(true);
            }
            else if (type == BurgerIngredientData.IngredientType.TOPBUN && currentIngredient.cookAmount == Cookable.CookAmount.REGULAR)
            {
                currentIngredient.canAddToTray = true;
                topBun.transform.position = hologramLocation;
                topBun.transform.rotation = currentRotationToPutBurgerIngredient;
                topBun.SetActive(true);
            }
        }
        else
        {
            currentIngredient.canAddToTray = false;
        }
    }

    public void TurnOnBoxHologram()
    {
        if (burgerIsDone)
        {
            currentBox.canAddToTray = true;
            box.transform.position = burgerBoxTransform.position;
            box.SetActive(true);
        }
    }

    public void TurnOffAllHolograms()
    {
        onion.SetActive(false);
        lettuce.SetActive(false);
        tomato.SetActive(false);
        pickle.SetActive(false);
        patty.SetActive(false);
        cheese.SetActive(false);
        bottomBun.SetActive(false);
        topBun.SetActive(false);
        box.SetActive(false);
        sauce.SetActive(false);
    }

    // Tepside durum deðiþince (yeni malzeme gelince) hologramý güncellemek için
    public void RefreshHologram()
    {
        // 1. Elimde bir þey yoksa hologramý kapat
        if (currentIngredient == null)
        {
            TurnOffAllHolograms();
            return;
        }

        // 2. Önce bütün hologramlarý bir temizle (Pozisyon kaymasý olmasýn diye)
        TurnOffAllHolograms();

        // 3. Elimdeki Sos mu Normal Malzeme mi?
        if (currentIngredient.data.isSauce)
        {
            TurnOnSauceHologram(currentIngredient.data.sauceType);
        }
        else
        {
            TurnOnHologram(currentIngredient.data.ingredientType);
        }
    }

    private void ResetPosition()
    {
        currentLocationToPutBurgerIngredient = transform.position;

        Vector3 newSize = boxCollider.size;
        newSize.z = boxColliderStartZ;
        Vector3 newCenter = boxCollider.center;
        newCenter.z = boxColliderStartCenterZ;

        boxCollider.size = newSize;
        boxCollider.center = newCenter;


        newSize = sauceCollider.size;
        newSize.z += sauceColliderStartZ;
        newCenter = sauceCollider.center;
        newCenter.z += sauceColliderStartCenterZ;

        sauceCollider.size = newSize;
        sauceCollider.center = newCenter;

        sauceCollider.enabled = false;

        UpdateCurrentLocationToPutBurgerIngredient(startPointYHeight);
    }

    public void TrySquashingBurger()
    {
        // Eðer yükseklik min limitten küçükse squash yok
        if (currentLocationToPutBurgerIngredient.y <= boxClosingSquashMinLimit)
            return;

        // Ne kadar squash yapýlacaðýný hesapla
        float excessHeight = (currentLocationToPutBurgerIngredient.y - boxClosingSquashMinLimit) * 3f;

        // Ýstediðin oranda Z scale küçült
        float squashFactor = Mathf.Clamp01(excessHeight); // istersen çarpan ekleyebilirsin
        float targetZ = Mathf.Max(0f, 1f - squashFactor); // Z scale küçülür ama negatif olmaz

        // Tween ile squash (sadece Z ekseni)
        ingredientsParent
            .DOScale(new Vector3(ingredientsParent.localScale.x, ingredientsParent.localScale.y, targetZ), 0.16f)
            .SetEase(Ease.Linear); // lineer, geri yaylanma yok
    }

    public void RemoveIngredient()
    {
        UpdateCurrentLocationToPutBurgerIngredient(-2 * allBurgerIngredients[allBurgerIngredients.Count - 1].data.yHeight);
        allBurgerIngredients.RemoveAt(allBurgerIngredients.Count - 1);
        allGO.RemoveAt(allGO.Count - 1);

        if (allBurgerIngredients.Count > 0)
            allBurgerIngredients[allBurgerIngredients.Count - 1].SetOnGrabableLayer();
        else
            sauceCollider.enabled = false;

            burgerIsDone = false;

        // Yükseklik düþtü, hologramý güncelle
        RefreshHologram();
    }

    private void Squash()
    {
        // currentIngredient baðýmlýlýðýný tamamen kaldýrdýk.
        // Artýk sabit bir "ezilme" deðeri kullanýyoruz. 
        // X ve Y hafif geniþlerken (1.1), Z hafifçe basýlýyor (0.9).

        ingredientsParent
            .DOScale(new Vector3(1.1f, 1.1f, 0.9f), 0.2f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                // Geri eski haline (1,1,1) dönerken standart elastiklik kullanýyoruz.
                ingredientsParent.DOScale(Vector3.one, 0.3f)
                    .SetEase(Ease.OutElastic);
            });
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1. Layer Kontrolü
        if (other.gameObject.layer == onTrayLayer) return;

        // 2. Etiket Kontrolü
        if (other.CompareTag("BurgerIngredient"))
        {
            // Çarpan objenin scriptini al
            BurgerIngredient ingredient = other.GetComponent<BurgerIngredient>();

            // Eðer script yoksa veya tepsiye eklenmeye uygun deðilse (canAddToTray)
            // NOT: canAddToTray'i aþaðýda tekrar hesaplayacaðýz çünkü fýrlatýnca bozulmuþ olabilir.
            if (ingredient == null) return;

            // --- YENÝ KONTROL ---
            // Hologram sistemi "currentIngredient" üzerinden "canAddToTray"i set ediyordu.
            // Ama fýrlatýlan obje artýk "current" deðil.
            // O yüzden buraya manuel bir "Uygunluk Kontrolü" (Validation) eklememiz lazým.

            if (allBurgerIngredients.Contains(ingredient)) return;

            bool isCompatible = CheckIfIngredientIsCompatible(ingredient);

            if (isCompatible)
            {
                // Burger Bitti mi?
                if (ingredient.data.ingredientType == BurgerIngredientData.IngredientType.TOPBUN)
                    burgerIsDone = true;

                // Sos Collider'ý aç
                if (!sauceCollider.enabled)
                    sauceCollider.enabled = true;

                // Önceki malzemeyi sabitle
                if (allBurgerIngredients.Count > 0)
                    allBurgerIngredients[allBurgerIngredients.Count - 1].SetOnTrayLayer();

                // Yeni malzeme geldi! Hemen eski animasyonu durdur ve tepsiyi düzelt.
                // Böylece yeni gelen malzeme "yamuk/ezik" bir transform'a parent olmaz.
                ingredientsParent.DOKill();
                ingredientsParent.localScale = Vector3.one;

                // Listelere ekle
                if (ingredient.data.isSauce)
                    allSauces.Add(ingredient.data.sauceType);

                allBurgerIngredients.Add(ingredient);
                allGO.Add(ingredient.gameObject);

                // Yüksekliði güncelle
                UpdateCurrentLocationToPutBurgerIngredient(ingredient.data.yHeight);

                Quaternion targetRotation;

                if (ingredient.data.isSauce)
                {
                    // Soslar için özel açý (Hologram kodundaki gibi 90 derece)
                    targetRotation = Quaternion.Euler(90f, Random.Range(0f, 360f), 0f);
                }
                else
                {
                    // Normal malzemeler için standart açý (0 derece)
                    targetRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                }

                // Fiziksel yerleþtirme
                ingredient.PutOnTray(currentLocationToPutBurgerIngredient, ingredient.storedRotationForTray, ingredientsParent);

                // Efektler
                Invoke("Squash", ingredient.data.timeToPutOnTray / 1.2f);

                // Sonraki malzeme için yüksekliði ayarla
                UpdateCurrentLocationToPutBurgerIngredient(ingredient.data.yHeight);

                // Malzeme eklendi, yükseklik arttý.
                // Þimdi elindeki eþya için hologramý yeni duruma göre tekrar yak!
                RefreshHologram();
            }
        }
        else if (other.CompareTag("BurgerBox"))
        {
            // Kutu mantýðý genelde sadece elimizdeyken çalýþýr (fýrlatýlan kutu tepsiye girmez genelde)
            // Ama yine de "currentBox" kontrolünü kaldýrýp direkt componente bakabilirsin.
            BurgerBox boxComponent = other.GetComponent<BurgerBox>();

            if (boxComponent != null && boxComponent == currentBox && boxComponent.canAddToTray)
            {
                if (allBurgerIngredients.Count > 0)
                    allBurgerIngredients[allBurgerIngredients.Count - 1].SetOnTrayLayer();

                currentBox.PutOnTray(burgerBoxTransform.position);
            }
        }
    }

    // Bu fonksiyon, hologramýn yaptýðý iþi manuel yapar.
    // Tepsiye çarpan malzeme, þu anki sýraya uygun mu?
    private bool CheckIfIngredientIsCompatible(BurgerIngredient ingredient)
    {
        // 1. Eðer burger hiç baþlamadýysa (Boþ tepsi)
        if (allBurgerIngredients.Count == 0)
        {
            // Sadece BOTTOM BUN (Alt Ekmek) kabul et ve piþmiþ olmalý
            return ingredient.data.ingredientType == BurgerIngredientData.IngredientType.BOTTOMBUN &&
                   ingredient.cookAmount == Cookable.CookAmount.REGULAR;
        }

        // 2. Burger bitmiþse (burgerIsDone) -> Hiçbir þey kabul etme
        if (burgerIsDone) return false;

        // 3. Sýradaki malzemeler
        // Buradaki kurallar TurnOnHologram fonksiyonundaki kurallarla AYNI olmalý.
        BurgerIngredientData.IngredientType type = ingredient.data.ingredientType;

        switch (type)
        {
            case BurgerIngredientData.IngredientType.PICKLE:
            case BurgerIngredientData.IngredientType.LETTUCE:
            case BurgerIngredientData.IngredientType.ONION:
            case BurgerIngredientData.IngredientType.TOMATO:
            case BurgerIngredientData.IngredientType.CHEESE:
                return true; // Bunlar her zaman eklenebilir

            case BurgerIngredientData.IngredientType.PATTY:
                // Köfte sadece piþmiþse
                return ingredient.cookAmount == Cookable.CookAmount.REGULAR;

            case BurgerIngredientData.IngredientType.BOTTOMBUN:
                // Ýkinci bir alt ekmek? (Big Mac tarzý). Þimdilik izin verelim ama piþmiþ olmalý.
                return ingredient.cookAmount == Cookable.CookAmount.REGULAR;

            case BurgerIngredientData.IngredientType.TOPBUN:
                // Üst ekmek sadece piþmiþse ve burgeri bitirir.
                return ingredient.cookAmount == Cookable.CookAmount.REGULAR;

            default:
                return false;
        }
    }
}
