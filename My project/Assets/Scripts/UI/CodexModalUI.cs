using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AntTD.Data;
using AntTD.Managers;

namespace AntTD.UI
{
    public class CodexModalUI : MonoBehaviour
    {
        public static CodexModalUI Instance { get; private set; }

        [Header("Modal Root")]
        public GameObject codexModalPanel;
        public Button closeBtn;

        [Header("Tabs")]
        public Button tabTowersBtn;
        public Button tabEnemiesBtn;
        public Button tabSynergiesBtn;

        [Header("Content Area")]
        public Transform contentGrid;
        public Text sectionTitleText;

        [Header("Config Assets for Codex")]
        public List<TowerData> allAntTypes = new List<TowerData>();
        public List<EnemyData> allEnemyTypes = new List<EnemyData>();

        private string currentTab = "towers";

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            if (closeBtn != null) closeBtn.onClick.AddListener(CloseCodex);
            if (tabTowersBtn != null) tabTowersBtn.onClick.AddListener(() => SwitchTab("towers"));
            if (tabEnemiesBtn != null) tabEnemiesBtn.onClick.AddListener(() => SwitchTab("enemies"));
            if (tabSynergiesBtn != null) tabSynergiesBtn.onClick.AddListener(() => SwitchTab("synergies"));

            if (codexModalPanel != null) codexModalPanel.SetActive(false);
        }

        public void OpenCodex()
        {
            if (codexModalPanel != null)
            {
                codexModalPanel.SetActive(true);
                SwitchTab("towers");
            }
        }

        public void CloseCodex()
        {
            if (codexModalPanel != null)
            {
                codexModalPanel.SetActive(false);
            }
        }

        public void SwitchTab(string tabKey)
        {
            currentTab = tabKey;

            if (sectionTitleText != null)
            {
                switch (tabKey)
                {
                    case "towers":
                        sectionTitleText.text = "🐜 군집 아군 개미 도감 (기본 6종 + 특화 진화체 12종)";
                        break;
                    case "enemies":
                        sectionTitleText.text = "🦗 침입자 곤충 생태 도감 (일반 5종 + 보스 2종)";
                        break;
                    case "synergies":
                        sectionTitleText.text = "⚡ 군집 진형 시너지 연계 도감 (3종)";
                        break;
                }
            }

            RenderTabContent(tabKey);
        }

        private void RenderTabContent(string tabKey)
        {
            if (contentGrid == null) return;

            // Clear old entries
            for (int i = contentGrid.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(contentGrid.GetChild(i).gameObject);
            }

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            float cardW = 460f;
            float cardH = 50f;
            float gap = 6f;

            if (tabKey == "towers")
            {
                var towers = new (string name, string role, string evo)[]
                {
                    ("일개미 (Worker)", "자원 지원", "진화 A: 가위개미 (버섯 농장) / 진화 B: 꿀단지개미 (광역 버프)"),
                    ("병정개미 (Soldier)", "근접 저지", "진화 A: 군대개미 (출혈/광란) / 진화 B: 철갑개미 (철벽 방어)"),
                    ("불개미 (Fire Ant)", "원거리 고속", "진화 A: 맹독 불개미 (무한 화상) / 진화 B: 화염방사 (부채꼴 광역)"),
                    ("산성개미 (Acid Ant)", "방어력 부식", "진화 A: 부식 폭격개미 (산성 웅덩이) / 진화 B: 저격 개미 (관통 레일건)"),
                    ("자폭개미 (Exploder)", "자폭 누커", "진화 A: 연쇄폭발 개미 (연쇄 참사) / 진화 B: 신경독 개미 (광역 기절)"),
                    ("페로몬개미 (Pheromone)", "디버프 지원", "진화 A: 혼란 페로몬 (역주행) / 진화 B: 광란 페로몬 (아군 공속+50%)")
                };

                for (int i = 0; i < towers.Length; i++)
                {
                    CreateCodexCard(i, cardW, cardH, gap, $"🐜 {towers[i].name} <color=#eab308>[{towers[i].role}]</color>", towers[i].evo, font);
                }
            }
            else if (tabKey == "enemies")
            {
                var enemies = new (string name, string stats, string ability)[]
                {
                    ("정찰 달팽이 (Snail)", "HP 45 | 속도 0.85 | 방어 0%", "가장 기초적인 척후병. 무리를 지어 빠르게 진격합니다."),
                    ("단단한 무당벌레 (Ladybug)", "HP 240 | 속도 0.45 | 방어 70%", "두꺼운 껍질로 물리 공격을 튕겨냅니다. 산성 부식이 필수."),
                    ("그림자 거미 (Spider)", "HP 170 | 속도 1.10 | 방어 15%", "날렵한 기동성으로 타워에 거미줄을 쳐 공격을 봉쇄합니다."),
                    ("뿔애벌레 (Caterpillar)", "HP 130 | 속도 0.75 | 방어 10%", "땅속으로 잠복하여 원거리 공격을 회피하며 침투합니다."),
                    ("풍뎅이 타이탄 (Beetle Boss)", "HP 2,200 | 속도 0.35 | 방어 80%", "중간 보스. 엄청난 떡장갑과 돌진으로 전선을 붕괴시킵니다."),
                    ("사마귀 맹장 타이탄 (Mantis Boss)", "HP 3,500 | 속도 0.50 | 방어 40%", "최종 보스. 낫칼 휘두르기로 아군 병정을 단숨에 도륙합니다.")
                };

                for (int i = 0; i < enemies.Length; i++)
                {
                    CreateCodexCard(i, cardW, cardH, gap, $"🦗 {enemies[i].name} <color=#38bdf8>({enemies[i].stats})</color>", enemies[i].ability, font);
                }
            }
            else if (tabKey == "synergies")
            {
                var synergies = new (string title, string req, string effect)[]
                {
                    ("산성 불길 (Acid Flame)", "불개미 + 산성개미", "화상 디버프에 걸린 적에게 산성 부식 공격 시 피해량 +100% 증폭!"),
                    ("철벽 군단 (Iron Phalanx)", "병정개미 3기 이상 배치", "모든 병정개미 방어력 +35% 및 최대 저지수 +1 추가 확보!"),
                    ("여왕의 친위대 (Queen's Guard)", "페로몬개미 + 일개미", "당분 채집 속도 +40% 증가 및 모든 여왕 스킬 쿨다운 15% 가속!")
                };

                for (int i = 0; i < synergies.Length; i++)
                {
                    CreateCodexCard(i, cardW, cardH, gap, $"⚡ {synergies[i].title} <color=#fbbf24>[조건: {synergies[i].req}]</color>", synergies[i].effect, font);
                }
            }
        }

        private void CreateCodexCard(int index, float w, float h, float gap, string headerText, string descText, Font font)
        {
            float py = -((index * (h + gap)) + (h * 0.5f));
            GameObject card = new GameObject($"Card_{index}", typeof(RectTransform));
            card.transform.SetParent(contentGrid, false);
            RectTransform crt = card.GetComponent<RectTransform>();
            crt.anchorMin = new Vector2(0.5f, 1f);
            crt.anchorMax = new Vector2(0.5f, 1f);
            crt.pivot = new Vector2(0.5f, 0.5f);
            crt.sizeDelta = new Vector2(w, h);
            crt.anchoredPosition = new Vector2(0, py);

            Image img = card.AddComponent<Image>();
            img.color = new Color(0.12f, 0.14f, 0.16f, 0.92f);
            img.raycastTarget = false;

            GameObject titleObj = new GameObject("Text", typeof(RectTransform));
            titleObj.transform.SetParent(card.transform, false);
            RectTransform trt = titleObj.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.sizeDelta = new Vector2(-16, 0);
            trt.anchoredPosition = Vector2.zero;

            Text t = titleObj.AddComponent<Text>();
            t.font = font;
            t.fontSize = 11;
            t.fontStyle = FontStyle.Bold;
            t.color = Color.white;
            t.alignment = TextAnchor.MiddleLeft;
            t.text = $"{headerText}\n<color=#cbd5e1><size=9>{descText}</size></color>";
            t.raycastTarget = false;
        }
    }
}
