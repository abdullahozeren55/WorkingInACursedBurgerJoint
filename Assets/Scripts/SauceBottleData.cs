using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SauceBottle;

[CreateAssetMenu(fileName = "NewSauceBottleData", menuName = "Data/Sauce Bottle")]
public class SauceBottleData : ScriptableObject
{
    public Vector3 grabPositionOffset;
    public Vector3 grabRotationOffset;
    [Space]
    public Vector3 stabPositionOffset;
    public Vector3 stabRotationOffset;
    [Space]
    public float timeToStab = 0.3f;
    [Space]
    public AudioClip[] audioClips;
}
