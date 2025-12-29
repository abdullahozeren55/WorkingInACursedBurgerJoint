using UnityEngine;

public class Holder : MonoBehaviour, IGrabable
{
    public enum HolderIngredient
    {
        Empty,
        Fries,
    }

    [Header("Visual References")]
    [SerializeField] private GameObject contentMeshObject; // Ýçindeki doluluk (Child obje)

    [Header("Data & IGrabable")]
    [SerializeField] private HolderData data; // Külahýn kendi datasý (Icon, tutuþ tipi vs.)
    
    // --- IGrabable Properties ---
    public bool IsGrabbed { get => isGrabbed; set => isGrabbed = value; }
    private bool isGrabbed;

    public Sprite Icon { get => data.icon; set => data.icon = value; }
    public PlayerManager.HandGrabTypes HandGrabType { get => data.handGrabType; set => data.handGrabType = value; }
    
    public bool OutlineShouldBeRed { get => outlineShouldBeRed; set => outlineShouldBeRed = value; }
    private bool outlineShouldBeRed;
    public bool OutlineShouldBeGreen { get => outlineShouldBeGreen; set => outlineShouldBeGreen = value; }
    private bool outlineShouldBeGreen;

    public bool IsThrowable { get => data.isThrowable; set => data.isThrowable = value; }
    public float ThrowMultiplier { get => data.throwMultiplier; set => data.throwMultiplier = value; }
    public bool IsUseable { get => data.isUseable; set => data.isUseable = value; }
    
    public Vector3 GrabPositionOffset { get => data.grabPositionOffset; set => data.grabPositionOffset = value; }
    public Vector3 GrabRotationOffset { get => data.grabRotationOffset; set => data.grabRotationOffset = value; }
    public string FocusTextKey { get => data.focusTextKeys[(int)currentIngredientType]; set => data.focusTextKeys[(int)currentIngredientType] = value; }

    private HolderIngredient currentIngredientType = HolderIngredient.Empty;

    // --- References ---
    private Rigidbody rb;
    private Collider col;
    private int grabableLayer;
    private int ungrabableLayer;
    private int grabbedLayer;
    private int grabableOutlinedLayer;
    private int interactableOutlinedRedLayer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        
        // Content baþlarken kapalý olsun (veya editördeki duruma göre)
        UpdateVisuals();

        grabableLayer = LayerMask.NameToLayer("Grabable");
        ungrabableLayer = LayerMask.NameToLayer("Ungrabable");
        grabbedLayer = LayerMask.NameToLayer("Grabbed");
        grabableOutlinedLayer = LayerMask.NameToLayer("GrabableOutlined");
        interactableOutlinedRedLayer = LayerMask.NameToLayer("InteractableOutlinedRed");
    }

    // --- ASIL OLAY: BÝRLEÞTÝRME MANTIÐI ---
    public bool TryCombine(IGrabable otherItem)
    {
        // 1. Zaten doluysak kimseyi alamayýz
        if (currentIngredientType != HolderIngredient.Empty) return false;

        // 2. Gelen þey bir "Fryable" (Kýzartmalýk) mý?
        // (Cast iþlemi: Eðer otherItem Fryable deðilse 'item' null olur)
        Fryable item = otherItem as Fryable;
        if (item == null) return false;

        //sadece REGULAR (Piþmiþ) kabul edelim.
        if (item.CurrentCookingState != Cookable.CookAmount.REGULAR) 
        {
            return false; // Çið ise alma
        }

        // 4. Türüne göre doldur
        if (item.data.type == FryableData.FryableType.Fries)
        {
            Fill(HolderIngredient.Fries, item);
            return true;
        }

        return false;
    }

    public bool CanCombine(IGrabable otherItem)
    {
        // 1. Doluysak olmaz
        if (currentIngredientType != HolderIngredient.Empty) return false;

        // 2. Fryable mý?
        Fryable item = otherItem as Fryable;
        if (item == null) return false;

        // 3. Piþmiþ mi?
        if (item.CurrentCookingState != Cookable.CookAmount.REGULAR) return false;

        // 4. Tür uyuyor mu? (Þimdilik sadece patates)
        if (item.data.type == FryableData.FryableType.Fries) return true;

        return false;
    }

    private void Fill(HolderIngredient newIngedient, Fryable sourceItem)
    {
        currentIngredientType = newIngedient;
        
        // Görseli aç
        UpdateVisuals();

        // Yerdeki malzemeyi yok et!
        // Not: Controller bu objeyi referans tutuyor olabilir, dikkatli yok etmek lazým.
        // Ama bool döndüðümüz için Controller onu "býrakacak".
        Destroy(sourceItem.gameObject);
        
        // Ses efekti eklenebilir: "Hýþýrt"
    }

    private void UpdateVisuals()
    {
        if (contentMeshObject != null)
        {
            contentMeshObject.SetActive(currentIngredientType != HolderIngredient.Empty);
        }
    }

    // --- IGrabable Standartlarý (Kopyala/Yapýþtýr) ---
    // (Burayý kýsa tutuyorum, Fryable ile ayný mantýk)
    
    public void OnGrab(Transform grabPoint)
    {
        IsGrabbed = true;
        rb.isKinematic = true;
        rb.useGravity = false;
        col.enabled = false; // Ele alýnca collider kapansýn
        
        transform.SetParent(grabPoint);
        transform.localPosition = data.grabLocalPositionOffset;
        transform.localRotation = Quaternion.Euler(data.grabLocalRotationOffset);
        
        ChangeLayer(grabbedLayer);
    }

    public void OnDrop(Vector3 direction, float force) => Release(direction, force);
    public void OnThrow(Vector3 direction, float force) => Release(direction, force);

    private void Release(Vector3 direction, float force)
    {
        IsGrabbed = false;
        transform.SetParent(null);
        rb.isKinematic = false;
        rb.useGravity = true;
        col.enabled = true;
        rb.AddForce(direction * force, ForceMode.Impulse);
        ChangeLayer(ungrabableLayer);
    }

    public void OnFocus() { ChangeLayer(OutlineShouldBeRed ? interactableOutlinedRedLayer : grabableOutlinedLayer); }
    public void OnLoseFocus() { ChangeLayer(grabableLayer); }
    
    public void OutlineChangeCheck()
    {
        if (gameObject.layer == grabableOutlinedLayer && OutlineShouldBeRed) ChangeLayer(interactableOutlinedRedLayer);
        else if (gameObject.layer == interactableOutlinedRedLayer && !OutlineShouldBeRed) ChangeLayer(grabableOutlinedLayer);
    }
    
    public void ChangeLayer(int layer)
    {
        gameObject.layer = layer;
        contentMeshObject.layer = layer;
    }
    public void OnHolster() { gameObject.SetActive(false); }
    public void OnUseHold() { }
    public void OnUseRelease() { }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (!IsGrabbed && gameObject.layer == ungrabableLayer) ChangeLayer(grabableLayer);
    }
}