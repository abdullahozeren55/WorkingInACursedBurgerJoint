using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic; // List için

public class RebindUI : MonoBehaviour
{
    [Header("Hangi Aksiyon?")]
    [SerializeField] private InputActionReference inputActionReference;

    [Header("Ayarlar")]
    [SerializeField] private bool excludeGamepad = false;
    [SerializeField] private int selectedBindingIndex = 0;

    [Header("Klavye Görseli (Buton 1)")]
    [SerializeField] private GameObject keyboardButtonObj; // Klavye butonu (Parent)
    [SerializeField] private TMP_Text keyboardButtonText;  // İçindeki yazı
    [SerializeField] private Button keyboardButtonComp;    // Tıklanma özelliği için

    [Header("Mouse Görseli (Buton 2)")]
    [SerializeField] private GameObject mouseButtonObj;    // Mouse butonu (Parent)
    [SerializeField] private Image mouseButtonImage;       // İçindeki ikon resmi
    [SerializeField] private Button mouseButtonComp;       // Tıklanma özelliği için

    [Header("Mouse Sprite Tanımları")]
    [Tooltip("Unity'den gelen tuş ismi (örn: leftButton) ile Sprite eşleşmesi")]
    [SerializeField] private List<MouseIconMap> mouseIcons;

    [Header("Helper (Yanıp Sönen Yazı)")]
    [SerializeField] private GameObject helperTextObj;     // "Bir tuşa basın..." objesi
    [SerializeField] private TMP_Text helperTextComp;      // Alpha ayarı için Text bileşeni

    private InputActionRebindingExtensions.RebindingOperation _rebindingOperation;
    private InputAction _targetAction;
    private static bool isAnyRebindingInProgress = false;
    private Coroutine _pulseCoroutine; // Yanıp sönme animasyonu için

    [System.Serializable]
    public struct MouseIconMap
    {
        public string controlName; // örn: "leftButton", "rightButton", "middleButton"
        public Sprite icon;
    }

    private void Start()
    {
        // 1. Hedef Aksiyonu Bul
        string actionId = inputActionReference.action.id.ToString();

        if (InputManager.Instance != null)
        {
            _targetAction = InputManager.Instance.GetAction(actionId);
        }
        else
        {
            _targetAction = inputActionReference.action;
        }

        helperTextObj.SetActive(false);

        // 2. Başlangıçta doğru butonu göster
        UpdateUI();
    }

    private void OnEnable()
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged += UpdateUI;

        UpdateUI();
    }

    private void OnDisable()
    {
        StopRebindingLogic();

        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged -= UpdateUI;
    }

    // --- REBIND BAŞLATMA ---
    // Bu fonksiyonu hem Klavye Butonuna hem Mouse Butonuna onClick olarak vereceksin.
    public void StartRebinding()
    {
        StartCoroutine(StartRebindingRoutine());
    }

    private IEnumerator StartRebindingRoutine()
    {
        if (isAnyRebindingInProgress) yield break;
        if (_targetAction == null) { Debug.LogError("HATA: Hedef Aksiyon Yok!"); yield break; }

        isAnyRebindingInProgress = true;

        // 1. Tıklamayı engelle
        if (keyboardButtonComp) keyboardButtonComp.interactable = false;
        if (mouseButtonComp) mouseButtonComp.interactable = false;

        yield return new WaitForSecondsRealtime(0.1f);

        // 2. Aksiyonu durdur
        _targetAction.Disable();

        // 3. GÖRSEL DÜZENLEME (Senin istediğin yapı)
        keyboardButtonObj.SetActive(false); // Butonları gizle
        mouseButtonObj.SetActive(false);
        helperTextObj.SetActive(true);      // Helper'ı aç

        // 4. Animasyonu Başlat (Ping Pong)
        if (_pulseCoroutine != null) StopCoroutine(_pulseCoroutine);
        _pulseCoroutine = StartCoroutine(PulseHelperText());

        // 5. Operasyonu Başlat
        var rebindOperation = _targetAction.PerformInteractiveRebinding(selectedBindingIndex);

        if (excludeGamepad)
        {
            // Klavye + Mouse Modu
            rebindOperation.WithControlsExcluding("<Gamepad>");
            rebindOperation.WithControlsExcluding("<Mouse>/position");
            rebindOperation.WithControlsExcluding("<Mouse>/delta");
        }
        else
        {
            // Gamepad Modu
            rebindOperation.WithControlsHavingToMatchPath("<Gamepad>");
        }

        rebindOperation.WithControlsExcluding("<Pointer>/position");
        rebindOperation.WithCancelingThrough("<Keyboard>/escape");
        rebindOperation.OnComplete(operation => RebindCompleted());
        rebindOperation.OnCancel(operation => RebindCompleted());

        _rebindingOperation = rebindOperation.Start();
    }

    // --- YANIP SÖNME ANİMASYONU ---
    private IEnumerator PulseHelperText()
    {
        if (helperTextComp == null) yield break;

        float alpha = 1f;
        while (true)
        {
            // Realtime kullandık ki Pause menüsünde de yanıp sönsün
            float time = Time.unscaledTime * 3f; // Hız çarpanı
            alpha = Mathf.PingPong(time, 1f); // 0 ile 1 arası gider gelir

            // Text'in rengini güncelle (sadece Alpha değişir)
            Color c = helperTextComp.color;
            c.a = 0.2f + (alpha * 0.8f); // Tam kaybolmasın, min 0.2 olsun
            helperTextComp.color = c;

            yield return null;
        }
    }

    private void RebindCompleted()
    {
        StopRebindingLogic();
        if (InputManager.Instance != null) InputManager.Instance.SaveBindingOverrides();
    }

    private void StopRebindingLogic()
    {
        // Operasyonu temizle
        if (_rebindingOperation != null)
        {
            _rebindingOperation.Dispose();
            _rebindingOperation = null;
        }

        if (_targetAction != null) _targetAction.Enable();

        // Animasyonu durdur
        if (_pulseCoroutine != null) StopCoroutine(_pulseCoroutine);

        // Helper'ı kapat
        if (helperTextObj != null) helperTextObj.SetActive(false);

        // Kilitleri kaldır ama hemen açma (Gecikmeli)
        isAnyRebindingInProgress = false;

        // UI'ı güncelle (Bu fonksiyon butonları tekrar açacak)
        UpdateUI();

        // Butonları tıklanabilir yap (Gecikmeli)
        StartCoroutine(EnableButtonsAfterDelay());
    }

    private IEnumerator EnableButtonsAfterDelay()
    {
        yield return new WaitForSecondsRealtime(0.1f);
        if (keyboardButtonComp) keyboardButtonComp.interactable = true;
        if (mouseButtonComp) mouseButtonComp.interactable = true;
    }

    // --- KRİTİK BÖLÜM: UI GÜNCELLEME ---
    public void UpdateUI()
    {
        if (_targetAction == null) return;

        // 1. Ham path'i al (örn: "<Keyboard>/space" veya "<Mouse>/leftButton")
        // Not: effectivePath kullanmak daha garantidir
        string bindingPath = _targetAction.bindings[selectedBindingIndex].effectivePath;

        // Eğer override varsa onu, yoksa default'u alır
        // Ancak biz Unity'nin o an neyi seçili tuttuğunu binding mask'ten de anlayabiliriz
        // Daha güvenli yöntem: GetBindingDisplayString kullanıp cihazı tahmin etmek
        // Ama senin durumunda Path kontrolü en temizi.

        // Eğer binding override yapılmışsa path değişmiş olabilir.
        // Garanti olsun diye binding'in override path'ine bakalım.
        if (!string.IsNullOrEmpty(_targetAction.bindings[selectedBindingIndex].overridePath))
        {
            bindingPath = _targetAction.bindings[selectedBindingIndex].overridePath;
        }

        // 2. Ayrımı Yap: Klavye mi, Mouse mu?
        bool isMouse = bindingPath.Contains("<Mouse>");

        if (isMouse)
        {
            // --- MOUSE MODU ---
            keyboardButtonObj.SetActive(false);
            mouseButtonObj.SetActive(true);

            // İkonu ayarla
            // Path genelde "<Mouse>/leftButton" şeklindedir.
            // Sadece "leftButton" kısmını alalım.
            string controlName = bindingPath.Replace("<Mouse>/", "").Trim();

            Sprite icon = GetMouseSprite(controlName);
            if (mouseButtonImage != null) mouseButtonImage.sprite = icon;
        }
        else
        {
            // --- KLAVYE MODU ---
            mouseButtonObj.SetActive(false);
            keyboardButtonObj.SetActive(true);

            // Yazıyı ayarla
            string displayString = _targetAction.GetBindingDisplayString(selectedBindingIndex, InputBinding.DisplayStringOptions.DontUseShortDisplayNames);
            string prettyName = GetKeyboardKeyText(displayString);

            if (keyboardButtonText != null) keyboardButtonText.text = prettyName;
        }
    }

    // Mouse ismine göre Sprite getiren fonksiyon
    private Sprite GetMouseSprite(string controlName)
    {
        // Inspector'daki listeden ara
        foreach (var item in mouseIcons)
        {
            if (item.controlName == controlName)
                return item.icon;
        }

        // Bulamazsa ilkini veya null döndür
        Debug.LogWarning($"Mouse ikonu bulunamadı: {controlName}");
        return null;
    }

    // ... (Senin GetKeyboardKeyText fonksiyonun AYNEN BURAYA GELECEK) ...
    // Kopyala yapıştır yaparken önceki mesajındaki GetKeyboardKeyText'i buraya koymayı unutma.
    private string GetKeyboardKeyText(string originalName)
    {
        if (string.IsNullOrEmpty(originalName)) return "NONE";

        // Unity'den gelen ismi temizle
        string cleanName = originalName.Replace("Keyboard/", "").Trim();

        // --- 1. "İ" ve "I" AYRIMI (KESİN ÇÖZÜM) ---
        string lowerRaw = cleanName.ToLowerInvariant();

        if (lowerRaw == "i") return "İ"; // Küçük i (noktalı) -> Büyük İ
        if (lowerRaw == "ı") return "I"; // Küçük ı (noktasız) -> Büyük I

        // Artık diğerleri için büyütebiliriz
        string upperKey = cleanName.ToUpperInvariant();

        // --- Dil Kontrolü ---
        string lang = "en";
        if (LocalizationManager.Instance != null)
            lang = LocalizationManager.Instance.GetCurrentLanguageCode();
        else
            lang = PlayerPrefs.GetString("Language", "en");

        bool isTR = lang == "tr";

        // --- 2. ADIM: ÖZEL TUŞLAR ---

        switch (upperKey)
        {
            // Temel Tuşlar
            case "SPACE": return isTR ? "BOŞLUK" : "SPACE";
            case "ENTER": case "RETURN": return isTR ? "GİRİŞ" : "ENTER";

            case "TAB": return "TAB";
            case "ESCAPE": case "ESC": return "ESC";
            case "CAPS LOCK": case "CAPSLOCK": return "CAPS";
            case "BACKSPACE": return isTR ? "SİL" : "BACK";

            // --- MODIFIER TUŞLAR ---
            case "LEFT SHIFT":
            case "LSHIFT":
            case "SHIFT":
                return isTR ? "SOL SHIFT" : "L.SHIFT";
            case "RIGHT SHIFT":
            case "RSHIFT":
                return isTR ? "SAĞ SHIFT" : "R.SHIFT";

            case "LEFT CTRL":
            case "LCTRL":
            case "CTRL":
            case "CONTROL":
            case "LEFT CONTROL":
                return isTR ? "SOL CTRL" : "L.CTRL";
            case "RIGHT CTRL":
            case "RCTRL":
            case "RIGHT CONTROL":
                return isTR ? "SAĞ CTRL" : "R.CTRL";

            case "LEFT ALT":
            case "LALT":
            case "ALT":
                return isTR ? "SOL ALT" : "L.ALT";
            case "RIGHT ALT":
            case "RALT":
            case "ALT GR":
                return isTR ? "SAĞ ALT" : "R.ALT";

            // Yön Tuşları 
            case "UP ARROW": case "UP": return "↑";
            case "DOWN ARROW": case "DOWN": return "↓";
            case "LEFT ARROW": case "LEFT": return "←";
            case "RIGHT ARROW": case "RIGHT": return "→";

            // Mouse (Fallback olarak yazı kalsın ama normalde Sprite görünecek)
            case "LEFT BUTTON": case "LMB": return isTR ? "SOL TIK" : "L.CLICK";
            case "RIGHT BUTTON": case "RMB": return isTR ? "SAĞ TIK" : "R.CLICK";
            case "MIDDLE BUTTON": case "MMB": return isTR ? "ORTA TIK" : "M.CLICK";
            case "FORWARD": return isTR ? "İLERİ" : "FWD";
            case "BACK": return isTR ? "GERİ" : "BACK";
        }

        // --- 3. ADIM: NUMPAD FİLTRESİ ---
        if (upperKey.StartsWith("NUM"))
        {
            string suffix = upperKey.Replace("NUMPAD", "").Replace("NUM", "").Trim();
            if (suffix == "ENTER") return isTR ? "NUM GİRİŞ" : "NUM ENTER";

            if (int.TryParse(suffix, out int number))
            {
                return "NUM " + number;
            }
            return "?";
        }

        // --- 4. ADIM: TEK KARAKTER KONTROLÜ ---
        if (upperKey.Length == 1)
        {
            string allAllowed = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789ÇĞİÖŞÜI";
            if (allAllowed.Contains(upperKey)) return upperKey;
        }

        return "?";
    }
}