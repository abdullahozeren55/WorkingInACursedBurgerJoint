using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BroomHolder : MonoBehaviour, IInteractable
{

    [Header("Text Settings")]
    public GameObject grabBroomText;

    [Header("Layer Settings")]
    private int interactableLayer;
    private int interactableOutlinedLayer;

    [Header("Broom Settings")]
    [SerializeField] private GameObject broom;
    [SerializeField] private Transform pointToSpawnKnife;

    public GameManager.HandRigTypes HandRigType { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    private void Awake()
    {
        interactableLayer = LayerMask.NameToLayer("Interactable");
        interactableOutlinedLayer = LayerMask.NameToLayer("InteractableOutlined");
    }

    public void OnFocus()
    {
        grabBroomText.SetActive(true);
        gameObject.layer = interactableOutlinedLayer;
    }

    public void OnLoseFocus()
    {
        grabBroomText.SetActive(false);
        gameObject.layer = interactableLayer;
    }

    public void OnInteract()
    {
        GameObject instantiatedKnife = Instantiate(broom, pointToSpawnKnife.position, Quaternion.Euler(-135f, 0f, 0f), null);
        GameManager.Instance.ResetPlayerGrabAndInteract();
        GameManager.Instance.ChangePlayerCurrentGrabable(instantiatedKnife.GetComponent<IGrabable>());
    }
}
