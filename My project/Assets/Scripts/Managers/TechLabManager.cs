using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AntTD.Managers
{
    [System.Serializable]
    public class TechNode
    {
        public string id;
        public string name;
        public string category; // "economy", "defense", "acid", "queen"
        public int tier; // 1, 2, 3
        public int dnaCost;
        [TextArea] public string description;

        public bool isUnlocked => PlayerPrefs.GetInt($"TECH_{id}", 0) == 1;

        public void Unlock()
        {
            PlayerPrefs.SetInt($"TECH_{id}", 1);
            PlayerPrefs.Save();
        }

        public void Lock()
        {
            PlayerPrefs.SetInt($"TECH_{id}", 0);
            PlayerPrefs.Save();
        }
    }

    public class TechLabManager : MonoBehaviour
    {
        public static TechLabManager Instance { get; private set; }

        [Header("UI References")]
        public GameObject techLabModal;
        public Text availableDnaText;
        public Transform techNodesContainer;
        public Button closeBtn;

        [Header("Tech Tree Definitions (12 Nodes)")]
        public List<TechNode> allTechNodes = new List<TechNode>();

        // Passive Cache
        public float sugarBonusRate { get; private set; } = 1.0f;
        public int startSugarBonus { get; private set; } = 0;
        public float soldierHpMultiplier { get; private set; } = 1.0f;
        public int soldierBlockBonus { get; private set; } = 0;
        public float acidCorrosionBonus { get; private set; } = 0f;
        public int queenMaxHpBonus { get; private set; } = 0;
        public float skillCooldownReduction { get; private set; } = 0f;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            InitDefaultNodesIfEmpty();
            RecalculatePassives();
        }

        private void Start()
        {
            if (closeBtn != null)
            {
                closeBtn.onClick.AddListener(CloseTechLab);
            }
        }

        public void InitDefaultNodesIfEmpty()
        {
            if (allTechNodes == null || allTechNodes.Count == 0)
            {
                allTechNodes = new List<TechNode>
                {
                    // Category: Economy (채집 본능)
                    new TechNode { id = "eco_1", name = "페로몬 길잡이 I", category = "economy", tier = 1, dnaCost = 2, description = "당분 채집 효율 +15% 증가" },
                    new TechNode { id = "eco_2", name = "페로몬 길잡이 II", category = "economy", tier = 2, dnaCost = 4, description = "당분 채집 효율 +30% 증가" },
                    new TechNode { id = "eco_3", name = "버섯 균사체 농장", category = "economy", tier = 3, dnaCost = 7, description = "당분 채집 +50% 및 초기 당분 🍬+50 지급" },

                    // Category: Defense (키틴질 외골격)
                    new TechNode { id = "def_1", name = "경질 키틴갑피 I", category = "defense", tier = 1, dnaCost = 2, description = "병정개미 체력 +25% 증가" },
                    new TechNode { id = "def_2", name = "경질 키틴갑피 II", category = "defense", tier = 2, dnaCost = 5, description = "병정개미 체력 +50% 및 최대 저지수 +1" },
                    new TechNode { id = "def_3", name = "불굴의 군단", category = "defense", tier = 3, dnaCost = 8, description = "병정개미 받는 피해 20% 경감 및 생존력 대폭 강화" },

                    // Category: Acid (맹독 합성)
                    new TechNode { id = "acd_1", name = "고농도 개미산 I", category = "acid", tier = 1, dnaCost = 3, description = "산성 부식 방어력 감소율 +10% 추가" },
                    new TechNode { id = "acd_2", name = "고농도 개미산 II", category = "acid", tier = 2, dnaCost = 6, description = "산성 부식 지속시간 +2초 연장" },
                    new TechNode { id = "acd_3", name = "산성 폭격 촉매", category = "acid", tier = 3, dnaCost = 9, description = "산성 폭우 쿨다운 -10초 및 폭격 데미지 +50%" },

                    // Category: Queen (여왕의 위엄)
                    new TechNode { id = "que_1", name = "왕실 산란실 증축", category = "queen", tier = 1, dnaCost = 3, description = "여왕개미 최대 체력 +5 증가" },
                    new TechNode { id = "que_2", name = "로열 젤리 비축고", category = "queen", tier = 2, dnaCost = 7, description = "3웨이브마다 보너스 단백질 🍖+1 추가 공급" },
                    new TechNode { id = "que_3", name = "군체 정신 (하이브 마인드)", category = "queen", tier = 3, dnaCost = 15, description = "모든 여왕/전술 스킬 쿨다운 15% 영구 단축" }
                };
            }
        }

        public void RecalculatePassives()
        {
            sugarBonusRate = 1.0f;
            startSugarBonus = 0;
            soldierHpMultiplier = 1.0f;
            soldierBlockBonus = 0;
            acidCorrosionBonus = 0f;
            queenMaxHpBonus = 0;
            skillCooldownReduction = 0f;

            foreach (var node in allTechNodes)
            {
                if (!node.isUnlocked) continue;

                switch (node.id)
                {
                    case "eco_1": sugarBonusRate += 0.15f; break;
                    case "eco_2": sugarBonusRate += 0.15f; break;
                    case "eco_3": sugarBonusRate += 0.20f; startSugarBonus += 50; break;

                    case "def_1": soldierHpMultiplier += 0.25f; break;
                    case "def_2": soldierHpMultiplier += 0.25f; soldierBlockBonus += 1; break;
                    case "def_3": soldierHpMultiplier += 0.30f; break;

                    case "acd_1": acidCorrosionBonus += 0.10f; break;
                    case "acd_2": acidCorrosionBonus += 0.10f; break;
                    case "acd_3": acidCorrosionBonus += 0.15f; break;

                    case "que_1": queenMaxHpBonus += 5; break;
                    case "que_2": break;
                    case "que_3": skillCooldownReduction += 0.15f; break;
                }
            }

            Debug.Log($"🧬 [TechLab] Passives updated: SugarBonus={sugarBonusRate:F2}x, SoldierHp={soldierHpMultiplier:F2}x, QueenHpBonus=+{queenMaxHpBonus}");
        }

        public bool TryUnlockNode(TechNode node)
        {
            if (node == null || node.isUnlocked) return false;

            int currentDna = PlayerPrefs.GetInt("ANT_COLONY_DNA", 0);
            if (currentDna >= node.dnaCost)
            {
                currentDna -= node.dnaCost;
                PlayerPrefs.SetInt("ANT_COLONY_DNA", currentDna);
                node.Unlock();
                RecalculatePassives();
                UpdateUI();
                RefreshNodeButtons();
                return true;
            }
            return false;
        }

        public void ResetAllTech()
        {
            int refundDna = 0;
            foreach (var node in allTechNodes)
            {
                if (node.isUnlocked)
                {
                    refundDna += node.dnaCost;
                    node.Lock();
                }
            }

            int currentDna = PlayerPrefs.GetInt("ANT_COLONY_DNA", 0) + refundDna;
            PlayerPrefs.SetInt("ANT_COLONY_DNA", currentDna);
            PlayerPrefs.Save();

            RecalculatePassives();
            UpdateUI();
            RefreshNodeButtons();

            if (LobbyManager.Instance != null)
            {
                LobbyManager.Instance.UpdateDnaDisplay();
            }
            Debug.Log($"🧬 [TechLab] All tech reset! Refunded: {refundDna} DNA.");
        }

        public void RenderNodesUI()
        {
            if (techNodesContainer == null) return;

            // Clear old children
            for (int i = techNodesContainer.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(techNodesContainer.GetChild(i).gameObject);
            }

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            float cardW = 460f;
            float cardH = 44f;
            float gap = 6f;

            for (int i = 0; i < allTechNodes.Count; i++)
            {
                var node = allTechNodes[i];
                float py = -((i * (cardH + gap)) + (cardH * 0.5f));

                GameObject card = new GameObject($"Node_{node.id}", typeof(RectTransform));
                card.transform.SetParent(techNodesContainer, false);
                RectTransform crt = card.GetComponent<RectTransform>();
                crt.anchorMin = new Vector2(0.5f, 1f);
                crt.anchorMax = new Vector2(0.5f, 1f);
                crt.pivot = new Vector2(0.5f, 0.5f);
                crt.sizeDelta = new Vector2(cardW, cardH);
                crt.anchoredPosition = new Vector2(0, py);

                Image img = card.AddComponent<Image>();
                img.color = node.isUnlocked ? new Color(0.12f, 0.28f, 0.18f, 0.95f) : new Color(0.12f, 0.14f, 0.16f, 0.9f);
                img.raycastTarget = true;

                Button btn = card.AddComponent<Button>();
                var capturedNode = node;
                btn.onClick.AddListener(() => TryUnlockNode(capturedNode));

                // Title & Cost Text
                GameObject titleObj = new GameObject("Title", typeof(RectTransform));
                titleObj.transform.SetParent(card.transform, false);
                RectTransform trt = titleObj.GetComponent<RectTransform>();
                trt.anchorMin = Vector2.zero;
                trt.anchorMax = Vector2.one;
                trt.sizeDelta = new Vector2(-20, 0);
                trt.anchoredPosition = Vector2.zero;

                Text t = titleObj.AddComponent<Text>();
                t.font = font;
                t.fontSize = 11;
                t.fontStyle = FontStyle.Bold;
                t.color = node.isUnlocked ? new Color(0.4f, 0.95f, 0.5f) : Color.white;
                t.alignment = TextAnchor.MiddleLeft;
                t.text = $"{(node.isUnlocked ? "✓ " : "")}{node.name}  [소모: {node.dnaCost} 🧬]\n<color=#aaaaaa><size=9>{node.description}</size></color>";
                t.raycastTarget = false;
            }
        }

        public void RefreshNodeButtons()
        {
            if (techNodesContainer == null) return;
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            for (int i = 0; i < allTechNodes.Count; i++)
            {
                if (i >= techNodesContainer.childCount) break;
                var node = allTechNodes[i];
                Transform card = techNodesContainer.GetChild(i);
                Image img = card.GetComponent<Image>();
                if (img != null)
                {
                    img.color = node.isUnlocked ? new Color(0.12f, 0.28f, 0.18f, 0.95f) : new Color(0.12f, 0.14f, 0.16f, 0.9f);
                }

                Transform titleTf = card.Find("Title");
                if (titleTf != null)
                {
                    Text t = titleTf.GetComponent<Text>();
                    if (t != null)
                    {
                        t.color = node.isUnlocked ? new Color(0.4f, 0.95f, 0.5f) : Color.white;
                        t.text = $"{(node.isUnlocked ? "✓ " : "")}{node.name}  [소모: {node.dnaCost} 🧬]\n<color=#aaaaaa><size=9>{node.description}</size></color>";
                    }
                }
            }
        }

        public void OpenTechLab()
        {
            if (techLabModal != null)
            {
                techLabModal.SetActive(true);
                UpdateUI();
                RefreshNodeButtons();
            }
        }

        public void CloseTechLab()
        {
            if (techLabModal != null)
            {
                techLabModal.SetActive(false);
            }
            if (LobbyManager.Instance != null)
            {
                LobbyManager.Instance.UpdateDnaDisplay();
            }
        }

        public void UpdateUI()
        {
            int currentDna = PlayerPrefs.GetInt("ANT_COLONY_DNA", 0);
            if (availableDnaText != null)
            {
                availableDnaText.text = $"{currentDna} 🧬";
            }
        }
    }
}
