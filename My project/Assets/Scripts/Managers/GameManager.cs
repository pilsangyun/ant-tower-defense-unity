using System;
using System.Collections.Generic;
using UnityEngine;
using AntTD.Data;
using AntTD.Entities;

namespace AntTD.Managers
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Stage & Difficulty Configuration")]
        public StageWaveData currentStageData;
        public StageConfigData currentStageConfig;
        public DifficultyData currentDifficulty;
        public List<Vector2> waypoints = new List<Vector2>();

        [Header("Economy & Queen")]
        public int sugar = 160;
        public int protein = 0;
        public int dnaRewardAccumulated = 0;
        public int queenHp = 20;
        public int maxQueenHp = 20;
        public int totalKills = 0;

        public float sellRefundRate => currentDifficulty != null ? currentDifficulty.sellRefundRate : 0.75f;

        public event Action OnResourcesChanged;
        public event Action<int> OnQueenHpChanged;
        public event Action OnGameOver;
        public event Action OnVictory;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void AddSugar(int amount)
        {
            sugar += amount;
            OnResourcesChanged?.Invoke();
        }

        public bool SpendSugar(int amount)
        {
            if (sugar >= amount)
            {
                sugar -= amount;
                OnResourcesChanged?.Invoke();
                return true;
            }
            return false;
        }

        public void AddProtein(int amount)
        {
            protein += amount;
            OnResourcesChanged?.Invoke();
        }

        public bool SpendProtein(int amount)
        {
            if (protein >= amount)
            {
                protein -= amount;
                OnResourcesChanged?.Invoke();
                return true;
            }
            return false;
        }

        public void DamageQueen(int damage)
        {
            queenHp = Mathf.Max(0, queenHp - damage);
            OnQueenHpChanged?.Invoke(queenHp);

            if (queenHp <= 0)
            {
                OnGameOver?.Invoke();
            }
        }

        public void ApplyDifficultyAndStage(DifficultyData diff, StageConfigData stage)
        {
            currentDifficulty = diff ?? DifficultyData.CreateDefault(DifficultyType.Normal);
            currentStageConfig = stage;

            int bonusSugar = (TechLabManager.Instance != null) ? TechLabManager.Instance.startSugarBonus : 0;
            int bonusQueenHp = (TechLabManager.Instance != null) ? TechLabManager.Instance.queenMaxHpBonus : 0;

            sugar = currentDifficulty.startSugar + bonusSugar;
            protein = 0;
            dnaRewardAccumulated = 0;
            maxQueenHp = currentDifficulty.startQueenHp + bonusQueenHp;
            queenHp = maxQueenHp;
            totalKills = 0;

            if (currentStageConfig != null)
            {
                waypoints = currentStageConfig.GetWorldWaypoints();
            }

            OnResourcesChanged?.Invoke();
            OnQueenHpChanged?.Invoke(queenHp);
        }

        public void AddDna(int amount)
        {
            dnaRewardAccumulated += amount;
            int total = PlayerPrefs.GetInt("ANT_COLONY_DNA", 0) + amount;
            PlayerPrefs.SetInt("ANT_COLONY_DNA", total);
            PlayerPrefs.Save();
            OnResourcesChanged?.Invoke();
        }

        public void TriggerVictory()
        {
            if (currentDifficulty != null)
            {
                AddDna(currentDifficulty.bonusDnaReward);
            }
            OnVictory?.Invoke();
        }

        public void RegisterKill(Enemy enemy)
        {
            totalKills++;
            float rewardMul = currentDifficulty != null ? currentDifficulty.rewardMultiplier : 1.0f;
            float labRate = (TechLabManager.Instance != null) ? TechLabManager.Instance.sugarBonusRate : 1.0f;
            int sugarGain = Mathf.RoundToInt(enemy.data.sugarReward * rewardMul * labRate);
            AddSugar(sugarGain);

            if (enemy.data.proteinReward > 0)
            {
                AddProtein(enemy.data.proteinReward);
            }
        }
    }
}
