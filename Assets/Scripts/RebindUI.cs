using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;
using System.Collections; // Coroutine için gerekli

public class RebindUI : MonoBehaviour
{
    [Header("Hangi Aksiyon?")]
    [SerializeField] private InputActionReference inputActionReference;

    [Header("Ayarlar")]
    [SerializeField] private bool excludeGamepad = false; // True ise: Klavye + Mouse kabul eder, Gamepad'i dýþlar.
    [SerializeField] private int selectedBindingIndex = 0;

    [Header("UI Bileþenleri")]
    [SerializeField] private TMP_Text actionNameText;
    [SerializeField] private TMP_Text bindingText;
    [SerializeField] private GameObject helperText;
    [SerializeField] private GameObject keyText;
    [SerializeField] private Button myButton;

    private InputActionRebindingExtensions.RebindingOperation _rebindingOperation;
    private InputAction _targetAction;

    private static bool isAnyRebindingInProgress = false;

    private void Start()
    {
        if (myButton == null) myButton = GetComponent<Button>();

        string actionId = inputActionReference.action.id.ToString();

        if (InputManager.Instance != null)
        {
            _targetAction = InputManager.Instance.GetAction(actionId);
        }
        else
        {
            _targetAction = inputActionReference.action;
        }

        UpdateUI();
    }

    private void OnDisable()
    {
        StopRebindingLogic();
    }

    // Butona basýnca artýk Coroutine baþlatýyoruz (Bekleme için)
    public void StartRebinding()
    {
        StartCoroutine(StartRebindingRoutine());
    }

    private IEnumerator StartRebindingRoutine()
    {
        if (isAnyRebindingInProgress) yield break;
        if (_targetAction == null) { Debug.LogError("HATA: Hedef Aksiyon Yok!"); yield break; }

        isAnyRebindingInProgress = true;
        if (myButton != null) myButton.interactable = false;

        // --- KRÝTÝK NOKTA 1: GECÝKME ---
        // Kullanýcý butona týkladý, parmaðýný çekmesi için 0.2sn veriyoruz.
        // Böylece "Sol Týk" anýnda algýlanýp menü kapanmýyor.
        yield return new WaitForSecondsRealtime(0.1f);

        _targetAction.Disable();

        keyText.SetActive(false);
        helperText.SetActive(true);

        // Operasyonu Hazýrla
        var rebindOperation = _targetAction.PerformInteractiveRebinding(selectedBindingIndex);

        // --- KRÝTÝK NOKTA 2: FÝLTRELEME MANTIÐI ---

        if (excludeGamepad)
        {
            // SENARYO: KLAVYE & MOUSE MENÜSÜ
            // Gamepad'i yasakla
            rebindOperation.WithControlsExcluding("<Gamepad>");

            // Mouse'un kendisini deðil, HAREKETÝNÝ yasakla (Delta ve Position)
            // Böylece týklamalar serbest, ama fareyi kaydýrmak tuþ atamasý yapmaz.
            rebindOperation.WithControlsExcluding("<Mouse>/position");
            rebindOperation.WithControlsExcluding("<Mouse>/delta");
        }
        else
        {
            // SENARYO: GAMEPAD MENÜSÜ
            // Sadece Gamepad'i kabul et
            rebindOperation.WithControlsHavingToMatchPath("<Gamepad>");
        }

        // Ortak Ayarlar
        rebindOperation.WithControlsExcluding("<Pointer>/position"); // Dokunmatik ekran vs için garanti olsun
        rebindOperation.WithCancelingThrough("<Keyboard>/escape");
        rebindOperation.OnComplete(operation => RebindCompleted());
        rebindOperation.OnCancel(operation => RebindCompleted());

        // Baþlat
        _rebindingOperation = rebindOperation.Start();
    }

    private void RebindCompleted()
    {
        StopRebindingLogic();

        if (InputManager.Instance != null)
        {
            InputManager.Instance.SaveBindingOverrides();
        }
    }

    private void StopRebindingLogic()
    {
        if (_rebindingOperation != null)
        {
            _rebindingOperation.Dispose();
            _rebindingOperation = null;
        }

        if (_targetAction != null) _targetAction.Enable();

        if (helperText != null) helperText.SetActive(false);
        if (keyText != null) keyText.SetActive(true);

        // --- DEÐÝÞÝKLÝK BURADA: Butonu hemen açma! ---
        // myButton.interactable = true; // ESKÝSÝ BUNU SÝL

        // YENÝSÝ: Butonu açmak için biraz bekle
        StartCoroutine(EnableButtonAfterDelay());

        isAnyRebindingInProgress = false;

        UpdateUI();
    }

    private IEnumerator EnableButtonAfterDelay()
    {
        // 0.2 saniye bekle ki "Sol Týk" olayý sönümlensin
        yield return new WaitForSecondsRealtime(0.1f);

        if (myButton != null)
            myButton.interactable = true;
    }

    public void UpdateUI()
    {
        if (_targetAction != null)
        {
            string keyName = _targetAction.GetBindingDisplayString(selectedBindingIndex, InputBinding.DisplayStringOptions.DontUseShortDisplayNames);

            if (string.IsNullOrEmpty(keyName)) keyName = "NONE";
            if (bindingText != null) bindingText.text = keyName.ToUpper();
        }
    }
}