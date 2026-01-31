using System;
using System.Collections.Generic;
using UnityEngine;

public enum ScenarioStepType
{
    Wave,   // müþteri grubu çaðýr
    Wait,   // saniye bekle
    Event   // scripted olay oynat
}

[Serializable]
public class ScenarioStep
{
    public string name;
    public ScenarioStepType type;

    [Header("Wave")]
    public CustomerGroupData wave; // sende zaten var diye varsayýyorum

    [Header("Wait")]
    public float waitSeconds;

    [Header("Event")]
    public ScenarioEventSO ev;
}

[CreateAssetMenu(menuName = "Game/Scenario Asset")]
public class ScenarioAsset : ScriptableObject
{
    public List<ScenarioStep> steps = new();
}
