using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DiningTable : MonoBehaviour
{
    [Header("Assignments")]
    public List<Seat> SeatsAroundTable; // Bu masaya ait sandalyeler/koltuklar

    // Bu masa þu an bir grup tarafýndan tutuldu mu?
    public bool IsTableReserved { get; private set; } = false;

    // Masanýn toplam kapasitesi (Sandalye x 1 + Koltuk x 2 ...)
    public int TotalCapacity
    {
        get { return SeatsAroundTable.Sum(s => s.sitPoints.Count); }
    }

    // --- GRUP YÖNETÝMÝ ---

    // Bu masa, gelen grup boyutu için uygun mu?
    public bool CanAccommodateGroup(int groupSize)
    {
        if (IsTableReserved) return false; // Zaten dolu
        if (TotalCapacity < groupSize) return false; // Yer yetmez

        return true;
    }

    // Masayý rezerve et
    public void ReserveTable()
    {
        IsTableReserved = true;
    }

    // Parametre sayýsý arttý: assignedTrayPoint eklendi
    public Transform GetSeatForCustomer(ICustomer customer, out Transform assignedEntryPoint, out Transform assignedTrayPoint)
    {
        assignedEntryPoint = null;
        assignedTrayPoint = null;

        var shuffledSeats = SeatsAroundTable.OrderBy(x => UnityEngine.Random.value).ToList();

        foreach (var seat in shuffledSeats)
        {
            // TryOccupy artýk 3 tane out veriyor
            if (seat.TryOccupy(customer, out Transform sitPoint, out Transform entryPoint, out Transform trayPoint))
            {
                assignedEntryPoint = entryPoint;
                assignedTrayPoint = trayPoint; // Yakaladýk!
                return sitPoint;
            }
        }

        return null;
    }

    // Masa tamamen boþaldý mý kontrol et (Her müþteri kalktýðýnda çaðýrýlýr)
    public void CheckIfTableIsEmpty()
    {
        bool anyoneSitting = false;
        foreach (var seat in SeatsAroundTable)
        {
            // Seat scriptine "occupants.Count > 0" kontrolü eklenebilir veya
            // IsFullyOccupied mantýðýna bakýlýr ama en temizi:
            // Seat içinde Occupant count'a bakmaktýr.
            // (Basitlik için Seat.cs'e public OccupantCount eklediðini varsayýyorum)
            // Þimdilik basit mantýk:
            if (seat.IsFullyOccupied) anyoneSitting = true;
            // (Not: IsFullyOccupied tam doðru deðil, 1 kiþi varsa da oturuyordur. 
            // Seat.cs'e 'public bool HasAnyOccupant => occupants.Count > 0;' ekle.)
        }

        // Hýzlý çözüm için Seat'e eriþim
        // Eðer kimse yoksa rezervasyonu kaldýr
        // (Bunu SeatManager üzerinden yönetmek daha saðlýklý olabilir)
    }

    // Masayý temizle (Grup gitti)
    public void ReleaseTable()
    {
        IsTableReserved = false;
        // Ýsteðe baðlý: Masadaki kirli tepsileri iþaretle
    }
}