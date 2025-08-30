using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HologramNoodle : MonoBehaviour
{
    private Collider col;

    private void Awake()
    {
        col = GetComponent<Collider>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Noodle"))
        {
            col.enabled = false;

            other.GetComponent<Noodle>().PutOnHologram(transform.localPosition, transform.localRotation, false);
        }
    }
}
