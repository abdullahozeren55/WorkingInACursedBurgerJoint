using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToiletChild : MonoBehaviour, IInteractable
{
    private Toilet parentToilet;

    public GameManager.HandRigTypes HandRigType { get => parentToilet.HandRigType; set => parentToilet.HandRigType = value; }

    public bool OutlineShouldBeRed { get => parentToilet.OutlineShouldBeRed; set => parentToilet.OutlineShouldBeRed = value; }

    private void Awake()
    {
        parentToilet = GetComponentInParent<Toilet>();
    }

    public void OnFocus()
    {
        parentToilet.OnFocus();
    }

    public void OnInteract()
    {
        parentToilet.OnInteract();
    }

    public void OnLoseFocus()
    {
        parentToilet.OnLoseFocus();
    }

    public void OutlineChangeCheck()
    {
        parentToilet.OutlineChangeCheck();
    }
}
