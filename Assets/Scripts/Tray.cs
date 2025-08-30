using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tray : MonoBehaviour
{
    [SerializeField] private float startPointYHeight = 0.01f;
    [SerializeField] private Transform burgerBoxTransform;
    [SerializeField] private BoxCollider boxCollider;
    [SerializeField] private BoxCollider burgerCollider;

    [Header("Holograms")]
    [SerializeField] private GameObject onion;
    [SerializeField] private GameObject lettuce;
    [SerializeField] private GameObject tomato;
    [SerializeField] private GameObject pickle;
    [SerializeField] private GameObject patty;
    [SerializeField] private GameObject cheese;
    [SerializeField] private GameObject bottomBun;
    [SerializeField] private GameObject topBun;
    [SerializeField] private GameObject box;

    private Vector3 currentLocationToPutBurgerIngredient;
    private Vector3 hologramLocation;

    private List<BurgerIngredient> allBurgerIngredients = new List<BurgerIngredient>();
    private List<SauceBottle.SauceType> allSauces = new List<SauceBottle.SauceType>();

    private float boxColliderStartZ;
    private float boxColliderStartCenterZ;

    private float burgerColliderStartZ;
    private float burgerColliderStartCenterZ;

    private bool burgerIsDone;

    [HideInInspector] public BurgerIngredient currentIngredient;
    [HideInInspector] public BurgerBox currentBox;

    private int onTrayLayer;

    private void Awake()
    {
        currentIngredient = null;
        currentBox = null;

        boxColliderStartZ = boxCollider.size.z;
        boxColliderStartCenterZ = boxCollider.center.z;

        burgerColliderStartZ = burgerCollider.size.z;
        burgerColliderStartCenterZ = burgerCollider.center.z;

        burgerIsDone = false;

        ResetPosition();

        onTrayLayer = LayerMask.NameToLayer("OnTray");
    }

    private void UpdateCurrentLocationToPutBurgerIngredient(float heightIncreaseAmount)
    {
        currentLocationToPutBurgerIngredient.y += heightIncreaseAmount;

        Vector3 newSize = boxCollider.size;
        newSize.z += heightIncreaseAmount/12;
        Vector3 newCenter = boxCollider.center;
        newCenter.z += heightIncreaseAmount / 24f;
        boxCollider.size = newSize;
        boxCollider.center = newCenter;

        newSize = burgerCollider.size;
        newSize.z += heightIncreaseAmount/12;
        newCenter = burgerCollider.center;
        newCenter.z += heightIncreaseAmount / 24f;

        burgerCollider.size = newSize;
        burgerCollider.center = newCenter;

    }

    public void AddSauce(SauceBottle.SauceType type)
    {
        if (!allSauces.Contains(type))
            allSauces.Add(type);
    }

    public void ResetTray()
    {
        for (int i = 1; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < burgerCollider.transform.childCount; i++)
        {
            Destroy(burgerCollider.transform.GetChild(i).gameObject);
        }

        foreach (BurgerIngredient burgerIngredient in allBurgerIngredients)
        {
            currentBox.allBurgerIngredientTypes.Add(burgerIngredient.data.ingredientType);
        }

        foreach(SauceBottle.SauceType sauceType in allSauces)
        {
            currentBox.allSauces.Add(sauceType);
        }

        allBurgerIngredients.Clear();
        allSauces.Clear();

        burgerIsDone = false;
        currentBox = null;
        currentIngredient = null;

        ResetPosition();
    }

    public void TurnOnHologram(BurgerIngredientData.IngredientType type)
    {
        hologramLocation = currentLocationToPutBurgerIngredient;
        hologramLocation.y += currentIngredient.data.yHeight;

        if (allBurgerIngredients.Count == 0)
        {
            if (type == BurgerIngredientData.IngredientType.BOTTOMBUN && currentIngredient.cookAmount == Cookable.CookAmount.REGULAR)
            {
                currentIngredient.canAddToTray = true;
                bottomBun.transform.position = hologramLocation;
                bottomBun.SetActive(true);
            }
        }
        else if (!burgerIsDone)
        {
            if (type == BurgerIngredientData.IngredientType.PICKLE)
            {
                currentIngredient.canAddToTray = true;
                pickle.transform.position = hologramLocation;
                pickle.SetActive(true);
            }
            else if (type == BurgerIngredientData.IngredientType.LETTUCE)
            {
                currentIngredient.canAddToTray = true;
                lettuce.transform.position = hologramLocation;
                lettuce.SetActive(true);
            }
            else if (type == BurgerIngredientData.IngredientType.ONION)
            {
                currentIngredient.canAddToTray = true;
                onion.transform.position = hologramLocation;
                onion.SetActive(true);
            }
            else if (type == BurgerIngredientData.IngredientType.TOMATO)
            {
                currentIngredient.canAddToTray = true;
                tomato.transform.position = hologramLocation;
                tomato.SetActive(true);
            }
            else if (type == BurgerIngredientData.IngredientType.CHEESE)
            {
                currentIngredient.canAddToTray = true;
                cheese.transform.position = hologramLocation;
                cheese.SetActive(true);
            }
            else if (type == BurgerIngredientData.IngredientType.PATTY && currentIngredient.cookAmount == Cookable.CookAmount.REGULAR)
            {
                currentIngredient.canAddToTray = true;
                patty.transform.position = hologramLocation;
                patty.SetActive(true);
            }
            else if (type == BurgerIngredientData.IngredientType.BOTTOMBUN && currentIngredient.cookAmount == Cookable.CookAmount.REGULAR)
            {
                currentIngredient.canAddToTray = true;
                bottomBun.transform.position = hologramLocation;
                bottomBun.SetActive(true);
            }
            else if (type == BurgerIngredientData.IngredientType.TOPBUN && currentIngredient.cookAmount == Cookable.CookAmount.REGULAR)
            {
                currentIngredient.canAddToTray = true;
                topBun.transform.position = hologramLocation;
                topBun.SetActive(true);
            }
        }
    }

    public void TurnOnBoxHologram()
    {
        if (burgerIsDone)
        {
            currentBox.canAddToTray = true;
            box.transform.position = burgerBoxTransform.position;
            box.SetActive(true);
        }
    }

    public void TurnOffAllHolograms()
    {
        onion.SetActive(false);
        lettuce.SetActive(false);
        tomato.SetActive(false);
        pickle.SetActive(false);
        patty.SetActive(false);
        cheese.SetActive(false);
        bottomBun.SetActive(false);
        topBun.SetActive(false);
        box.SetActive(false);
    }

    private void ResetPosition()
    {
        currentLocationToPutBurgerIngredient = transform.position;

        Vector3 newSize = boxCollider.size;
        newSize.z = boxColliderStartZ;
        Vector3 newCenter = boxCollider.center;
        newCenter.z = boxColliderStartCenterZ;
        boxCollider.size = newSize;
        boxCollider.center = newCenter;

        newSize = burgerCollider.size;
        newSize.z = burgerColliderStartZ;
        newCenter = burgerCollider.center;
        newCenter.z = burgerColliderStartCenterZ;
        burgerCollider.size = newSize;
        burgerCollider.center = newCenter;

        burgerCollider.enabled = false;

        UpdateCurrentLocationToPutBurgerIngredient(startPointYHeight);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != onTrayLayer && (other.CompareTag("BurgerIngredient") || other.CompareTag("BurgerBox")))
        {
            if (currentIngredient != null && other.gameObject.GetInstanceID() == currentIngredient.gameObject.GetInstanceID() && currentIngredient.canAddToTray)
            {

                if (currentIngredient.data.ingredientType == BurgerIngredientData.IngredientType.TOPBUN)
                    burgerIsDone = true;

                if (!burgerCollider.enabled)
                    burgerCollider.enabled = true;

                allBurgerIngredients.Add(currentIngredient);

                UpdateCurrentLocationToPutBurgerIngredient(currentIngredient.data.yHeight);

                currentIngredient.PutOnTray(currentLocationToPutBurgerIngredient, this.transform);

                UpdateCurrentLocationToPutBurgerIngredient(currentIngredient.data.yHeight);
            }
            else if (currentBox != null && other.gameObject.GetInstanceID() == currentBox.gameObject.GetInstanceID() && currentBox.canAddToTray)
            {
                currentBox.PutOnTray(burgerBoxTransform.position);
            }
            
        }
    }
}
