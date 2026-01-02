using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DrinkCup : MonoBehaviour, IGrabable
{
    // ... (Mevcut Deðiþkenler Aynen Kalýyor) ...
    public IGrabable Master => this;
    public bool IsGrabbed { get => isGrabbed; set => isGrabbed = value; }
    private bool isGrabbed;
    public Sprite Icon { get => data.icon; set => data.icon = value; }

    [SerializeField] private GameObject drinkGO;
    [SerializeField] private GameObject lidGO;
    [SerializeField] private GameObject strawGO;

    // --- YENÝ EKLENENLER ---
    [HideInInspector] public SodaMachine currentMachine;
    private bool isFull = false; // Bardak dolu mu?
    public bool IsFull => isFull; // Dýþarýdan okumak için
    // -----------------------

    // ... (Diðer deðiþkenler ayný) ...
    public bool OutlineShouldBeRed { get => outlineShouldBeRed; set => outlineShouldBeRed = value; }
    private bool outlineShouldBeRed;
    public bool OutlineShouldBeGreen { get => outlineShouldBeGreen; set => outlineShouldBeGreen = value; }
    private bool outlineShouldBeGreen;
    public Vector3 GrabPositionOffset { get => data.grabPositionOffset; set => data.grabPositionOffset = value; }
    public Vector3 GrabRotationOffset { get => data.grabRotationOffset; set => data.grabRotationOffset = value; }
    public PlayerManager.HandGrabTypes HandGrabType { get => data.handGrabType; set => data.handGrabType = value; }
    public bool IsThrowable { get => data.isThrowable; set => data.isThrowable = value; }
    public float ThrowMultiplier { get => data.throwMultiplier; set => data.throwMultiplier = value; }
    public bool IsUseable { get => data.isUseable; set => data.isUseable = value; }
    public DrinkCupData data;
    public string FocusTextKey { get => data.focusTextKey; set => data.focusTextKey = value; }

    private Rigidbody rb;
    private Collider col;
    private int grabableLayer;
    private int grabableOutlinedLayer;
    private int interactableOutlinedRedLayer;
    private int ungrabableLayer;
    private int grabbedLayer;
    [HideInInspector] public bool isJustThrowed;
    [HideInInspector] public bool isJustDropped;
    private float lastSoundTime = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        grabableLayer = LayerMask.NameToLayer("Grabable");
        grabableOutlinedLayer = LayerMask.NameToLayer("GrabableOutlined");
        interactableOutlinedRedLayer = LayerMask.NameToLayer("InteractableOutlinedRed");
        ungrabableLayer = LayerMask.NameToLayer("Ungrabable");
        grabbedLayer = LayerMask.NameToLayer("Grabbed");

        IsGrabbed = false;
        isJustThrowed = false;
        isJustDropped = false;

        // Baþlangýçta içecek kapalý olsun
        if (drinkGO != null) drinkGO.SetActive(false);
    }

    // --- YENÝ FONKSÝYON: DOLUM ÝÞLEMÝ ---
    public void StartFilling(Color liquidColor, float duration)
    {
        // 1. Zaten doluysa iþlem yapma (Renk deðiþtirme, tekrar doldurma vs.)
        if (isFull) return;

        // 2. Ýçecek objesini aç
        if (drinkGO != null)
        {
            drinkGO.SetActive(true);

            // 3. Rengi Ayarla
            SkinnedMeshRenderer meshRenderer = drinkGO.GetComponent<SkinnedMeshRenderer>();
            if (meshRenderer != null)
            {
                // Material rengini deðiþtir
                meshRenderer.material.color = liquidColor;
                // Eðer shader graph veya özel shader kullanýyorsan:
                // meshRenderer.material.SetColor("_BaseColor", liquidColor);

                // 4. BlendShape Animasyonu (0 -> 100)
                // "Key 1" genelde Index 0'dýr. Eðer Blender'da sýralamada 2. sýradaysa Index 1 yap.
                int blendShapeIndex = 0;

                // Önce sýfýrla
                meshRenderer.SetBlendShapeWeight(blendShapeIndex, 0f);

                // DOTween ile sürece yayarak 100 yap
                DOTween.To(() => meshRenderer.GetBlendShapeWeight(blendShapeIndex),
                           x => meshRenderer.SetBlendShapeWeight(blendShapeIndex, x),
                           100f, duration)
                           .SetEase(Ease.Linear) // Sabit hýzla dolsun
                           .OnComplete(() => isFull = true); // Bitince "Dolu" iþaretle
            }
        }
    }
    // -------------------------------------

    public void OnHolster() { }

    public void OnGrab(Transform grabPoint)
    {
        ChangeLayer(grabbedLayer);

        if (currentMachine != null)
        {
            currentMachine.ReleaseCup();
            currentMachine = null;
        }

        // DOTween kill: Eðer dolarken alýrsan dolmayý durdur.
        if (drinkGO != null) drinkGO.GetComponent<SkinnedMeshRenderer>()?.DOKill();

        col.enabled = false;
        SoundManager.Instance.PlaySoundFX(data.audioClips[0], transform, data.grabSoundVolume, data.grabSoundMinPitch, data.grabSoundMaxPitch);
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.useGravity = false;
        rb.isKinematic = false;
        IsGrabbed = true;
        transform.SetParent(grabPoint);
        transform.position = grabPoint.position;
        transform.localPosition = data.grabLocalPositionOffset;
        transform.localRotation = Quaternion.Euler(data.grabLocalRotationOffset);
    }

    // ... (Diðer metodlar ayný: OnFocus, OnDrop, OnThrow, Collision vs.) ...

    public void OnFocus()
    {
        if (!isJustDropped && !isJustThrowed)
            ChangeLayer(grabableOutlinedLayer);
    }
    public void OnLoseFocus()
    {
        if (!isJustDropped && !isJustThrowed)
            ChangeLayer(grabableLayer);
    }

    public void OnDrop(Vector3 direction, float force)
    {
        col.enabled = true;
        IsGrabbed = false;
        transform.SetParent(null);
        rb.useGravity = true;
        rb.AddForce(direction * force, ForceMode.Impulse);
        isJustDropped = true;
        ChangeLayer(ungrabableLayer);
    }

    public void OnThrow(Vector3 direction, float force)
    {
        col.enabled = true;
        IsGrabbed = false;
        transform.SetParent(null);
        rb.useGravity = true;
        rb.AddForce(direction * force, ForceMode.Impulse);
        isJustThrowed = true;
        ChangeLayer(ungrabableLayer);
    }

    public void ChangeLayer(int layer)
    {
        gameObject.layer = layer;
        drinkGO.layer = layer;
        lidGO.layer = layer;
        strawGO.layer = layer;
    }

    public void OutlineChangeCheck()
    {
        if (gameObject.layer == grabableOutlinedLayer && OutlineShouldBeRed)
        {
            ChangeLayer(interactableOutlinedRedLayer);
        }
        else if (gameObject.layer == interactableOutlinedRedLayer && !OutlineShouldBeRed)
        {
            ChangeLayer(grabableOutlinedLayer);
        }
    }

    private void OnDisable() { OnLoseFocus(); }
    private void OnDestroy() { OnLoseFocus(); }

    private void HandleSoundFX(Collision collision)
    {
        float impactForce = collision.relativeVelocity.magnitude;
        if (impactForce < data.dropThreshold || Time.time - lastSoundTime < data.soundCooldown) return;
        if (impactForce >= data.throwThreshold)
            SoundManager.Instance.PlaySoundFX(data.audioClips[2], transform, data.throwSoundVolume, data.throwSoundMinPitch, data.throwSoundMaxPitch);
        else
            SoundManager.Instance.PlaySoundFX(data.audioClips[1], transform, data.dropSoundVolume, data.dropSoundMinPitch, data.dropSoundMaxPitch);
        lastSoundTime = Time.time;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsGrabbed && !collision.gameObject.CompareTag("Player"))
        {
            if (isJustThrowed) { ChangeLayer(grabableLayer); isJustThrowed = false; }
            else if (isJustDropped) { ChangeLayer(grabableLayer); isJustDropped = false; }
            HandleSoundFX(collision);
        }
    }

    public void OnUseHold() { throw new System.NotImplementedException(); }
    public void OnUseRelease() { throw new System.NotImplementedException(); }
    public bool TryCombine(IGrabable otherItem) { return false; }
    public bool CanCombine(IGrabable otherItem) { return false; }

    public void FinishPuttingOnSodaMachine()
    {
        ChangeLayer(grabableLayer);
        isJustDropped = false;
        isJustThrowed = false;
    }
}