using System.Collections.Generic;
using UnityEngine;
using AntTD.Data;
using AntTD.Managers;

namespace AntTD.Entities
{
    public class Tower : MonoBehaviour
    {
        public TowerData data;
        public int level = 1;
        public string branch = null; // "A" or "B"
        public int overclockLevel = 0;
        public float webDuration = 0f;

        [Header("Runtime Stats")]
        public float currentRange;
        public float currentDamage;
        public float currentAttackSpeed;
        public int maxBlockCount;

        [Header("Projectile Setting")]
        public GameObject projectilePrefab;

        private SpriteRenderer spriteRenderer;
        private float attackTimer = 0f;
        private float workerSugarTimer = 0f;
        private List<Enemy> blockedEnemies = new List<Enemy>();

        public void Initialize(TowerData towerData)
        {
            data = towerData;
            level = 1;
            branch = null;
            overclockLevel = 0;
            webDuration = 0f;

            currentRange = data.baseRange;
            currentDamage = data.baseDamage;
            currentAttackSpeed = data.baseAttackSpeed;
            maxBlockCount = data.baseBlockCount;

            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null) spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

            if (data != null && data.icon != null)
            {
                spriteRenderer.sprite = data.icon;
                spriteRenderer.sortingOrder = 10;
            }

            transform.localScale = new Vector3(0.60f, 0.60f, 1f);
        }

        private void Update()
        {
            if (webDuration > 0f)
            {
                webDuration -= Time.deltaTime;
                return;
            }

            if (attackTimer > 0f)
            {
                attackTimer -= Time.deltaTime;
            }

            if (data == null) return;

            if (data.role == AntRole.MeleeBlock)
            {
                UpdateSoldierBlocking();
            }
            else if (data.role == AntRole.ResourceSupport)
            {
                UpdateWorkerProduction();
            }
            else if (data.role == AntRole.DebuffSupport)
            {
                UpdatePheromoneDebuff();
            }
            else if (data.role == AntRole.SpecialNuke)
            {
                UpdateExploderNuke();
            }
            else if (attackTimer <= 0f)
            {
                AttackTarget();
            }
        }

        private GameObject levelBadgeObj;
        private GameObject evolutionAuraObj;

        private void AttackTarget()
        {
            Enemy target = null;
            float maxProgress = -1f;

            if (WaveManager.Instance != null && WaveManager.Instance.activeEnemies != null)
            {
                var enemies = WaveManager.Instance.activeEnemies;
                for (int i = 0; i < enemies.Count; i++)
                {
                    var enemy = enemies[i];
                    if (enemy == null || enemy.isDead || enemy.isBurrowed) continue;
                    float dist = Vector2.Distance(transform.position, enemy.transform.position);
                    if (dist <= currentRange)
                    {
                        if (enemy.pathProgress > maxProgress)
                        {
                            maxProgress = enemy.pathProgress;
                            target = enemy;
                        }
                    }
                }
            }

            if (target != null)
            {
                attackTimer = (currentAttackSpeed > 0f) ? (1f / currentAttackSpeed) : 1f;

                // Face target (flip X)
                if (Mathf.Abs(target.transform.position.x - transform.position.x) > 0.05f)
                {
                    float sign = (target.transform.position.x < transform.position.x) ? -1f : 1f;
                    transform.localScale = new Vector3(sign * Mathf.Abs(transform.localScale.x), transform.localScale.y, 1f);
                }

                if (projectilePrefab != null)
                {
                    GameObject projObj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
                    Projectile proj = projObj.GetComponent<Projectile>();
                    if (proj != null)
                    {
                        string dmgType = (data.role == AntRole.RangedCorrosion) ? "acid" : "fire";
                        float splash = (data.role == AntRole.RangedCorrosion) ? (branch == "A" ? 1.2f : 0.8f) : 0f;
                        float corr = (data.role == AntRole.RangedCorrosion) ? (branch == "A" ? 0.45f : 0.25f) : 0f;
                        if (branch == "A" && data.towerId == "fire") dmgType = "toxic_fire";
                        proj.Seek(target, currentDamage, dmgType, splash, corr);
                    }
                }
                else
                {
                    target.TakeDamage(currentDamage, (data.role == AntRole.RangedCorrosion) ? "acid" : "physical", false);
                }
            }
        }

        private void UpdateSoldierBlocking()
        {
            blockedEnemies.RemoveAll(e => e == null || e.isDead);

            if (blockedEnemies.Count < maxBlockCount && WaveManager.Instance != null && WaveManager.Instance.activeEnemies != null)
            {
                var enemies = WaveManager.Instance.activeEnemies;
                for (int i = 0; i < enemies.Count; i++)
                {
                    var enemy = enemies[i];
                    if (enemy != null && !enemy.isDead && !enemy.isBlocked && !enemy.isBurrowed)
                    {
                        if (Vector2.Distance(transform.position, enemy.transform.position) <= currentRange)
                        {
                            enemy.SetBlocked(this);
                            blockedEnemies.Add(enemy);
                            if (blockedEnemies.Count >= maxBlockCount) break;
                        }
                    }
                }
            }

            if (attackTimer <= 0f && blockedEnemies.Count > 0)
            {
                attackTimer = (currentAttackSpeed > 0f) ? (1f / currentAttackSpeed) : 1f;
                var target = blockedEnemies[0];
                if (target != null && !target.isDead)
                {
                    target.TakeDamage(currentDamage, "physical", false);
                }
            }
        }

        private void UpdateWorkerProduction()
        {
            workerSugarTimer += Time.deltaTime;
            float interval = (branch == "A") ? 4.0f : 3.0f;

            if (workerSugarTimer >= interval)
            {
                workerSugarTimer = 0f;
                int sugarAmt = (level == 1) ? 8 : ((level == 2) ? 14 : ((branch == "A") ? 26 : 16));
                GameManager.Instance.AddSugar(sugarAmt);

                if (branch == "A")
                {
                    GameManager.Instance.AddProtein(1);
                }
            }
        }

        private void UpdatePheromoneDebuff()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, currentRange);
            float slowRate = (branch == "A") ? 0.55f : ((level == 2) ? 0.65f : 0.75f);

            foreach (var hit in hits)
            {
                Enemy enemy = hit.GetComponent<Enemy>();
                if (enemy != null && !enemy.isDead && !enemy.isBurrowed)
                {
                    enemy.ApplySlow(0.5f, slowRate);
                }
            }
        }

        private void UpdateExploderNuke()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, currentRange);
            bool triggered = false;

            foreach (var hit in hits)
            {
                Enemy enemy = hit.GetComponent<Enemy>();
                if (enemy != null && !enemy.isDead && !enemy.isBurrowed)
                {
                    triggered = true;
                    break;
                }
            }

            if (triggered)
            {
                Collider2D[] blastHits = Physics2D.OverlapCircleAll(transform.position, currentRange * 1.5f);
                foreach (var h in blastHits)
                {
                    Enemy e = h.GetComponent<Enemy>();
                    if (e != null && !e.isDead && !e.isBurrowed)
                    {
                        e.TakeDamage(currentDamage, "physical", true);
                        e.ApplySlow(2.5f, 0.4f);
                    }
                }

                webDuration = 5.0f;
            }
        }

        public void OnBlockedEnemyRemoved(Enemy enemy)
        {
            blockedEnemies.Remove(enemy);
        }

        public void UpgradeToLevel2()
        {
            if (level != 1) return;
            level = 2;
            currentDamage *= 1.5f;
            currentRange *= 1.15f;
            transform.localScale = new Vector3(0.66f, 0.66f, 1f);
            CreateEvolutionAura(new Color(0.95f, 0.75f, 0.2f, 0.35f), 0.6f);
        }

        public void Evolve(string branchKey)
        {
            level = 3;
            branch = branchKey;
            var evo = (branchKey == "A") ? data.branchA : data.branchB;
            if (evo != null)
            {
                if (evo.damage > 0) currentDamage = evo.damage;
                if (evo.range > 0) currentRange = evo.range;
                if (evo.attackSpeed > 0) currentAttackSpeed = evo.attackSpeed;
                if (evo.blockCount > 0) maxBlockCount = evo.blockCount;

                if (evo.evolvedSprite != null && spriteRenderer != null)
                {
                    spriteRenderer.sprite = evo.evolvedSprite;
                }
            }
            transform.localScale = new Vector3(0.74f, 0.74f, 1f);
            Color auraColor = (branchKey == "A") ? new Color(0.95f, 0.35f, 0.2f, 0.5f) : new Color(0.4f, 0.6f, 1f, 0.5f);
            CreateEvolutionAura(auraColor, 0.85f);
        }

        public void ApplyOverclock()
        {
            overclockLevel++;
            currentDamage *= 1.18f;
            currentAttackSpeed *= 1.06f;
            currentRange *= 1.03f;
            transform.localScale = new Vector3(0.74f + overclockLevel * 0.02f, 0.74f + overclockLevel * 0.02f, 1f);
            CreateEvolutionAura(new Color(0.9f, 0.2f, 0.9f, 0.65f), 0.9f + overclockLevel * 0.05f);
        }

        private void CreateEvolutionAura(Color auraColor, float scale)
        {
            if (evolutionAuraObj == null)
            {
                evolutionAuraObj = new GameObject("EvolutionAura");
                evolutionAuraObj.transform.SetParent(transform);
                evolutionAuraObj.transform.localPosition = new Vector3(0, -0.2f, 0);

                SpriteRenderer auraSr = evolutionAuraObj.AddComponent<SpriteRenderer>();
                Texture2D tex = new Texture2D(32, 32);
                Vector2 center = new Vector2(15.5f, 15.5f);
                for (int y = 0; y < 32; y++)
                {
                    for (int x = 0; x < 32; x++)
                    {
                        float d = Vector2.Distance(new Vector2(x, y), center);
                        if (d >= 10f && d <= 15f)
                        {
                            float alpha = 1f - Mathf.Abs(d - 12.5f) / 2.5f;
                            tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                        }
                        else
                        {
                            tex.SetPixel(x, y, Color.clear);
                        }
                    }
                }
                tex.Apply();
                auraSr.sprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 100);
                auraSr.sortingOrder = 8;
            }

            SpriteRenderer sr = evolutionAuraObj.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = auraColor;
            evolutionAuraObj.transform.localScale = new Vector3(scale, scale * 0.65f, 1f);
        }

        public void RemoveWeb()
        {
            webDuration = 0f;
        }
    }
}
