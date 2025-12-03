using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DayManager : MonoBehaviour
{
    public static DayManager Instance;

    public bool DayInLoop = false;
    public int DayPartToInitialize;

    [System.Serializable]
    public class DayState
    {
        [Header("Time Info")]
        [Range(0, 10)] public int Day;
        [Range(0, 10)] public int Part;

        [Header("Sun Settings")]
        public Vector3 sunRotation;
        public float sunIntensity;
        public Color sunColor;

        [Header("Skybox Settings")]
        public float skyboxExposure;
        public float skyboxRotate;
        public Color skyboxColor;
        public Color environmentColor;

        [Header("Atmosphere")]
        public Color fogColor;
        public float fogDensity;

        [Header("City Lights")]
        public bool shouldLightsUp;
    }

    [Header("Main Config")]
    public int DayCount = 0;
    public float transitionDuration = 10f;

    [Header("Scene References")]
    public Light sun;
    [SerializeField] private Material originalSkyboxMat;

    [Header("Street Lights")]
    [SerializeField] private Material[] lightMats; // Ana materyaller
    public List<GameObject> allLights = new List<GameObject>();

    [Header("Data")]
    public DayState[] DayStates;
    public DayState CurrentDayState;

    private int currentIndex = 0;
    private Coroutine transitionRoutine;
    private Material instancedSkybox;

    // --- YENÝ DEÐÝÞKENLER (HAFIZA) ---
    // Iþýklarýn orijinal parlaklýklarýný saklamak için
    private Dictionary<Light, float> defaultLightIntensities = new Dictionary<Light, float>();
    // Materyallerin orijinal renklerini saklamak için
    private Dictionary<Material, Color> defaultEmissions = new Dictionary<Material, Color>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // --- HAFIZAYA ALMA ÝÞLEMÝ ---
        // 1. Materyallerin orijinal Emission renklerini kaydet
        foreach (var mat in lightMats)
        {
            if (mat != null && !defaultEmissions.ContainsKey(mat))
            {
                // Emission keyword'ü kapalý olsa bile rengi alalým
                mat.EnableKeyword("_EMISSION");
                defaultEmissions.Add(mat, mat.GetColor("_EmissionColor"));
            }
        }

        // 2. Iþýklarýn orijinal Intensity deðerlerini kaydet (Register edilmiþse)
        foreach (var obj in allLights)
        {
            RegisterLightInternal(obj); // Kod tekrarý olmasýn diye fonksiyona aldým
        }

        // --- MATERIAL FIX ---
        if (originalSkyboxMat != null)
        {
            instancedSkybox = new Material(originalSkyboxMat);
            RenderSettings.skybox = instancedSkybox;
        }

        InitializeDay(DayCount, DayPartToInitialize);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
            NextDayState();
    }

    // --- IÞIK KAYIT SÝSTEMÝ GÜNCELLEME ---
    public void RegisterLight(GameObject lightObj)
    {
        if (!allLights.Contains(lightObj))
        {
            allLights.Add(lightObj);
            RegisterLightInternal(lightObj);

            // Yeni gelen ýþýðý o anki duruma eþitle (Anýnda)
            float currentLightVal = (CurrentDayState != null && CurrentDayState.shouldLightsUp) ? 1f : 0f;
            UpdateSingleLight(lightObj, currentLightVal);
        }
    }

    private void RegisterLightInternal(GameObject lightObj)
    {
        if (lightObj == null) return;

        Light l = lightObj.GetComponent<Light>();
        if (l == null) l = lightObj.GetComponentInChildren<Light>(); // Belki child'dadýr

        if (l != null && !defaultLightIntensities.ContainsKey(l))
        {
            defaultLightIntensities.Add(l, l.intensity);
        }
    }

    public void UnregisterLight(GameObject lightObj)
    {
        if (allLights.Contains(lightObj))
            allLights.Remove(lightObj);

        // Dictionary'den silmeye gerek yok, RAM'de yer kaplamaz, kalsýn.
    }

    // ... (NextDayState ve NextDay ayný kalýyor) ...
    public void NextDayState()
    {
        var todaysStates = DayStates.Where(s => s.Day == DayCount).OrderBy(s => s.Part).ToArray();
        if (todaysStates.Length == 0) return;

        int nextIndex = (currentIndex + 1) % todaysStates.Length;

        if (transitionRoutine != null) StopCoroutine(transitionRoutine);
        transitionRoutine = StartCoroutine(TransitionRoutine(todaysStates[currentIndex], todaysStates[nextIndex]));

        currentIndex = nextIndex;
    }

    public void NextDay()
    {
        DayCount++;
        currentIndex = 0;
    }

    public void InitializeDay(int dayIndex, int partNumber = 1)
    {
        DayCount = dayIndex;
        // lightsHandled = false; // Buna gerek kalmadý, sürekli lerp yapacaðýz

        var targetState = DayStates.FirstOrDefault(s => s.Day == DayCount && s.Part == partNumber);
        CurrentDayState = targetState;

        var todaysStates = DayStates.Where(s => s.Day == DayCount).OrderBy(s => s.Part).ToList();
        currentIndex = todaysStates.IndexOf(targetState);

        if (transitionRoutine != null) StopCoroutine(transitionRoutine);

        ApplyStateInstant(targetState);

        if (DayInLoop) NextDayState();
    }

    // --- YENÝ LERP FONKSÝYONU ---
    // percent: 0 (Karanlýk/Kapalý) - 1 (Aydýnlýk/Açýk)
    private void UpdateCityLightsLerp(float percent)
    {
        // 1. Materyaller (Emission Lerp)
        foreach (var kvp in defaultEmissions)
        {
            Material mat = kvp.Key;
            Color originalColor = kvp.Value;

            if (mat != null)
            {
                // Siyahtan -> Orijinal Renge doðru geçiþ
                Color targetEmission = Color.Lerp(Color.black, originalColor, percent);
                mat.SetColor("_EmissionColor", targetEmission);

                // Optimizasyon: Eðer tamamen siyahsa Emission hesaplamasýný kapat
                if (percent <= 0.01f) mat.DisableKeyword("_EMISSION");
                else mat.EnableKeyword("_EMISSION");
            }
        }

        // 2. Iþýklar (Intensity Lerp)
        foreach (var kvp in defaultLightIntensities)
        {
            Light l = kvp.Key;
            float originalIntensity = kvp.Value;

            if (l != null)
            {
                l.intensity = Mathf.Lerp(0f, originalIntensity, percent);

                // Optimizasyon: Iþýk 0'sa objeyi kapat (Culling yemez, CPU rahatlar)
                if (percent <= 0.01f) l.gameObject.SetActive(false);
                else l.gameObject.SetActive(true);
            }
        }
    }

    // Tekil ýþýk güncelleme (Yeni doðan ýþýklar için)
    private void UpdateSingleLight(GameObject obj, float percent)
    {
        Light l = obj.GetComponent<Light>();
        if (l == null) l = obj.GetComponentInChildren<Light>();

        if (l != null && defaultLightIntensities.ContainsKey(l))
        {
            l.intensity = Mathf.Lerp(0f, defaultLightIntensities[l], percent);
            obj.SetActive(percent > 0.01f);
        }
    }

    private void ApplyStateInstant(DayState state)
    {
        // ... (Sun, Skybox, Fog ayný kalýyor) ...
        sun.transform.rotation = Quaternion.Euler(state.sunRotation);
        sun.intensity = state.sunIntensity;
        sun.color = state.sunColor;

        if (instancedSkybox != null)
        {
            instancedSkybox.SetFloat("_Exposure", state.skyboxExposure);
            instancedSkybox.SetFloat("_Rotation", state.skyboxRotate);
            instancedSkybox.SetColor("_Tint", state.skyboxColor);
        }

        RenderSettings.fogColor = state.fogColor;
        RenderSettings.fogDensity = state.fogDensity;
        RenderSettings.ambientLight = state.environmentColor;

        // Iþýklarý ANINDA ayarla (Lerp yok, direkt hedef deðer)
        UpdateCityLightsLerp(state.shouldLightsUp ? 1f : 0f);
    }

    private IEnumerator TransitionRoutine(DayState from, DayState to)
    {
        CurrentDayState = to;
        float t = 0f;

        // Geçiþin baþýndaki ýþýk durumu (0 veya 1)
        float startLightVal = from.shouldLightsUp ? 1f : 0f;
        // Geçiþin sonundaki ýþýk durumu (0 veya 1)
        float endLightVal = to.shouldLightsUp ? 1f : 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / transitionDuration;
            float smoothT = Mathf.SmoothStep(0, 1, t);

            // ... (Sun, Skybox, Fog Lerp'leri ayný) ...
            sun.transform.rotation = Quaternion.Slerp(Quaternion.Euler(from.sunRotation), Quaternion.Euler(to.sunRotation), smoothT);
            sun.intensity = Mathf.Lerp(from.sunIntensity, to.sunIntensity, smoothT);
            sun.color = Color.Lerp(from.sunColor, to.sunColor, smoothT);

            if (instancedSkybox != null)
            {
                instancedSkybox.SetFloat("_Exposure", Mathf.Lerp(from.skyboxExposure, to.skyboxExposure, smoothT));
                instancedSkybox.SetFloat("_Rotation", Mathf.Lerp(from.skyboxRotate, to.skyboxRotate, smoothT));
                instancedSkybox.SetColor("_Tint", Color.Lerp(from.skyboxColor, to.skyboxColor, smoothT));
            }

            RenderSettings.fogColor = Color.Lerp(from.fogColor, to.fogColor, smoothT);
            RenderSettings.fogDensity = Mathf.Lerp(from.fogDensity, to.fogDensity, smoothT);
            RenderSettings.ambientLight = Color.Lerp(from.environmentColor, to.environmentColor, smoothT);

            // --- IÞIK LERP ---
            // Baþlangýç deðerinden bitiþ deðerine smoothT ile git
            float currentLightVal = Mathf.Lerp(startLightVal, endLightVal, smoothT);
            UpdateCityLightsLerp(currentLightVal);

            yield return null;
        }

        ApplyStateInstant(to);

        if (DayInLoop) NextDayState();
    }

    // Oyun kapanýrken materyalleri düzelt (Editör kirlenmesin)
    private void OnDestroy()
    {
        foreach (var kvp in defaultEmissions)
        {
            if (kvp.Key != null)
            {
                kvp.Key.SetColor("_EmissionColor", kvp.Value);
                kvp.Key.EnableKeyword("_EMISSION");
            }
        }
    }
}