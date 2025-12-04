using DG.Tweening; // DOTween kütüphanesi þart
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;

    [Header("UI Stuff")]
    public bool CanPause = false;
    [Space]
    public Volume globalVolume;
    [Space]
    public GameObject mainMenu; // GameObject olarak kalabilir (SetActive için)
    public GameObject pauseMenu;
    public GameObject settingsMenu; // YENÝ: Settings menüsü objesi

    [Header("Animation References")]
    // Animasyon yapacaðýmýz panellerin RectTransform'larý
    public UIFlicker[] mainMenuFlickers;
    public RectTransform mainMenuRect;
    public RectTransform settingsRect;

    [Header("Animation Settings")]
    public float slideDuration = 0.5f;
    public Ease slideEase = Ease.OutBack; // Juicy efekt için
    [Space]
    public AudioClip swingSound;
    public float swingVolume = 1f;
    public float swingMinPitch = 0.8f;
    public float swingMaxPitch = 1.2f;

    private Canvas myCanvas;
    private RectTransform canvasRect; // Canvas'ýn boyutunu almak için
    private List<PixelPerfectCanvasScaler> activeScalers = new List<PixelPerfectCanvasScaler>();
    private bool isBusy = false; // Animasyon sýrasýnda týklamayý engellemek için
    private bool isSettingsOpen = false;

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
        }

        myCanvas = GetComponentInChildren<Canvas>();
        if (myCanvas != null) canvasRect = myCanvas.GetComponent<RectTransform>();

        HandleCursorState(true);
    }

    private void Start()
    {
        // BAÞLANGIÇ POZÝSYONLARINI AYARLA
        // Oyun açýldýðýnda Main ortada, Settings saðda (ekran dýþýnda) olsun.
        // Ýkisini de Active yapýyoruz ki kayarken görünsünler (Gizlemeyi pozisyonla yapýyoruz)

        if (mainMenuRect != null && settingsRect != null)
        {
            float width = GetCanvasWidth();

            mainMenu.SetActive(true);
            settingsMenu.SetActive(true);

            mainMenuRect.anchoredPosition = Vector2.zero; // Merkezde
            settingsRect.anchoredPosition = new Vector2(width, 0); // Saðda dýþarýda
        }
    }

    private void Update()
    {
        if (CanPause && Input.GetKeyDown(KeyCode.Escape))
        {
            // Settings açýksa önce onu kapatýp Main'e mi dönsün yoksa direkt oyuna mý?
            // Basitlik için: Pause menüsü mantýðý aynen kalsýn.
            HandlePauseMenu(!pauseMenu.activeSelf);
            HandleTimeScale(pauseMenu.activeSelf ? 0f : 1f);
            HandleCursorState(pauseMenu.activeSelf);
            SetPlayerCanPlay(!pauseMenu.activeSelf);
        }
    }

    // --- YENÝ EKLENEN ANÝMASYON FONKSÝYONLARI ---

    public void FixMenuPositions()
    {
        // 1. Önce devam eden tüm animasyonlarý ÖLDÜR (Complete false, yani olduðu yerde dursun)
        mainMenuRect.DOKill(true); // true = Hemen bitir
        settingsRect.DOKill(true);
        isBusy = false;

        // 2. Yeni geniþliði al
        float width = GetCanvasWidth();

        // 3. Pozisyonlarý IÞINLA (Animasyon yok)
        if (isSettingsOpen)
        {
            // Settings AÇIK: Settings ortada (0), Main solda (-width)
            settingsRect.anchoredPosition = Vector2.zero;
            mainMenuRect.anchoredPosition = new Vector2(-width, 0);
        }
        else
        {
            // Main AÇIK: Main ortada (0), Settings saðda (+width)
            mainMenuRect.anchoredPosition = Vector2.zero;
            settingsRect.anchoredPosition = new Vector2(width, 0);
        }

        // 4. Garanti olsun diye Layout'u tekrar yenile
        LayoutRebuilder.ForceRebuildLayoutImmediate(mainMenuRect);
        LayoutRebuilder.ForceRebuildLayoutImmediate(settingsRect);
    }

    public void OpenSettings()
    {
        if (isBusy) return;
        isBusy = true;
        isSettingsOpen = true; // <-- ARTIK SETTINGS AÇIK

        foreach (var flicker in mainMenuFlickers)
        {
            if (flicker != null)
                flicker.enabled = false; // Sadece scripti kapatýyoruz, obje kalýyor
        }

        SoundManager.Instance.PlayUISoundFX(swingSound, swingVolume, swingMinPitch, swingMaxPitch);

        float width = GetCanvasWidth();

        mainMenuRect.DOAnchorPosX(-width, slideDuration).SetEase(slideEase).SetUpdate(true);

        // Saðdan gelmesi için önce konumunu garantiye al
        settingsRect.anchoredPosition = new Vector2(width, 0);

        settingsRect.DOAnchorPosX(0, slideDuration)
            .SetEase(slideEase)
            .SetUpdate(true)
            .OnComplete(() => isBusy = false);
    }

    public void CloseSettings()
    {
        if (isBusy) return;
        isBusy = true;
        isSettingsOpen = false; // <-- ARTIK MAIN AÇIK

        foreach (var flicker in mainMenuFlickers)
        {
            if (flicker != null)
                flicker.enabled = true; // Sadece scripti kapatýyoruz, obje kalýyor
        }

        SoundManager.Instance.PlayUISoundFX(swingSound, swingVolume, swingMinPitch, swingMaxPitch);

        float width = GetCanvasWidth();

        settingsRect.DOAnchorPosX(width, slideDuration).SetEase(slideEase).SetUpdate(true);

        // Soldan gelmesi için önce konumunu garantiye al
        mainMenuRect.anchoredPosition = new Vector2(-width, 0);

        mainMenuRect.DOAnchorPosX(0, slideDuration)
            .SetEase(slideEase)
            .SetUpdate(true)
            .OnComplete(() => isBusy = false);
    }

    // Canvas geniþliðini dinamik al (Çözünürlük deðiþse de çalýþýr)
    private float GetCanvasWidth()
    {
        if (canvasRect != null) return canvasRect.rect.width;
        return 1920f; // Fallback
    }

    // ---------------------------------------------

    public void HandleMainMenu(bool shouldTurnOn)
    {
        mainMenu.SetActive(shouldTurnOn);
        // Ana menü açýldýðýnda pozisyonlarý resetle (Oyun içinden dönünce kaymýþ olmasýn)
        if (shouldTurnOn && mainMenuRect != null && settingsRect != null)
        {
            mainMenuRect.anchoredPosition = Vector2.zero;
            settingsRect.anchoredPosition = new Vector2(GetCanvasWidth(), 0);
            settingsMenu.SetActive(true); // Settings de arkada hazýr beklesin
        }
    }

    public void HandlePauseMenu(bool shouldTurnOn) => pauseMenu.SetActive(shouldTurnOn);
    public void HandleTimeScale(float timeScale) => Time.timeScale = timeScale;
    public void SetCanPause(bool pause) => CanPause = pause;
    public void SetPlayerCanPlay(bool can) => PlayerManager.Instance.SetPlayerBasicMovements(can);

    public void HandleCursorState(bool shouldBeFree)
    {
        Cursor.lockState = shouldBeFree ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = shouldBeFree;
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        Debug.Log("QUIT");
        Application.Quit();
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
        FindAndAssignCamera();

        if (DayManager.Instance != null)
        {
            if (scene.name == "Scene0")
            {
                DayManager.Instance.ResetForGameplay();

                SetPlayerCanPlay(true);
                HandleTimeScale(1);
                HandleCursorState(false);
                HandleMainMenu(false);
                HandlePauseMenu(false);
                SetCanPause(true);
                UpdateDoFState(false);
                if (SoundManager.Instance) SoundManager.Instance.SwitchSnapshot("Outside", 0f);
            }
            else if (scene.name == "MainMenu")
            {
                DayManager.Instance.ResetForMainMenu();

                HandleTimeScale(1);
                HandleCursorState(true);
                HandleMainMenu(true);
                HandlePauseMenu(false);
                SetCanPause(false);
                UpdateDoFState(true);
                if (SoundManager.Instance) SoundManager.Instance.SwitchSnapshot("Outside", 0f);
            }
        }
    }

    void FindAndAssignCamera()
    {
        GameObject camObj = GameObject.Find("UI_Camera");

        if (camObj != null)
        {
            Camera uiCam = camObj.GetComponent<Camera>();
            Camera mainCam = Camera.main;

            if (myCanvas != null)
            {
                myCanvas.worldCamera = uiCam;
                myCanvas.planeDistance = 5;
            }

            if (mainCam != null)
            {
                var cameraData = mainCam.GetUniversalAdditionalCameraData();
                bool isAlreadyInStack = false;
                foreach (var c in cameraData.cameraStack)
                {
                    if (c == uiCam) isAlreadyInStack = true;
                }

                if (!isAlreadyInStack)
                {
                    cameraData.cameraStack.Add(uiCam);
                }
            }
        }
    }

    private void UpdateDoFState(bool enableDoF)
    {
        if (globalVolume == null) globalVolume = FindObjectOfType<Volume>();

        if (globalVolume != null)
        {
            if (globalVolume.profile.TryGet(out DepthOfField dof))
            {
                dof.active = enableDoF;
            }
        }
    }

    public void RegisterScaler(PixelPerfectCanvasScaler scaler)
    {
        if (!activeScalers.Contains(scaler))
        {
            activeScalers.Add(scaler);
            scaler.UpdateScale();
        }
    }

    public void UnregisterScaler(PixelPerfectCanvasScaler scaler)
    {
        if (activeScalers.Contains(scaler))
        {
            activeScalers.Remove(scaler);
        }
    }

    public void RefreshAllCanvases()
    {
        foreach (var scaler in activeScalers)
        {
            if (scaler != null)
                scaler.UpdateScale();
        }
    }
}