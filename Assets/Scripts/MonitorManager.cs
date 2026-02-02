using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class MonitorManager : MonoBehaviour
{
    public static MonitorManager Instance;

    public bool IsFocused;

    public Monitor MonitorSC;

    public Transform windowParent;

    [System.Serializable]
    public struct SongData
    {
        public string trackName; // Normal için: "Track Name", Horror için: "Localization Key"
        public AudioClip clip;   // Müzik dosyasý
    }

    public enum PageState
    {
        Closed,
        Opened,
        Minimized
    }

    [Header("Burger Page Settings")]
    public FontType burgerFontType = FontType.RetroUINoOutline;

    public Image burgerImage;
    public Image burgerImageWorld;
    public Sprite[] burgerSprites;
    [Space]
    public TMP_Text ingredientsTMP;
    public TMP_Text ingredientsTMPWorld;
    public string[] ingredientKeys;
    [Space]
    public TMP_Text descriptionTMP;
    public TMP_Text descriptionTMPWorld;
    public string[] descriptionKeys;
    [Space]
    public TMP_Text headerTMP;
    public TMP_Text headerTMPWorld;
    public string[] headerKeys;
    [Space]
    private int currentBurgerIndex = 0;

    // --- HAFIZA DEÐÝÞKENLERÝ (Burger) ---
    private float _initHeaderSize;
    private float _initHeaderSpacing;
    private float _initIngSize;
    private float _initIngSpacing;
    private float _initDescSize;
    private float _initDescSpacing;

    [Header("Order List Settings")]
    public FontType orderFontType = FontType.RetroUINoOutline;
    public TMP_Text currentOrdersTMP;
    public TMP_Text currentOrdersTMPWorld;

    private List<OrderData> _activeOrderList = new List<OrderData>();
    private float _initOrderSize;
    private float _initOrderSpacing;
    private float _initOrderLineSpacing;

    [Header("Notepad Page Settings")]
    public InputMirror notePadInputMirror;

    [Header("Music Player Settings")]
    public PageState musicPlayerPageState;
    public AudioSource musicSource;
    public List<SongData> playlist;

    // YENÝ: Slider Referanslarý (Sesi fullemek için görsel update lazým)
    public Slider musicVolumeSlider;
    public Slider musicVolumeSliderWorld;

    // YENÝ: Horror Playlist ve Ayarlarý
    [Header("Horror Music Settings")]
    public List<SongData> horrorPlaylist; // Gizli liste
    public FontType musicFontType = FontType.DialogueOutlined; // Horror font tipi
    public Color horrorMusicColor = Color.red; // Korkunçlu müzik rengi

    // Marquee'nin içindeki Text bileþenleri (Font ve Renk deðiþimi için)
    public TMP_Text musicTitleTMP;
    public TMP_Text musicTitleTMPWorld;

    private bool _isHorrorPlaying = false; // Þu an korkunçlu müzik mi çalýyor?

    // Müzik Text Hafýzasý (Geri dönmek için)
    private float _initMusicSize;
    private float _initMusicSpacing;
    private float _initMusicSizeWorld;   // World için hafýza
    private float _initMusicSpacingWorld; // World için hafýza

    private Color _initMusicColor; // Normal renk hafýzasý
    private TMP_FontAsset _defaultMusicFont;

    public bool MusicIsPlaying = false;
    public float musicVolumeMultiplier = 0.2f;
    private int currentIndex = 0;
    [Space]
    public RetroMarquee universalMarquee;
    public float marqueeStepInterval = 0.4f;
    [Space]
    public GameObject musicPlayerWindow;
    public Image playPauseImage;
    public Image playPauseImageWorld;
    public Sprite[] playPauseSprites;

    [Header("Video Player Settings")]
    public VideoPlayer monitorVideoPlayer;
    public VideoClip[] videoClips;

    public DesktopIcon CurrentSelectedIcon { get; private set; }

    private void OnEnable()
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged += RefreshPage;
        }
    }

    private void OnDisable()
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged -= RefreshPage;
        }
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // --- BURGER HAFIZA ---
        if (headerTMP != null)
        {
            _initHeaderSize = headerTMP.fontSize;
            _initHeaderSpacing = headerTMP.characterSpacing;
        }
        if (ingredientsTMP != null)
        {
            _initIngSize = ingredientsTMP.fontSize;
            _initIngSpacing = ingredientsTMP.characterSpacing;
        }
        if (descriptionTMP != null)
        {
            _initDescSize = descriptionTMP.fontSize;
            _initDescSpacing = descriptionTMP.characterSpacing;
        }

        // --- ORDER LIST HAFIZA ---
        if (currentOrdersTMP != null)
        {
            _initOrderSize = currentOrdersTMP.fontSize;
            _initOrderSpacing = currentOrdersTMP.characterSpacing;
            _initOrderLineSpacing = currentOrdersTMP.lineSpacing;
        }

        // --- MÜZÝK PLAYER HAFIZA (YENÝ) ---
        // UI Text
        if (musicTitleTMP != null)
        {
            _initMusicSize = musicTitleTMP.fontSize;
            _initMusicSpacing = musicTitleTMP.characterSpacing;
            _initMusicColor = musicTitleTMP.color; // Rengi kaydet
            _defaultMusicFont = musicTitleTMP.font;
        }

        // World Text
        if (musicTitleTMPWorld != null)
        {
            _initMusicSizeWorld = musicTitleTMPWorld.fontSize;
            _initMusicSpacingWorld = musicTitleTMPWorld.characterSpacing;
            // World rengini ayrýca kaydetmeye gerek yok, ikisi ayný renk varsayýyoruz ama istersen alabilirsin.
        }
    }

    private void Start()
    {
        if (playlist.Count > 0)
        {
            OpenWindow(musicPlayerWindow);
            HandleMusicPlayerPage(true);
            SetMusicPlayerPageState(1);
            LoadTrack(0, true);
        }

        StartCoroutine(MarqueeDriverLoop());
        RefreshPage();
    }

    private void Update()
    {
        if (IsFocused)
        {
            if (InputManager.Instance.PlayerThrow())
            {
                notePadInputMirror.SyncInputs();
                MonitorSC.FinishMonitorUI();
                IsFocused = false;
            }
        }

        // --- ÞARKI BÝTÝÞ KONTROLÜ ---
        if (MusicIsPlaying && !musicSource.isPlaying)
        {
            // Eðer Horror modundaysak, bittiði gibi normal listeye dön
            if (_isHorrorPlaying)
            {
                NextSong();
            }
        }
    }

    // ... (Video Player, Window Focus, Icon Selection, Burger Page ayný) ...

    // Video Player
    public void PlayMonitorVideo(int index)
    {
        if (index < 0 || index >= videoClips.Length) return;
        if (monitorVideoPlayer == null) monitorVideoPlayer = GetComponent<VideoPlayer>();
        monitorVideoPlayer.clip = videoClips[index];
        monitorVideoPlayer.Play();
    }

    public void StopMonitorVideo()
    {
        if (monitorVideoPlayer == null) return;
        if (monitorVideoPlayer.isPlaying) monitorVideoPlayer.Stop();
        monitorVideoPlayer.clip = null;
    }

    // Window Management
    public void FocusWindow(WindowController targetWindow)
    {
        targetWindow.SetLast();
        foreach (Transform child in windowParent)
        {
            if (!child.gameObject.activeSelf) continue;
            WindowController wc = child.GetComponent<WindowController>();
            if (wc != null) wc.SetState(wc == targetWindow);
        }
    }

    public void OnWindowClosed(WindowController closedWindow)
    {
        WindowController nextActive = null;
        int highestIndex = -1;
        foreach (Transform child in windowParent)
        {
            if (child == closedWindow.transform || !child.gameObject.activeSelf) continue;
            WindowController wc = child.GetComponent<WindowController>();
            if (wc != null && child.GetSiblingIndex() > highestIndex)
            {
                highestIndex = child.GetSiblingIndex();
                nextActive = wc;
            }
        }
        if (nextActive != null) FocusWindow(nextActive);
    }

    public void OpenWindow(GameObject windowObj)
    {
        WindowController wc = windowObj.GetComponent<WindowController>();
        if (wc != null)
        {
            wc.TurnOnOff(true);
            FocusWindow(wc);
        }
    }

    public void SelectIcon(DesktopIcon newIcon)
    {
        if (CurrentSelectedIcon != null && CurrentSelectedIcon != newIcon)
            CurrentSelectedIcon.DeselectVisuals();
        CurrentSelectedIcon = newIcon;
    }

    public void DeselectAll()
    {
        if (CurrentSelectedIcon != null)
        {
            CurrentSelectedIcon.DeselectVisuals();
            CurrentSelectedIcon = null;
        }
    }

    // Burger & Order Logic
    public void SetBurgerPage(int value)
    {
        currentBurgerIndex = value;
        RefreshPage();
    }

    public void SetCurrentOrder(List<OrderData> orders)
    {
        _activeOrderList = orders;
        RefreshOrderListDisplay();
    }

    public void ClearCurrentOrder()
    {
        _activeOrderList.Clear();
        if (currentOrdersTMP != null) currentOrdersTMP.text = "";
        if (currentOrdersTMPWorld != null) currentOrdersTMPWorld.text = "";
    }

    private void RefreshPage()
    {
        RefreshBurgerPageDisplay();
        RefreshOrderListDisplay();
    }

    private void RefreshBurgerPageDisplay()
    {
        if (currentBurgerIndex < 0 || currentBurgerIndex >= burgerSprites.Length) return;
        burgerImage.sprite = burgerSprites[currentBurgerIndex];
        burgerImageWorld.sprite = burgerImage.sprite;

        if (LocalizationManager.Instance != null)
        {
            var targetData = LocalizationManager.Instance.GetFontDataForCurrentLanguage(burgerFontType);
            var defaultData = LocalizationManager.Instance.GetDefaultFontData(burgerFontType);

            float defaultBase = Mathf.Max(defaultData.basePixelSize, 0.1f);
            float ratio = targetData.basePixelSize / defaultBase;
            float charSpacingDiff = targetData.characterSpacingOffset - defaultData.characterSpacingOffset;

            string headText = LocalizationManager.Instance.GetText(headerKeys[currentBurgerIndex]);
            UpdateTextComp(headerTMP, headText, targetData.font, _initHeaderSize * ratio, _initHeaderSpacing + charSpacingDiff);
            UpdateTextComp(headerTMPWorld, headText, targetData.font, _initHeaderSize * ratio, _initHeaderSpacing + charSpacingDiff);

            string ingText = LocalizationManager.Instance.GetText(ingredientKeys[currentBurgerIndex]);
            UpdateTextComp(ingredientsTMP, ingText, targetData.font, _initIngSize * ratio, _initIngSpacing + charSpacingDiff);
            UpdateTextComp(ingredientsTMPWorld, ingText, targetData.font, _initIngSize * ratio, _initIngSpacing + charSpacingDiff);

            string descText = LocalizationManager.Instance.GetText(descriptionKeys[currentBurgerIndex]);
            UpdateTextComp(descriptionTMP, descText, targetData.font, _initDescSize * ratio, _initDescSpacing + charSpacingDiff);
            UpdateTextComp(descriptionTMPWorld, descText, targetData.font, _initDescSize * ratio, _initDescSpacing + charSpacingDiff);
        }
    }

    private void RefreshOrderListDisplay()
    {
        if (_activeOrderList == null || _activeOrderList.Count == 0) return;
        if (currentOrdersTMP == null && currentOrdersTMPWorld == null) return;
        if (LocalizationManager.Instance == null) return;

        var targetData = LocalizationManager.Instance.GetFontDataForCurrentLanguage(orderFontType);
        var defaultData = LocalizationManager.Instance.GetDefaultFontData(orderFontType);

        float defaultBase = Mathf.Max(defaultData.basePixelSize, 0.1f);
        float ratio = targetData.basePixelSize / defaultBase;
        float charSpacingDiff = targetData.characterSpacingOffset - defaultData.characterSpacingOffset;
        float lineSpacingDiff = targetData.lineSpacingOffset - defaultData.lineSpacingOffset;

        StringBuilder finalSb = new StringBuilder();
        for (int i = 0; i < _activeOrderList.Count; i++)
        {
            string singleOrderText = BuildOrderString(_activeOrderList[i]);
            if (!string.IsNullOrEmpty(singleOrderText))
            {
                finalSb.Append(singleOrderText);
                if (i < _activeOrderList.Count - 1) finalSb.Append("\n\n");
            }
        }
        string finalContent = finalSb.ToString();

        ApplyOrderTextSettings(currentOrdersTMP, finalContent, targetData.font,
            _initOrderSize * ratio, _initOrderSpacing + charSpacingDiff, _initOrderLineSpacing + lineSpacingDiff);

        ApplyOrderTextSettings(currentOrdersTMPWorld, finalContent, targetData.font,
            _initOrderSize * ratio, _initOrderSpacing + charSpacingDiff, _initOrderLineSpacing + lineSpacingDiff);
    }

    private string BuildOrderString(OrderData data)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var item in data.RequiredBurgers) { if (!string.IsNullOrEmpty(item.OrderKey) && item.Count > 0) sb.AppendLine($"{LocalizationManager.Instance.GetText(item.OrderKey)} x{item.Count}"); }
        foreach (var item in data.RequiredDrinks) { if (!string.IsNullOrEmpty(item.OrderKey) && item.Count > 0) sb.AppendLine($"{LocalizationManager.Instance.GetText(item.OrderKey)} x{item.Count}"); }
        foreach (var item in data.RequiredSides) { if (!string.IsNullOrEmpty(item.OrderKey) && item.Count > 0) sb.AppendLine($"{LocalizationManager.Instance.GetText(item.OrderKey)} x{item.Count}"); }
        foreach (var item in data.RequiredSauces) { if (!string.IsNullOrEmpty(item.OrderKey) && item.Count > 0) sb.AppendLine($"{LocalizationManager.Instance.GetText(item.OrderKey)} x{item.Count}"); }
        foreach (var item in data.RequiredToys) { if (!string.IsNullOrEmpty(item.OrderKey) && item.Count > 0) sb.AppendLine($"{LocalizationManager.Instance.GetText(item.OrderKey)} x{item.Count}"); }
        return sb.ToString().TrimEnd();
    }

    private void ApplyOrderTextSettings(TMP_Text textComp, string content, TMP_FontAsset font, float size, float spacing, float lineSpacing)
    {
        if (textComp == null) return;
        textComp.text = content;
        if (font != null) textComp.font = font;
        textComp.fontSize = size;
        textComp.characterSpacing = spacing;
        textComp.lineSpacing = lineSpacing;
    }

    private void UpdateTextComp(TMP_Text textComp, string content, TMP_FontAsset font, float size, float spacing)
    {
        if (textComp == null) return;
        textComp.text = content;
        if (font != null) textComp.font = font;
        textComp.fontSize = size;
        textComp.characterSpacing = spacing;
    }

    // ==================================================================================
    // MUSIC PLAYER MANTIKLARI
    // ==================================================================================

    public void HandleMusicPlayerPage(bool open)
    {
        if (open)
        {
            if (musicPlayerPageState == PageState.Closed)
                LoadTrack(currentIndex, false);
        }
        else
        {
            if (MusicIsPlaying)
            {
                MusicIsPlaying = false;
                musicSource.Stop();
                playPauseImage.sprite = playPauseSprites[0];
                playPauseImageWorld.sprite = playPauseImage.sprite;
            }
        }
    }

    public void SetMusicPlayerPageState(int num)
    {
        musicPlayerPageState = (PageState)num;
    }

    public void PlayPauseMusic()
    {
        MusicIsPlaying = !MusicIsPlaying;
        playPauseImage.sprite = MusicIsPlaying ? playPauseSprites[1] : playPauseSprites[0];
        playPauseImageWorld.sprite = playPauseImage.sprite;

        if (MusicIsPlaying) musicSource.Play();
        else musicSource.Pause();
    }

    public void NextSong()
    {
        // Horror Modundan Çýkýþ -> Normal Mod
        if (_isHorrorPlaying)
        {
            _isHorrorPlaying = false;
            // Sýradaki normal parçayý çal, volume vs. eski haline DÖNMEZ.
            LoadTrack(currentIndex, true);
            return;
        }

        currentIndex++;
        if (currentIndex >= playlist.Count) currentIndex = 0;
        LoadTrack(currentIndex, true);
    }

    public void PreviousSong()
    {
        // Horror Modundan Çýkýþ -> Normal Mod
        if (_isHorrorPlaying)
        {
            _isHorrorPlaying = false;
            LoadTrack(currentIndex, true);
            return;
        }

        currentIndex--;
        if (currentIndex < 0) currentIndex = playlist.Count - 1;
        LoadTrack(currentIndex, true);
    }

    // --- DIÞARIDAN ÇAÐIRILACAK HORROR FONKSÝYONU ---
    public void PlayRandomHorrorTrack()
    {
        if (horrorPlaylist == null || horrorPlaylist.Count == 0)
        {
            Debug.LogWarning("Horror Playlist boþ!");
            return;
        }

        // 1. PENCEREYÝ ZORLA AÇ (Görünür yap)
        // Kapalýysa veya Minimized ise -> Açýk hale getir.
        if (musicPlayerPageState != PageState.Opened)
        {
            SetMusicPlayerPageState(1); // 1 = Opened
            HandleMusicPlayerPage(true); // Gerekli yüklemeleri yap
            OpenWindow(musicPlayerWindow); // UI'da en öne getir ve aktifleþtir
        }

        // 2. SESÝ FULLE (Max Volume)
        musicSource.volume = 1.0f * musicVolumeMultiplier;

        // Slider görsellerini güncelle (Hem UI hem World)
        if (musicVolumeSlider != null) musicVolumeSlider.value = 1.0f;
        if (musicVolumeSliderWorld != null) musicVolumeSliderWorld.value = 1.0f;

        // 3. RASTGELE SEÇÝM
        int rnd = Random.Range(0, horrorPlaylist.Count);
        SongData horrorSong = horrorPlaylist[rnd];
        _isHorrorPlaying = true;
        musicSource.clip = horrorSong.clip;

        // 4. TEXT LOCALIZATION & FONT & RENK AYARLAMASI
        string localizedName = LocalizationManager.Instance.GetText(horrorSong.trackName);

        if (LocalizationManager.Instance != null)
        {
            var targetData = LocalizationManager.Instance.GetFontDataForCurrentLanguage(musicFontType);
            var defaultData = LocalizationManager.Instance.GetDefaultFontData(musicFontType);

            float defaultBase = Mathf.Max(defaultData.basePixelSize, 0.1f);
            float ratio = targetData.basePixelSize / defaultBase;
            float charSpacingDiff = targetData.characterSpacingOffset - defaultData.characterSpacingOffset;

            // --- UI TEXT ---
            if (musicTitleTMP != null)
            {
                musicTitleTMP.font = targetData.font;
                musicTitleTMP.fontSize = _initMusicSize * ratio;
                musicTitleTMP.characterSpacing = _initMusicSpacing + charSpacingDiff;
                musicTitleTMP.color = horrorMusicColor; // KORKUNÇLU RENK
            }

            // --- WORLD TEXT ---
            if (musicTitleTMPWorld != null)
            {
                musicTitleTMPWorld.font = targetData.font;
                musicTitleTMPWorld.fontSize = _initMusicSizeWorld * ratio;
                musicTitleTMPWorld.characterSpacing = _initMusicSpacingWorld + charSpacingDiff;
                musicTitleTMPWorld.color = horrorMusicColor; // KORKUNÇLU RENK
            }
        }

        // 5. Marquee Güncelle
        if (universalMarquee != null)
        {
            universalMarquee.RefreshText($"{localizedName}");
        }

        // 6. OYNAT (Pause ise play yap)
        PlayMusicInternal();
    }

    // --- NORMAL ÞARKI YÜKLEME ---
    private void LoadTrack(int index, bool autoPlay)
    {
        if (playlist.Count == 0) return;

        // Eðer Horror modundan normal moda dönüyorsak:
        // Sadece FONT ve RENGÝ eski haline getiriyoruz.
        // Volume ve Pencere açýklýðý KALIYOR.

        // UI Restore
        if (musicTitleTMP != null)
        {
            musicTitleTMP.font = _defaultMusicFont;
            musicTitleTMP.fontSize = _initMusicSize;
            musicTitleTMP.characterSpacing = _initMusicSpacing;
            musicTitleTMP.color = _initMusicColor; // Rengi normale döndür
        }

        // World Restore
        if (musicTitleTMPWorld != null)
        {
            musicTitleTMPWorld.font = _defaultMusicFont;
            musicTitleTMPWorld.fontSize = _initMusicSizeWorld;
            musicTitleTMPWorld.characterSpacing = _initMusicSpacingWorld;
            musicTitleTMPWorld.color = _initMusicColor; // Rengi normale döndür
        }

        SongData song = playlist[index];
        musicSource.clip = song.clip;

        string displayName = $"{index + 1:00}. {song.trackName}";

        if (universalMarquee != null)
        {
            universalMarquee.RefreshText(displayName);
        }

        if (autoPlay || musicSource.isPlaying)
        {
            PlayMusicInternal();
        }
    }

    // Yardýmcý oynatma fonksiyonu
    private void PlayMusicInternal()
    {
        musicSource.Play();
        MusicIsPlaying = true;

        // Ýkonlarý 'Pause' sembolüne çevir (çünkü çalýyor)
        if (playPauseImage != null && playPauseSprites.Length > 1)
            playPauseImage.sprite = playPauseSprites[1];

        if (playPauseImageWorld != null && playPauseSprites.Length > 1)
            playPauseImageWorld.sprite = playPauseSprites[1];
    }

    // Bu fonksiyon slider'dan çaðýrýlýyorsa, UI'dan elle deðiþtirildiðinde çalýþýr.
    public void SetMusicVolume(float value)
    {
        musicSource.volume = value * musicVolumeMultiplier;
    }

    public void UpadateShowHint()
    {
        MonitorSC.UpdateShowHint();
    }

    private IEnumerator MarqueeDriverLoop()
    {
        WaitForSeconds wait = new WaitForSeconds(marqueeStepInterval);
        while (true)
        {
            yield return wait;
            if (universalMarquee != null) universalMarquee.Step();
        }
    }
}