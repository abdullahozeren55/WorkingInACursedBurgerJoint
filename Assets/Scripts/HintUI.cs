using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class HintUI : MonoBehaviour
{
    [System.Serializable]
    public struct InputIconData
    {
        public string controlName;
        public Sprite icon;
        public Sprite emissionMap;
        [ColorUsage(true, true)]
        public Color glowColor;
    }

    [Header("Hangi Tuþu Gösterecek?")]
    [SerializeField] private InputActionReference actionReference;

    [Header("Bileþenler")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text keyText;
    [SerializeField] private UIGlowController glowController;

    [Header("Görsel Veritabaný")]
    [SerializeField] private List<InputIconData> gamepadIcons;
    [SerializeField] private List<InputIconData> mouseIcons;

    [Header("Varsayýlanlar")]
    [SerializeField] private InputIconData defaultGamepadData;
    [SerializeField] private InputIconData defaultMouseData;
    [ColorUsage(true, true)][SerializeField] private Color keyboardGlowColor = Color.white * 2f;

    private InputAction _targetAction;

    private void Start()
    {
        if (InputManager.Instance != null)
            _targetAction = InputManager.Instance.GetAction(actionReference.action.name);
        else
            _targetAction = actionReference.action;

        UpdateVisuals();
    }

    private void OnEnable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnBindingsReset += UpdateVisuals;
            InputManager.Instance.OnInputDeviceChanged += OnDeviceChanged;
        }

        Settings.OnPromptsChanged += UpdateVisuals;
        UpdateVisuals();
    }

    private void OnDisable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnBindingsReset -= UpdateVisuals;
            InputManager.Instance.OnInputDeviceChanged -= OnDeviceChanged;
        }

        Settings.OnPromptsChanged -= UpdateVisuals;
    }

    private void OnDeviceChanged(bool isMouse)
    {
        UpdateVisuals();
    }

    // --- BURASI DEÐÝÞTÝ ---
    public void UpdateVisuals()
    {
        if (_targetAction == null) return;

        // 1. Þu an hangi cihazý kullanýyoruz?
        bool useGamepad = false;
        if (InputManager.Instance != null)
            useGamepad = !InputManager.Instance.IsUsingMouse();

        int foundBindingIndex = -1;

        // 2. Bindingleri TEK TEK gez ve uygun olaný bul (Manuel Arama)
        for (int i = 0; i < _targetAction.bindings.Count; i++)
        {
            InputBinding b = _targetAction.bindings[i];

            // Binding'in adresini al (Override varsa onu, yoksa orijinalini)
            string path = !string.IsNullOrEmpty(b.overridePath) ? b.overridePath : b.path;

            // Composite (WASD gibi paketler) baþlýðýný atla, içindeki tuþlara bak
            if (b.isComposite) continue;

            if (useGamepad)
            {
                // Gamepad modundayýz, içinde <Gamepad> geçen ilk tuþu kap
                if (path.Contains("<Gamepad>") || path.Contains("<Joystick>"))
                {
                    foundBindingIndex = i;
                    break; // Bulduk, döngüden çýk
                }
            }
            else
            {
                // Klavye modundayýz, içinde <Keyboard> veya <Mouse> geçeni kap
                if (path.Contains("<Keyboard>") || path.Contains("<Mouse>"))
                {
                    foundBindingIndex = i;
                    break; // Bulduk, döngüden çýk
                }
            }
        }

        // Eðer uygun binding bulamadýysak (örn: Sadece klavye atamasý var ama gamepaddeyiz),
        // mecburen 0. indexi göster ki boþ kalmasýn.
        if (foundBindingIndex == -1) foundBindingIndex = 0;

        // Bulduðumuz bindingin path'ini tekrar alalým
        InputBinding finalBinding = _targetAction.bindings[foundBindingIndex];
        string finalPath = !string.IsNullOrEmpty(finalBinding.overridePath) ? finalBinding.overridePath : finalBinding.path;

        // 3. Görseli Güncelle
        if (finalPath.Contains("<Gamepad>"))
        {
            // --- GAMEPAD ---
            string controlName = finalPath.Replace("<Gamepad>/", "").Trim();
            InputIconData data = GetGamepadData(controlName);

            iconImage.sprite = data.icon;

            if (glowController)
                glowController.SetVisualData(true, data.emissionMap != null ? data.emissionMap.texture : null, data.glowColor);
        }
        else if (finalPath.Contains("<Mouse>"))
        {
            // --- MOUSE ---
            string controlName = finalPath.Replace("<Mouse>/", "").Trim();
            InputIconData data = GetMouseData(controlName);

            iconImage.sprite = data.icon;

            if (glowController)
                glowController.SetVisualData(true, data.emissionMap != null ? data.emissionMap.texture : null, data.glowColor);
        }
        else
        {
            // --- KLAVYE ---
            // Display String klavye tuþunun okunabilir ismini verir (E, Space, Enter vb.)
            string displayString = _targetAction.GetBindingDisplayString(foundBindingIndex, InputBinding.DisplayStringOptions.DontUseShortDisplayNames);
            keyText.text = displayString;

            if (glowController)
                glowController.SetVisualData(false, null, keyboardGlowColor);
        }
    }

    // --- ARAMA FONKSÝYONLARI ---
    private InputIconData GetGamepadData(string unityControlName)
    {
        bool isXbox = Settings.IsXboxPrompts;
        string prefix = isXbox ? "xb_" : "ps_";
        string suffix = "";

        switch (unityControlName)
        {
            case "buttonSouth": suffix = isXbox ? "a" : "cross"; break;
            case "buttonEast": suffix = isXbox ? "b" : "circle"; break;
            case "buttonWest": suffix = isXbox ? "x" : "square"; break;
            case "buttonNorth": suffix = isXbox ? "y" : "triangle"; break;
            case "leftShoulder": suffix = isXbox ? "lb" : "l1"; break;
            case "rightShoulder": suffix = isXbox ? "rb" : "r1"; break;
            case "leftTrigger": suffix = isXbox ? "lt" : "l2"; break;
            case "rightTrigger": suffix = isXbox ? "rt" : "r2"; break;
            case "leftStickPress": suffix = isXbox ? "ls" : "l3"; break;
            case "rightStickPress": suffix = isXbox ? "rs" : "r3"; break;
            case "start": suffix = "start"; break;
            case "select": suffix = "select"; break;
            case "dpad/up": suffix = "dpad_up"; break;
            case "dpad/down": suffix = "dpad_down"; break;
            case "dpad/left": suffix = "dpad_left"; break;
            case "dpad/right": suffix = "dpad_right"; break;
            case "dpad": suffix = "dpad_up"; break;
            case "leftStick": suffix = "stick_l"; break;
            case "rightStick": suffix = "stick_r"; break;
            default: return defaultGamepadData;
        }

        string finalName = prefix + suffix;
        foreach (var data in gamepadIcons) { if (data.controlName == finalName) return data; }
        return defaultGamepadData;
    }

    private InputIconData GetMouseData(string controlName)
    {
        foreach (var data in mouseIcons) { if (data.controlName == controlName) return data; }
        return defaultMouseData;
    }
}