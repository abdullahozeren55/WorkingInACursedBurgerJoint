using UnityEngine;

// Zemin türlerini burada tanýmlýyoruz.
// Ýstediðin kadar ekleyip çýkarabilirsin.
public enum SurfaceType
{
    Wood,   // Tahta
    Metal,  // Metal
    Grass,  // Çimen
    Stone,  // Taþ (Beton)
    Tile,   // Fayans
    Gravel  // Çakýl
}

public class SurfaceIdentity : MonoBehaviour
{
    [Header("Bu Zemin Hangi Türden?")]
    public SurfaceType type;
}