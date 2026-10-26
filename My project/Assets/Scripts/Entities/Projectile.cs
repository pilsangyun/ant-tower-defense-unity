using UnityEngine;
using AntTD.Entities;

namespace AntTD.Entities
{
    public class Projectile : MonoBehaviour
    {
        public float speed = 10f;
        public float damage = 10f;
        public string damageType = "physical"; // "physical", "fire", "acid"
        public float splashRadius = 0f;
        public float corrosionAmount = 0f;

        private Enemy targetEnemy;
        private Vector2 targetPosition;

        public void Seek(Enemy target, float dmg, string dmgType, float splash = 0f, float corrosion = 0f)
        {
            targetEnemy = target;
            damage = dmg;
            damageType = dmgType;
            splashRadius = splash;
            corrosionAmount = corrosion;
            if (target != null) targetPosition = target.transform.position;
        }

        private void Update()
        {
            if (targetEnemy != null && !targetEnemy.isDead)
            {
                targetPosition = targetEnemy.transform.position;
            }

            Vector2 dir = (targetPosition - (Vector2)transform.position);
            float distThisFrame = speed * Time.deltaTime;

            if (dir.magnitude <= distThisFrame)
            {
                HitTarget();
                return;
            }

            transform.Translate(dir.normalized * distThisFrame, Space.World);
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        private void HitTarget()
        {
            if (splashRadius > 0f)
            {
                Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, splashRadius);
                foreach (var hit in hits)
                {
                    Enemy e = hit.GetComponent<Enemy>();
                    if (e != null && !e.isDead && !e.isBurrowed)
                    {
                        e.TakeDamage(damage, damageType, false);
                        if (corrosionAmount > 0f) e.ApplyCorrosion(corrosionAmount);
                    }
                }
            }
            else if (targetEnemy != null && !targetEnemy.isDead)
            {
                targetEnemy.TakeDamage(damage, damageType, false);
                if (corrosionAmount > 0f) targetEnemy.ApplyCorrosion(corrosionAmount);
            }

            Destroy(gameObject);
        }
    }
}
