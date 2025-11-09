using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightSwitch : MonoBehaviour, IInteractable
{
    public bool CanInteract { get => canInteract; set => canInteract = value; }
    [SerializeField] private bool canInteract;

    public PlayerManager.HandRigTypes HandRigType { get => handRigType; set => handRigType = value; }
    [SerializeField] private PlayerManager.HandRigTypes handRigType;

    public bool OutlineShouldBeRed { get => outlineShouldBeRed; set => outlineShouldBeRed = value; }
    [SerializeField] private bool outlineShouldBeRed;

    public string FocusText { get => focusTexts[switchStateNum]; set => focusTexts[switchStateNum] = value; }
    [SerializeField] private string[] focusTexts;
    private int switchStateNum = 0;
    [Space]

    [Header("On Off Settings")]
    [SerializeField] private GameObject switchPart;
    [SerializeField] private float timeToSwitchRotate = 0.2f;
    [SerializeField] private float onSwitchXRotation = 20f;
    private Quaternion offRotation;
    private Quaternion onRotation;
    private Coroutine rotateCoroutine;
    private bool isOn;

    [Header("Audio Settings")]
    public AudioClip onSound;
    public AudioClip offSound;

    [Header("Layer Settings")]
    private int interactableLayer;
    private int interactableOutlinedLayer;
    private int interactableOutlinedRedLayer;

    private void Awake()
    {
        interactableLayer = LayerMask.NameToLayer("Interactable");
        interactableOutlinedLayer = LayerMask.NameToLayer("InteractableOutlined");
        interactableOutlinedRedLayer = LayerMask.NameToLayer("InteractableOutlinedRed");

        isOn = false;
        offRotation = switchPart.transform.localRotation;
        onRotation = Quaternion.Euler(onSwitchXRotation, offRotation.y, offRotation.z);
    }

    public void ChangeLayer(int layerIndex)
    {
        gameObject.layer = layerIndex;
        switchPart.layer = layerIndex;
    }

    public void HandleFinishDialogue()
    {
    }

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
        {
            ChangeLayer(interactableOutlinedRedLayer);
        }
        else if (gameObject.layer == interactableOutlinedRedLayer && !OutlineShouldBeRed)
        {
            ChangeLayer(interactableOutlinedLayer);
        }
    }

    public void HandleRotation()
    {
        isOn = !isOn;

        SoundManager.Instance.PlaySoundFX(isOn ? onSound : offSound, transform);

        switchStateNum = isOn ? 1 : 0;

        PlayerManager.Instance.TryChangingFocusText(this, FocusText);

        if (rotateCoroutine != null)
        {
            StopCoroutine(rotateCoroutine);
            rotateCoroutine = null;
        }

        rotateCoroutine = StartCoroutine(ToogleRotate(isOn));
    }

    private IEnumerator ToogleRotate(bool shouldOn)
    {

        Quaternion targetRotation = shouldOn ? onRotation : offRotation;
        Quaternion startingRotation = switchPart.transform.localRotation;

        float timeElapsed = 0f;

        while (timeElapsed < timeToSwitchRotate)
        {
            switchPart.transform.localRotation = Quaternion.Slerp(startingRotation, targetRotation, timeElapsed / timeToSwitchRotate);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        switchPart.transform.localRotation = targetRotation;
        rotateCoroutine = null;
    }
}
