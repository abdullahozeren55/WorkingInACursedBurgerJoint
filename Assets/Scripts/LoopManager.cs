using AtmosphericHeightFog;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoopManager : MonoBehaviour
{
    public static LoopManager Instance;

    public int LoopPartToInitialize;

    [System.Serializable]
    public class LoopState
    {
        [Header("Time Info")]
        [Range(0, 10)] public int Loop;
        [Range(0, 10)] public int Part;

        [Header("Sun Settings")]
        public Vector3 sunRotation;
        public float sunIntensity;
        public Color sunColor;

        [Header("Skybox Settings")]
        public float skyboxExposure;
        public float skyboxRotate;
        public Color skyboxColor;
        [ColorUsage(true, true)] public Color environmentColor;

        [Header("Atmospheric Fog Settings")]
        [Range(0f, 1f)] public float fogIntensity;
        public float fogHeightEnd;
        public Color fogColorStart;
        public Color fogColorEnd;

        [Header("City Lights")]
        public bool shouldLightsUp;
    }

    [Header("Main Config")]
    public int LoopCount = 0;
    public int LoopPartCount = 0;
    public float transitionDuration = 10f;

    [Header("Scene References")]
    public Light sun;
    [SerializeField] private Material originalSkyboxMat;

    public HeightFogGlobal HeightFogScript;

    [Header("Street Lights")]
    [SerializeField] private Material[] lightMats; // Sokak lambalarýnýn materyalleri (Emmision kontrolü için)
    public List<GameObject> allLights = new List<GameObject>(); // Sokak lambalarýnýn ýþýk bileþenleri (Intensity kontrolü için)

    [Header("Data")]
    public LoopState[] LoopStates;
    public LoopState CurrentLoopState;

    private int currentIndex = 0;
    private Coroutine transitionRoutine;
    private Material instancedSkybox;

    // Hedef (açýk haldeki) renkleri ve ýþýk þiddetlerini hafýzada tutuyoruz
    private Dictionary<Light, float> defaultLightIntensities = new Dictionary<Light, float>();
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

        // 1. Materyallerin "AÇIK" halindeki renklerini kaydet (Target Color)
        foreach (var mat in lightMats)
        {
            if (mat != null && !defaultEmissions.ContainsKey(mat))
            {
                mat.EnableKeyword("_EMISSION");
                defaultEmissions.Add(mat, mat.GetColor("_EmissionColor"));
            }
        }

        // 2. Iþýklarýn "AÇIK" halindeki intensity deðerlerini kaydet
        foreach (var obj in allLights)
        {
            RegisterLightInternal(obj);
        }

        if (originalSkyboxMat != null)
        {
            instancedSkybox = new Material(originalSkyboxMat);
            RenderSettings.skybox = instancedSkybox;
        }

        if (HeightFogScript == null)
            HeightFogScript = FindObjectOfType<HeightFogGlobal>();

        InitializeLoop(LoopCount, LoopPartToInitialize);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (instancedSkybox != null)
        {
            RenderSettings.skybox = instancedSkybox;
        }

        HeightFogScript = null;
        HeightFogScript = FindObjectOfType<HeightFogGlobal>();

        if (CurrentLoopState != null)
        {
            ApplyStateInstant(CurrentLoopState);
        }

        allLights.RemoveAll(item => item == null);

        // Null olan ýþýk keylerini temizle
        var keysToRemove = defaultLightIntensities.Keys.Where(k => k == null).ToList();
        foreach (var key in keysToRemove) defaultLightIntensities.Remove(key);
    }

    public void RegisterLight(GameObject lightObj)
    {
        if (!allLights.Contains(lightObj))
        {
            allLights.Add(lightObj);
            RegisterLightInternal(lightObj);

            float currentLightVal = (CurrentLoopState != null && CurrentLoopState.shouldLightsUp) ? 1f : 0f;
            UpdateSingleLight(lightObj, currentLightVal);
        }
    }

    private void RegisterLightInternal(GameObject lightObj)
    {
        if (lightObj == null) return;

        Light l = lightObj.GetComponent<Light>();
        if (l == null) l = lightObj.GetComponentInChildren<Light>();

        if (l != null && !defaultLightIntensities.ContainsKey(l))
        {
            defaultLightIntensities.Add(l, l.intensity);
        }
    }

    public void UnregisterLight(GameObject lightObj)
    {
        if (allLights.Contains(lightObj))
            allLights.Remove(lightObj);
    }

    public void NextLoopState()
    {
        var todaysStates = LoopStates.Where(s => s.Loop == LoopCount).OrderBy(s => s.Part).ToArray();
        if (todaysStates.Length == 0) return;

        int nextIndex = (currentIndex + 1) % todaysStates.Length;

        if (transitionRoutine != null) StopCoroutine(transitionRoutine);
        transitionRoutine = StartCoroutine(TransitionRoutine(todaysStates[currentIndex], todaysStates[nextIndex]));

        currentIndex = nextIndex;
        LoopPartCount = todaysStates[currentIndex].Part;
    }

    public void NextDay()
    {
        LoopCount++;
        currentIndex = 0;
        InitializeLoop(LoopCount, 0);
    }

    public void InitializeLoop(int dayIndex, int partNumber = 0)
    {
        LoopCount = dayIndex;

        var targetState = LoopStates.FirstOrDefault(s => s.Loop == LoopCount && s.Part == partNumber);

        if (targetState == null)
        {
            Debug.LogWarning($"LoopState {dayIndex}-{partNumber} bulunamadý!");
            return;
        }

        CurrentLoopState = targetState;

        var todaysStates = LoopStates.Where(s => s.Loop == LoopCount).OrderBy(s => s.Part).ToList();
        currentIndex = todaysStates.IndexOf(targetState);
        LoopPartCount = partNumber;

        if (transitionRoutine != null) StopCoroutine(transitionRoutine);

        ApplyStateInstant(targetState);
    }

    private void UpdateCityLightsLerp(float percent)
    {
        // 3. Materyal Emission Kontrolü (Siyah -> Orjinal Renk arasý geçiþ)
        foreach (var kvp in defaultEmissions)
        {
            Material mat = kvp.Key;
            Color originalColor = kvp.Value;

            if (mat != null)
            {
                Color targetEmission = Color.Lerp(Color.black, originalColor, percent);
                mat.SetColor("_EmissionColor", targetEmission);

                if (percent <= 0.01f) mat.DisableKeyword("_EMISSION");
                else mat.EnableKeyword("_EMISSION");
            }
        }

        // 4. Iþýk Intensity Kontrolü (0 -> Orjinal Þiddet arasý geçiþ)
        foreach (var kvp in defaultLightIntensities)
        {
            Light l = kvp.Key;
            float originalIntensity = kvp.Value;

            if (l != null)
            {
                l.intensity = Mathf.Lerp(0f, originalIntensity, percent);

                if (percent <= 0.01f) l.gameObject.SetActive(false);
                else l.gameObject.SetActive(true);
            }
        }
    }

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

    private void ApplyFogValues(float intensity, float height, Color startColor, Color endColor)
    {
        if (HeightFogScript != null)
        {
            HeightFogScript.fogIntensity = intensity;
            HeightFogScript.fogHeightEnd = height;
            HeightFogScript.fogColorStart = startColor;
            HeightFogScript.fogColorEnd = endColor;
        }
    }

    private void ApplyStateInstant(LoopState state)
    {
        sun.transform.rotation = Quaternion.Euler(state.sunRotation);
        sun.intensity = state.sunIntensity;
        sun.color = state.sunColor;

        if (instancedSkybox != null)
        {
            instancedSkybox.SetFloat("_Exposure", state.skyboxExposure);
            instancedSkybox.SetFloat("_Rotation", state.skyboxRotate);
            instancedSkybox.SetColor("_Tint", state.skyboxColor);
        }

        ApplyFogValues(state.fogIntensity, state.fogHeightEnd, state.fogColorStart, state.fogColorEnd);

        RenderSettings.ambientLight = state.environmentColor;

        UpdateCityLightsLerp(state.shouldLightsUp ? 1f : 0f);
    }

    private IEnumerator TransitionRoutine(LoopState from, LoopState to)
    {
        CurrentLoopState = to;
        float t = 0f;

        float startLightVal = from.shouldLightsUp ? 1f : 0f;
        float endLightVal = to.shouldLightsUp ? 1f : 0f;

        float startRot = from.skyboxRotate;
        float endRot = to.skyboxRotate;

        if (endRot > startRot)
        {
            endRot -= 360f;
        }

        while (t < 1f)
        {
            t += Time.deltaTime / transitionDuration;
            float lerpT = t;

            sun.transform.rotation = Quaternion.Slerp(Quaternion.Euler(from.sunRotation), Quaternion.Euler(to.sunRotation), lerpT);
            sun.intensity = Mathf.Lerp(from.sunIntensity, to.sunIntensity, lerpT);
            sun.color = Color.Lerp(from.sunColor, to.sunColor, lerpT);

            if (instancedSkybox != null)
            {
                instancedSkybox.SetFloat("_Exposure", Mathf.Lerp(from.skyboxExposure, to.skyboxExposure, lerpT));
                instancedSkybox.SetFloat("_Rotation", Mathf.Lerp(startRot, endRot, lerpT));
                instancedSkybox.SetColor("_Tint", Color.Lerp(from.skyboxColor, to.skyboxColor, lerpT));
            }

            ApplyFogValues
                (
                    Mathf.Lerp(from.fogIntensity, to.fogIntensity, lerpT),
                    Mathf.Lerp(from.fogHeightEnd, to.fogHeightEnd, lerpT),
                    Color.Lerp(from.fogColorStart, to.fogColorStart, lerpT),
                    Color.Lerp(from.fogColorEnd, to.fogColorEnd, lerpT)
                );

            RenderSettings.ambientLight = Color.Lerp(from.environmentColor, to.environmentColor, lerpT);

            float currentLightVal = Mathf.Lerp(startLightVal, endLightVal, lerpT);
            UpdateCityLightsLerp(currentLightVal);

            yield return null;
        }

        ApplyStateInstant(to);
    }

    private void OnDestroy()
    {
        // --- TEMÝZLÝK: Emission geri yükleme döngüsü silindi ---
        // Sadece Skybox instance'ýný temizliyoruz.

        if (instancedSkybox != null)
        {
#if UNITY_EDITOR
            DestroyImmediate(instancedSkybox);
#else
            Destroy(instancedSkybox);
#endif
        }
    }
}