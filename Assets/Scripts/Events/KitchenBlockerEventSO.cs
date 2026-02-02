using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Scenario Events/Block Kitchen")]
public class KitchenBlockerEventSO : ScenarioEventSO
{
    public float delayAfterTriggered = 0f;
    public override IEnumerator Play(ScenarioContext ctx)
    {
        yield return new WaitForSeconds(delayAfterTriggered);

        if (EventManager.Instance)
            EventManager.Instance.MakeKitchenUnleavable();
    }
}
