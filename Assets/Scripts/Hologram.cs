using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hologram : MonoBehaviour
{
    private enum HologramType
    {
        HouseNoodle,
        HouseSaucePack,
        HouseKettle
    }

    [SerializeField] private HologramType hologramType;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Noodle") && hologramType == HologramType.HouseNoodle)
        {
            NoodleManager.Instance.PutCurrentNoodleOnHologramHouse();
        }
        else if (other.CompareTag("SaucePack") && hologramType == HologramType.HouseSaucePack)
        {
            NoodleManager.Instance.PutCurrentSaucePackOnHologram();
        }
        else if (other.CompareTag("Kettle") && hologramType == HologramType.HouseKettle)
        {
            NoodleManager.Instance.PutKettleOnHologram();
        }
    }
}
