using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Scenario Events/Play Random Horror Music")]
public class PlayRandomHorrorMusicEventSO : ScenarioEventSO
{
    public float delayAfterTriggered = 0f;
    public override IEnumerator Play(ScenarioContext ctx)
    {
        yield return new WaitForSeconds(delayAfterTriggered);

        if (EventManager.Instance)
            EventManager.Instance.PlayRandomHorrorMusic();
    }
}
