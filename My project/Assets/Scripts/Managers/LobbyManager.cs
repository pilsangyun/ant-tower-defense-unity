using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AntTD.Data;

namespace AntTD.Managers
{
    public class LobbyManager : MonoBehaviour
    {
        public static LobbyManager Instance { get; private set; }

        [Header("UI Root Panels")]
        public GameObject lobbyPanel;
        public GameObject battleCanvas;
        public GameObject techLabModal;
        public GameObject codexModal;

        [Header("Lobby Header")]
        public Text dnaDisplayText;

        [Header("Difficulty Tabs")]
        public List<Button> difficultyTabButtons = new List<Button>();
        public Text difficultyDetailText;
        public DifficultyType selectedDifficulty = DifficultyType.Easy;

        [Header("Stage Cards")]
        public List<Button> stageCardButtons = new List<Button>();
        public string selectedStageId = "sunny";

        [Header("Lobby Action Buttons")]
        public Button openTechLabBtn;
        public Button openCodexBtn;
        public Button startBattleBtn;

        [Header("Config Assets")]
        public List<DifficultyData> difficultyConfigs = new List<DifficultyData>();
        public List<StageConfigData> stageConfigs = new List<StageConfigData>();

        public event Action<DifficultyData, StageConfigData> OnBattleStarted;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            InitDefaultConfigsIfEmpty();
            BindUIEvents();
            UpdateDnaDisplay();
            SelectDifficulty(DifficultyType.Easy);
            SelectStage("sunny");
        }

        private void InitDefaultConfigsIfEmpty()
        {
            if (difficultyConfigs == null || difficultyConfigs.Count == 0)
            {
                difficultyConfigs = new List<DifficultyData>
                {
                    DifficultyData.CreateDefault(DifficultyType.Easy),
                    DifficultyData.CreateDefault(DifficultyType.Normal),
                    DifficultyData.CreateDefault(DifficultyType.Hard),
                    DifficultyData.CreateDefault(DifficultyType.Nightmare),
                    DifficultyData.CreateDefault(DifficultyType.Infinite)
                };
            }
        }

        private void BindUIEvents()
        {
            if (openTechLabBtn != null)
                openTechLabBtn.onClick.AddListener(OpenTechLab);

            if (openCodexBtn != null)
                openCodexBtn.onClick.AddListener(OpenCodex);

            if (startBattleBtn != null)
                startBattleBtn.onClick.AddListener(OnStartBattleClicked);
        }

        public void UpdateDnaDisplay()
        {
            int dna = PlayerPrefs.GetInt("ANT_COLONY_DNA", 0);
            if (dnaDisplayText != null)
            {
                dnaDisplayText.text = dna.ToString();
            }
        }

        public void SelectDifficulty(DifficultyType diffType)
        {
            selectedDifficulty = diffType;
            var cfg = difficultyConfigs.Find(d => d.diffType == diffType);
            if (cfg != null && difficultyDetailText != null)
            {
                difficultyDetailText.text = $"<b>{cfg.displayName}</b>: {cfg.description}\n" +
                    $"• 적 체력 {cfg.hpMultiplier:F2}x | 이속 {cfg.speedMultiplier:F2}x | 준비시간 {cfg.waveCountdownSeconds}s | 철거환급 {cfg.sellRefundRate * 100:F0}%";
            }
        }

        public void SelectStage(string stageId)
        {
            selectedStageId = stageId;
        }

        public void OpenTechLab()
        {
            if (techLabModal != null)
            {
                techLabModal.SetActive(true);
            }
        }

        public void CloseTechLab()
        {
            if (techLabModal != null)
            {
                techLabModal.SetActive(false);
                UpdateDnaDisplay();
            }
        }

        public void OpenCodex()
        {
            if (codexModal != null)
            {
                codexModal.SetActive(true);
            }
        }

        public void CloseCodex()
        {
            if (codexModal != null)
            {
                codexModal.SetActive(false);
            }
        }

        public void OnStartBattleClicked()
        {
            var diffData = difficultyConfigs.Find(d => d.diffType == selectedDifficulty) 
                ?? DifficultyData.CreateDefault(selectedDifficulty);

            var stageData = stageConfigs.Find(s => s.stageId == selectedStageId);

            // Load selected stage slots & waypoints via StageManager
            if (StageManager.Instance != null && stageData != null)
            {
                StageManager.Instance.LoadStage(stageData);
            }

            // Pass configured settings to GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ApplyDifficultyAndStage(diffData, stageData);
            }

            // Hide lobby overlay and activate in-game canvas
            if (lobbyPanel != null) lobbyPanel.SetActive(false);
            if (battleCanvas != null) battleCanvas.SetActive(true);

            OnBattleStarted?.Invoke(diffData, stageData);

            // Start wave preparation
            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.wavePrepCountdown = diffData.waveCountdownSeconds;
            }
        }

        public void ReturnToLobby()
        {
            if (battleCanvas != null) battleCanvas.SetActive(false);
            if (lobbyPanel != null) lobbyPanel.SetActive(true);
            UpdateDnaDisplay();
        }
    }
}
