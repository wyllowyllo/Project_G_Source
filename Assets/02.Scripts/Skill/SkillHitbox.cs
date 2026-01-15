using System;
using System.Collections.Generic;
using Combat.Core;
using Combat.Damage;
using UnityEngine;

namespace Skill
{
    public class SkillHitbox : MonoBehaviour
    {
        private const int BufferSize = 32;
        private static readonly Collider[] _hitBuffer = new Collider[BufferSize];

        private readonly HashSet<IDamageable> _hitTargets = new HashSet<IDamageable>();

        public event Action<HitInfo> OnHit;

        public int PerformCheck(SkillAreaContext context)
        {
            _hitTargets.Clear();

            int hitCount = GetTargetsInArea(context);
            int validHits = 0;

            for (int i = 0; i < hitCount; i++)
            {
                var collider = _hitBuffer[i];

                if (!TryGetValidTarget(collider, context.AttackerTeam, out var damageable, out var targetCombatant))
                    continue;

                _hitTargets.Add(damageable);

                var targetHealth = collider.GetComponent<IHealthProvider>();
                var hitInfo = new HitInfo(damageable, targetCombatant, targetHealth, collider);

                OnHit?.Invoke(hitInfo);
                validHits++;
            }

            return validHits;
        }

        private int GetTargetsInArea(SkillAreaContext context)
        {
            Vector3 origin = transform.position + transform.TransformDirection(context.PositionOffset);

            return context.AreaType switch
            {
                SkillAreaType.Sphere => GetTargetsInSphere(origin, context.Range, context.EnemyLayer),
                SkillAreaType.Box => GetTargetsInBox(origin, context.Range, context.BoxWidth, context.BoxHeight, context.EnemyLayer),
                SkillAreaType.Cone => GetTargetsInCone(origin, context.Range, context.Angle, context.EnemyLayer),
                _ => 0
            };
        }

        private int GetTargetsInSphere(Vector3 origin, float range, LayerMask enemyLayer)
        {
            return Physics.OverlapSphereNonAlloc(origin, range, _hitBuffer, enemyLayer);
        }

        private int GetTargetsInBox(Vector3 origin, float range, float boxWidth, float boxHeight, LayerMask enemyLayer)
        {
            Vector3 center = origin + transform.forward * (range * 0.5f);
            Vector3 halfExtents = new Vector3(boxWidth * 0.5f, boxHeight * 0.5f, range * 0.5f);
            return Physics.OverlapBoxNonAlloc(
                center, halfExtents, _hitBuffer, transform.rotation, enemyLayer);
        }

        private int GetTargetsInCone(Vector3 origin, float range, float angle, LayerMask enemyLayer)
        {
            int count = Physics.OverlapSphereNonAlloc(origin, range, _hitBuffer, enemyLayer);

            Vector3 forward = transform.forward;
            float cosHalfAngle = Mathf.Cos(angle * 0.5f * Mathf.Deg2Rad);

            int validCount = 0;
            for (int i = 0; i < count; i++)
            {
                Vector3 dirToTarget = (_hitBuffer[i].transform.position - origin).normalized;
                float dot = Vector3.Dot(forward, dirToTarget);

                if (dot >= cosHalfAngle)
                {
                    _hitBuffer[validCount++] = _hitBuffer[i];
                }
            }
            return validCount;
        }

        private bool TryGetValidTarget(
            Collider other,
            CombatTeam attackerTeam,
            out IDamageable damageable,
            out ICombatant targetCombatant)
        {
            damageable = null;
            targetCombatant = null;

            damageable = other.GetComponent<IDamageable>();
            if (damageable == null) return false;

            if (_hitTargets.Contains(damageable)) return false;

            targetCombatant = other.GetComponent<ICombatant>();
            if (targetCombatant != null && targetCombatant.IsAlly(attackerTeam)) return false;

            if (!damageable.CanTakeDamage) return false;

            return true;
        }

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _showGizmos;
        [SerializeField] private SkillAreaType _debugAreaType;
        [SerializeField] private float _debugRange = 5f;
        [SerializeField] private float _debugAngle = 180f;
        [SerializeField] private float _debugBoxWidth = 2f;
        [SerializeField] private float _debugBoxHeight = 2f;

        private void OnDrawGizmosSelected()
        {
            if (!_showGizmos) return;

            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);

            switch (_debugAreaType)
            {
                case SkillAreaType.Sphere:
                    Gizmos.DrawWireSphere(transform.position, _debugRange);
                    break;

                case SkillAreaType.Box:
                    Gizmos.matrix = Matrix4x4.TRS(
                        transform.position + transform.forward * (_debugRange * 0.5f),
                        transform.rotation,
                        Vector3.one);
                    Gizmos.DrawWireCube(Vector3.zero, new Vector3(_debugBoxWidth, _debugBoxHeight, _debugRange));
                    Gizmos.matrix = Matrix4x4.identity;
                    break;

                case SkillAreaType.Cone:
                    DrawConeGizmo(_debugRange, _debugAngle);
                    break;
            }
        }

        private void DrawConeGizmo(float range, float angle)
        {
            int segments = 20;
            float halfAngle = angle * 0.5f * Mathf.Deg2Rad;

            Vector3 origin = transform.position;
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;

            for (int i = 0; i < segments; i++)
            {
                float t1 = (float)i / segments;
                float t2 = (float)(i + 1) / segments;

                float a1 = Mathf.Lerp(-halfAngle, halfAngle, t1);
                float a2 = Mathf.Lerp(-halfAngle, halfAngle, t2);

                Vector3 dir1 = (forward * Mathf.Cos(a1) + right * Mathf.Sin(a1)).normalized;
                Vector3 dir2 = (forward * Mathf.Cos(a2) + right * Mathf.Sin(a2)).normalized;

                Gizmos.DrawLine(origin, origin + dir1 * range);
                Gizmos.DrawLine(origin + dir1 * range, origin + dir2 * range);
            }
        }
#endif
    }
}
