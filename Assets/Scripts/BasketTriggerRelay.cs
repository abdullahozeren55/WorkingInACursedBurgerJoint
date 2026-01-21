using UnityEngine;

public class BasketTriggerRelay : MonoBehaviour
{
    [SerializeField] private Fryer connectedFryer;

    private void OnTriggerEnter(Collider other)
    {
        // Tetiklenince kararý Fryer'a býrak
        if (connectedFryer != null)
        {
            connectedFryer.HandleGlobalCatch(other);
        }
    }
}