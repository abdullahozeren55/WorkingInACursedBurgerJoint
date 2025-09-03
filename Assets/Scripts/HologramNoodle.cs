using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HologramNoodle : MonoBehaviour
{
    private enum HologramType
    {
        NOODLE,
        STORE,
        SAUCEPACK
    }

    private Collider col;

    [SerializeField] private HologramType hologramType;

    private void Awake()
    {
        col = GetComponent<Collider>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Noodle") && (hologramType == HologramType.NOODLE || hologramType == HologramType.STORE))
        {
            col.enabled = false;

            other.GetComponent<Noodle>().PutOnHologram(transform.position, transform.rotation, hologramType == HologramType.STORE);
        }
        else if (other.CompareTag("SaucePack") && hologramType == HologramType.SAUCEPACK)
        {
            col.enabled = false;

            other.GetComponent<SaucePack>().PutOnHologram(transform.position, transform.rotation);
        }
    }
}
