using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewKnifeData", menuName = "Data/Knife")]
public class KnifeData : ScriptableObject
{
    public bool isUseable = true;
    public PlayerManager.HandGrabTypes handGrabType;
    [Space]
    public Vector3 grabPositionOffset;
    public Vector3 grabRotationOffset;
    [Space]
    public Vector3 stabPositionOffset;
    public Vector3 stabRotationOffset;
    [Space]
    public float timeToStab = 0.3f;
    [Space]
    public Sprite focusImage;
    public AudioClip[] audioClips;
}
