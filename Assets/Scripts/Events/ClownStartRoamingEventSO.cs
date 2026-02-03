using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Scenario Events/Set Clown To Roam")]
public class ClownStartRoamingEventSO : ScenarioEventSO
{
    public float delayAfterTriggered = 10f;
    public override IEnumerator Play(ScenarioContext ctx)
    {
        yield return new WaitForSeconds(delayAfterTriggered);

        if (Clown.Instance)
            Clown.Instance.SetState(Clown.ClownState.Roaming);
    }
}
