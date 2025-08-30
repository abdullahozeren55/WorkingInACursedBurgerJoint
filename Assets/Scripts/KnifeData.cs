using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewKnifeData", menuName = "Data/Knife")]
public class KnifeData : ScriptableObject
{
    public float trashSpaceValue = 1f;
    public float followingSpeed = 40f;
    public float timeToPutOnTrash = 0.3f;
    public AudioClip[] audioClips;
}
