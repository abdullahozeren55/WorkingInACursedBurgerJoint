using UnityEngine;
using System.Collections;
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

    [Header("Current Level")]
    public LevelScenario CurrentScenario;

    // Kasadakiler Listesi
    private List<CustomerController> customersAtCounter = new List<CustomerController>();

    // --- YENÝ: Dalga Takibi ---
    private int currentWaveIndex = 0; // Hangi gruptayýz?
    private bool isScenarioActive = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        Instance = this;
    }

    private IEnumerator Start()
    {
        yield return null; // Singletonlar otursun

        if (CurrentScenario != null && CurrentScenario.Waves.Count > 0)
        {
            StartScenario();
        }
    }

    // --- YENÝ: Senaryoyu Baþlat ---
    private void StartScenario()
    {
        currentWaveIndex = 0;
        isScenarioActive = true;

        // Ýlk grubu çaðýrmak için hazýrlýk yap
        // (Ýlk grubun delay'i genelde oyun baþlama süresidir)
        StartCoroutine(PrepareNextWaveRoutine());
    }

    // --- YENÝ: Sýradaki Grubu Hazýrla ve Çaðýr ---
    private IEnumerator PrepareNextWaveRoutine()
    {
        // 1. Senaryo bitti mi kontrol et
        if (currentWaveIndex >= CurrentScenario.Waves.Count)
        {
            Debug.Log("Tüm dalgalar bitti! Dükkan kapanabilir.");
            isScenarioActive = false;
            yield break;
        }

        // 2. Sýradaki grubun verisini al
        CustomerGroupData nextGroup = CurrentScenario.Waves[currentWaveIndex];

        // 3. Grubun gelmesi için gereken süreyi (Delay) bekle
        // "DelayAfterPreviousGroup", önceki grup gittikten kaç saniye sonra geleceklerini belirtir.
        Debug.Log($"Sýradaki grup ({nextGroup.GroupName}) için {nextGroup.DelayAfterPreviousGroup} saniye bekleniyor...");
        yield return new WaitForSeconds(nextGroup.DelayAfterPreviousGroup);

        // 4. Grubu Spawnla
        SpawnGroup(nextGroup);

        // 5. Index'i artýr ki bir sonraki sefere diðer grubu alalým
        currentWaveIndex++;
    }

    // --- MEVCUT: Liste Yönetimi (Güncellendi) ---
    public void RegisterCustomerAtCounter(CustomerController customer)
    {
        if (!customersAtCounter.Contains(customer))
        {
            customersAtCounter.Add(customer);
        }
    }

    public void UnregisterCustomerAtCounter(CustomerController customer)
    {
        if (customersAtCounter.Contains(customer))
        {
            customersAtCounter.Remove(customer);

            // --- YENÝ: ZÝNCÝRLEME TETÝKLEYÝCÝ ---
            // Eðer kasadaki SON kiþi de ayrýldýysa VE senaryo devam ediyorsa...
            if (customersAtCounter.Count == 0 && isScenarioActive)
            {
                Debug.Log("Kasa boþaldý. Sýradaki grup için sayaç baþlýyor.");
                StartCoroutine(PrepareNextWaveRoutine());
            }
        }
    }

    // --- MEVCUT: Sipariþ Kontrol ---
    public bool TryServeTray(Tray tray)
    {
        if (customersAtCounter.Count == 0) return false;

        // Erken Teslimat Kontrolü
        bool isAnyoneWaiting = customersAtCounter.Any(x => x.CurrentState == CustomerState.WaitingForFood);
        if (!isAnyoneWaiting) return false;

        foreach (var customer in customersAtCounter)
        {
            if (customer.CurrentState == CustomerState.WaitingForFood)
            {
                if (customer.TryReceiveTray(tray))
                {
                    return true;
                }
            }
        }

        // Yanlýþ sipariþ
        var victim = customersAtCounter.FirstOrDefault(x => x.CurrentState == CustomerState.WaitingForFood);
        if (victim != null) victim.OnWrongOrderReceived();

        return false;
    }

    // --- MEVCUT: Spawn Ýþlemi ---
    private void SpawnGroup(CustomerGroupData group)
    {
        int groupSize = group.Members.Count;
        DiningTable allocatedTable = SeatManager.Instance.FindTableForGroup(groupSize);

        if (allocatedTable == null)
        {
            Debug.LogError($"Grup '{group.GroupName}' için masa yok!");
            // Masa yoksa ne yapalým? 
            // 1. Bekletip sonra tekrar deneyebiliriz.
            // 2. Þimdilik pas geçip bir sonraki dalgayý tetikleyelim ki oyun týkanmasýn.
            if (customersAtCounter.Count == 0) StartCoroutine(PrepareNextWaveRoutine());
            return;
        }

        Debug.Log($"Grup Spawnlanýyor: {group.GroupName}");

        foreach (var assignment in group.Members)
        {
            var charRef = SceneCharacters.FirstOrDefault(x => x.ID == assignment.ID);
            if (charRef.Controller != null)
            {
                CustomerController customer = charRef.Controller;

                // Spawn Pozisyonu
                customer.transform.position = WorldManager.Instance.GetSpawnPosition().position;
                customer.gameObject.SetActive(true);

                // Masa ve Profil Atama
                customer.AssignTable(allocatedTable);
                customer.Initialize(assignment.ProfileForToday);
            }
        }
    }
}