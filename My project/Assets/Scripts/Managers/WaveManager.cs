using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AntTD.Data;
using AntTD.Entities;

namespace AntTD.Managers
{
    public class WaveManager : MonoBehaviour
    {
        public static WaveManager Instance { get; private set; }

        public GameObject enemyPrefab;
        [System.Serializable]
        public struct EnemyPrefabEntry
        {
            public string enemyId;
            public GameObject prefab;
        }
        public List<EnemyPrefabEntry> enemyPrefabList = new List<EnemyPrefabEntry>();
        [System.NonSerialized] public Dictionary<string, GameObject> enemyPrefabMap = new Dictionary<string, GameObject>();

        public int currentWaveIndex = 0;
        public bool isWaveRunning = false;
        public float wavePrepCountdown = 10.0f;

        public List<Enemy> activeEnemies = new List<Enemy>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            InitPrefabMap();
        }

        private void Update()
        {
            if (!isWaveRunning)
            {
                var stage = GameManager.Instance != null ? GameManager.Instance.currentStageData : null;
                if (stage != null && currentWaveIndex < stage.waves.Count)
                {
                    wavePrepCountdown -= Time.deltaTime;

                    if (Input.GetKeyDown(KeyCode.N))
                    {
                        StartNextWave();
                    }
                    else if (wavePrepCountdown <= 0f)
                    {
                        StartNextWave();
                    }
                }
            }
        }

        public void InitPrefabMap()
        {
            if (enemyPrefabMap == null) enemyPrefabMap = new Dictionary<string, GameObject>();
            if (enemyPrefabList != null)
            {
                foreach (var entry in enemyPrefabList)
                {
                    if (!string.IsNullOrEmpty(entry.enemyId) && entry.prefab != null)
                    {
                        enemyPrefabMap[entry.enemyId] = entry.prefab;
                    }
                }
            }
        }

        public void StartNextWave()
        {
            if (isWaveRunning) return;
            var stage = GameManager.Instance.currentStageData;
            if (stage == null || currentWaveIndex >= stage.waves.Count) return;

            StartCoroutine(SpawnWaveRoutine(stage.waves[currentWaveIndex]));
        }

        private IEnumerator SpawnWaveRoutine(WaveDefinition wave)
        {
            isWaveRunning = true;

            foreach (var spawnGroup in wave.spawns)
            {
                yield return new WaitForSeconds(spawnGroup.delay);

                for (int i = 0; i < spawnGroup.count; i++)
                {
                    SpawnEnemy(spawnGroup.enemyData);
                    yield return new WaitForSeconds(spawnGroup.interval);
                }
            }

            // Wait until all enemies in the current wave are defeated
            while (activeEnemies.Count > 0)
            {
                yield return new WaitForSeconds(0.3f);
            }

            OnWaveClear();
        }

        private void SpawnEnemy(EnemyData data)
        {
            GameObject targetPrefab = enemyPrefab;
            if (data != null && enemyPrefabMap != null && enemyPrefabMap.ContainsKey(data.enemyId))
            {
                targetPrefab = enemyPrefabMap[data.enemyId];
            }

            if (targetPrefab == null) return;

            GameObject obj = Instantiate(targetPrefab);
            Enemy enemy = obj.GetComponent<Enemy>();

            var diff = GameManager.Instance != null ? GameManager.Instance.currentDifficulty : null;
            float diffHpMul = diff != null ? diff.hpMultiplier : 1.0f;
            float diffSpdMul = diff != null ? diff.speedMultiplier : 1.0f;

            float hpMul = 1.0f + (currentWaveIndex * 0.12f);
            if (diff != null && diff.diffType == DifficultyType.Infinite)
            {
                hpMul = Mathf.Pow(1.25f, currentWaveIndex);
            }
            float finalHpMul = hpMul * diffHpMul;

            var spawnPath = (StageManager.Instance != null) ? StageManager.Instance.GetSpawnPathForNextEnemy() : GameManager.Instance.waypoints;
            enemy.Initialize(data, finalHpMul, diffSpdMul, spawnPath);

            enemy.OnEnemyDied += HandleEnemyDied;
            enemy.OnEnemyReachedQueen += HandleEnemyReachedQueen;
            activeEnemies.Add(enemy);
        }

        private void HandleEnemyDied(Enemy enemy)
        {
            activeEnemies.Remove(enemy);
            GameManager.Instance.RegisterKill(enemy);
        }

        private void HandleEnemyReachedQueen(Enemy enemy)
        {
            activeEnemies.Remove(enemy);
            int dmg = enemy.data.isBoss ? 8 : 1;
            GameManager.Instance.DamageQueen(dmg);
        }

        private void OnWaveClear()
        {
            isWaveRunning = false;
            currentWaveIndex++;
            var diff = GameManager.Instance != null ? GameManager.Instance.currentDifficulty : null;
            wavePrepCountdown = diff != null ? diff.waveCountdownSeconds : 10.0f;

            if (GameManager.Instance != null)
            {
                int sugarBonus = 25 + currentWaveIndex * 10;
                int proteinBonus = (currentWaveIndex % 3 == 0) ? 1 : 0;
                GameManager.Instance.AddSugar(sugarBonus);
                if (proteinBonus > 0) GameManager.Instance.AddProtein(proteinBonus);
            }

            var stage = GameManager.Instance != null ? GameManager.Instance.currentStageData : null;
            if (stage != null && currentWaveIndex >= stage.waves.Count)
            {
                Debug.Log("🎉 [AntTD] ALL 15 WAVES CLEARED! VICTORY!");
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.TriggerVictory();
                }
            }
        }
    }
}
