using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashCan : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Trash"))
        {
            Destroy(other.gameObject);
        }
        else if (other.gameObject.CompareTag("BurgerBoxClosed"))
        {
            Destroy(other.gameObject);
        }
        else if (other.gameObject.CompareTag("BurgerIngredient"))
        {
            BurgerIngredient burgerIngredient = other.gameObject.GetComponent<BurgerIngredient>();

            if (burgerIngredient.cookAmount == Cookable.CookAmount.BURNT)
            {
                Destroy(other.gameObject);
            }
        }
    }
}
