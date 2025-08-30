using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Keys : MonoBehaviour, IInteractable
{
    [Header("Audio Settings")]
    public AudioClip pickUpSound;
    private AudioSource audioSource;

    [Header("Text Settings")]
    public GameObject pickUpText;

    [Header("Lock Settings")]
    [SerializeField] private Door[] lockedDoors;
    [SerializeField] private Window window;

    [Header("Layer Settings")]
    private int interactableLayer;
    private int interactableOutlinedLayer;

    [Header("Random Place Settings")]
    [SerializeField] private Transform[] keyPossiblePositions;

    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;

    public GameManager.HandRigTypes HandRigType { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();

        interactableLayer = LayerMask.NameToLayer("Interactable");
        interactableOutlinedLayer = LayerMask.NameToLayer("InteractableOutlined");

        int place = Random.Range(0, keyPossiblePositions.Length);
        transform.position = keyPossiblePositions[place].position;
    }

    public void OnInteract()
    {
        meshRenderer.enabled = false;   
        meshCollider.enabled = false;
        audioSource.PlayOneShot(pickUpSound);

        foreach (Door door in lockedDoors)
            door.IsLocked = false;

        window.IsLocked = false;

        Destroy(gameObject, pickUpSound.length);
    }

    public void OnFocus()
    {
        pickUpText.SetActive(true);
        gameObject.layer = interactableOutlinedLayer;
    }

    public void OnLoseFocus()
    {
        pickUpText.SetActive(false);
        gameObject.layer = interactableLayer;
    }

    private void OnDestroy()
    {
        pickUpText.SetActive(false);
        GameManager.Instance.ResetPlayerGrabAndInteract();
    }
}
