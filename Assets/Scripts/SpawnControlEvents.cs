using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Scenario/Events/Pause Spawning")]
public class PauseSpawningEvent : ScenarioEventSO
{
    public override IEnumerator Play(ScenarioContext ctx)
    {
        ctx.scenario.PauseSpawning();
        yield return null; // Anýnda gerçekleþir, bekleme yapmaz
    }
}