using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CustomerManager : MonoBehaviour
{
    public static CustomerManager Instance { get; private set; }

    [System.Serializable]
    public struct CharacterReference
    {
        public CustomerID ID;
        public CustomerController Controller;
    }

    [Header("Scene References")]
    public List<CharacterReference> SceneCharacters;

    // Kasadakiler Listesi
    private readonly List<CustomerController> customersAtCounter = new List<CustomerController>();
    public List<CustomerController> GetCustomersAtCounter() => customersAtCounter;

    // --- YENÝ: EVENTLER ---
    // Tek müþteri kasaya geldi / kasadan ayrýldý
    public event System.Action<CustomerController> OnCustomerArrivedAtCounter;
    public event System.Action<CustomerController> OnCustomerLeftCounter;

    // Kasa tamamen boþaldý (bu senin “wave bitti” koþulun)
    public event System.Action OnCounterEmpty;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void UpdateMonitorWithGroupOrders()
    {
        var customers = GetCustomersAtCounter();
        if (customers == null || customers.Count == 0)
        {
            MonitorManager.Instance.ClearCurrentOrder();
            return;
        }

        List<OrderData> groupOrders = new List<OrderData>();

        foreach (var customer in customers)
        {
            OrderData individualOrder = customer.GetCurrentOrder();
            if (individualOrder != null)
                groupOrders.Add(individualOrder);
        }

        MonitorManager.Instance.SetCurrentOrder(groupOrders);
    }

    public void RegisterCustomerAtCounter(CustomerController customer)
    {
        if (!customersAtCounter.Contains(customer))
        {
            customersAtCounter.Add(customer);
            OnCustomerArrivedAtCounter?.Invoke(customer);
        }
    }

    public void UnregisterCustomerAtCounter(CustomerController customer)
    {
        if (!customersAtCounter.Contains(customer))
            return;

        customersAtCounter.Remove(customer);

        UpdateMonitorWithGroupOrders();

        // Tek müþteri ayrýldý
        OnCustomerLeftCounter?.Invoke(customer);

        // Kasa tamamen boþaldý
        if (customersAtCounter.Count == 0)
            OnCounterEmpty?.Invoke();
    }

    public bool TryServeTray(Tray tray)
    {
        if (customersAtCounter.Count == 0) return false;

        bool isAnyoneWaiting = customersAtCounter.Any(x => x.CurrentState == CustomerState.WaitingForFood);
        if (!isAnyoneWaiting) return false;

        var victim = customersAtCounter
            .Where(x => x.CurrentState == CustomerState.WaitingForFood)
            .OrderBy(x => (int)x.CurrentProfile.ID)
            .FirstOrDefault();

        if (tray.CurrentContent.IsEmpty())
        {
            if (victim != null) victim.OnEmptyTrayReceived();
            return false;
        }

        foreach (var customer in customersAtCounter)
        {
            if (customer.CurrentState == CustomerState.WaitingForFood)
            {
                if (customer.TryReceiveTray(tray)) return true;
            }
        }

        if (victim != null) victim.OnWrongOrderReceived();
        return false;
    }

    // --- YENÝ: DIÞARIDAN ÇAÐRILACAK SPAWN ---
    public bool SpawnGroupPublic(CustomerGroupData group)
    {
        int groupSize = group.Members.Count;
        DiningTable allocatedTable = SeatManager.Instance.FindTableForGroup(groupSize);

        if (allocatedTable == null)
        {
            Debug.LogError($"Grup '{group.GroupName}' için masa yok!");
            return false;
        }

        Debug.Log($"Grup Spawnlanýyor: {group.GroupName} ({groupSize} Kiþi)");

        float counterSpacing = 0.8f;
        float totalCounterWidth = (groupSize - 1) * counterSpacing;
        float startCounterX = -totalCounterWidth / 2f;

        float spawnSpacing = 0.6f;
        float totalSpawnWidth = (groupSize - 1) * spawnSpacing;
        float startSpawnX = -totalSpawnWidth / 2f;

        Transform spawnOrigin = WorldManager.Instance.GetSpawnPosition();

        List<CustomerController> currentGroupControllers = new List<CustomerController>();
        int spawnedCount = 0;

        for (int i = 0; i < groupSize; i++)
        {
            var assignment = group.Members[i];
            var charRef = SceneCharacters.FirstOrDefault(x => x.ID == assignment.ID);

            if (charRef.Controller != null)
            {
                CustomerController customer = charRef.Controller;
                currentGroupControllers.Add(customer);
                spawnedCount++;

                float myCounterOffset = startCounterX + (i * counterSpacing);
                customer.SetCounterOffset(myCounterOffset);

                float mySpawnOffset = startSpawnX + (i * spawnSpacing);
                Vector3 targetSpawnPos = spawnOrigin.position + (spawnOrigin.right * mySpawnOffset);

                UnityEngine.AI.NavMeshHit hit;
                if (UnityEngine.AI.NavMesh.SamplePosition(targetSpawnPos, out hit, 2.0f, UnityEngine.AI.NavMesh.AllAreas))
                    customer.transform.position = hit.position;
                else
                    customer.transform.position = spawnOrigin.position;

                customer.gameObject.SetActive(true);
                customer.AssignTable(allocatedTable);
                customer.Initialize(assignment.ProfileForToday);
            }
        }

        foreach (var member in currentGroupControllers)
            member.SetGroupMembers(currentGroupControllers);

        if (spawnedCount == 0)
        {
            Debug.LogError($"Grup '{group.GroupName}' için hiç müþteri spawnlanmadý (ID eþleþmesi / SceneCharacters kontrol).");
            return false;
        }

        return true;
    }
}
