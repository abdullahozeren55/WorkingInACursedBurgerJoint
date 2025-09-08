using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnifeHolder : MonoBehaviour, IInteractable
{
    public GameManager.HandRigTypes HandRigType { get => handRigType; set => handRigType = value; }
    [SerializeField] private GameManager.HandRigTypes handRigType;

    [Header("Text Settings")]
    public GameObject grabKnifeText;

    [Header("Layer Settings")]
    private int interactableLayer;
    private int interactableOutlinedLayer;

    [Header("Knife Settings")]
    [SerializeField] private GameObject knife;
    [SerializeField] private Transform pointToSpawnKnife;

    private void Awake()
    {
        int childCount = transform.childCount;

        interactableLayer = LayerMask.NameToLayer("Interactable");
        interactableOutlinedLayer = LayerMask.NameToLayer("InteractableOutlined");
    }

    public void OnFocus()
    {
        grabKnifeText.SetActive(true);
        gameObject.layer = interactableOutlinedLayer;
    }

    public void OnLoseFocus()
    {
        grabKnifeText.SetActive(false);
        gameObject.layer = interactableLayer;
    }

    public void OnInteract()
    {
        GameObject instantiatedKnife = Instantiate(knife, pointToSpawnKnife.position, Quaternion.Euler(0f, -90f, 180f), null);
        GameManager.Instance.ResetPlayerGrabAndInteract();
        GameManager.Instance.ChangePlayerCurrentGrabable(instantiatedKnife.GetComponent<IGrabable>());
    }
}
