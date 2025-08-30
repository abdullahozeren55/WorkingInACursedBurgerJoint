using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SauceBottle;

[CreateAssetMenu(fileName = "NewSauceBottleData", menuName = "Data/Sauce Bottle")]
public class SauceBottleData : ScriptableObject
{
    public Vector3 grabPositionOffset;
    public Vector3 grabRotationOffset;


    public AudioClip[] audioClips;
}
