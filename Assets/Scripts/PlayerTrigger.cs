using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTrigger : MonoBehaviour
{
    [Header("CloseTheDoorAndStartNoodlePrepare")]
    public Door houseDoor;
    public enum TriggerType
    {
        CloseTheDoorAndStartNoodlePrepare
    }

    public TriggerType type;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (type == TriggerType.CloseTheDoorAndStartNoodlePrepare)
            {

            }
        }
    }
}
