using System.Collections;
using UnityEngine;

public class ScenarioManager : MonoBehaviour
{
    public static ScenarioManager Instance { get; private set; }

    [Header("Scenario")]
    [SerializeField] private LevelScenario scenario;
    [SerializeField] private bool autoStart = true;

    private Coroutine routine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        if (CustomerManager.Instance != null)
            CustomerManager.Instance.OnCustomerLeftCounter += HandleCustomerLeftCounter;
    }

    private void OnDisable()
    {
        if (CustomerManager.Instance != null)
            CustomerManager.Instance.OnCustomerLeftCounter -= HandleCustomerLeftCounter;
    }

    private void Start()
    {
        if (autoStart)
            StartScenario();
    }

    public void StartScenario()
    {
        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(RunScenario());
    }

    private IEnumerator RunScenario()
    {
        if (scenario == null || scenario.Waves == null || scenario.Waves.Count == 0)
        {
            Debug.LogWarning("ScenarioManager: Scenario atanmadý ya da Waves boþ.");
            yield break;
        }

        for (int i = 0; i < scenario.Waves.Count; i++)
        {
            var wave = scenario.Waves[i];

            if (wave.DelayAfterPreviousGroup > 0f)
                yield return new WaitForSeconds(wave.DelayAfterPreviousGroup);

            yield return RunWave(wave);
        }

        Debug.Log("ScenarioManager: Tüm waveler bitti.");
    }

    private IEnumerator RunWave(CustomerGroupData wave)
    {
        var cm = CustomerManager.Instance;
        if (cm == null)
        {
            Debug.LogError("ScenarioManager: CustomerManager.Instance yok.");
            yield break;
        }

        bool started = false;
        bool finished = false;

        void OnCounterEmpty()
        {
            if (started) finished = true;
        }

        cm.OnCounterEmpty += OnCounterEmpty;

        bool ok = cm.SpawnGroupPublic(wave);
        started = true;

        if (!ok)
        {
            Debug.LogWarning($"ScenarioManager: Wave spawn baþarýsýz ({wave.GroupName}). Skip.");
            cm.OnCounterEmpty -= OnCounterEmpty;
            yield break;
        }

        yield return new WaitUntil(() => finished);

        cm.OnCounterEmpty -= OnCounterEmpty;
    }

    private void HandleCustomerLeftCounter(CustomerController customer)
    {
        // Þimdilik boþ. Bir sonraki adýmda buraya “olay tetikle” koyacaðýz.
        // Örn: Random scare, ses, ýþýk, diyalog, vb.
        // Debug.Log($"Counter left: {customer.name}");
    }
}
