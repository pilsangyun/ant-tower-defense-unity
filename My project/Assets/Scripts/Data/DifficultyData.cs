using System;
using UnityEngine;

namespace AntTD.Data
{
    public enum DifficultyType
    {
        Easy,       // 정찰병 (입문)
        Normal,     // 병정 (표준)
        Hard,       // 근위대 (도전)
        Nightmare,  // 악몽 (하드코어)
        Infinite    // 대재앙 (무한 웨이브)
    }

    [CreateAssetMenu(fileName = "NewDifficultyData", menuName = "AntTD/Difficulty Data")]
    public class DifficultyData : ScriptableObject
    {
        public DifficultyType diffType = DifficultyType.Normal;
        public string diffId = "normal";
        public string displayName = "병정 (보통)";
        [TextArea] public string description = "균형 잡힌 표준 전장 환경입니다.";

        [Header("Stat Multipliers")]
        public float hpMultiplier = 1.25f;
        public float speedMultiplier = 1.05f;
        public float countMultiplier = 1.10f;
        public float rewardMultiplier = 1.20f;
        public float bossArmorBonus = 0f;

        [Header("Rules & Economy")]
        public int startSugar = 160;
        public int startQueenHp = 20;
        public float waveCountdownSeconds = 10f;
        [Range(0f, 1f)] public float sellRefundRate = 0.75f; // 철거 환급률
        public int bonusDnaReward = 5;

        public static DifficultyData CreateDefault(DifficultyType type)
        {
            var data = CreateInstance<DifficultyData>();
            data.diffType = type;

            switch (type)
            {
                case DifficultyType.Easy:
                    data.diffId = "easy";
                    data.displayName = "정찰병 (쉬움)";
                    data.description = "완만한 적 체력과 넉넉한 준비 시간. 입문자를 위한 난이도.";
                    data.hpMultiplier = 0.75f;
                    data.speedMultiplier = 0.90f;
                    data.countMultiplier = 1.0f;
                    data.rewardMultiplier = 1.0f;
                    data.bossArmorBonus = 0f;
                    data.startSugar = 220;
                    data.startQueenHp = 30;
                    data.waveCountdownSeconds = 12f;
                    data.sellRefundRate = 0.90f;
                    data.bonusDnaReward = 3;
                    break;

                case DifficultyType.Normal:
                    data.diffId = "normal";
                    data.displayName = "병정 (보통)";
                    data.description = "표준 밸런스의 정규 전장. 적절한 전술적 배치가 요구됩니다.";
                    data.hpMultiplier = 1.0f;
                    data.speedMultiplier = 1.0f;
                    data.countMultiplier = 1.10f;
                    data.rewardMultiplier = 1.20f;
                    data.bossArmorBonus = 0f;
                    data.startSugar = 160;
                    data.startQueenHp = 20;
                    data.waveCountdownSeconds = 10f;
                    data.sellRefundRate = 0.70f;
                    data.bonusDnaReward = 5;
                    break;

                case DifficultyType.Hard:
                    data.diffId = "hard";
                    data.displayName = "근위대 (어려움)";
                    data.description = "빠르고 단단한 적군. 보스 방어력 +15% 및 촉박한 정비 시간.";
                    data.hpMultiplier = 1.40f;
                    data.speedMultiplier = 1.12f;
                    data.countMultiplier = 1.25f;
                    data.rewardMultiplier = 1.50f;
                    data.bossArmorBonus = 0.15f;
                    data.startSugar = 130;
                    data.startQueenHp = 15;
                    data.waveCountdownSeconds = 8f;
                    data.sellRefundRate = 0.60f;
                    data.bonusDnaReward = 8;
                    break;

                case DifficultyType.Nightmare:
                    data.diffId = "nightmare";
                    data.displayName = "악몽 (매우 어려움)";
                    data.description = "극도로 강화된 적 떼. 웨이브 타이머 6초, 환급률 50%의 한계 전투.";
                    data.hpMultiplier = 2.00f;
                    data.speedMultiplier = 1.20f;
                    data.countMultiplier = 1.40f;
                    data.rewardMultiplier = 2.00f;
                    data.bossArmorBonus = 0.25f;
                    data.startSugar = 120;
                    data.startQueenHp = 10;
                    data.waveCountdownSeconds = 6f;
                    data.sellRefundRate = 0.50f;
                    data.bonusDnaReward = 15;
                    break;

                case DifficultyType.Infinite:
                    data.diffId = "infinite";
                    data.displayName = "대재앙 (무한 웨이브)";
                    data.description = "끝없는 적들의 대침공. 웨이브마다 적 스탯이 1.25배씩 지수 스케일링됩니다.";
                    data.hpMultiplier = 1.00f;
                    data.speedMultiplier = 1.00f;
                    data.countMultiplier = 1.20f;
                    data.rewardMultiplier = 1.50f;
                    data.bossArmorBonus = 0.20f;
                    data.startSugar = 200;
                    data.startQueenHp = 10;
                    data.waveCountdownSeconds = 8f;
                    data.sellRefundRate = 0.70f;
                    data.bonusDnaReward = 20;
                    break;
            }

            return data;
        }
    }
}
