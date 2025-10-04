using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderThrowArea : MonoBehaviour
{
    [HideInInspector] public bool ShouldReceive;

    private void OnTriggerEnter(Collider other)
    {
        if (ShouldReceive)
        {
            if (other.CompareTag("Drink"))
            {
                Drink drink = other.GetComponent<Drink>();

                if (!drink.IsGrabbed && drink.CanBeReceived)
                {
                    GameManager.Instance.CustomerReceiveDrink(drink);
                }
                
            }
            else if (other.CompareTag("BurgerBoxClosed"))
            {
                BurgerBox burgerBox = other.GetComponent<BurgerBox>();

                if (!burgerBox.IsGrabbed && burgerBox.CanBeReceived)
                {
                    GameManager.Instance.CustomerReceiveBurger(burgerBox);
                }
                
            }
        }
    }
}
