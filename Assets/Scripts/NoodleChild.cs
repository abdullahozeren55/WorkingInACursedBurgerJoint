using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoodleChild : MonoBehaviour, IInteractable, IGrabable
{
    private NoodleInteractable parenNoodle;

    public GameManager.HandRigTypes HandRigType { get => handRigType; set => handRigType = value; }
    [SerializeField] private GameManager.HandRigTypes handRigType;
    public bool IsGrabbed { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public float HandLerp { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public GameManager.GrabTypes GrabType { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    

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

    public void OnGrab(Transform grabPoint)
    {
        throw new System.NotImplementedException();
    }

    public void OnThrow(Vector3 direction, float force)
    {
        throw new System.NotImplementedException();
    }

    public void OnDrop(Vector3 direction, float force)
    {
        throw new System.NotImplementedException();
    }
}
