using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWholeIngredientData", menuName = "Data/WholeIngredient")]
public class WholeIngredientData : ScriptableObject
{
    public GameObject destroyParticle;
    public float followingSpeed = 40f;
    public float trashSpaceValue = 1f;
    public float timeToPutOnTray = 0.3f;
    public float minForce = 0.2f;
    public float maxForce = 0.4f;
    public int objectAmount = 4;

    public AudioClip[] audioClips;
}
