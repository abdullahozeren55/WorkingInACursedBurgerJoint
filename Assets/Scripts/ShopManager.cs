using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private int startMoney;
    [SerializeField] private CursorFollow cursorFollow;
    [SerializeField] private List<Image> priceImages;
    [SerializeField] private List<Sprite> whiteSprites;
    [SerializeField] private List<Sprite> redSprites;
    [SerializeField] private List<int> prices;

    [SerializeField] private Transform spawnAreaCenter;
    [SerializeField] private Vector3 spawnAreaMin = new Vector3(-5f, 0, -5f);
    [SerializeField] private Vector3 spawnAreaMax = new Vector3(5f, 0, 5f);

    private bool isBought = false;
    public int moneyAmount {  get; private set; }

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

        moneyAmount = 0;
        ChangeMoney(startMoney);
    }

    public void ChangeMoney(int amount)
    {
        moneyAmount += amount;

        moneyText.text = "KASA: " + moneyAmount.ToString() + " TL";

        for (int i = 0; i < prices.Count; i++)
        {
            if (prices[i] > moneyAmount)
            {
                priceImages[i].sprite = redSprites[i];
            }
            else
            {
                priceImages[i].sprite = whiteSprites[i];
            }
        }
    }

    public void BuyIngredient(int price)
    {
        if (moneyAmount - price >= 0 && cursorFollow.shouldBuy)
        {
            ChangeMoney(-price);
            isBought = true;
        }
    }

    public void SpawnTheIngredient(GameObject supply)
    {
        if (isBought && cursorFollow.shouldBuy)
        {
            isBought = false;
            SpawnSupply(supply);
        }
            
    }

    private void SpawnSupply(GameObject supplyToSpawn)
    {

        // Calculate the spawn area width and height based on the original spawn area min and max
        float width = spawnAreaMax.x - spawnAreaMin.x;
        float depth = spawnAreaMax.z - spawnAreaMin.z;

        // Generate a random offset from the center within the calculated spawn area
        Vector3 randomOffset = new Vector3(
            Random.Range(-width / 2, width / 2),  // Random offset in X based on width
            spawnAreaMin.y,                      // Keep the Y position fixed (you can modify this as needed)
            Random.Range(-depth / 2, depth / 2)   // Random offset in Z based on depth
        );

        // Use the spawnAreaCenter position and add the random offset to determine the spawn position
        Vector3 spawnPosition = spawnAreaCenter.position + randomOffset;

        // Use the rotation of this GameObject (the one this script is attached to)
        Quaternion rotation = Random.rotation;

        // Instantiate the supply at the random position with the rotation
        Instantiate(supplyToSpawn, spawnPosition, rotation);
    }

    private void OnDrawGizmos()
    {
        // Ensure spawnAreaCenter is set before drawing the Gizmo
        if (spawnAreaCenter == null)
        {
            Debug.LogWarning("Spawn Area Center is not set! Unable to draw spawn area Gizmo.");
            return;
        }

        // Calculate the size of the spawn area based on the min and max values
        Vector3 areaSize = new Vector3(
            spawnAreaMax.x - spawnAreaMin.x, // Width
            0f,                             // Height (since we are drawing in 2D for now)
            spawnAreaMax.z - spawnAreaMin.z  // Depth
        );

        // Set the color for the Gizmo
        Gizmos.color = Color.green;

        // Draw a wireframe cube at the spawn area center with the calculated size
        Gizmos.DrawWireCube(spawnAreaCenter.position, areaSize);

        // Optionally, you can also draw the actual spawn center as a sphere
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(spawnAreaCenter.position, 0.1f);  // Adjust the size as needed
    }
}
