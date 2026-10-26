using UnityEngine;

namespace AntTD.Data
{
    [CreateAssetMenu(fileName = "NewEnemyData", menuName = "AntTD/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        public string enemyId; // "aphid", "ladybug", "spider", "mole_cricket", "beetle_boss", "mantis_boss", "golden_scarab"
        public string enemyName;
        public float baseHp;
        public float baseSpeed;
        [Range(0f, 1f)] public float baseArmor; // 0.0 ~ 1.0 (damage reduction)
        public int sugarReward;
        public int proteinReward;
        public int dnaReward;
        public float radius = 0.5f;
        public Sprite sprite;
        public Color tintColor = Color.white;
        public bool isBoss;
        public bool isBonus;

        [Header("Special Mechanics")]
        public float webCooldown = 7.0f; // Spider
        public float webRange = 2.5f;
        public float webDuration = 2.2f;
        public float burrowEndProgress = 0.25f; // Mole cricket underground
    }
}
