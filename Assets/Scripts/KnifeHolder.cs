using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnifeHolder : MonoBehaviour, IInteractable
{

    [Header("Text Settings")]
    public GameObject grabKnifeText;

    [Header("Layer Settings")]
    private int interactableLayer;
    private int interactableOutlinedLayer;

    [Header("Knife Settings")]
    [SerializeField] private GameObject knife;
    [SerializeField] private Transform pointToSpawnKnife;

    private GameObject[] allParts;

    public GameManager.HandRigTypes HandRigType { get => handRigType; set => handRigType = value; }
    [SerializeField] private GameManager.HandRigTypes handRigType;

    private void Awake()
    {
        int childCount = transform.childCount;

        // Initialize the array to hold the children
        allParts = new GameObject[childCount];

        // Loop through each child and store it in the array
        for (int i = 0; i < childCount; i++)
        {
            // Get the child GameObject
            allParts[i] = transform.GetChild(i).gameObject;
        }

        interactableLayer = LayerMask.NameToLayer("Interactable");
        interactableOutlinedLayer = LayerMask.NameToLayer("InteractableOutlined");
    }

    public void OnFocus()
    {
        grabKnifeText.SetActive(true);
        ChangeLayer(interactableOutlinedLayer);
    }

    public void OnLoseFocus()
    {
        grabKnifeText.SetActive(false);
        ChangeLayer(interactableLayer);
    }

    public void OnInteract()
    {
        GameObject instantiatedKnife = Instantiate(knife, pointToSpawnKnife.position, Quaternion.Euler(0f, -90f, 180f), null);
        GameManager.Instance.ResetPlayerGrabAndInteract();
        GameManager.Instance.ChangePlayerCurrentGrabable(instantiatedKnife.GetComponent<IGrabable>());
    }


    private void ChangeLayer(int layerIndex)
    {
        foreach (GameObject child in allParts)
            child.layer = layerIndex;
    }
}
