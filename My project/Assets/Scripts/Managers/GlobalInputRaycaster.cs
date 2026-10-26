using UnityEngine;
using AntTD.Entities;
using AntTD.Managers;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

namespace AntTD.Managers
{
    public class GlobalInputRaycaster : MonoBehaviour
    {
        private Camera mainCamera;

        private void Start()
        {
            mainCamera = Camera.main;
        }

        private void Update()
        {
            bool isPressed = false;
            Vector2 screenPos = Vector2.zero;

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                isPressed = true;
                screenPos = Mouse.current.position.ReadValue();
            }
            else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                isPressed = true;
                screenPos = Touchscreen.current.primaryTouch.position.ReadValue();
            }

            if (!isPressed) return;

            // If user clicked over UI button/card, let UI handle it!
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            if (mainCamera == null) mainCamera = Camera.main;
            if (mainCamera == null) return;

            Vector3 worldPos3D = mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -mainCamera.transform.position.z));
            Vector2 worldPos2D = new Vector2(worldPos3D.x, worldPos3D.y);

            // Check exact point hit
            Collider2D hit = Physics2D.OverlapPoint(worldPos2D);
            if (hit != null)
            {
                TowerSlot slot = hit.GetComponent<TowerSlot>();
                if (slot != null)
                {
                    slot.TriggerSlotClick();
                    return;
                }
            }

            // Generous Circle check around cursor (0.45 unit radius)
            Collider2D[] circleHits = Physics2D.OverlapCircleAll(worldPos2D, 0.45f);
            foreach (var ch in circleHits)
            {
                TowerSlot slot = ch.GetComponent<TowerSlot>();
                if (slot != null)
                {
                    slot.TriggerSlotClick();
                    return;
                }
            }
        }
    }
}
