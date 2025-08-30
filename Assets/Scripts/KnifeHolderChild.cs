using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnifeHolderChild : MonoBehaviour, IInteractable
{
    private KnifeHolder parentKnifeHolder;

    public GameManager.HandRigTypes HandRigType { get => handRigType; set => handRigType = value; }
    [SerializeField] private GameManager.HandRigTypes handRigType;

    private void Awake()
    {
        parentKnifeHolder = GetComponentInParent<KnifeHolder>();
    }
    public void OnFocus()
    {
        parentKnifeHolder.OnFocus();
    }

    public void OnInteract()
    {
        parentKnifeHolder.OnInteract();
    }

    public void OnLoseFocus()
    {
        parentKnifeHolder.OnLoseFocus();
    }
}
