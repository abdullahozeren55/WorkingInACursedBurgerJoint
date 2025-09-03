using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewNoodleData", menuName = "Data/Noodle")]
public class NoodleData : ScriptableObject
{
    public float timeToPutOnHologram = 0.3f;
    public Vector3 grabPositionOffset;
    public Vector3 grabRotationOffset;

    public AudioClip[] audioClips;
}
