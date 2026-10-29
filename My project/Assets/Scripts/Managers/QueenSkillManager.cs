using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using AntTD.Entities;

namespace AntTD.Managers
{
    public class QueenSkillManager : MonoBehaviour
    {
        public static QueenSkillManager Instance { get; private set; }

        [Header("Skill Buttons (Queen Q/W/E)")]
        public Button swarmCallBtn;
        public Image swarmCdOverlay;
        public Text swarmCdText;

        public Button acidRainBtn;
        public Image acidCdOverlay;
        public Text acidCdText;

        public Button honeyTrapBtn;
        public Image honeyCdOverlay;
        public Text honeyCdText;

        [Header("Tactical Buttons (R/F/G)")]
        public Button cleanseBtn;
        public Image cleanseCdOverlay;
        public Button overchargeBtn;
        public Image overchargeCdOverlay;
        public Button empowerBtn;
        public Text empowerCostText;

        [Header("Cooldowns & Timers")]
        public float swarmCdMax = 35f;
        public float acidCdMax = 40f;
        public float honeyCdMax = 30f;
        public float cleanseCdMax = 45f;
        public float overchargeCdMax = 60f;

        private float swarmTimer = 0f;
        private float acidTimer = 0f;
        private float honeyTimer = 0f;
        private float cleanseTimer = 0f;
        private float overchargeTimer = 0f;

        [Header("Empower Stacks")]
        public int empowerStacks = 0;
        public int baseEmpowerCost = 350;
        public float currentGlobalDamageBonus => 1f + (empowerStacks * 0.15f);

        // Active Buffs
        public float swarmBuffDuration = 0f;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            BindSkillButtons();
            UpdateEmpowerCostUI();
        }

        private void Update()
        {
            UpdateCooldowns();
            HandleKeyboardShortcuts();
        }

        private void BindSkillButtons()
        {
            if (swarmCallBtn != null) swarmCallBtn.onClick.AddListener(UseSwarmCall);
            if (acidRainBtn != null) acidRainBtn.onClick.AddListener(UseAcidRain);
            if (honeyTrapBtn != null) honeyTrapBtn.onClick.AddListener(UseHoneyTrap);

            if (cleanseBtn != null) cleanseBtn.onClick.AddListener(UseCleanse);
            if (overchargeBtn != null) overchargeBtn.onClick.AddListener(UseOvercharge);
            if (empowerBtn != null) empowerBtn.onClick.AddListener(UseEmpower);
        }

        private void HandleKeyboardShortcuts()
        {
            if (Keyboard.current == null) return;

            if (Keyboard.current.qKey.wasPressedThisFrame) UseSwarmCall();
            if (Keyboard.current.wKey.wasPressedThisFrame) UseAcidRain();
            if (Keyboard.current.eKey.wasPressedThisFrame) UseHoneyTrap();
            if (Keyboard.current.rKey.wasPressedThisFrame) UseCleanse();
            if (Keyboard.current.fKey.wasPressedThisFrame || Keyboard.current.cKey.wasPressedThisFrame) UseOvercharge();
            if (Keyboard.current.gKey.wasPressedThisFrame) UseEmpower();
        }

        private void UpdateCooldowns()
        {
            float dt = Time.deltaTime;

            if (swarmTimer > 0f) swarmTimer -= dt;
            if (acidTimer > 0f) acidTimer -= dt;
            if (honeyTimer > 0f) honeyTimer -= dt;
            if (cleanseTimer > 0f) cleanseTimer -= dt;
            if (overchargeTimer > 0f) overchargeTimer -= dt;

            if (swarmBuffDuration > 0f) swarmBuffDuration -= dt;

            UpdateOverlay(swarmCdOverlay, swarmCdText, swarmTimer, swarmCdMax);
            UpdateOverlay(acidCdOverlay, acidCdText, acidTimer, acidCdMax);
            UpdateOverlay(honeyCdOverlay, honeyCdText, honeyTimer, honeyCdMax);
            UpdateOverlay(cleanseCdOverlay, null, cleanseTimer, cleanseCdMax);
            UpdateOverlay(overchargeCdOverlay, null, overchargeTimer, overchargeCdMax);
        }

        private void UpdateOverlay(Image overlay, Text cdText, float current, float max)
        {
            if (overlay != null)
            {
                overlay.gameObject.SetActive(current > 0f);
                overlay.fillAmount = Mathf.Clamp01(current / max);
            }
            if (cdText != null)
            {
                cdText.gameObject.SetActive(current > 0f);
                if (current > 0f) cdText.text = Mathf.CeilToInt(current).ToString();
            }
        }

        // --- Skill Implementations ---

        public void UseSwarmCall()
        {
            if (swarmTimer > 0f) return;
            swarmTimer = swarmCdMax;
            swarmBuffDuration = 8f;

            Debug.Log("⚡ [QueenSkill] Swarm Call (Q) Activated! All ants AttackSpeed +40% for 8s.");
        }

        public void UseAcidRain()
        {
            if (acidTimer > 0f) return;
            acidTimer = acidCdMax;

            if (WaveManager.Instance != null && WaveManager.Instance.activeEnemies != null)
            {
                foreach (var enemy in WaveManager.Instance.activeEnemies)
                {
                    if (enemy != null && !enemy.isDead)
                    {
                        enemy.TakeDamage(150f, isAcid: true);
                    }
                }
            }

            Debug.Log("🧪 [QueenSkill] Acid Rain (W) Activated! All enemies drenched in acid.");
        }

        public void UseHoneyTrap()
        {
            if (honeyTimer > 0f) return;
            honeyTimer = honeyCdMax;

            if (WaveManager.Instance != null && WaveManager.Instance.activeEnemies != null)
            {
                foreach (var enemy in WaveManager.Instance.activeEnemies)
                {
                    if (enemy != null && !enemy.isDead)
                    {
                        enemy.ApplySlow(4f, 0.05f); // 95% slow (0.05 speed ratio) for 4 seconds
                    }
                }
            }

            Debug.Log("🍯 [QueenSkill] Royal Honey Trap (E) Activated! All enemies immobilized.");
        }

        public void UseCleanse()
        {
            if (cleanseTimer > 0f) return;
            if (GameManager.Instance == null || !GameManager.Instance.SpendSugar(120)) return;

            cleanseTimer = cleanseCdMax;

            // Remove webs from all towers
            var towers = FindObjectsByType<Tower>(FindObjectsInactive.Exclude);
            foreach (var t in towers)
            {
                if (t != null) t.RemoveWeb();
            }

            Debug.Log("💨 [Tactical] Cleanse (R) Activated! All webs cleared.");
        }

        public void UseOvercharge()
        {
            if (overchargeTimer > 0f) return;
            if (GameManager.Instance == null || !GameManager.Instance.SpendSugar(200)) return;

            overchargeTimer = overchargeCdMax;

            // Heal queen +3
            GameManager.Instance.queenHp = Mathf.Min(GameManager.Instance.maxQueenHp, GameManager.Instance.queenHp + 3);

            // Cut Q/W/E cooldowns by 50%
            swarmTimer *= 0.5f;
            acidTimer *= 0.5f;
            honeyTimer *= 0.5f;

            Debug.Log("👑 [Tactical] Royal Overcharge (F) Activated! Queen HP +3, All Skill CD -50%.");
        }

        public void UseEmpower()
        {
            int cost = baseEmpowerCost + (empowerStacks * 100);
            if (GameManager.Instance == null || !GameManager.Instance.SpendSugar(cost)) return;

            empowerStacks++;
            UpdateEmpowerCostUI();

            Debug.Log($"⚔️ [Tactical] Army Empower (G) Activated! Stack #{empowerStacks} (+{empowerStacks * 15}% Permanent Damage).");
        }

        private void UpdateEmpowerCostUI()
        {
            if (empowerCostText != null)
            {
                int cost = baseEmpowerCost + (empowerStacks * 100);
                empowerCostText.text = $"🍬{cost}";
            }
        }
    }
}
