using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ErtanManager : MonoBehaviour
{
    public static ErtanManager Instance;

    [Header("Ertan Settings")]
    [SerializeField] private GameObject[] allErtans; //0 for regular, turning into an abomination as the number goes up
    [SerializeField] private Ertan[] allErtansSC; //0 for regular, turning into an abomination as the number goes up
    [Space]
    [SerializeField] private ICustomer.CustomerDayChangesSegment[] ErtanDayChanges;
    [Space]
    [HideInInspector] public bool ertanDidEatCheeseYesterday;
    [HideInInspector] public int levelOfMadness = 1; //1 for regular, turning into an abomination as the number goes up

    private void Awake()
    {
        if (Instance == null)
        {
            // If not, set this instance as the singleton
            Instance = this;

            // Optionally, mark GameManager as not destroyed between scene loads
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // If an instance already exists, destroy this one to enforce the singleton pattern
            Destroy(gameObject);
        }
    }
}
