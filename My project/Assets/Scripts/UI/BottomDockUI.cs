using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AntTD.Data;
using AntTD.Managers;

namespace AntTD.UI
{
    public class BottomDockUI : MonoBehaviour
    {
        public List<TowerData> availableAnts = new List<TowerData>();
        public GameObject cardPrefab;
        public Transform cardContainer;

        private List<GameObject> spawnedCards = new List<GameObject>();

        private void Start()
        {
            GenerateAntCards();
        }

        private void GenerateAntCards()
        {
            if (cardPrefab == null || cardContainer == null) return;

            foreach (var ant in availableAnts)
            {
                GameObject cardObj = Instantiate(cardPrefab, cardContainer);
                spawnedCards.Add(cardObj);

                Button btn = cardObj.GetComponent<Button>();
                Text nameText = cardObj.transform.Find("NameText")?.GetComponent<Text>();
                Text costText = cardObj.transform.Find("CostText")?.GetComponent<Text>();
                Image iconImg = cardObj.transform.Find("Icon")?.GetComponent<Image>();

                if (nameText != null) nameText.text = ant.antName;
                if (costText != null) costText.text = ant.baseCost.ToString();
                if (iconImg != null && ant.icon != null) iconImg.sprite = ant.icon;

                if (btn != null)
                {
                    btn.onClick.AddListener(() =>
                    {
                        PlacementManager.Instance.SelectAntToBuild(ant);
                    });
                }
            }
        }
    }
}
