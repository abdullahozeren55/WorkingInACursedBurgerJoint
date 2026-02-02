using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenarioManager : MonoBehaviour
{
    public static ScenarioManager Instance { get; private set; }

    [Header("Scenario")]
    [SerializeField] private LevelScenario scenario;

    [Header("Scenario Control")]
    // DEÐÝÞÝKLÝK 1: Varsayýlan deðer true yapýldý. Böylece oyun baþlar baþlamaz akýþ durur.
    public bool IsSpawningPaused = true;

    [Header("Ambient (Wave Based)")]
    [SerializeField] private AmbientEventPoolSO defaultAmbientPool;

    private Coroutine routine;
    private ScenarioContext ctx;
    private bool eventBusy;
    private bool waveRunning = false;
    private AmbientEventPoolSO activeAmbientPool = null;

    // pool bazlý cooldown ve repeat kontrolü
    private readonly Dictionary<AmbientEventPoolSO, float> nextAllowedByPool = new();
    private readonly Dictionary<AmbientEventPoolSO, ScenarioEventSO> lastEventByPool = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        ctx = new ScenarioContext
        {
            scenario = this,
            customers = CustomerManager.Instance
        };
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

    public void PlayEvent(ScenarioEventSO ev)
    {
        StartCoroutine(PlayEventInternal(ev));
    }

    // --- KONTROL METODLARI ---
    public void PauseSpawning()
    {
        IsSpawningPaused = true;
        Debug.Log("ScenarioManager: Senaryo akýþý DURAKLATILDI.");
    }

    public void ResumeSpawning()
    {
        IsSpawningPaused = false;
        Debug.Log("ScenarioManager: Senaryo akýþý DEVAM EDÝYOR.");
    }

    private void Start()
    {
        StartScenario();
    }

    public void StartScenario()
    {
        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(RunScenario());
    }

    // DEÐÝÞÝKLÝK 2: Pause durumunu kontrol eden yardýmcý bir özellik (property)
    // WaitUntil içine direkt lambda yazmak yerine okunaklýlýðý artýrýr.
    private bool IsScenarioActive => !IsSpawningPaused;

    private IEnumerator RunScenario()
    {
        if (scenario == null || scenario.Waves == null || scenario.Waves.Count == 0)
        {
            Debug.LogWarning("ScenarioManager: Scenario atanmadý ya da Waves boþ.");
            yield break;
        }

        // DEÐÝÞÝKLÝK 3: Döngüye girmeden önce, oyunun baþlamasý için Resume komutunu bekle.
        // Eðer oyun baþýnda IsSpawningPaused = true ise, kod burada asýlý kalýr.
        yield return new WaitUntil(() => IsScenarioActive);

        for (int i = 0; i < scenario.Waves.Count; i++)
        {
            var wave = scenario.Waves[i];

            // Wave baþlamadan önce duraklatma kontrolü
            yield return new WaitUntil(() => IsScenarioActive);

            // wave baþý scripted eventler
            if (wave.EventsBeforeSpawn != null)
            {
                foreach (var ev in wave.EventsBeforeSpawn)
                {
                    // Her event oynamadan önce duraklatma kontrolü
                    yield return new WaitUntil(() => IsScenarioActive);
                    yield return PlayEventBlocking(ev);
                }
            }

            // Delay süresince duraklatýlýrsa, delay sayacý iþlemez; akýþ donar.
            if (wave.DelayAfterPreviousGroup > 0f)
            {
                yield return new WaitUntil(() => IsScenarioActive);
                yield return new WaitForSeconds(wave.DelayAfterPreviousGroup);
            }

            // Wave'i çalýþtýrmadan hemen önce son kontrol
            yield return new WaitUntil(() => IsScenarioActive);
            yield return RunWave(wave);

            // wave sonu scripted eventler
            if (wave.EventsAfterCounterEmpty != null)
            {
                foreach (var ev in wave.EventsAfterCounterEmpty)
                {
                    yield return new WaitUntil(() => IsScenarioActive);
                    yield return PlayEventBlocking(ev);
                }
            }
        }

        Debug.Log("ScenarioManager: Tüm waveler bitti.");
    }

    private IEnumerator RunWave(CustomerGroupData wave)
    {
        var cm = CustomerManager.Instance;

        bool finished = false;
        void OnWaveComplete() => finished = true;

        cm.OnWaveCompleted += OnWaveComplete;

        activeAmbientPool = (wave.AmbientPool != null) ? wave.AmbientPool : defaultAmbientPool;
        waveRunning = true;

        // NOT: Eðer ScenarioManager duraklatýldýysa, CustomerManager'a spawn emri vermeden önce bekle
        yield return new WaitUntil(() => IsScenarioActive);

        cm.StartWaveSpawn(wave);

        // Wave bitene kadar bekle.
        // ÖNEMLÝ: Wave ortasýnda pause yapýldýðýnda burasý sadece beklemeye devam eder.
        // CustomerManager spawn iþlemine devam edecektir (Aþaðýdaki dürüstlük notuna bakýnýz).
        yield return new WaitUntil(() => finished);

        waveRunning = false;
        activeAmbientPool = null;

        cm.OnWaveCompleted -= OnWaveComplete;
    }

    private void HandleCustomerLeftCounter(CustomerController customer)
    {

        if (!waveRunning) return;
        if (eventBusy) return;

        var pool = activeAmbientPool;
        if (pool == null) return;
        if (!pool.enabled) return;

        var cm = CustomerManager.Instance;
        if (cm != null && cm.GetCustomersAtCounter().Count == 0)
            return;

        if (Time.time < GetNextAllowedTime(pool))
            return;

        if (Random.value > pool.chancePerCustomerLeave)
            return;

        var picked = PickFromPool(pool);
        if (picked == null) return;

        float minCd = Mathf.Min(pool.cooldownSecondsRange.x, pool.cooldownSecondsRange.y);
        float maxCd = Mathf.Max(pool.cooldownSecondsRange.x, pool.cooldownSecondsRange.y);
        SetNextAllowedTime(pool, Time.time + Random.Range(minCd, maxCd));

        SetLastEvent(pool, picked);

        PlayEventNonBlocking(picked);
    }

    // ----- Pool helpers (Deðiþmedi) -----
    private float GetNextAllowedTime(AmbientEventPoolSO pool)
        => nextAllowedByPool.TryGetValue(pool, out var t) ? t : 0f;

    private void SetNextAllowedTime(AmbientEventPoolSO pool, float time)
        => nextAllowedByPool[pool] = time;

    private ScenarioEventSO GetLastEvent(AmbientEventPoolSO pool)
        => lastEventByPool.TryGetValue(pool, out var e) ? e : null;

    private void SetLastEvent(AmbientEventPoolSO pool, ScenarioEventSO ev)
        => lastEventByPool[pool] = ev;

    private ScenarioEventSO PickFromPool(AmbientEventPoolSO pool)
    {
        if (pool.events == null || pool.events.Count == 0) return null;

        var valid = new List<WeightedScenarioEvent>();
        for (int i = 0; i < pool.events.Count; i++)
        {
            var w = pool.events[i];
            if (w != null && w.ev != null && w.weight > 0)
                valid.Add(w);
        }
        if (valid.Count == 0) return null;
        if (valid.Count == 1) return valid[0].ev;

        var last = GetLastEvent(pool);
        bool canExcludeLast = pool.avoidImmediateRepeat && last != null && valid.Exists(x => x.ev != last);

        int total = 0;
        for (int i = 0; i < valid.Count; i++)
        {
            if (canExcludeLast && valid[i].ev == last) continue;
            total += valid[i].weight;
        }

        if (total <= 0)
            return valid[0].ev;

        int roll = Random.Range(0, total);
        int running = 0;

        for (int i = 0; i < valid.Count; i++)
        {
            if (canExcludeLast && valid[i].ev == last) continue;

            running += valid[i].weight;
            if (roll < running)
                return valid[i].ev;
        }

        return valid[0].ev;
    }

    // ----- Event runner (Deðiþmedi) -----
    private IEnumerator PlayEventInternal(ScenarioEventSO ev)
    {
        if (ev == null) yield break;

        while (eventBusy) yield return null;
        eventBusy = true;

        if (ctx.customers == null)
            ctx.customers = CustomerManager.Instance;

        yield return ev.Play(ctx);

        eventBusy = false;
    }

    private IEnumerator PlayEventBlocking(ScenarioEventSO ev)
    {
        yield return PlayEventInternal(ev);
    }

    private void PlayEventNonBlocking(ScenarioEventSO ev)
    {
        if (ev == null) return;
        StartCoroutine(PlayEventInternal(ev));
    }
}