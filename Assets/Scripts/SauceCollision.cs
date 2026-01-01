using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class SauceCollision : MonoBehaviour
{
    public SauceBottle.SauceType sauceType;
    public float targetScale = 0.15f;
    public float randomMultiplier = 1.2f;

    private ParticleSystem ps;
    private List<ParticleCollisionEvent> collisionEvents;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        collisionEvents = new List<ParticleCollisionEvent>();
    }

    void OnParticleCollision(GameObject other)
    {
        if (other.CompareTag("BurgerSauceArea"))
        {
            GameManager.Instance.AddSauceToTray(sauceType);
        }
        else
        {
            int count = ps.GetCollisionEvents(other, collisionEvents);

            for (int i = 0; i < count; i++)
            {
                Vector3 hitPoint = collisionEvents[i].intersection;
                Vector3 normal = collisionEvents[i].normal;

                // Normal yönüne göre rotation hesapla
                Quaternion finalRotation = Quaternion.LookRotation(normal) * Quaternion.Euler(0, 180, 0);

                float randomValue = Random.Range(targetScale / randomMultiplier, targetScale * randomMultiplier);

                Vector3 finalScale = new Vector3(randomValue, randomValue, randomValue);

                SauceManager.Instance.SpawnDrop(sauceType, hitPoint, finalRotation, finalScale, other.transform);
            }
        }
    }

}
