using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewNoodleData", menuName = "Data/Noodle")]
public class NoodleData : ScriptableObject
{
    public float trashSpaceValue = 1f;
    public float followingSpeed = 40f;
    public float timeToPutOnTray = 0.3f;

    public AudioClip[] audioClips;
}
