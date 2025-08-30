using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyColliderController : MonoBehaviour
{
    public ICustomer currentCustomer;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("BurgerBoxClosed"))
        {
            Destroy(other.gameObject);
            BurgerBox burgerBox = other.GetComponent<BurgerBox>();
            currentCustomer.ReceiveBurger(burgerBox);
        }
    }
}
