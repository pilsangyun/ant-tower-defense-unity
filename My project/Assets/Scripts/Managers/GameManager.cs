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

        [Header("Stage Configuration")]
        public StageWaveData currentStageData;
        public List<Vector2> waypoints = new List<Vector2>();

        [Header("Economy & Queen")]
        public int sugar = 160;
        public int protein = 0;
        public int queenHp = 20;
        public int maxQueenHp = 20;
        public int totalKills = 0;

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

        public void RegisterKill(Enemy enemy)
        {
            totalKills++;
            AddSugar(enemy.data.sugarReward);
            if (enemy.data.proteinReward > 0)
            {
                AddProtein(enemy.data.proteinReward);
            }
        }
    }
}
