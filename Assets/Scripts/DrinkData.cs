using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDrinkData", menuName = "Data/Drink")]
public class DrinkData : ScriptableObject
{
    public GameManager.DrinkTypes drinkType;
    [Space]
    public Vector3 grabPositionOffset;
    public Vector3 grabRotationOffset;


    public AudioClip[] audioClips;
    
}
