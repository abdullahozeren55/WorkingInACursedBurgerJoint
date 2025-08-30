using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoodleChild : MonoBehaviour, IInteractable
{
    private NoodleInteractable parenNoodle;

    public GameManager.HandRigTypes HandRigType { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    private void Awake()
    {
        parenNoodle = GetComponentInParent<NoodleInteractable>();
    }

    public void OnFocus()
    {
        parenNoodle.OnFocus();
    }

    public void OnInteract()
    {
        parenNoodle.OnInteract();
    }

    public void OnLoseFocus()
    {
        parenNoodle.OnLoseFocus();
    }
}
