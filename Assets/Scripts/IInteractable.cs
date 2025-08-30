using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    public void OnInteract();
    public void OnFocus();
    public void OnLoseFocus();
    public GameManager.HandRigTypes HandRigType {  get; set; }

}
