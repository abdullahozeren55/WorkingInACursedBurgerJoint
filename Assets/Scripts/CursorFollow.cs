using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CursorFollow : MonoBehaviour
{
    [SerializeField] private Image cursorImage; // Reference to the cursor image.
    [SerializeField] private RectTransform canvasTransform; // Reference to the canvas transform where the cursor will move.
    [SerializeField] private DynamicUIDisplayScreen dynamicUIDisplayScreen;
    [SerializeField] private DynamicUIStage dynamicUIStage;
    [HideInInspector] public bool shouldBuy;

    private void Awake()
    {
        dynamicUIStage.IsActive = false;
        dynamicUIDisplayScreen.IsActive = false;
        shouldBuy = false;
    }
    public void StartCursorFollow()
    {
        dynamicUIStage.IsActive = true;
        dynamicUIDisplayScreen.IsActive = true;
        shouldBuy = true;

        // Unlock the cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;

        // Get mouse position
        Vector2 mousePosition;
#if ENABLE_LEGACY_INPUT_MANAGER
        mousePosition = Input.mousePosition;
#else
    mousePosition = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
#endif

        // Get the event camera (Capture Camera)
        Canvas canvas = canvasTransform.GetComponentInParent<Canvas>();
        Camera eventCamera = canvas.worldCamera;

        // Convert mouse position to world space
        Ray ray = eventCamera.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 worldPosition = hit.point;

            // Convert world position to local position relative to canvas
            Vector3 localPosition = canvasTransform.InverseTransformPoint(worldPosition);

            // Adjust for canvas scale
            Vector3 canvasScale = canvasTransform.localScale;
            Vector2 scaledPosition = new Vector2(
                localPosition.x / canvasScale.x,
                localPosition.y / canvasScale.y
            );

            // Set the cursor position
            cursorImage.rectTransform.anchoredPosition = scaledPosition;
            
        }

        if (dynamicUIDisplayScreen != null)
        {
            dynamicUIDisplayScreen.onCursorInput.AddListener(UpdateCursor);
        }
    }





    public void EndCursorFollow()
    {
        dynamicUIStage.IsActive = false;
        dynamicUIDisplayScreen.IsActive = false;
        shouldBuy = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        dynamicUIDisplayScreen.onCursorInput.RemoveListener(UpdateCursor);
    }

    private void UpdateCursor(Vector2 normalizedPosition)
    {

        // Convert normalized position (0-1 range) to canvas local space
        Vector3 cursorPosition = new Vector3(
            normalizedPosition.x * canvasTransform.sizeDelta.x,
            normalizedPosition.y * canvasTransform.sizeDelta.y,
            0);

        // Adjust for canvas anchor point (local space assumes center anchor at (0,0))
        cursorPosition.x -= canvasTransform.sizeDelta.x * 0.5f;
        cursorPosition.y -= canvasTransform.sizeDelta.y * 0.5f;

        // Update cursor image position
        cursorImage.rectTransform.anchoredPosition = cursorPosition;
    }
}
