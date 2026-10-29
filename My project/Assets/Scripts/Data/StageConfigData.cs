using System;
using System.Collections.Generic;
using UnityEngine;

namespace AntTD.Data
{
    [System.Serializable]
    public struct SlotCoordinate
    {
        public int id;
        public float canvasX;
        public float canvasY;
        public string label;
        public float rangeBonus;

        public Vector2 ToWorldPosition()
        {
            // Canvas (540 x 860, top-left origin) -> Unity World (Center 0,0)
            return new Vector2((canvasX - 270f) / 100f, (430f - canvasY) / 100f);
        }
    }

    [System.Serializable]
    public struct SugarCoordinate
    {
        public string id;
        public float canvasX;
        public float canvasY;
        public int hp;
        public int sugarReward;
        public int proteinReward;

        public Vector2 ToWorldPosition()
        {
            return new Vector2((canvasX - 270f) / 100f, (430f - canvasY) / 100f);
        }
    }

    [CreateAssetMenu(fileName = "NewStageConfig", menuName = "AntTD/Stage Config")]
    public class StageConfigData : ScriptableObject
    {
        public string stageId = "sunny";
        public string stageName = "햇살 정원";
        public string engName = "Sunny Garden";
        public string badge = "입문용";
        public string rating = "★☆☆";
        [TextArea] public string description = "5단 직각 S자 황토 벽돌 길과 울창한 숲속 정원.";

        public Sprite backgroundSprite;
        public Vector2 queenCanvasPos = new Vector2(268, 742);

        [Header("Slots & Sugars")]
        public List<SlotCoordinate> slots = new List<SlotCoordinate>();
        public List<SugarCoordinate> sugars = new List<SugarCoordinate>();

        [Header("Waypoints (Canvas Pixels)")]
        public List<Vector2> waypoints = new List<Vector2>();

        [Header("Stage 2 Dual Lanes (if applicable)")]
        public List<Vector2> laneA = new List<Vector2>();
        public List<Vector2> laneB = new List<Vector2>();
        public List<Vector2> laneFinal = new List<Vector2>();

        [Header("Stage 3 Sinkhole (if applicable)")]
        public bool hasSinkhole = false;
        public Vector2 sinkholeCanvasPos = new Vector2(270, 410);
        public float sinkholeRadius = 75f;
        public float sinkholeSlowRate = 0.40f;

        public List<Vector2> GetWorldWaypoints()
        {
            List<Vector2> result = new List<Vector2>();
            foreach (var wp in waypoints)
            {
                result.Add(new Vector2((wp.x - 270f) / 100f, (430f - wp.y) / 100f));
            }
            return result;
        }

        public Vector2 GetQueenWorldPos()
        {
            return new Vector2((queenCanvasPos.x - 270f) / 100f, (430f - queenCanvasPos.y) / 100f);
        }
    }
}
