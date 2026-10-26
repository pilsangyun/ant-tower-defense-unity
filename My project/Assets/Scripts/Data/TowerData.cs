using System;
using UnityEngine;

namespace AntTD.Data
{
    public enum AntRole
    {
        ResourceSupport,
        MeleeBlock,
        RangedRapid,
        RangedCorrosion,
        SpecialNuke,
        DebuffSupport
    }

    [System.Serializable]
    public class TowerEvolutionBranch
    {
        public string branchKey; // "A" or "B"
        public string id;
        public string name;
        public string title;
        public int sugarCost = 180;
        public int proteinCost = 2;
        public float damage;
        public float range;
        public float attackSpeed;
        public int blockCount;
        public Sprite evolvedSprite;
        public string description;
    }

    [CreateAssetMenu(fileName = "NewTowerData", menuName = "AntTD/Tower Data")]
    public class TowerData : ScriptableObject
    {
        public string towerId; // "worker", "soldier", "fire", "acid", "exploder", "pheromone"
        public string antName;
        public AntRole role;
        public int baseCost;
        public int upgradeCost;
        public float baseRange;
        public float baseDamage;
        public float baseAttackSpeed;
        public int baseBlockCount;
        public float cooldownTime;
        public float respawnTime;
        public Sprite icon;
        [TextArea] public string description;

        [Header("Evolutions")]
        public TowerEvolutionBranch branchA;
        public TowerEvolutionBranch branchB;
    }
}
