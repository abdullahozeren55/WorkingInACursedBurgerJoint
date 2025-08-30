using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kettle : MonoBehaviour, IInteractable
{
    [Header("Audio Settings")]
    public AudioClip takeOutSound;
    public AudioClip waterPourSound;
    private AudioSource audioSource;

    [Header("Text Settings")]
    public GameObject takeOutText;

    [Header("Layer Settings")]
    private int interactableLayer;
    private int interactableOutlinedLayer;
    private int uninteractableLayer;

    public NoodleInteractable currentNoodle;

    private Animator anim;

    public GameManager.HandRigTypes HandRigType { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    void Awake()
    {
        anim = GetComponent<Animator>();

        audioSource = GetComponent<AudioSource>();

        interactableLayer = LayerMask.NameToLayer("Interactable");
        interactableOutlinedLayer = LayerMask.NameToLayer("InteractableOutlined");
        uninteractableLayer = LayerMask.NameToLayer("Uninteractable");

    }

    public void OnFocus()
    {
        takeOutText.SetActive(true);
        gameObject.layer = interactableOutlinedLayer;
    }

    public void OnInteract()
    {
        GameManager.Instance.ResetPlayerGrabAndInteract();

        anim.Play("UseKettle");

        audioSource.PlayOneShot(takeOutSound);

    }

    private void KettleUninteractable()
    {
        gameObject.layer = uninteractableLayer;
    }

    public void OnLoseFocus()
    {
        takeOutText.SetActive(false);
        gameObject.layer = interactableLayer;
    }

    private void PourWater()
    {
        if (currentNoodle != null)
        {
            currentNoodle.PourWater();
            audioSource.PlayOneShot(waterPourSound);
        }  

    }
}
