using UnityEngine;
using UnityEngine.UI;
using AntTD.Managers;
using AntTD.Data;

namespace AntTD.UI
{
    public class InGameCanvasController : MonoBehaviour
    {
        [Header("Top HUD Elements")]
        public Text waveText;
        public Text weatherText;
        public Text sugarText;
        public Text proteinText;
        public Text queenHpText;
        public Image queenHpFill;
        public Button waveStartBtn;
        public Text waveStartBtnText;
        public Button speedToggleBtn;
        public Text speedToggleBtnText;

        [Header("Skills Dock Elements")]
        public Button swarmCallBtn;
        public Button acidRainBtn;
        public Button honeyTrapBtn;

        [Header("Bottom Sheet Elements")]
        public GameObject bottomSheetPanel;
        public Text sheetTitleText;
        public Text sheetStatsText;
        public Button sheetUpgradeBtn;
        public Text sheetUpgradeBtnText;
        public Button sheetOverclockBtn;
        public Text sheetOverclockBtnText;
        public Button sheetSellBtn;
        public Text sheetSellBtnText;
        public Button sheetPriorityBtn;
        public Text sheetPriorityBtnText;
        public Button sheetCloseBtn;

        [Header("Evolution Modal Elements")]
        public GameObject evolutionModalPanel;
        public Text evTitleText;
        public Text evSubtitleText;
        public Button evCloseBtn;

        public Image evCardAIcon;
        public Text evCardAName;
        public Text evCardATitle;
        public Text evCardADesc;
        public Text evCardACost;
        public Button evCardABtn;

        public Image evCardBIcon;
        public Text evCardBName;
        public Text evCardBTitle;
        public Text evCardBDesc;
        public Text evCardBCost;
        public Button evCardBBtn;

        private Entities.Tower inspectedTowerForEvo;
        private int currentSpeed = 1;

        private void Start()
        {
            if (waveStartBtn != null) waveStartBtn.onClick.AddListener(OnWaveStartClicked);
            if (speedToggleBtn != null) speedToggleBtn.onClick.AddListener(OnSpeedToggleClicked);
            if (sheetCloseBtn != null) sheetCloseBtn.onClick.AddListener(CloseBottomSheet);
            if (evCloseBtn != null) evCloseBtn.onClick.AddListener(CloseEvolutionModal);

            if (evolutionModalPanel != null) evolutionModalPanel.SetActive(false);

            if (PlacementManager.Instance != null)
            {
                PlacementManager.Instance.OnTowerInspected += OpenBottomSheet;
            }

            if (bottomSheetPanel != null) bottomSheetPanel.SetActive(false);

            if (WeatherManager.Instance != null && weatherText != null)
            {
                WeatherManager.Instance.hudWeatherText = weatherText;
                WeatherManager.Instance.SetWeather(WeatherManager.Instance.currentWeather);
            }

            UpdateHUD();
        }

        private void Update()
        {
            UpdateHUD();
        }

        private void UpdateHUD()
        {
            if (GameManager.Instance != null)
            {
                if (sugarText != null) sugarText.text = GameManager.Instance.sugar.ToString();
                if (proteinText != null) proteinText.text = GameManager.Instance.protein.ToString();
                if (queenHpText != null) queenHpText.text = $"{GameManager.Instance.queenHp} / {GameManager.Instance.maxQueenHp}";
                if (queenHpFill != null)
                {
                    queenHpFill.fillAmount = (float)GameManager.Instance.queenHp / GameManager.Instance.maxQueenHp;
                }
            }

            if (WaveManager.Instance != null)
            {
                if (waveText != null)
                {
                    waveText.text = $"WAVE {WaveManager.Instance.currentWaveIndex + 1} / 15";
                }

                if (waveStartBtn != null && waveStartBtnText != null)
                {
                    if (WaveManager.Instance.isWaveRunning)
                    {
                        waveStartBtn.interactable = false;
                        waveStartBtnText.text = "전투 진행 중...";
                    }
                    else
                    {
                        var stage = GameManager.Instance != null ? GameManager.Instance.currentStageData : null;
                        if (stage != null && WaveManager.Instance.currentWaveIndex >= stage.waves.Count)
                        {
                            waveStartBtn.interactable = false;
                            waveStartBtnText.text = "전체 클리어!";
                        }
                        else
                        {
                            waveStartBtn.interactable = true;
                            int secs = Mathf.Max(1, Mathf.CeilToInt(WaveManager.Instance.wavePrepCountdown));
                            waveStartBtnText.text = $"웨이브 시작 ({secs}s) [N]";
                        }
                    }
                }
            }
        }

        private void OnWaveStartClicked()
        {
            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.StartNextWave();
            }
        }

        private void OnSpeedToggleClicked()
        {
            currentSpeed = (currentSpeed == 1) ? 2 : 1;
            Time.timeScale = currentSpeed;
            if (speedToggleBtnText != null)
            {
                speedToggleBtnText.text = $"{currentSpeed}x";
            }
        }

        public void OpenBottomSheet(Entities.Tower tower)
        {
            if (tower == null)
            {
                if (bottomSheetPanel != null) bottomSheetPanel.SetActive(false);

            if (WeatherManager.Instance != null && weatherText != null)
            {
                WeatherManager.Instance.hudWeatherText = weatherText;
                WeatherManager.Instance.SetWeather(WeatherManager.Instance.currentWeather);
            }
                return;
            }

            if (bottomSheetPanel == null) return;
            bottomSheetPanel.SetActive(true);

            sheetTitleText.text = $"{tower.data.antName} (Lv.{tower.level} {(tower.branch ?? "")} +{tower.overclockLevel})";
            sheetStatsText.text = $"공격력: {Mathf.Round(tower.currentDamage)}  |  공속: {tower.currentAttackSpeed:F1}  |  사거리: {tower.currentRange:F1}";

            if (sheetPriorityBtn != null)
            {
                sheetPriorityBtn.gameObject.SetActive(true);
                if (sheetPriorityBtnText != null) sheetPriorityBtnText.text = tower.GetPriorityLabel();
                sheetPriorityBtn.onClick.RemoveAllListeners();
                sheetPriorityBtn.onClick.AddListener(() =>
                {
                    tower.CycleTargetingPriority();
                    if (sheetPriorityBtnText != null) sheetPriorityBtnText.text = tower.GetPriorityLabel();
                });
            }

            if (sheetSellBtn != null)
            {
                sheetSellBtn.gameObject.SetActive(true);
                int refund = tower.CalculateRefundSugar();
                if (sheetSellBtnText != null) sheetSellBtnText.text = $"철거 (🍬 {refund} 환급)";
                sheetSellBtn.onClick.RemoveAllListeners();
                sheetSellBtn.onClick.AddListener(() =>
                {
                    tower.DemolishAndRefund();
                    CloseBottomSheet();
                });
            }

            if (tower.level == 1)
            {
                sheetUpgradeBtnText.text = $"2단계 강화 (🍬{tower.data.upgradeCost})";
                sheetUpgradeBtn.onClick.RemoveAllListeners();
                sheetUpgradeBtn.onClick.AddListener(() =>
                {
                    if (GameManager.Instance.SpendSugar(tower.data.upgradeCost))
                    {
                        tower.UpgradeToLevel2();
                        OpenBottomSheet(tower);
                    }
                });
            }
            else if (tower.level == 2)
            {
                sheetUpgradeBtnText.text = $"🧬 3단계 진화 분기 선택 (🍖 2)";
                sheetUpgradeBtn.onClick.RemoveAllListeners();
                sheetUpgradeBtn.onClick.AddListener(() =>
                {
                    OpenEvolutionModal(tower);
                });
            }
            else if (tower.level == 3)
            {
                int costS = 150 + tower.overclockLevel * 80;
                sheetOverclockBtnText.text = $"🔥 한계돌파 +{tower.overclockLevel + 1} (🍬{costS}/🍖1)";
                sheetOverclockBtn.onClick.RemoveAllListeners();
                sheetOverclockBtn.onClick.AddListener(() =>
                {
                    if (GameManager.Instance.SpendSugar(costS) && GameManager.Instance.SpendProtein(1))
                    {
                        tower.ApplyOverclock();
                        OpenBottomSheet(tower);
                    }
                });
            }
        }

        public void OpenEvolutionModal(Entities.Tower tower)
        {
            if (tower == null || tower.data == null) return;
            inspectedTowerForEvo = tower;

            if (evolutionModalPanel == null) return;
            evolutionModalPanel.SetActive(true);

            var brA = tower.data.branchA;
            var brB = tower.data.branchB;

            if (evTitleText != null) evTitleText.text = $"🧬 3레벨 특화 진화 분기: {tower.data.antName}";
            if (evSubtitleText != null) evSubtitleText.text = "단백질(🍖 2)을 소모하여 개미 군단의 궁극의 진화체를 선택하세요.";

            // Card A
            if (brA != null)
            {
                if (evCardAName != null) evCardAName.text = $"[A] {brA.name}";
                if (evCardATitle != null) evCardATitle.text = brA.title;
                if (evCardADesc != null) evCardADesc.text = brA.description;
                if (evCardACost != null) evCardACost.text = $"비용: 🍬 {brA.sugarCost} + 🍖 {brA.proteinCost}";
                if (evCardAIcon != null) evCardAIcon.sprite = brA.evolvedSprite != null ? brA.evolvedSprite : tower.data.icon;

                if (evCardABtn != null)
                {
                    evCardABtn.onClick.RemoveAllListeners();
                    evCardABtn.onClick.AddListener(() =>
                    {
                        if (GameManager.Instance.SpendSugar(brA.sugarCost) && GameManager.Instance.SpendProtein(brA.proteinCost))
                        {
                            tower.Evolve("A");
                            CloseEvolutionModal();
                            OpenBottomSheet(tower);
                        }
                    });
                }
            }

            // Card B
            if (brB != null)
            {
                if (evCardBName != null) evCardBName.text = $"[B] {brB.name}";
                if (evCardBTitle != null) evCardBTitle.text = brB.title;
                if (evCardBDesc != null) evCardBDesc.text = brB.description;
                if (evCardBCost != null) evCardBCost.text = $"비용: 🍬 {brB.sugarCost} + 🍖 {brB.proteinCost}";
                if (evCardBIcon != null) evCardBIcon.sprite = brB.evolvedSprite != null ? brB.evolvedSprite : tower.data.icon;

                if (evCardBBtn != null)
                {
                    evCardBBtn.onClick.RemoveAllListeners();
                    evCardBBtn.onClick.AddListener(() =>
                    {
                        if (GameManager.Instance.SpendSugar(brB.sugarCost) && GameManager.Instance.SpendProtein(brB.proteinCost))
                        {
                            tower.Evolve("B");
                            CloseEvolutionModal();
                            OpenBottomSheet(tower);
                        }
                    });
                }
            }
        }

        public void CloseEvolutionModal()
        {
            if (evolutionModalPanel != null) evolutionModalPanel.SetActive(false);
        }

        public void CloseBottomSheet()
        {
            if (bottomSheetPanel != null) bottomSheetPanel.SetActive(false);

            if (WeatherManager.Instance != null && weatherText != null)
            {
                WeatherManager.Instance.hudWeatherText = weatherText;
                WeatherManager.Instance.SetWeather(WeatherManager.Instance.currentWeather);
            }
            if (PlacementManager.Instance != null)
            {
                PlacementManager.Instance.CloseInspectorSilently();
            }
        }
    }
}
