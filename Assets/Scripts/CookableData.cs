using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCookableData", menuName = "Data/Cookable")]
public class CookableData : ScriptableObject
{
    public GameObject cookingParticles;
    public AudioClip cookingSound;
    public float audioFadeOutDuration = 1f;

    public Material[] materials = new Material[3]; //0 raw, 1 regular, 2 burnt
    public float[] cookTime = new float[2]; //0 raw to regular, 1 regular to burnt
}
