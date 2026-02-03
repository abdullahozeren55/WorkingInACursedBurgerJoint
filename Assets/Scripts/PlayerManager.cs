using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;
    [Space]
    public Animator playerAnim;

    [Header("UI Settings")]
    public GameObject crosshairGO;

    [SerializeField] private SkinnedMeshRenderer headRenderer; // Inspector'dan kafayý ata

    public enum HandRigTypes
    {
        Interaction,
        SingleHandGrab,
        Nothing,
        HoldingTray
    }

    public enum HandGrabTypes
    {
        RegularGrab,
        BottleGrab,
        TrashGrab,
        KnifeGrab,
        ThinBurgerIngredientGrab,
        RegularBurgerIngredientGrab,
        ThickBurgerIngredientGrab,
        NoodleGrab,
        KettleGrab,
        WholeIngredientGrab,
        BigWholeIngredientGrab,
        WholeBunGrab,
        WholeBurgerGrab,
        SauceCapsuleGrab,
    }

    private FirstPersonController firstPersonController;
    private CharacterController characterController;

    private void Awake()
    {
        if (Instance == null)
        {
            // If not, set this instance as the singleton
            Instance = this;
        }
        else
        {
            // If an instance already exists, destroy this one to enforce the singleton pattern
            Destroy(gameObject);
        }

        firstPersonController = FindFirstObjectByType<FirstPersonController>();
        characterController = FindFirstObjectByType<CharacterController>();
    }

    public IInteractable GetCurrentInteractable()
    {
        return firstPersonController.GetCurrentInteractable();
    }

    public IGrabable GetCurrentGrabable()
    {
        return firstPersonController?.GetCurrentGrabable();
    }

    public bool ShouldIGoWithOutlineWhenTurningBackToGrabable(IGrabable grabable)
    {
        return firstPersonController.ShouldIGoWithOutlineWhenTurningBackToGrabable(grabable);
    }

    public void ResetPlayerGrabAndInteract()
    {
        firstPersonController.ResetGrabAndInteract();
    }

    public void ResetPlayerGrab(IGrabable grabable)
    {
        firstPersonController.ResetGrab(grabable);
    }

    public void ResetPlayerInteract(IInteractable interactable, bool shouldBeUninteractable)
    {
        firstPersonController.ResetInteract(interactable, shouldBeUninteractable);
    }

    public void PlayerOnUseReleaseGrabable(bool shouldDecideOutlineAndCrosshair)
    {
        firstPersonController.OnUseReleaseGrabable(shouldDecideOutlineAndCrosshair);
    }

    public void SetPlayerBasicMovements(bool can)
    {
        firstPersonController.CanMove = can;
        firstPersonController.CanSprint = can;
        firstPersonController.CanJump = can;
        firstPersonController.CanCrouch = can;
        firstPersonController.CanInteract = can;
        firstPersonController.CanGrab = can;
        firstPersonController.CanLook = can;
        firstPersonController.CanFootstep = can;
        crosshairGO.SetActive(can);
    }

    public void SetPlayerFinalSceneMovements()
    {
        firstPersonController.CanMove = false;
        firstPersonController.CanSprint = false;
        firstPersonController.CanJump = false;
        firstPersonController.CanCrouch = false;
        firstPersonController.CanInteract = true;
        firstPersonController.CanGrab = true;
        firstPersonController.CanLook = true;
        firstPersonController.CanFootstep = false;
        crosshairGO.SetActive(true);
    }
    public void SetPlayerCanInteract(bool can)
    {
        firstPersonController.CanInteract = can;
    }

    public void SetPlayerCanGrab(bool can)
    {
        firstPersonController.CanGrab = can;
    }

    public void SetPlayerCanGrabAndInteract(bool can)
    {
        firstPersonController.CanGrab = can;
        firstPersonController.CanInteract = can;
    }

    public void SetInteractKeyIsDone(bool value)
    {
        firstPersonController.InteractKeyIsDone = value;
    }

    public void SetPlayerCanPlay(bool can)
    {
        firstPersonController.CanPlay = can;
    }

    public void SetPlayerCanHeadBob(bool can)
    {
        firstPersonController.CanUseHeadbob = can;
    }

    public void ForceUpdatePlayerGrab(IGrabable newGrabable)
    {
        // FirstPersonController referansýna (playerController vs.) ulaþ
        firstPersonController.ForceUpdateCurrentGrabableReference(newGrabable);
    }

    public void ForceUpdatePlayerSlotIcon(IGrabable targetGrabable, ItemIcon newIconData)
    {
        firstPersonController.ForceUpdateSlotIcon(targetGrabable, newIconData);
    }

    public void ChangePlayerCurrentGrabable(IGrabable objectToGrab)
    {
        firstPersonController.ChangeCurrentGrabable(objectToGrab);
    }

    public void MovePlayer(Vector3 moveForce)
    {
        characterController.Move(moveForce);
    }

    public void SetPlayerAnimBool(string boolName, bool value)
    {
        playerAnim.SetBool(boolName, value);
    }

    public void SetPlayerUseHandLerp(Vector3 targetPos, Vector3 targetRot, float timeToDo)
    {
        firstPersonController.SetUseHandLerp(targetPos, targetRot, timeToDo);
    }

    public void SetPlayerLeftUseHandLerp(Vector3 targetPos, Vector3 targetRot)
    {
        firstPersonController.SetLeftUseHandLerp(targetPos, targetRot);
    }

    public void PlayerResetLeftHandLerp()
    {
        firstPersonController.ResetLeftHandLerp();
    }

    public void PlayerStopUsingObject()
    {
        firstPersonController.StopUsingObject();
    }

    public void SetPlayerIsUsingItemXY(bool xValue, bool yValue)
    {
        firstPersonController.IsUsingItemX = xValue;
        firstPersonController.IsUsingItemY = yValue;
    }

    public void TryChangingFocusText(IInteractable interactable, string text)
    {
        firstPersonController.TryChangingFocusText(interactable, text);
    }

    public void TryChangingFocusText(IGrabable grabable, string text)
    {
        firstPersonController.TryChangingFocusText(grabable, text);
    }

    public Transform GetHeadTransform() => CameraManager.Instance.GetFirstPersonCamTransform();

    /// <summary>
    /// True ise kafa görünür (Menu modu), False ise kafa sadece gölge atar (FPS modu).
    /// </summary>
    public void SetHeadVisibility(bool isVisible)
    {
        if (headRenderer != null)
        {
            headRenderer.shadowCastingMode = isVisible
                ? ShadowCastingMode.On
                : ShadowCastingMode.ShadowsOnly;
        }
    }

    public void DecideUIText()
    {
        firstPersonController.DecideUIText();
    }

    public void UpdateGameplaySettings()
    {
        firstPersonController.RefreshUISettings();
    }

    public void SetPlayerIgnoreNextThrow(bool state)
    {
        // FPC referansýnýn adý genelde 'playerController' veya 'firstPersonController'dýr.
        // Kendi projendeki deðiþken ismine göre uyarla:
        if (firstPersonController != null)
        {
            firstPersonController.SetIgnoreNextThrowRelease(state);
        }
    }

    public bool IsPlayerHoldingItem()
    {
        if (firstPersonController != null)
        {
            return firstPersonController.IsHoldingItem();
        }
        return false;
    }

    public void CancelPlayerThrow()
    {
        if (firstPersonController != null)
        {
            firstPersonController.ForceCancelThrowAndResetHand();
        }
    }

    public void HandlePlayerEnterExitColdRoom(bool isEntering)
    {

        if (isEntering)
        {
            CameraManager.Instance.PlayColdRoomEffects(true);

            firstPersonController.CanBreathe = true;
        }
        else
        {
            CameraManager.Instance.PlayColdRoomEffects(false);

            firstPersonController.CanBreathe = false;
        }
            
    }

    /// <summary>
    /// Belirtilen collider listesinin Player ile çarpýþmasýný açar veya kapatýr.
    /// </summary>
    /// <param name="targetColliders">Etkilenecek colliderlar</param>
    /// <param name="shouldIgnore">TRUE ise çarpýþmaz (içinden geçer), FALSE ise çarpýþýr.</param>
    public void SetIgnoreCollisionWithPlayer(List<Collider> targetColliders, bool shouldIgnore)
    {
        if (characterController == null)
        {
            Debug.LogWarning("PlayerManager: PlayerController atanmamýþ! Çarpýþma yoksayýlamýyor.");
            return;
        }

        if (targetColliders == null) return;

        foreach (Collider col in targetColliders)
        {
            if (col != null)
            {
                // Unity'nin fizik motoruna bu iki collider'ýn birbirini görmezden gelmesini söylüyoruz.
                Physics.IgnoreCollision(characterController, col, shouldIgnore);
            }
        }
    }
}
