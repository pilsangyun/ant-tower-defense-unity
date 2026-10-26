using System.Collections.Generic;
using UnityEngine;

namespace AntTD.Data
{
    [System.Serializable]
    public class SpawnGroup
    {
        public EnemyData enemyData;
        public int count;
        public float interval = 1.0f;
        public float delay = 0.5f;
    }

    [System.Serializable]
    public class WaveDefinition
    {
        public int waveNumber;
        public string title;
        [TextArea] public string description;
        public List<SpawnGroup> spawns = new List<SpawnGroup>();
    }

    [CreateAssetMenu(fileName = "NewStageWaveData", menuName = "AntTD/Stage Wave Data")]
    public class StageWaveData : ScriptableObject
    {
        public string stageId;
        public string stageName;
        public List<WaveDefinition> waves = new List<WaveDefinition>();
    }
}
