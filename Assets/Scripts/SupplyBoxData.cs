using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSupplyBoxData", menuName = "Data/SupplyBox")]
public class SupplyBoxData : ScriptableObject
{
    public GameObject destroyParticle;
    public float followingSpeed = 40f;
    public float weight = 1.5f;
    [Space]
    public Vector3 grabPositionOffset;
    public Vector3 grabRotationOffset;
    [Space]

    public AudioClip[] audioClips;
}
