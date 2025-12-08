using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarManager : MonoBehaviour
{
    public static CarManager Instance;

    [Header("Spawn Settings")]
    public bool CanSpawn = true;
    public float spawnCooldown = 5f;
    public int maxCarCount = 30;

    // Aktif arabalarý takip listesi
    private readonly List<GameObject> activeCars = new List<GameObject>();

    [System.Serializable]
    public class CarDestinations
    {
        public Transform spawnPoint;
        public Quaternion spawnQuaternion;
        public Transform[] endPoint;
    }

    [Header("Optimization Settings")]
    // ARTIK MATERYAL DEÐÝL, SADECE RENK TUTUYORUZ (Batching bozulmasýn diye)
    public Color[] carColors;

    [SerializeField] private CarDestinations[] carDestinations;
    [SerializeField] private GameObject car0GO;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        StartCoroutine(SpawnRandomCar0Repeatedly());
    }

    public Color GetRandomCarColor()
    {
        if (carColors == null || carColors.Length == 0) return Color.white;
        return carColors[Random.Range(0, carColors.Length)];
    }

    private CarDestinations GetRandomDestination()
    {
        if (carDestinations == null || carDestinations.Length == 0) return null;
        return carDestinations[Random.Range(0, carDestinations.Length)];
    }

    public void SpawnRandomCar0()
    {
        // Temizlik: Yok olmuþ objeleri listeden düþür
        activeCars.RemoveAll(item => item == null);

        if (!CanSpawn || activeCars.Count >= maxCarCount) return;

        var chosen = GetRandomDestination();
        if (chosen == null) return;

        GameObject car = Instantiate(car0GO, chosen.spawnPoint.position, chosen.spawnQuaternion);

        activeCars.Add(car);

        var carScript = car.GetComponent<Car>();
        if (carScript != null)
        {
            carScript.DecideDestinations(chosen);
            // Araba yok olunca listeden silinecek
            carScript.OnCarDestroyed += HandleCarDestroyed;
        }
    }

    private void HandleCarDestroyed(GameObject car)
    {
        if (activeCars.Contains(car))
            activeCars.Remove(car);
    }

    private IEnumerator SpawnRandomCar0Repeatedly()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(spawnCooldown * 0.8f, spawnCooldown * 1.2f));
            SpawnRandomCar0();
        }
    }
}