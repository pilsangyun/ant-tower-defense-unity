using UnityEngine;

namespace AntTD.UI
{
    [ExecuteAlways]
    public class LetterboxCamera : MonoBehaviour
    {
        public float targetAspectWidth = 540f;
        public float targetAspectHeight = 860f;

        private Camera cam;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            ApplyLetterbox();
        }

        private void Update()
        {
            ApplyLetterbox();
        }

        public void ApplyLetterbox()
        {
            if (cam == null) cam = GetComponent<Camera>();
            if (cam == null) return;

            float targetAspect = targetAspectWidth / targetAspectHeight;
            float currentAspect = (float)Screen.width / Screen.height;
            float scaleHeight = currentAspect / targetAspect;

            if (scaleHeight < 1.0f)
            {
                // Screen is taller than target (e.g. ultra tall phone) -> letterbox top & bottom
                Rect rect = cam.rect;
                rect.width = 1.0f;
                rect.height = scaleHeight;
                rect.x = 0;
                rect.y = (1.0f - scaleHeight) / 2.0f;
                cam.rect = rect;
            }
            else
            {
                // Screen is wider than target (e.g. 16:9 PC monitor) -> pillarbox left & right
                float scaleWidth = 1.0f / scaleHeight;
                Rect rect = cam.rect;
                rect.width = scaleWidth;
                rect.height = 1.0f;
                rect.x = (1.0f - scaleWidth) / 2.0f;
                rect.y = 0;
                cam.rect = rect;
            }
        }
    }
}
