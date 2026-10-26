using UnityEngine;
using UnityEngine.UI;
using AntTD.Managers;
using AntTD.Data;
using AntTD.Entities;
using System.Collections.Generic;

namespace AntTD.UI
{
    [ExecuteAlways]
    public class RuntimeAppShellUI : MonoBehaviour
    {
        private CanvasScaler scaler;
        private RectTransform contentContainer;

        [Header("State")]
        public InGameCanvasController canvasController;

        private void Awake()
        {
            SetupResponsiveAppShell();
        }

        public void SetupResponsiveAppShell()
        {
            // Canvas Scaler: Fit Width (540)
            scaler = GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(540, 860);
                scaler.matchWidthOrHeight = 0.0f; // 0 = Match Width (Guarantees 540px exact fit, never shrinks or widens)
            }

            // Find or create ContentContainer
            Transform ccTransform = transform.Find("AppShellContent");
            if (ccTransform != null)
            {
                contentContainer = ccTransform.GetComponent<RectTransform>();
            }
        }
    }
}
