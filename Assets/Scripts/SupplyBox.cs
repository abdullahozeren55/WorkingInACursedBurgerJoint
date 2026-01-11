using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SupplyBox : MonoBehaviour, IInteractable
{
    public bool CanInteract { get => canInteract; set => canInteract = value; }
    [SerializeField] private bool canInteract;
    public PlayerManager.HandRigTypes HandRigType { get => handRigType; set => handRigType = value; }
    [SerializeField] private PlayerManager.HandRigTypes handRigType;

    public bool OutlineShouldBeRed { get => outlineShouldBeRed; set => outlineShouldBeRed = value; }
    [SerializeField] private bool outlineShouldBeRed;

    public string FocusTextKey { get => focusTextKey; set => focusTextKey = value; }
    [SerializeField] private string focusTextKey;
    [Space]
    [SerializeField] private Vector3 spawnRotation = new Vector3(0f, -90f, 180f);

    [Header("Layer Settings")]
    [Tooltip("Eðer true ise, layer deðiþimi tüm child objelere de uygulanýr.")]
    [SerializeField] private bool shouldChangeChilds = false; // Ýsteðin üzerine eklenen bool

    private int interactableLayer;
    private int interactableOutlinedLayer;
    private int interactableOutlinedRedLayer;

    [Header("Supply Settings")]
    [SerializeField] private GameObject supply;

    private void Awake()
    {
        // Child count burada sadece bilgi amaçlý duruyor olabilir,
        // logic'te kullanýlmýyorsa silebilirsin ama orijinal koda dokunmadým.
        int childCount = transform.childCount;

        interactableLayer = LayerMask.NameToLayer("Interactable");
        interactableOutlinedLayer = LayerMask.NameToLayer("InteractableOutlined");
        interactableOutlinedRedLayer = LayerMask.NameToLayer("InteractableOutlinedRed");
    }

    public void ChangeLayer(int layerIndex)
    {
        if (shouldChangeChilds)
        {
            // True ise hem kendini hem de altýndakileri deðiþtir
            SetLayerRecursively(gameObject, layerIndex);
        }
        else
        {
            // False ise sadece kendini deðiþtir (eski mantýk)
            gameObject.layer = layerIndex;
        }
    }

    // Recursive fonksiyon: Kendini ve altýndaki her þeyi gezerek layer deðiþtirir
    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;

        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            if (child == null) continue;
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    public void HandleFinishDialogue()
    {

    }

    public void OnFocus()
    {
        if (!CanInteract) return;

        ChangeLayer(OutlineShouldBeRed ? interactableOutlinedRedLayer : interactableOutlinedLayer);
    }

    public void OnLoseFocus()
    {
        if (!CanInteract) return;

        ChangeLayer(interactableLayer);
    }

    public void OutlineChangeCheck()
    {
        // Burada gameObject.layer kontrolü sadece parent'a bakýyor, bu mantýklý.
        // Childlar zaten parent ile senkronize olacaðý için ekstra kontrole gerek yok.
        if (gameObject.layer == interactableOutlinedLayer && OutlineShouldBeRed)
        {
            ChangeLayer(interactableOutlinedRedLayer);
        }
        else if (gameObject.layer == interactableOutlinedRedLayer && !OutlineShouldBeRed)
        {
            ChangeLayer(interactableOutlinedLayer);
        }
    }

    public void OnInteract()
    {
        if (!CanInteract) return;

        GameObject instantiatedSupply = Instantiate(supply, transform.position, Quaternion.Euler(spawnRotation), null);
        PlayerManager.Instance.ResetPlayerGrabAndInteract();
        PlayerManager.Instance.ChangePlayerCurrentGrabable(instantiatedSupply.GetComponent<IGrabable>());
    }
}