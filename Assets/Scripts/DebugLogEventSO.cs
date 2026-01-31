using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Scenario Events/Debug Log")]
public class DebugLogEventSO : ScenarioEventSO
{
    public string message = "Scenario event played";

    public override IEnumerator Play(ScenarioContext ctx)
    {
        Debug.Log(message);
        yield return null;
    }
}
