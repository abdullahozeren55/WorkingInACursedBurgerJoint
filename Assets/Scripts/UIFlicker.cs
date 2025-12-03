using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIFlicker : MonoBehaviour
{
    [Header("Parlaklýk Deðerleri")]
    public Vector2 dimRange = new Vector2(0.05f, 0.3f);
    public Vector2 brightRange = new Vector2(0.5f, 1.0f);

    [Header("Zamanlama Ayarlarý (Saniye)")]
    public Vector2 transitionTimeRange = new Vector2(0.05f, 0.2f);
    public Vector2 startDelay = new Vector2(0.2f, 0.6f);
    public Vector2 stayTimeRange = new Vector2(0.05f, 0.3f);

    [Header("Sound Settings")]
    public AudioClip[] neonClips;
    public float neonVolume = 1f;
    public float neonMinPitch = 0.8f;
    public float neonMaxPitch = 1.2f;

    private Image _targetImage;
    private Material _materialInstance;
    private int _alphaID;
    private float _currentAlpha;

    private void Awake()
    {
        _targetImage = GetComponent<Image>();
        _alphaID = Shader.PropertyToID("_Alpha");

        if (_targetImage != null)
        {
            _materialInstance = Instantiate(_targetImage.material);
            _targetImage.material = _materialInstance;
        }
    }

    private void OnEnable()
    {
        if (_materialInstance != null)
        {
            // Açýlýþta parlak baþla
            _currentAlpha = brightRange.y;
            _materialInstance.SetFloat(_alphaID, _currentAlpha);
            StartCoroutine(SmoothFlickerRoutine());
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator SmoothFlickerRoutine()
    {
        // DEÐÝÞÝKLÝK 1: Realtime bekleme
        yield return new WaitForSecondsRealtime(Random.Range(startDelay.x, startDelay.y));

        bool isGoingToBright = false;

        while (true)
        {
            float targetAlpha;
            if (isGoingToBright)
                targetAlpha = Random.Range(brightRange.x, brightRange.y);
            else
                targetAlpha = Random.Range(dimRange.x, dimRange.y);

            isGoingToBright = !isGoingToBright;

            float duration = Random.Range(transitionTimeRange.x, transitionTimeRange.y);
            float elapsed = 0f;
            float startAlpha = _currentAlpha;

            if (SoundManager.Instance != null)
                SoundManager.Instance.PlayRandomUISoundFX(neonClips, transform, neonVolume, neonMinPitch, neonMaxPitch);

            while (elapsed < duration)
            {
                // DEÐÝÞÝKLÝK 2: Oyun zamaný (deltaTime) yerine Gerçek zaman (unscaledDeltaTime)
                elapsed += Time.unscaledDeltaTime;

                float t = elapsed / duration;
                _currentAlpha = Mathf.SmoothStep(startAlpha, targetAlpha, t);
                _materialInstance.SetFloat(_alphaID, _currentAlpha);
                yield return null;
            }

            _currentAlpha = targetAlpha;
            _materialInstance.SetFloat(_alphaID, _currentAlpha);

            float waitTime = Random.Range(stayTimeRange.x, stayTimeRange.y);

            // DEÐÝÞÝKLÝK 3: Realtime bekleme
            yield return new WaitForSecondsRealtime(waitTime);
        }
    }
}