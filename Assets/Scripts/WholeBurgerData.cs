using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWholeBurgerData", menuName = "Data/WholeBurger")]
public class WholeBurgerData : ScriptableObject
{
    public bool isUseable = false;
    public bool isThrowable = true;
    public float throwMultiplier = 1f;
    [Space]
    public PlayerManager.HandGrabTypes handGrabType;
    public Sprite icon;
    [Space]
    public Vector3 grabPositionOffset; //0 for open, 1 for close
    public Vector3 grabRotationOffset; //0 for open, 1 for close
    [Space]
    public Vector3 grabLocalPositionOffset; //0 for open, 1 for close
    public Vector3 grabLocalRotationOffset; //0 for open, 1 for close
    [Space]
    public string[] focusTextKeys;
}
