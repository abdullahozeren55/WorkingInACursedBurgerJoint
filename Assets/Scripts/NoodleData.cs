using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewNoodleData", menuName = "Data/Noodle")]
public class NoodleData : ScriptableObject
{
    public bool isUseable = true;
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
    public Vector3 useLocalPositionOffset;
    public Vector3 useLocalRotationOffset;
    [Space]
    public float timeToUse = 0.3f;
    [Space]
    public AudioClip[] audioClips;
}
