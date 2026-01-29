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

    public List<CustomerController> GetCustomersAtCounter()
    {
        return customersAtCounter;
    }

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

        // --- HÝYERARÞÝK MAÐDUR SEÇÝMÝ (GÜNCELLENDÝ) ---
        // 1. Sadece yemek bekleyenleri al
        // 2. ID'yi artýk doðrudan SCRIPTABLE OBJECT (Profile) üzerinden okuyoruz.
        //    Böylece kod içinde ID atamayý unutsan bile Data neyse o çalýþýr.
        var victim = customersAtCounter
            .Where(x => x.CurrentState == CustomerState.WaitingForFood)
            .OrderBy(x => (int)x.CurrentProfile.ID) // <--- DEÐÝÞÝKLÝK BURADA
            .FirstOrDefault(); // En baþtakini (Lideri) seç
                               // --------------------------------

        // 1. BOÞ TEPSÝ KONTROLÜ
        if (tray.CurrentContent.IsEmpty())
        {
            if (victim != null) victim.OnEmptyTrayReceived();
            return false;
        }

        // 2. DOÐRU SÝPARÝÞ KONTROLÜ
        // (Burasý ayný kalýyor, doðru tepsiyi kimin aldýðý önemli deðil, sahibi alýr)
        foreach (var customer in customersAtCounter)
        {
            if (customer.CurrentState == CustomerState.WaitingForFood)
            {
                if (customer.TryReceiveTray(tray)) return true;
            }
        }

        // 3. YANLIÞ SÝPARÝÞ CEZASI
        // Lider (victim) konuþsun
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
            if (customersAtCounter.Count == 0) StartCoroutine(PrepareNextWaveRoutine());
            return;
        }

        Debug.Log($"Grup Spawnlanýyor: {group.GroupName} ({groupSize} Kiþi)");

        // --- KASA DÝZÝLÝM HESABI (Bu zaten vardý) ---
        float counterSpacing = 0.8f;
        float totalCounterWidth = (groupSize - 1) * counterSpacing;
        float startCounterX = -totalCounterWidth / 2f;

        // --- YENÝ: SPAWN DÝZÝLÝM HESABI ---
        // Spawn noktasýnda da birbirlerini ezmesinler diye hafif aralýklý doðacaklar.
        // Ama çok açýlmasýnlar ki sokaða taþmasýnlar (0.6f ideal).
        float spawnSpacing = 0.6f;
        float totalSpawnWidth = (groupSize - 1) * spawnSpacing;
        float startSpawnX = -totalSpawnWidth / 2f;

        Transform spawnOrigin = WorldManager.Instance.GetSpawnPosition();
        // ---------------------------------

        List<CustomerController> currentGroupControllers = new List<CustomerController>();

        for (int i = 0; i < groupSize; i++)
        {
            var assignment = group.Members[i];
            var charRef = SceneCharacters.FirstOrDefault(x => x.ID == assignment.ID);

            if (charRef.Controller != null)
            {
                CustomerController customer = charRef.Controller;
                currentGroupControllers.Add(customer);

                // 1. Kasa Ofsetini Ata (Hedef)
                float myCounterOffset = startCounterX + (i * counterSpacing);
                customer.SetCounterOffset(myCounterOffset);

                // 2. Spawn Pozisyonunu Hesapla (Baþlangýç)
                float mySpawnOffset = startSpawnX + (i * spawnSpacing);

                // Spawn noktasýnýn saðýna soluna (Right vektörü) göre yerleþtir
                Vector3 targetSpawnPos = spawnOrigin.position + (spawnOrigin.right * mySpawnOffset);

                // --- KRÝTÝK DÜZELTME: NAVMESH SNAP ---
                // Hesapladýðýmýz nokta belki duvara denk geldi. 
                // "Bana bu noktaya en yakýn, üzerinde yürünebilir (Walkable) yeri ver" diyoruz.
                UnityEngine.AI.NavMeshHit hit;

                // 1.0f yarýçap içinde geçerli bir zemin ara
                if (UnityEngine.AI.NavMesh.SamplePosition(targetSpawnPos, out hit, 2.0f, UnityEngine.AI.NavMesh.AllAreas))
                {
                    customer.transform.position = hit.position;
                }
                else
                {
                    // Eðer bulamazsa (çok nadir), spawn noktasýnýn tam merkezini kullan
                    customer.transform.position = spawnOrigin.position;
                }
                // -------------------------------------

                customer.gameObject.SetActive(true);
                customer.AssignTable(allocatedTable);
                customer.Initialize(assignment.ProfileForToday);
            }
        }

        // Grubu Tanýþtýr
        foreach (var member in currentGroupControllers)
        {
            member.SetGroupMembers(currentGroupControllers);
        }
    }
}