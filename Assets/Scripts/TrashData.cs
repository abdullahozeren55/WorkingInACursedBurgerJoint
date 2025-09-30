using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewTrashData", menuName = "Data/Trash")]
public class TrashData : ScriptableObject
{
    public bool isUseable = false;
    public PlayerManager.HandGrabTypes handGrabType;
    [Space]
    public Vector3 grabPositionOffset;
    public Vector3 grabRotationOffset;
    [Space]
    public AudioClip[] audioClips;
}
