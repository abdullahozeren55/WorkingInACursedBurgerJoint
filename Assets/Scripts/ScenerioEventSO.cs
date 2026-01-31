using System.Collections;
using UnityEngine;

public abstract class ScenarioEventSO : ScriptableObject
{
    public abstract IEnumerator Play(ScenarioContext ctx);
}

public class ScenarioContext
{
    public CustomerManager customers;
    // ileride: audio, lights, dialogue, ui vs eklenir
}
