using UnityEngine;
// YENÝ INPUT SÝSTEMÝNÝ KULLANMAK ÝÇÝN BU GEREKLÝ:
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    // Singleton (Her yerden eriþmek için)
    public static InputManager Instance;

    // Unity'nin oluþturduðu o C# sýnýfý
    private GameControls _gameControls;

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

        // Kontrol sýnýfýný baþlat
        _gameControls = new GameControls();
    }

    private void OnEnable()
    {
        // Oyuna girince kontrolleri dinlemeye baþla
        _gameControls.Enable();
    }

    private void OnDisable()
    {
        // Oyundan çýkýnca veya script kapanýnca dinlemeyi býrak
        _gameControls.Disable();
    }

    // --- OYUNCU ÝÇÝN TERCÜMELER ---

    // Hareket (WASD) - Bize Vector2 (x,y) verir
    public Vector2 GetMovementInput()
    {
        return _gameControls.Player.Movement.ReadValue<Vector2>();
    }

    // Bakýþ (Mouse) - Bize Vector2 (x,y) verir
    public Vector2 GetLookInput()
    {
        return _gameControls.Player.Look.ReadValue<Vector2>();
    }

    // Zýplama - Basýldýðý AN (ThisFrame) true döner
    public bool PlayerJump()
    {
        return _gameControls.Player.Jump.triggered;
    }

    // Koþma - Basýlý tutulduðu sürece true döner
    public bool PlayerSprint()
    {
        // Phase.Performed, tuþa basýlý tutuluyor demektir
        return _gameControls.Player.Sprint.phase == InputActionPhase.Performed;
    }

    // Eðilme - Basýlý tutulduðu sürece
    public bool PlayerCrouch()
    {
        return _gameControls.Player.Crouch.phase == InputActionPhase.Performed;
    }

    // Etkileþim (Sol Týk) - Basýldýðý AN
    public bool PlayerInteract()
    {
        return _gameControls.Player.Interact.triggered;
    }

    // Basýlý Tutma Durumu (Interaction Charge için lazým olabilir)
    public bool PlayerInteractHold()
    {
        return _gameControls.Player.Interact.phase == InputActionPhase.Performed;
    }

    public bool PlayerInteractRelease()
    {
        return _gameControls.Player.Interact.WasReleasedThisFrame();
    }

    // Fýrlatma (Sað Týk)
    public bool PlayerThrow()
    {
        return _gameControls.Player.Throw.triggered;
    }

    public bool PlayerThrowHold()
    {
        return _gameControls.Player.Throw.phase == InputActionPhase.Performed;
    }

    public bool PlayerThrowRelease()
    {
        return _gameControls.Player.Throw.WasReleasedThisFrame();
    }

    // Telefon (Tab)
    public bool PlayerPhone()
    {
        return _gameControls.Player.Phone.triggered;
    }

    public bool PlayerPause()
    {
        return _gameControls.Player.Pause.triggered;
    }
}