using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTrigger : MonoBehaviour
{
    public enum TriggerType
    {
        CloseTheDoorAndStartNoodlePrepare
    }

    public TriggerType type;

    private Collider col;

    private void Awake()
    {
        col = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            col.enabled = false;

            if (type == TriggerType.CloseTheDoorAndStartNoodlePrepare)
                NoodleManager.Instance.HandleCloseTheDoorAndStartNoodlePrepare();
        }
    }
}
