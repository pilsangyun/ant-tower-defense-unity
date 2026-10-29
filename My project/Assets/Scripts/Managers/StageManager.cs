using System;
using System.Collections.Generic;
using UnityEngine;
using AntTD.Data;
using AntTD.Entities;

namespace AntTD.Managers
{
    public class StageManager : MonoBehaviour
    {
        public static StageManager Instance { get; private set; }

        [Header("Containers & Visuals")]
        public Transform slotsContainer;
        public SpriteRenderer backgroundRenderer;
        public Sprite ringSprite;

        [Header("Stage Configs")]
        public List<StageConfigData> stageConfigs = new List<StageConfigData>();
        public StageConfigData activeStageConfig { get; private set; }

        private int spawnIndexCounter = 0;
        private List<TowerSlot> activeSlots = new List<TowerSlot>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void LoadStage(StageConfigData stageData)
        {
            if (stageData == null) return;
            activeStageConfig = stageData;
            spawnIndexCounter = 0;

            // 1. Update Background
            if (backgroundRenderer != null && stageData.backgroundSprite != null)
            {
                backgroundRenderer.sprite = stageData.backgroundSprite;
            }

            // 2. Clear Existing Slots
            ClearExistingSlots();

            // 3. Create Slots for the active stage
            if (slotsContainer == null)
            {
                var containerObj = GameObject.Find("SlotsContainer");
                if (containerObj != null) slotsContainer = containerObj.transform;
                else slotsContainer = new GameObject("SlotsContainer").transform;
            }

            foreach (var slotCoord in stageData.slots)
            {
                GameObject slotObj = new GameObject($"Slot_{slotCoord.id}");
                slotObj.transform.SetParent(slotsContainer);
                slotObj.transform.position = slotCoord.ToWorldPosition();

                TowerSlot slot = slotObj.AddComponent<TowerSlot>();
                slot.slotId = slotCoord.id;

                CircleCollider2D col = slotObj.AddComponent<CircleCollider2D>();
                col.radius = 0.45f;

                // Child Selection Ring
                GameObject ringObj = new GameObject("RingHighlight");
                ringObj.transform.SetParent(slotObj.transform);
                ringObj.transform.localPosition = Vector3.zero;
                ringObj.transform.localScale = new Vector3(0.42f, 0.42f, 1f);

                SpriteRenderer ringSr = ringObj.AddComponent<SpriteRenderer>();
                if (ringSprite != null) ringSr.sprite = ringSprite;
                ringSr.sortingOrder = -5;
                ringSr.enabled = false;
                slot.ringHighlightRenderer = ringSr;

                activeSlots.Add(slot);
            }

            // 4. Inject Waypoints to GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.waypoints = stageData.GetWorldWaypoints();
            }

            Debug.Log($"🗺️ [StageManager] Stage loaded: {stageData.stageName} with {activeSlots.Count} tower slots.");
        }

        private void ClearExistingSlots()
        {
            if (slotsContainer != null)
            {
                for (int i = slotsContainer.childCount - 1; i >= 0; i--)
                {
                    Destroy(slotsContainer.GetChild(i).gameObject);
                }
            }
            activeSlots.Clear();
        }

        public List<Vector2> GetSpawnPathForNextEnemy()
        {
            if (activeStageConfig == null)
            {
                return GameManager.Instance != null ? GameManager.Instance.waypoints : new List<Vector2>();
            }

            // Stage 2 Dual Lanes Logic
            if (activeStageConfig.stageId == "split" && activeStageConfig.laneA.Count > 0 && activeStageConfig.laneB.Count > 0)
            {
                bool useLaneA = (spawnIndexCounter % 2 == 0);
                spawnIndexCounter++;

                List<Vector2> path = new List<Vector2>();
                var lane = useLaneA ? activeStageConfig.laneA : activeStageConfig.laneB;

                foreach (var wp in lane)
                {
                    path.Add(new Vector2((wp.x - 270f) / 100f, (430f - wp.y) / 100f));
                }
                foreach (var wp in activeStageConfig.laneFinal)
                {
                    path.Add(new Vector2((wp.x - 270f) / 100f, (430f - wp.y) / 100f));
                }
                return path;
            }

            return activeStageConfig.GetWorldWaypoints();
        }
    }
}
