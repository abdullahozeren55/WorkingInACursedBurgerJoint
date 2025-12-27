using DG.Tweening;
using UnityEngine;
using static FryableData;

public class Fryable : MonoBehaviour, IGrabable
{
    [Header("Data")]
    public FryableData data;

    [Header("Cooking State")]
    [SerializeField] private float currentCookingTime = 0f;
    [SerializeField] private Cookable.CookAmount state = Cookable.CookAmount.RAW;

    // --- IGrabable Properties ---
    public bool IsGrabbed { get => isGrabbed; set => isGrabbed = value; }
    private bool isGrabbed;

    public Sprite Icon { get => data.icon; set => data.icon = value; }
    public PlayerManager.HandGrabTypes HandGrabType { get => data.handGrabType; set => data.handGrabType = value; }

    public bool OutlineShouldBeRed { get => outlineShouldBeRed; set => outlineShouldBeRed = value; }
    private bool outlineShouldBeRed;

    public bool IsThrowable { get => data.isThrowable; set => data.isThrowable = value; }
    public float ThrowMultiplier { get => data.throwMultiplier; set => data.throwMultiplier = value; }
    public bool IsUseable { get => data.isUseable; set => data.isUseable = value; }

    public Vector3 GrabPositionOffset { get => data.grabPositionOffset; set => data.grabPositionOffset = value; }
    public Vector3 GrabRotationOffset { get => data.grabRotationOffset; set => data.grabRotationOffset = value; }
    public string FocusTextKey { get => data.focusTextKeys[(int)state]; set => data.focusTextKeys[(int)state] = value; }

    // --- Logic Variables ---
    private bool isGettingPutOnBasket; // Yerleþme animasyonu kilidi
    private bool isAddedToBasket;      // Þu an sepetin içinde mi?

    // References
    private Rigidbody rb;
    private Collider col;
    private MeshRenderer meshRenderer;

    // Cache Layers
    private int grabableLayer;
    private int grabableOutlinedLayer;
    private int interactableOutlinedRedLayer;
    private int ungrabableLayer;
    private int grabbedLayer;
    private int onTrayLayer; // "OnTray" ile ayný olabilir veya ayrý bir layer açabilirsin

    // Baðlý olduðu sepet (Varsa)
    [HideInInspector] public FryerBasket currentBasket;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        meshRenderer = GetComponent<MeshRenderer>();

        // Layer ID'lerini al
        grabableLayer = LayerMask.NameToLayer("Grabable");
        grabableOutlinedLayer = LayerMask.NameToLayer("GrabableOutlined");
        interactableOutlinedRedLayer = LayerMask.NameToLayer("InteractableOutlinedRed");
        ungrabableLayer = LayerMask.NameToLayer("Ungrabable");
        grabbedLayer = LayerMask.NameToLayer("Grabbed");
        onTrayLayer = LayerMask.NameToLayer("OnTray"); // Þimdilik OnTray kullanýyoruz, karýþýklýk olmasýn

        UpdateVisuals();
    }

    // --- COOKING LOGIC (Mevcut Kodun) ---
    public void Cook(float heatAmount)
    {
        if (state == Cookable.CookAmount.BURNT) return;
        currentCookingTime += heatAmount;
        CheckState();
    }

    private void CheckState()
    {
        Cookable.CookAmount oldState = state;

        if (currentCookingTime >= data.timeToBurn) state = Cookable.CookAmount.BURNT;
        else if (currentCookingTime >= data.timeToCook) state = Cookable.CookAmount.REGULAR;
        else state = Cookable.CookAmount.RAW;

        if (state != oldState)
        {
            UpdateVisuals();
            // Ýleride buraya ses/efekt eklenebilir
        }
    }

    private void UpdateVisuals()
    {
        if (meshRenderer == null || data == null) return;
        switch (state)
        {
            case Cookable.CookAmount.RAW: meshRenderer.material = data.rawMat; break;
            case Cookable.CookAmount.REGULAR: meshRenderer.material = data.cookedMat; break;
            case Cookable.CookAmount.BURNT: meshRenderer.material = data.burntMat; break;
        }
    }

    // --- BASKET LOGIC (Sepete Yerleþme) ---

    public void PutOnBasket(Vector3 targetPos, Quaternion targetRot, Transform containerParent)
    {
        // Kilidi aç, grablanamasýn
        isGettingPutOnBasket = true;
        PlayerManager.Instance.ResetPlayerGrab(this);

        ChangeLayer(onTrayLayer);

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true; // Fiziði kapat

        transform.parent = containerParent; // Sepetin containerýna child ol

        // Yerel pozisyona çevir (Sepet hareket ederse sapýtmasýn)
        Vector3 targetLocalPos = containerParent.InverseTransformPoint(targetPos);

        // DOTween ile yerine kay
        Sequence seq = DOTween.Sequence();
        seq.Join(transform.DOLocalMove(targetLocalPos, 0.2f).SetEase(Ease.OutQuad));
        seq.Join(transform.DORotateQuaternion(targetRot, 0.2f).SetEase(Ease.OutCubic));

        seq.OnComplete(() => {
            isAddedToBasket = true;
            isGettingPutOnBasket = false;

            // Burasý önemli: Sadece en üsttekini Grabable yapacaðýz.
            // Bu kararý Sepet (FryerBasket) verecek, o yüzden þimdilik OnBasketLayer'da kalýyor.
            // Sepet scriptini yazýnca oradan "UpdateLayers" çaðýrýp en üsttekini açacaðýz.

            // Yerleþme sesi
            //SoundManager.Instance.PlaySoundFX(data.dropSound, transform, 1f, 0.9f, 1.1f);
        });
    }

    // --- IGRABABLE IMPLEMENTATION ---

    public void OnGrab(Transform grabPoint)
    {
        ChangeLayer(grabbedLayer);

        // Eðer bir sepetten alýyorsak, sepetten kaydýný sil
        if (currentBasket != null && isAddedToBasket)
        {
            //currentBasket.RemoveItem(this); // Sepet koduna bunu ekleyeceðiz
            currentBasket = null;
            isAddedToBasket = false;
        }

        col.enabled = false;
        rb.isKinematic = false; // Ele alýnca fizik açýlýr (ama gravity kapalý)
        rb.useGravity = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        IsGrabbed = true;

        transform.SetParent(grabPoint);
        transform.position = grabPoint.position;
        transform.localPosition = data.grabLocalPositionOffset;
        transform.localRotation = Quaternion.Euler(data.grabLocalRotationOffset);

        //SoundManager.Instance.PlaySoundFX(data.dropSound, transform, 1f, 1f, 1.2f); // Tutma sesi (Drop sound kullandým geçici)
    }

    public void OnThrow(Vector3 direction, float force)
    {
        Release(direction, force);
    }

    public void OnDrop(Vector3 direction, float force)
    {
        Release(direction, force);
    }

    private void Release(Vector3 direction, float force)
    {
        col.enabled = true;
        IsGrabbed = false;
        transform.SetParent(null);
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.AddForce(direction * force, ForceMode.Impulse);

        ChangeLayer(ungrabableLayer); // Havada tutulamasýn
    }

    public void OnFocus()
    {
        if (!isGettingPutOnBasket)
            ChangeLayer(grabableOutlinedLayer);
    }

    public void OnLoseFocus()
    {
        if (!isGettingPutOnBasket)
            ChangeLayer(grabableLayer);
    }

    public void ChangeLayer(int layer)
    {
        gameObject.layer = layer;
    }

    public void OutlineChangeCheck()
    {
        if (gameObject.layer == grabableOutlinedLayer && OutlineShouldBeRed)
            ChangeLayer(interactableOutlinedRedLayer);
        else if (gameObject.layer == interactableOutlinedRedLayer && !OutlineShouldBeRed)
            ChangeLayer(grabableOutlinedLayer);
    }

    // --- COLLISION & SOUNDS ---

    private void OnCollisionEnter(Collision collision)
    {
        // Eðer grab edildiyse veya sepete yerleþiyorsa ses çalma
        if (IsGrabbed || isGettingPutOnBasket || collision.gameObject.CompareTag("Player")) return;

        // Yere düþtüðünde layer'ý düzelt
        if (gameObject.layer == ungrabableLayer)
        {
            ChangeLayer(grabableLayer);
        }

        HandleSoundFX(collision);
    }

    private void HandleSoundFX(Collision collision)
    {
        float impactForce = collision.relativeVelocity.magnitude;
        // Basit bir ses kontrolü (Veri eksikse hata vermesin diye null check)
        //if (impactForce > 0.5f && Time.time - lastSoundTime > 0.2f && data.dropSound != null)
        //{
            //SoundManager.Instance.PlaySoundFX(data.dropSound, transform, 0.5f, 0.8f, 1.2f, false);
            //lastSoundTime = Time.time;
        //}
    }

    // Gereksiz Interface Metodlarý
    public void OnHolster() { }
    public void OnUseHold() { }
    public void OnUseRelease() { }
}