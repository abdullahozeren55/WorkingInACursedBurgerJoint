using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static FryableData;

[CreateAssetMenu(fileName = "NewHolderData", menuName = "Data/Holder")]
public class HolderData : ScriptableObject
{
    [Header("Identity")]
    public Sprite icon;
    public string[] focusTextKeys;
    public PlayerManager.HandGrabTypes handGrabType;
    public PlayerManager.HandRigTypes handRigType = PlayerManager.HandRigTypes.SingleHandGrab;
    [Space]
    public bool isUseable = false;
    public bool isThrowable = true;
    public float throwMultiplier = 1f;
    [Space]
    public Vector3 grabPositionOffset;
    public Vector3 grabRotationOffset;
    public Vector3 grabLocalPositionOffset;
    public Vector3 grabLocalRotationOffset;
}
