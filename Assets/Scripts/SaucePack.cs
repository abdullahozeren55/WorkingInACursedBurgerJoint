using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaucePack : MonoBehaviour, IInteractable
{
    [Header("Audio Settings")]
    public AudioClip takeOutSound;
    private AudioSource audioSource;

    [Header("Text Settings")]
    public GameObject takeOutText;
    public GameObject addText;

    [Header("Layer Settings")]
    private int interactableLayer;
    private int interactableOutlinedLayer;
    private int uninteractableLayer;

    private Animator anim;
    private Collider col;

    private bool isSaucePackOut;

    public GameManager.HandRigTypes HandRigType { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    void Awake()
    {

        anim = GetComponentInParent<Animator>();
        col = GetComponent<Collider>();

        audioSource = GetComponent<AudioSource>();

        interactableLayer = LayerMask.NameToLayer("Interactable");
        interactableOutlinedLayer = LayerMask.NameToLayer("InteractableOutlined");
        uninteractableLayer = LayerMask.NameToLayer("Uninteractable");

        isSaucePackOut = false;

    }

    public void OnFocus()
    {
        HandleText(true);
        gameObject.layer = interactableOutlinedLayer;
    }

    public void OnInteract()
    {
        gameObject.layer = uninteractableLayer;
        GameManager.Instance.ResetPlayerGrabAndInteract();

        col.enabled = false;

        if (!isSaucePackOut)
        {
            anim.Play("NoodleSaucePackOut");
            isSaucePackOut = true;
        }
        else
        {
            anim.Play("NoodleSaucePackIn");
        }

        audioSource.PlayOneShot(takeOutSound);

    }

    public void OnLoseFocus()
    {
        HandleText(false);
        gameObject.layer = interactableLayer;
    }

    private void HandleText(bool isFocused)
    {
        if (isFocused)
        {
            takeOutText.SetActive(!isSaucePackOut);
            addText.SetActive(isSaucePackOut);
        }
        else
        {
            if (takeOutText.activeSelf) takeOutText.SetActive(false);
            if (addText.activeSelf) addText.SetActive(false);
        }
    }
}
