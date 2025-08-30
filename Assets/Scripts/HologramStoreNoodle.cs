using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HologramStoreNoodle : MonoBehaviour
{
    [SerializeField] private NoodleShelf noodleShelf;
    [SerializeField] private ShopSeller seller;

    private int uninteractableLayer;

    private void Awake()
    {
        uninteractableLayer = LayerMask.NameToLayer("Uninteractable");
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Noodle"))
        {
            seller.noodle = other.GetComponent<Noodle>();
            noodleShelf.ChangeLayer(uninteractableLayer);
            seller.isNoodlePlaced = true;
            other.GetComponent<Noodle>().PutOnHologram(transform.position, transform.rotation, true);
        }
    }
}
