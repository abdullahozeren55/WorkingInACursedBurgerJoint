using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DynamicUIDisplayScreen : MonoBehaviour
{
    [SerializeField] LayerMask raycastMask = ~0;
    [SerializeField] float raycastDistance = 5.0f;

    public bool IsActive; // Public property to enable or disable the functionality

    public UnityEvent<Vector2> onCursorInput = new UnityEvent<Vector2>();

    private void Update()
    {

        if (!IsActive) return; // Exit early if the script is disabled

#if ENABLE_LEGACY_INPUT_MANAGER
        Vector3 mousePosition = Input.mousePosition;
#else
        Vector3 mousePosition = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
#endif // ENABLE_LEGACY_INPUT_MANAGER

        // construct our ray from the mouse position
        Ray mouseRay = Camera.main.ScreenPointToRay(mousePosition);

        // perform our raycast
        RaycastHit hitResult;
        if (Physics.Raycast(mouseRay, out hitResult, raycastDistance, raycastMask, QueryTriggerInteraction.Ignore))
        {
            // ignore if not us
            if (hitResult.collider.gameObject != gameObject)
                return;

            onCursorInput.Invoke(hitResult.textureCoord);
        }
    }
}
