using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarManager : MonoBehaviour
{
    public static CarManager Instance;

    [System.Serializable]
    public class CarDestinations
    {
        public Transform spawnPoint;
        public Quaternion spawnQuaternion;
        public Transform[] endPoint;
    }

    [SerializeField] private Material[] car0Materials;
    [SerializeField] private CarDestinations[] carDestinations;
    [SerializeField] private GameObject car0GO;
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

        SpawnRandomCar0();
        SpawnRandomCar0();
        SpawnRandomCar0();
        SpawnRandomCar0();
        SpawnRandomCar0();
    }

    public Material GetRandomCar0Material() => car0Materials[Random.Range(0, car0Materials.Length)];

    private CarDestinations GetRandomDestination() => carDestinations[Random.Range(0, carDestinations.Length)];

    public void SpawnRandomCar0()
    {
        var chosen = GetRandomDestination();

        GameObject car = Instantiate(car0GO, chosen.spawnPoint.position, chosen.spawnQuaternion);

        // yeni arabaya rotayý direkt aktar
        var carScript = car.GetComponent<Car>();
        if (carScript != null)
            carScript.DecideDestinations(chosen);
    }
}
