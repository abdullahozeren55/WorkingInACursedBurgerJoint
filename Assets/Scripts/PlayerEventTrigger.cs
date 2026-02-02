using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEventTrigger : MonoBehaviour
{
    public ScenarioEventSO[] events;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            foreach (var ev in events)
            {
                ScenarioManager.Instance.PlayEvent(ev);
            }

            Destroy(gameObject);
        }
    }
}
