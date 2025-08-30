using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowChild : MonoBehaviour, IInteractable
{
    private Window parentWindow;

    public GameManager.HandRigTypes HandRigType { get => handRigType; set => handRigType = value; }
    [SerializeField] private GameManager.HandRigTypes handRigType;

    private void Awake()
    {
        parentWindow = GetComponentInParent<Window>();
    }

    public void OnFocus()
    {
        parentWindow.OnFocus();
    }

    public void OnInteract()
    {
        parentWindow.OnInteract();
    }

    public void OnLoseFocus()
    {
        parentWindow.OnLoseFocus();
    }
}
