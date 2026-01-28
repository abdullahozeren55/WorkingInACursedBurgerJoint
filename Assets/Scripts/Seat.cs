using UnityEngine;
using System.Collections.Generic;

public class Seat : MonoBehaviour
{
    [Header("Settings")]
    // Oturulacak fiziksel noktalar (Transformlar).
    // Tekli sandalye için 1 tane, Koltuk için 2 tane nokta ekle.
    public List<Transform> sitPoints;

    // YENÝ: Her oturma noktasýna karþýlýk gelen tepsi noktasý (Sýrasý ayný olmalý!)
    // Örn: sitPoints[0] -> trayPoints[0] ile eþleþir.
    public List<Transform> trayPoints;

    // Þu an bu koltukta kimler oturuyor?
    private Dictionary<Transform, ICustomer> occupants = new Dictionary<Transform, ICustomer>();

    // YENÝ: Müþterinin buraya oturmadan önce yürüyeceði nokta
    public Transform entryPoint;

    public bool IsFullyOccupied => occupants.Count >= sitPoints.Count;

    // Boþ bir nokta ver
    public Transform GetFreePoint()
    {
        // Önce boþ olan tüm noktalarý bir listeye topla
        List<Transform> availablePoints = new List<Transform>();

        foreach (var point in sitPoints)
        {
            if (!occupants.ContainsKey(point))
            {
                availablePoints.Add(point);
            }
        }

        // Eðer hiç boþ yer yoksa null dön
        if (availablePoints.Count == 0) return null;

        // Boþ yerler arasýndan RASTGELE birini seç
        return availablePoints[Random.Range(0, availablePoints.Count)];
    }

    // --- TryOccupy GÜNCELLENDÝ (3. out parametresi eklendi) ---
    public bool TryOccupy(ICustomer customer, out Transform sitTransform, out Transform entryTransform, out Transform assignedTrayPoint)
    {
        sitTransform = null;
        entryTransform = null;
        assignedTrayPoint = null; // Default

        // 1. Boþ noktayý bulurken index'ini de bulmamýz lazým
        int foundIndex = -1;
        Transform freePoint = null;

        for (int i = 0; i < sitPoints.Count; i++)
        {
            if (!occupants.ContainsKey(sitPoints[i]))
            {
                freePoint = sitPoints[i];
                foundIndex = i;
                break; // Ýlk boþu al
            }
        }

        if (freePoint != null && foundIndex != -1)
        {
            occupants.Add(freePoint, customer);
            sitTransform = freePoint;

            // Entry point (yoksa sitPoint ver)
            entryTransform = (entryPoint != null) ? entryPoint : freePoint;

            // YENÝ: Tray Point Eþleþmesi
            // Eðer trayPoints listesi doluysa ve index sýnýrlarý içindeyse
            if (trayPoints != null && foundIndex < trayPoints.Count)
            {
                assignedTrayPoint = trayPoints[foundIndex];
            }
            else
            {
                // Fallback: Eðer editörde atamayý unuttuysan masanýn ortasýna falan koymasýn diye uyarý
                Debug.LogWarning($"{name} objesinde TrayPoint eksik! Index: {foundIndex}");
                assignedTrayPoint = freePoint; // Mecbur kucaðýna koyacak
            }

            return true;
        }
        return false;
    }

    // Müþteriyi kaldýr
    public void Release(ICustomer customer)
    {
        // Dictionary'den value ile key bulma (biraz tersten ama güvenli)
        Transform keyToRemove = null;
        foreach (var pair in occupants)
        {
            if (pair.Value == customer)
            {
                keyToRemove = pair.Key;
                break;
            }
        }

        if (keyToRemove != null)
        {
            occupants.Remove(keyToRemove);
        }
    }
}