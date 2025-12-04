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

    // --- DEÐÝÞÝKLÝK BURADA: ÝKÝ AYRI SÜRE ---
    [Tooltip("Sönük moda geçtiðinde ne kadar öyle kalsýn?")]
    public Vector2 dimStayTimeRange = new Vector2(0.05f, 0.1f); // Genelde kýsa olur (pýrpýr gibi)

    [Tooltip("Parlak moda geçtiðinde ne kadar öyle kalsýn?")]
    public Vector2 brightStayTimeRange = new Vector2(0.2f, 2.0f); // Genelde daha uzun olur
    // ----------------------------------------

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
        yield return new WaitForSecondsRealtime(Random.Range(startDelay.x, startDelay.y));

        bool isGoingToBright = false;

        while (true)
        {
            float targetAlpha;

            // Hedefi belirliyoruz
            if (isGoingToBright)
                targetAlpha = Random.Range(brightRange.x, brightRange.y);
            else
                targetAlpha = Random.Range(dimRange.x, dimRange.y);

            // Durumu tersine çeviriyoruz (Bir sonraki tur için hazýrlýk)
            // DÝKKAT: Burada 'isGoingToBright'ý deðiþtiriyoruz ama 
            // aþaðýda bekleme süresini seçerken "Þu an neye dönüþtüm?" diye bakacaðýz.
            isGoingToBright = !isGoingToBright;

            float duration = Random.Range(transitionTimeRange.x, transitionTimeRange.y);
            float elapsed = 0f;
            float startAlpha = _currentAlpha;

            if (SoundManager.Instance != null)
                SoundManager.Instance.PlayRandomUISoundFX(neonClips, transform, neonVolume, neonMinPitch, neonMaxPitch);

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                _currentAlpha = Mathf.SmoothStep(startAlpha, targetAlpha, t);
                _materialInstance.SetFloat(_alphaID, _currentAlpha);
                yield return null;
            }

            _currentAlpha = targetAlpha;
            _materialInstance.SetFloat(_alphaID, _currentAlpha);

            // --- DEÐÝÞÝKLÝK BURADA: HANGÝ SÜREYÝ SEÇECEÐÝZ? ---
            float waitTime;

            // Mantýk: Yukarýda 'isGoingToBright'ý tersine çevirmiþtik (!).
            // Yani eðer þu an 'isGoingToBright' FALSE ise, demek ki az önce TRUE idi ve Parlak moda geçtik.
            // Eðer þu an 'isGoingToBright' TRUE ise, demek ki az önce FALSE idi ve Sönük moda geçtik.

            if (!isGoingToBright)
            {
                // Þu an PARLAK durumdayýz (Bright Mode) -> Parlak kalma süresini kullan
                waitTime = Random.Range(brightStayTimeRange.x, brightStayTimeRange.y);
            }
            else
            {
                // Þu an SÖNÜK durumdayýz (Dim Mode) -> Sönük kalma süresini kullan
                waitTime = Random.Range(dimStayTimeRange.x, dimStayTimeRange.y);
            }

            yield return new WaitForSecondsRealtime(waitTime);
        }
    }
}