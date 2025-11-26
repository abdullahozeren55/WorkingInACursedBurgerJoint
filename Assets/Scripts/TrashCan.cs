using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashCan : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Trash") ||
            other.gameObject.CompareTag("BurgerBox") ||
            other.gameObject.CompareTag("BurgerBoxClosed") ||
            other.gameObject.CompareTag("BurgerIngredient") ||
            other.gameObject.CompareTag("Knife") ||
            other.gameObject.CompareTag("WholeIngredient") ||
            other.gameObject.CompareTag("FoodPack") ||
            other.gameObject.CompareTag("Drink"))
        {
            Destroy(other.gameObject);
        }
    }
}
