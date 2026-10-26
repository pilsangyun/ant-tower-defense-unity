using UnityEngine;
using UnityEngine.UI;
using AntTD.Data;
using AntTD.Managers;

namespace AntTD.UI
{
    public class AntCardUI : MonoBehaviour
    {
        public TowerData antData;
        public Image cardBg;
        public Image iconImg;
        public Text nameText;
        public Text costText;
        public Button button;

        public Sprite normalCardSprite;
        public Sprite activeCardSprite;

        private void Start()
        {
            if (button == null) button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(OnClick);
            }

            if (PlacementManager.Instance != null)
            {
                PlacementManager.Instance.OnAntSelectionChanged += OnSelectionChanged;
            }

            UpdateVisuals();
        }

        private void OnDestroy()
        {
            if (PlacementManager.Instance != null)
            {
                PlacementManager.Instance.OnAntSelectionChanged -= OnSelectionChanged;
            }
        }

        private void Update()
        {
            UpdateAffordability();
        }

        public void Setup(TowerData data, Sprite normalSp, Sprite activeSp)
        {
            antData = data;
            normalCardSprite = normalSp;
            activeCardSprite = activeSp;
            UpdateVisuals();
        }

        private void OnClick()
        {
            if (PlacementManager.Instance != null && antData != null)
            {
                PlacementManager.Instance.SelectAntToBuild(antData);
            }
        }

        private void OnSelectionChanged(TowerData selected)
        {
            bool isSelected = (selected != null && antData != null && selected.towerId == antData.towerId);
            if (cardBg != null)
            {
                cardBg.sprite = isSelected ? activeCardSprite : normalCardSprite;
            }
            transform.localScale = isSelected ? new Vector3(1.05f, 1.05f, 1f) : Vector3.one;
        }

        private void UpdateAffordability()
        {
            if (GameManager.Instance == null || antData == null) return;
            bool canAfford = GameManager.Instance.sugar >= antData.baseCost;
            if (costText != null)
            {
                costText.color = canAfford ? new Color(0.96f, 0.45f, 0.71f) : new Color(0.65f, 0.35f, 0.35f);
            }
        }

        private void UpdateVisuals()
        {
            if (antData == null) return;
            if (nameText != null) nameText.text = antData.antName;
            if (costText != null) costText.text = antData.baseCost.ToString();
            if (iconImg != null && antData.icon != null) iconImg.sprite = antData.icon;
            OnSelectionChanged(PlacementManager.Instance != null ? PlacementManager.Instance.selectedAntData : null);
        }
    }
}
