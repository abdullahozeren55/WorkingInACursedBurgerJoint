using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SeatManager : MonoBehaviour
{
    public static SeatManager Instance { get; private set; }

    [SerializeField] private List<DiningTable> allTables;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        Instance = this;

        // Sahnedeki tüm masalarý otomatik bul (Opsiyonel)
        if (allTables == null || allTables.Count == 0)
            allTables = FindObjectsOfType<DiningTable>().ToList();
    }

    public DiningTable FindTableForGroup(int groupSize)
    {
        // 1. Kapasitesi yeten ve rezerve edilmemiþ masalarý bul
        var possibleTables = allTables
            .Where(t => t.CanAccommodateGroup(groupSize))
            .ToList();

        if (possibleTables.Count == 0) return null; // Yer yok!

        // 2. En verimli kapasiteyi bul (Örn: 2 kiþi için en iyi kapasite 2'dir, 4 deðil)
        int bestCapacity = possibleTables.Min(t => t.TotalCapacity);

        // 3. Bu kapasiteye sahip TÜM masalarý al (Örn: 3 tane 4 kiþilik masa varsa üçünü de al)
        var candidateTables = possibleTables
            .Where(t => t.TotalCapacity == bestCapacity)
            .ToList();

        // 4. Aralarýndan RASTGELE birini seç
        DiningTable selectedTable = candidateTables[Random.Range(0, candidateTables.Count)];

        selectedTable.ReserveTable();
        return selectedTable;
    }
}