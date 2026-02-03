using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Scenario/Events/Resume Spawning")]
public class ResumeSpawnEventSO : ScenarioEventSO
{
    [Tooltip("Resume etmeden önce kaç saniye beklesin?")]
    public float delayBeforeResume = 0f;

    public override IEnumerator Play(ScenarioContext ctx)
    {
        if (delayBeforeResume > 0)
            yield return new WaitForSeconds(delayBeforeResume);

        ctx.scenario.ResumeSpawning();
    }
}