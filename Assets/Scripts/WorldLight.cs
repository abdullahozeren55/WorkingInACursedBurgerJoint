using UnityEngine;

public class WorldLight : MonoBehaviour
{
    void Awake()
    {
        LoopManager.Instance.RegisterLight(gameObject);
        gameObject.SetActive(LoopManager.Instance.CurrentLoopState.shouldLightsUp);
    }

    void OnDestroy()
    {
        // Eðer oyun kapanýyorsa ve DayManager çoktan gittiyse hata verme
        if (LoopManager.Instance != null)
        {
            LoopManager.Instance.UnregisterLight(gameObject);
        }
    }
}