using UnityEngine;
using AntTD.Data;
using AntTD.Managers;
using UnityEngine.EventSystems;

namespace AntTD.Entities
{
    public class TowerSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public int slotId;
        public Tower currentTower { get; private set; }
        public bool isOccupied => currentTower != null;

        [Header("Visuals")]
        public SpriteRenderer slotRenderer;
        public SpriteRenderer ringHighlightRenderer;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color hoverColor = new Color(0.4f, 1f, 0.4f, 0.95f);

        private void Awake()
        {
            if (slotRenderer == null) slotRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            // If an ant is selected to build and slot is empty, show glowing placement ring
            if (ringHighlightRenderer != null)
            {
                bool isPlacing = (PlacementManager.Instance != null && PlacementManager.Instance.selectedAntData != null && !isOccupied);
                ringHighlightRenderer.enabled = isPlacing;
                if (isPlacing)
                {
                    float pulse = 0.65f + Mathf.PingPong(Time.time * 2.5f, 0.35f);
                    ringHighlightRenderer.color = new Color(1f, 0.85f, 0.2f, pulse);
                }
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (ringHighlightRenderer != null && !isOccupied)
            {
                ringHighlightRenderer.enabled = true;
                ringHighlightRenderer.color = hoverColor;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // Reset state
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            TriggerSlotClick();
        }

        // Direct fallback click
        private void OnMouseDown()
        {
            TriggerSlotClick();
        }

        public void TriggerSlotClick()
        {
            if (PlacementManager.Instance != null)
            {
                PlacementManager.Instance.OnSlotClicked(this);
            }
        }

        public bool BuildTower(GameObject towerPrefab, TowerData data)
        {
            if (isOccupied) return false;

            GameObject obj = Instantiate(towerPrefab, transform.position, Quaternion.identity, transform);
            currentTower = obj.GetComponent<Tower>();
            currentTower.currentSlot = this;
            currentTower.Initialize(data);

            if (ringHighlightRenderer != null) ringHighlightRenderer.enabled = false;
            return true;
        }

        public void ClearTower()
        {
            if (currentTower != null)
            {
                Destroy(currentTower.gameObject);
                currentTower = null;
            }
            if (ringHighlightRenderer != null) ringHighlightRenderer.enabled = false;
        }
    }
}
