using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewNoodleData", menuName = "Data/Noodle")]
public class NoodleData : ScriptableObject
{
    public PlayerManager.HandGrabTypes handGrabType;
    [Space]
    public float timeToPutOnHologram = 0.3f;
    [Space]
    public Vector3 grabPositionOffset;
    public Vector3 grabRotationOffset;
    [Space]
    public Vector3 grabLocalPositionOffset;
    public Vector3 grabLocalRotationOffset;
    [Space]
    public Vector3 usePositionOffset;
    public Vector3 useRotationOffset;
    [Space]
    public Vector3 useLeftPositionOffset;
    public Vector3 useLeftRotationOffset;
    [Space]
    public Vector3 use2PositionOffset;
    public Vector3 use2RotationOffset;
    [Space]
    public float timeToUse = 0.3f;
    public float timeToHandleLid = 0.2f;
    [Space]
    public AudioClip[] audioClips;
}
