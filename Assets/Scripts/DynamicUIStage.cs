using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DynamicUIStage : MonoBehaviour
{
    [SerializeField] RectTransform canvasTransform;
    [SerializeField] GraphicRaycaster raycaster;

    private GameObject currentPressedButton;
    public bool IsActive; // Public property to enable or disable the functionality

    public void OnCursorInput(Vector2 inNormalizedPosition)
    {
        if (!IsActive) return; // Exit early if the script is disabled

        // Get the input position in canvas space
        Vector3 inputPosition = new Vector3(
            canvasTransform.sizeDelta.x * inNormalizedPosition.x,
            canvasTransform.sizeDelta.y * inNormalizedPosition.y,
            0);

        // Build a pointer event
        PointerEventData pointerEvent = new PointerEventData(EventSystem.current)
        {
            position = inputPosition
        };

        // Determine what we've hit in the UI
        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(pointerEvent, results);

#if ENABLE_LEGACY_INPUT_MANAGER
        bool bMouseDownThisFrame = Input.GetMouseButtonDown(0);
        bool bMouseUpThisFrame = Input.GetMouseButtonUp(0);
#else
        bool bMouseDownThisFrame = UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame;
        bool bMouseUpThisFrame = UnityEngine.InputSystem.Mouse.current.leftButton.wasReleasedThisFrame;
#endif

        // Process pointer down
        if (bMouseDownThisFrame)
        {
            foreach (var result in results)
            {
                if (result.gameObject.GetComponent<Button>() != null)
                {
                    currentPressedButton = result.gameObject;
                    ExecuteEvents.Execute(currentPressedButton, pointerEvent, ExecuteEvents.pointerDownHandler);
                    break;
                }
            }
        }

        // Process pointer up
        if (bMouseUpThisFrame && currentPressedButton != null)
        {
            ExecuteEvents.Execute(currentPressedButton, pointerEvent, ExecuteEvents.pointerUpHandler);

            // Trigger pointer click only if pointer is still over the same button
            if (results.Exists(r => r.gameObject == currentPressedButton))
            {
                ExecuteEvents.Execute(currentPressedButton, pointerEvent, ExecuteEvents.pointerClickHandler);
            }

            currentPressedButton = null; // Clear the stored button
        }
    }
}
