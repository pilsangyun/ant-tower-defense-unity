using System;
using System.Collections.Generic;
using UnityEngine;
using AntTD.Data;

namespace AntTD.Entities
{
    public class Enemy : MonoBehaviour
    {
        public EnemyData data;
        public float currentHp { get; private set; }
        public float maxHp { get; private set; }
        public float currentSpeed { get; private set; }
        public bool isDead { get; private set; }
        public bool isBurrowed { get; private set; }
        public bool isBlocked { get; private set; }
        public Tower blockedBy { get; private set; }
        public float pathProgress => currentWaypointIndex + (waypoints != null && currentWaypointIndex < waypoints.Count ? (1f / (1f + Vector2.Distance(transform.position, waypoints[currentWaypointIndex]))) : 0f);

        private List<Vector2> waypoints;
        private int currentWaypointIndex = 0;
        private float slowTimer = 0f;
        private float slowRatio = 1f;
        private float stunTimer = 0f;
        private float corrosionAmount = 0f;
        private float baseScaleX = 0.6f;

        // Visual HP bar
        private GameObject hpBarBg;
        private GameObject hpBarFill;

        public event Action<Enemy> OnEnemyDied;
        public event Action<Enemy> OnEnemyReachedQueen;

        public void Initialize(EnemyData enemyData, float hpMultiplier, float speedMultiplier, List<Vector2> path)
        {
            data = enemyData;
            maxHp = Mathf.Round(data.baseHp * hpMultiplier);
            currentHp = maxHp;
            currentSpeed = data.baseSpeed * speedMultiplier;
            waypoints = path;
            currentWaypointIndex = 0;
            isDead = false;
            isBlocked = false;

            baseScaleX = Mathf.Abs(transform.localScale.x);

            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
            if (data != null && data.sprite != null)
            {
                sr.sprite = data.sprite;
                sr.sortingOrder = 5;
            }

            CreateMiniHpBar();

            if (waypoints != null && waypoints.Count > 0)
            {
                transform.position = waypoints[0];
            }
        }

        private void CreateMiniHpBar()
        {
            if (hpBarBg != null) return;

            hpBarBg = new GameObject("HpBarBg");
            hpBarBg.transform.SetParent(transform);
            hpBarBg.transform.localPosition = new Vector3(0, 0.45f, 0);

            SpriteRenderer bgSr = hpBarBg.AddComponent<SpriteRenderer>();
            bgSr.sprite = CreatePixelSprite();
            bgSr.color = new Color(0.1f, 0.1f, 0.1f, 0.85f);
            bgSr.sortingOrder = 20;
            hpBarBg.transform.localScale = new Vector3(0.55f, 0.08f, 1f);

            hpBarFill = new GameObject("HpBarFill");
            hpBarFill.transform.SetParent(hpBarBg.transform);
            hpBarFill.transform.localPosition = Vector3.zero;

            SpriteRenderer fillSr = hpBarFill.AddComponent<SpriteRenderer>();
            fillSr.sprite = CreatePixelSprite();
            fillSr.color = (data != null && data.isBoss) ? new Color(0.95f, 0.2f, 0.2f) : new Color(0.9f, 0.25f, 0.25f);
            fillSr.sortingOrder = 21;
            hpBarFill.transform.localScale = Vector3.one;
        }

        private Sprite CreatePixelSprite()
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        }

        private void Update()
        {
            if (isDead) return;

            if (stunTimer > 0f)
            {
                stunTimer -= Time.deltaTime;
                return;
            }

            if (slowTimer > 0f)
            {
                slowTimer -= Time.deltaTime;
                if (slowTimer <= 0f) slowRatio = 1f;
            }

            if (isBlocked) return;

            MoveAlongPath();
            UpdateHpBar();
        }

        private void MoveAlongPath()
        {
            if (waypoints == null || currentWaypointIndex >= waypoints.Count) return;

            Vector2 target = waypoints[currentWaypointIndex];
            Vector2 currentPos = transform.position;
            Vector2 dir = target - currentPos;

            // Flip facing direction
            if (Mathf.Abs(dir.x) > 0.02f)
            {
                float sign = (dir.x < 0) ? -1f : 1f;
                transform.localScale = new Vector3(sign * baseScaleX, transform.localScale.y, 1f);
                if (hpBarBg != null)
                {
                    hpBarBg.transform.localScale = new Vector3(sign * Mathf.Abs(hpBarBg.transform.localScale.x), hpBarBg.transform.localScale.y, 1f);
                }
            }

            float step = currentSpeed * slowRatio * Time.deltaTime;
            transform.position = Vector2.MoveTowards(currentPos, target, step);

            if (Vector2.Distance(transform.position, target) < 0.05f)
            {
                currentWaypointIndex++;
                if (currentWaypointIndex >= waypoints.Count)
                {
                    ReachQueen();
                }
            }
        }

        private void UpdateHpBar()
        {
            if (hpBarFill == null) return;
            float ratio = Mathf.Clamp01(currentHp / maxHp);
            hpBarFill.transform.localScale = new Vector3(ratio, 1f, 1f);
            hpBarFill.transform.localPosition = new Vector3((ratio - 1f) * 0.5f, 0, 0);
        }

        public void TakeDamage(float amount, string damageType, bool isCrit)
        {
            if (isDead) return;

            float effectiveArmor = Mathf.Max(0f, data.baseArmor * (1f - corrosionAmount));
            float damageAfterArmor = (damageType == "acid" || damageType == "magic") ? amount : amount * (1f - effectiveArmor);

            currentHp -= damageAfterArmor;
            UpdateHpBar();

            if (currentHp <= 0f)
            {
                Die();
            }
        }

        public void ApplySlow(float duration, float ratio)
        {
            slowTimer = Mathf.Max(slowTimer, duration);
            slowRatio = Mathf.Min(slowRatio, ratio);
        }

        public void ApplyStun(float duration)
        {
            stunTimer = Mathf.Max(stunTimer, duration);
        }

        public void ApplyCorrosion(float amount)
        {
            corrosionAmount = Mathf.Clamp01(corrosionAmount + amount);
        }

        public void SetBlocked(Tower soldier)
        {
            isBlocked = (soldier != null);
            blockedBy = soldier;
        }

        private void Die()
        {
            if (isDead) return;
            isDead = true;
            if (blockedBy != null)
            {
                blockedBy.OnBlockedEnemyRemoved(this);
            }
            OnEnemyDied?.Invoke(this);
            Destroy(gameObject);
        }

        private void ReachQueen()
        {
            if (isDead) return;
            isDead = true;
            OnEnemyReachedQueen?.Invoke(this);
            Destroy(gameObject);
        }
    }
}
