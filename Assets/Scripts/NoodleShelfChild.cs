using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoodleShelfChild : MonoBehaviour, IInteractable
{
    private NoodleShelf parentNoodleShelf;

    public GameManager.HandRigTypes HandRigType { get => handRigType; set => handRigType = value; }
    [SerializeField] private GameManager.HandRigTypes handRigType;

    private void Awake()
    {
        parentNoodleShelf = GetComponentInParent<NoodleShelf>();
    }
    public void OnFocus()
    {
        parentNoodleShelf.OnFocus();
    }

    public void OnInteract()
    {
        parentNoodleShelf.OnInteract();
    }

    public void OnLoseFocus()
    {
        parentNoodleShelf.OnLoseFocus();
    }
}
