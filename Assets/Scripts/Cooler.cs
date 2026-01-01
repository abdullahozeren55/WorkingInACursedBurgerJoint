using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Cooler : MonoBehaviour, IInteractable
{
    public bool CanInteract { get => canInteract; set => canInteract = value; }
    [SerializeField] private bool canInteract = true;

    [Header("Target Objects")]
    [Tooltip("Buraya 2 kapý objesini inspectordan sürükle")]
    [SerializeField] private Transform[] doors;

    // --- YENÝ: Baþlangýç açýlarýný saklamak için liste ---
    private List<Vector3> closedRotations = new List<Vector3>();

    [Header("Audio Settings")]
    public AudioClip openSound;
    public AudioClip closeSound;
    public string audioTag;

    public string FocusTextKey { get => focusTextKeys[coolerStateNum]; set => focusTextKeys[coolerStateNum] = value; }
    [SerializeField] private string[] focusTextKeys;
    private int coolerStateNum = 0;
    [Space]

    [Header("Open Close Settings")]
    [SerializeField] private float closeDuration = 0.4f; // Kapanma hýzý (Baz süre)
    [SerializeField] private float openZRotation = 130f; // Açýlýnca eklenecek Z açýsý

    private bool isOpened;

    [Header("Layer Settings")]
    private int interactableLayer;
    private int interactableOutlinedLayer;
    private int interactableOutlinedRedLayer;

    public PlayerManager.HandRigTypes HandRigType { get => handRigType; set => handRigType = value; }
    [SerializeField] private PlayerManager.HandRigTypes handRigType;

    public bool OutlineShouldBeRed { get => outlineShouldBeRed; set => outlineShouldBeRed = value; }
    [SerializeField] private bool outlineShouldBeRed;

    void Awake()
    {
        isOpened = false;

        interactableLayer = LayerMask.NameToLayer("Interactable");
        interactableOutlinedLayer = LayerMask.NameToLayer("InteractableOutlined");
        interactableOutlinedRedLayer = LayerMask.NameToLayer("InteractableOutlinedRed");

        // --- BAÞLANGIÇ AÇILARINI KAYDET ---
        // Sað kapýnýn -180, sol kapýnýn 0 olduðunu buradan otomatik öðrenecek.
        if (doors != null)
        {
            foreach (Transform door in doors)
            {
                if (door != null)
                {
                    // Yerel Euler açýlarýný (X, Y, Z) kaydediyoruz
                    closedRotations.Add(door.localEulerAngles);
                }
                else
                {
                    // Boþ slot varsa listeyi bozmamak için boþ veri ekle
                    closedRotations.Add(Vector3.zero);
                }
            }
        }
    }

    public void HandleFinishDialogue() { }

    public void OnFocus()
    {
        if (!CanInteract) return;
        ChangeLayer(OutlineShouldBeRed ? interactableOutlinedRedLayer : interactableOutlinedLayer);
    }

    public void OnInteract()
    {
        if (!CanInteract) return;
        HandleRotation();
    }

    public void OnLoseFocus()
    {
        if (!CanInteract) return;
        ChangeLayer(interactableLayer);
    }

    public void OutlineChangeCheck()
    {
        if (gameObject.layer == interactableOutlinedLayer && OutlineShouldBeRed)
            ChangeLayer(interactableOutlinedRedLayer);
        else if (gameObject.layer == interactableOutlinedRedLayer && !OutlineShouldBeRed)
            ChangeLayer(interactableOutlinedLayer);
    }

    public void HandleRotation()
    {
        isOpened = !isOpened;

        SoundManager.Instance.PlaySoundFX(isOpened ? openSound : closeSound, transform, 1f, 1f, 1f, true, audioTag);

        coolerStateNum = isOpened ? 1 : 0;
        PlayerManager.Instance.TryChangingFocusText(this, FocusTextKey);

        // --- YENÝ ZAMANLAMA MANTIÐI ---
        // Açýlýrken: Kapanma süresinin 1.5 katý
        // Kapanýrken: Normal süre
        float currentDuration = isOpened ? (closeDuration * 1.5f) : closeDuration;

        // Döngüye index ile giriyoruz ki 'closedRotations' listesinden doðru açýyý çekelim
        for (int i = 0; i < doors.Length; i++)
        {
            Transform door = doors[i];
            if (door == null) continue;

            door.DOKill();

            // Hedef Açýyý Belirle:
            // Kapalýysa -> Hafýzadaki açýsý (closedRotations[i])
            // Açýksa -> Hafýzadaki açýsý + Ýstenen Z açýsý
            // (Örn sað kapý için: (-180, 0, 0) + (0, 0, 130) = (-180, 0, 130) olur)
            Vector3 closedRot = closedRotations[i];
            Vector3 targetRot = isOpened ? closedRot + new Vector3(0, 0, openZRotation) : closedRot;

            door.DOLocalRotate(targetRot, currentDuration)
                .SetEase(isOpened ? Ease.OutBack : Ease.OutQuad);
        }
    }

    public void ChangeLayer(int layerIndex)
    {
        gameObject.layer = layerIndex;
        if (doors != null)
        {
            foreach (Transform door in doors)
                if (door != null) door.gameObject.layer = layerIndex;
        }
    }
}