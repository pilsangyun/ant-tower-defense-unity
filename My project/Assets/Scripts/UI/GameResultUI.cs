using System;
using UnityEngine;
using UnityEngine.UI;
using AntTD.Managers;

namespace AntTD.UI
{
    public class GameResultUI : MonoBehaviour
    {
        [Header("Victory Screen")]
        public GameObject victoryPanel;
        public Text victoryTitleText;
        public Text victoryStatsText;
        public Button victoryLobbyBtn;
        public Button victoryRestartBtn;

        [Header("Defeat Screen")]
        public GameObject defeatPanel;
        public Text defeatTitleText;
        public Text defeatStatsText;
        public Button defeatLobbyBtn;
        public Button defeatRestartBtn;

        private void Start()
        {
            if (victoryPanel != null) victoryPanel.SetActive(false);
            if (defeatPanel != null) defeatPanel.SetActive(false);

            BindEvents();
        }

        private void BindEvents()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnVictory += ShowVictory;
                GameManager.Instance.OnGameOver += ShowDefeat;
            }

            if (victoryLobbyBtn != null) victoryLobbyBtn.onClick.AddListener(OnReturnToLobby);
            if (victoryRestartBtn != null) victoryRestartBtn.onClick.AddListener(OnRestartGame);

            if (defeatLobbyBtn != null) defeatLobbyBtn.onClick.AddListener(OnReturnToLobby);
            if (defeatRestartBtn != null) defeatRestartBtn.onClick.AddListener(OnRestartGame);
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnVictory -= ShowVictory;
                GameManager.Instance.OnGameOver -= ShowDefeat;
            }
        }

        public void ShowVictory()
        {
            if (victoryPanel == null) return;
            victoryPanel.SetActive(true);

            int kills = GameManager.Instance != null ? GameManager.Instance.totalKills : 0;
            int qHp = GameManager.Instance != null ? GameManager.Instance.queenHp : 20;
            int dna = GameManager.Instance != null ? GameManager.Instance.dnaRewardAccumulated : 0;

            string rank = "S";
            if (qHp < 10) rank = "B";
            else if (qHp < 18) rank = "A";

            if (victoryStatsText != null)
            {
                victoryStatsText.text = $"<b>전 15 웨이브 완전 방어 성공!</b>\n\n" +
                    $"• 작전 평가 랭크: <b><color=#fbbf24>{rank}</color></b>\n" +
                    $"• 총 처치한 침입자: {kills} 마리\n" +
                    $"• 잔여 여왕개미 체력: {qHp} / 20\n" +
                    $"• 획득 군집 유전자: <b><color=#38bdf8>+{dna} 🧬 DNA</color></b>";
            }
        }

        public void ShowDefeat()
        {
            if (defeatPanel == null) return;
            defeatPanel.SetActive(true);

            int wave = WaveManager.Instance != null ? (WaveManager.Instance.currentWaveIndex + 1) : 1;
            int kills = GameManager.Instance != null ? GameManager.Instance.totalKills : 0;
            int sugar = GameManager.Instance != null ? GameManager.Instance.sugar : 0;

            if (defeatStatsText != null)
            {
                defeatStatsText.text = $"<b>여왕개미 방어선이 함락되었습니다!</b>\n\n" +
                    $"• 최종 도달 웨이브: WAVE {wave} / 15\n" +
                    $"• 총 처치한 침입자: {kills} 마리\n" +
                    $"• 잔여 자원: 🍬 {sugar}";
            }
        }

        private void OnReturnToLobby()
        {
            if (victoryPanel != null) victoryPanel.SetActive(false);
            if (defeatPanel != null) defeatPanel.SetActive(false);

            if (LobbyManager.Instance != null)
            {
                LobbyManager.Instance.ReturnToLobby();
            }
        }

        private void OnRestartGame()
        {
            if (victoryPanel != null) victoryPanel.SetActive(false);
            if (defeatPanel != null) defeatPanel.SetActive(false);

            // Restart current battle with same stage/diff
            if (LobbyManager.Instance != null)
            {
                LobbyManager.Instance.OnStartBattleClicked();
            }
        }
    }
}
