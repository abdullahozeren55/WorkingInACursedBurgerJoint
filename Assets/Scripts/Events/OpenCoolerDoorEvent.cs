using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Scenario Events/Open Cooler Door")]
public class OpenCoolerDoorEventSO : ScenarioEventSO
{
    public float delayAfterTriggered = 4f;
    public override IEnumerator Play(ScenarioContext ctx)
    {
        yield return new WaitForSeconds(delayAfterTriggered);

        if (EventManager.Instance)
            EventManager.Instance.OpenCoolerDoor();
    }
}