using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupplyBox : MonoBehaviour, IInteractable
{
    public GameManager.HandRigTypes HandRigType { get => handRigType; set => handRigType = value; }
    [SerializeField] private GameManager.HandRigTypes handRigType;

    [Header("Text Settings")]
    public GameObject grabSupplyText;

    [Header("Layer Settings")]
    private int interactableLayer;
    private int interactableOutlinedLayer;

    [Header("Supply Settings")]
    [SerializeField] private GameObject supply;
    [SerializeField] private Transform pointToSpawn;

    private void Awake()
    {
        int childCount = transform.childCount;

        interactableLayer = LayerMask.NameToLayer("Interactable");
        interactableOutlinedLayer = LayerMask.NameToLayer("InteractableOutlined");
    }

    public void OnFocus()
    {
        grabSupplyText.SetActive(true);
        gameObject.layer = interactableOutlinedLayer;
    }

    public void OnLoseFocus()
    {
        grabSupplyText.SetActive(false);
        gameObject.layer = interactableLayer;
    }

    public void OnInteract()
    {
        GameObject instantiatedSupply = Instantiate(supply, pointToSpawn.position, Quaternion.Euler(0f, -90f, 180f), null);
        GameManager.Instance.ResetPlayerGrabAndInteract();
        GameManager.Instance.ChangePlayerCurrentGrabable(instantiatedSupply.GetComponent<IGrabable>());
    }
}
