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

    private List<Vector3> closedRotations = new List<Vector3>();
    private List<Collider> doorColliders = new List<Collider>(); // Colliderlarý tutmaya devam ediyoruz

    [Header("Audio Settings")]
    public AudioClip openSound;
    public AudioClip closeSound;
    public string audioTag;

    public string FocusTextKey { get => focusTextKeys[coolerStateNum]; set => focusTextKeys[coolerStateNum] = value; }
    [SerializeField] private string[] focusTextKeys;
    private int coolerStateNum = 0;
    [Space]

    [Header("Open Close Settings")]
    [SerializeField] private float closeDuration = 0.4f;
    [SerializeField] private float openZRotation = 130f;

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

        if (doors != null)
        {
            foreach (Transform door in doors)
            {
                if (door != null)
                {
                    closedRotations.Add(door.localEulerAngles);

                    Collider col = door.GetComponent<Collider>();
                    if (col != null)
                    {
                        doorColliders.Add(col);
                    }
                }
                else
                {
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

        float currentDuration = isOpened ? (closeDuration * 1.5f) : closeDuration;

        // --- GÜNCELLEME: PlayerManager üzerinden çarpýþmayý yoksay ---
        // True gönderiyoruz: Yani "Yoksaymayý aktif et" (Çarpýþma KAPALI)
        PlayerManager.Instance.SetIgnoreCollisionWithPlayer(doorColliders, true);

        for (int i = 0; i < doors.Length; i++)
        {
            Transform door = doors[i];
            if (door == null) continue;

            door.DOKill();

            Vector3 closedRot = closedRotations[i];
            Vector3 targetRot = isOpened ? closedRot + new Vector3(0, 0, openZRotation) : closedRot;

            Tweener rotationTween = door.DOLocalRotate(targetRot, currentDuration)
                .SetEase(isOpened ? Ease.OutBack : Ease.OutQuad);

            if (i == doors.Length - 1)
            {
                rotationTween.OnComplete(() =>
                {
                    // --- GÜNCELLEME: Hareket bitti, çarpýþmayý geri aç ---
                    // False gönderiyoruz: Yani "Yoksaymayý kapat" (Çarpýþma AÇIK)
                    PlayerManager.Instance.SetIgnoreCollisionWithPlayer(doorColliders, false);
                });
            }
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