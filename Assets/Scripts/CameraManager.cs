using Cinemachine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;

    [System.Serializable]
    public class JumpscareSettings
    {
        public JumpscareType type;
        [Header("Screen Shake")]
        public float amplitude = 0.2f;
        public float frequency = 0.4f;
        public float shakeDuration = 0.5f;

        [Header("Vignette")]
        public float vignetteIntensity = 0.4f;
        public Color vignetteColor = Color.red;
        public float vignetteDuration = 0.5f;

        [Header("FOV Kick")]
        public float fov = 55f;
        public float fovDuration = 0.4f;
    }

    public enum JumpscareType
    {
        Small,
        Mid,
        Big
    }

    public enum CameraName
    {
        Null,
        FirstPerson,
        Monitor,
        Customer0
    }

    [System.Serializable]
    public class CameraEntry
    {
        public CameraName camName;
        public CinemachineVirtualCamera vCam;
    }

    [Space]
    [SerializeField] private CameraEntry[] cameras;
    [Space]
    [SerializeField] private List<JumpscareSettings> jumpscarePresets;
    [Space]

    [SerializeField] private Volume postProcessVolume;
    [Space]

    [Header("Player Throw Charge Effect Settings")]
    [SerializeField] private float maxAmplitudeGain = 0.2f;
    [SerializeField] private float maxFrequencyGain = 0.4f;
    [SerializeField] private float maxFOV = 50f;
    [SerializeField] private float throwMaxChargeTime = 1.5f;
    [SerializeField] private float vignetteIntensity = 0.25f;
    [SerializeField] private Color throwChargeColor;
    [SerializeField] private float releaseSpeedMultiplier = 4f;
    private float normalFOV;

    private CinemachineVirtualCamera firstPersonCam;

    private CinemachineBasicMultiChannelPerlin perlin;
    private Vignette vignette;

    private CameraEntry currentCam;
    private int basePriority = 10;

    private void Awake()
    {
        if (Instance == null)
        {
            // If not, set this instance as the singleton
            Instance = this;

            // Optionally, mark GameManager as not destroyed between scene loads
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // If an instance already exists, destroy this one to enforce the singleton pattern
            Destroy(gameObject);
        }

        foreach (CameraEntry entry in cameras)
        {
            if (entry.camName == CameraName.FirstPerson)
            {
                firstPersonCam = entry.vCam;

                perlin = firstPersonCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

                normalFOV = firstPersonCam.m_Lens.FieldOfView;

                break;
            }
        }

        if (postProcessVolume.profile.TryGet(out Vignette v))
            vignette = v;

    }

    public void SwitchToCamera(CameraName name)
    {
        if (name == currentCam.camName || name == CameraName.Null)
            return;

        // Lower priority of current camera
        foreach (CameraEntry entry in cameras)
        {
            if (entry.camName == name)
            {
                currentCam.vCam.Priority = basePriority;
                currentCam = entry;
                currentCam.vCam.Priority = basePriority + 1;

                break;
            }
        }
    }

    public void InitializeCamera(CameraName name)
    {
        if (currentCam != null)
            return;

        foreach (CameraEntry entry in cameras)
        {
            if (entry.camName == name)
            {
                currentCam = entry;
                currentCam.vCam.Priority = basePriority + 1;

                break;
            }
        }
    }

    public CinemachineVirtualCamera GetCamera()
    {
        return currentCam.vCam;
    }

    public void SwitchToFirstPersonCamera()
    {
        SwitchToCamera(CameraName.FirstPerson);
    }

    public void PlayScreenShake(float targetAmplitude, float targetFrequency, float duration, Ease ease = Ease.OutSine, string tweenId = "ScreenShake")
    {
        DOTween.Kill(tweenId);

        Sequence seq = DOTween.Sequence().SetId(tweenId);
        seq.Join(DOTween.To(() => perlin.m_AmplitudeGain, x => perlin.m_AmplitudeGain = x, targetAmplitude, duration));
        seq.Join(DOTween.To(() => perlin.m_FrequencyGain, x => perlin.m_FrequencyGain = x, targetFrequency, duration));
        seq.SetEase(ease);
    }

    public void PlayVignette(float targetIntensity, float duration, Color? color = null, Ease ease = Ease.OutSine, string tweenId = "Vignette")
    {
        DOTween.Kill(tweenId);

        if (color.HasValue)
            vignette.color.value = color.Value;

        DOTween.To(() => vignette.intensity.value, x => vignette.intensity.value = x, targetIntensity, duration)
            .SetEase(ease)
            .SetId(tweenId);
    }

    public void PlayFOV(float targetFOV, float duration, Ease ease = Ease.OutSine, string tweenId = "FOV")
    {
        DOTween.Kill(tweenId);

        DOTween.To(() => firstPersonCam.m_Lens.FieldOfView, x => firstPersonCam.m_Lens.FieldOfView = x, targetFOV, duration)
            .SetEase(ease)
            .SetId(tweenId);
    }

    public void PlayThrowEffects(bool isCharging)
    {
        float duration = isCharging ? throwMaxChargeTime : throwMaxChargeTime / releaseSpeedMultiplier;
        Ease ease = isCharging ? Ease.OutSine : Ease.InSine;

        PlayScreenShake(isCharging ? maxAmplitudeGain : 0f,
                        isCharging ? maxFrequencyGain : 0f,
                        duration, ease, "ThrowEffects_Shake");

        PlayVignette(isCharging ? vignetteIntensity : 0f,
                     duration, throwChargeColor, ease, "ThrowEffects_Vignette");

        PlayFOV(isCharging ? maxFOV : normalFOV,
                duration, ease, "ThrowEffects_FOV");
    }

    public void PlayJumpscareEffects(JumpscareType type)
    {
        var preset = jumpscarePresets.Find(p => p.type == type);
        if (preset == null)
        {
            Debug.LogWarning($"No preset found for jumpscare type {type}");
            return;
        }

        // Screen Shake
        PlayScreenShake(preset.amplitude, preset.frequency, preset.shakeDuration, Ease.OutBack, $"Jumpscare_{type}_Shake");

        // Vignette
        PlayVignette(preset.vignetteIntensity, preset.vignetteDuration, preset.vignetteColor, Ease.OutSine, $"Jumpscare_{type}_Vignette");

        // FOV
        PlayFOV(preset.fov, preset.fovDuration, Ease.OutSine, $"Jumpscare_{type}_FOV");
    }
}
