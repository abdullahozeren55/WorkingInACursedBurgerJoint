using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewTrashData", menuName = "Data/Trash")]
public class TrashData : ScriptableObject
{
    public float trashSpaceValue = 1f;
    public float followingSpeed = 40f;
    public float timeToPutOnTray = 0.3f;


    public AudioClip[] audioClips;
}
