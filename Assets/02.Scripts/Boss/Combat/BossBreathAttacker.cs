using System.Collections.Generic;
using Combat.Core;
using Combat.Damage;
using UnityEngine;

namespace Boss.Combat
{
    /// <summary>
    /// 보스 브레스 공격 처리기
    /// 부채꼴 범위 내 대상에게 지속 데미지 적용
    /// </summary>
    public class BossBreathAttacker : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform _breathOrigin;
        [SerializeField] private ParticleSystem _breathEffect;

        [Header("Settings")]
        [SerializeField] private LayerMask _targetLayers;
        [SerializeField] private float _tickInterval = 0.5f;

        private Combatant _combatant;
        private bool _isBreathing;
        private float _angle;
        private float _range;
        private float _damagePerTick;
        private float _tickTimer;

        private HashSet<IDamageable> _hitTargetsThisTick = new();

        public bool IsBreathing => _isBreathing;

        private void Awake()
        {
            _combatant = GetComponentInParent<Combatant>();

            if (_breathOrigin == null)
            {
                _breathOrigin = transform;
            }
        }

        private void Update()
        {
            if (!_isBreathing) return;

            _tickTimer += Time.deltaTime;
            if (_tickTimer >= _tickInterval)
            {
                _tickTimer = 0f;
                ProcessBreathDamage();
            }
        }

        public void StartBreath(float angle, float range, float damage)
        {
            _angle = angle;
            _range = range;
            _damagePerTick = damage * _tickInterval; // 틱당 데미지
            _tickTimer = 0f;
            _isBreathing = true;

            // 이펙트 시작
            if (_breathEffect != null)
            {
                _breathEffect.Play();
            }
        }

        public void StopBreath()
        {
            _isBreathing = false;

            // 이펙트 종료
            if (_breathEffect != null)
            {
                _breathEffect.Stop();
            }
        }

        private void ProcessBreathDamage()
        {
            _hitTargetsThisTick.Clear();

            // 범위 내 대상 검색
            Collider[] colliders = Physics.OverlapSphere(_breathOrigin.position, _range, _targetLayers);

            foreach (var col in colliders)
            {
                // 각도 체크
                Vector3 directionToTarget = (col.transform.position - _breathOrigin.position).normalized;
                float angleToTarget = Vector3.Angle(_breathOrigin.forward, directionToTarget);

                if (angleToTarget > _angle * 0.5f) continue;

                // 데미지 처리
                var damageable = col.GetComponentInParent<IDamageable>();
                if (damageable == null || !damageable.CanTakeDamage) continue;
                if (_hitTargetsThisTick.Contains(damageable)) continue;

                var combatant = col.GetComponentInParent<ICombatant>();
                if (combatant == null) continue;

                // 같은 팀이면 무시
                if (_combatant != null && combatant.Team == _combatant.Team) continue;

                _hitTargetsThisTick.Add(damageable);
                ApplyBreathDamage(damageable, col);
            }
        }

        private void ApplyBreathDamage(IDamageable target, Collider collider)
        {
            var hitContext = HitContext.FromCollision(
                collider.ClosestPoint(_breathOrigin.position),
                (_breathOrigin.position - collider.transform.position).normalized,
                DamageType.Skill // 브레스는 화염 데미지
            );

            var damageInfo = new DamageInfo(_damagePerTick, false, hitContext);
            target.TakeDamage(damageInfo);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_breathOrigin == null) return;

            // 브레스 범위 시각화
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);

            Vector3 origin = _breathOrigin.position;
            Vector3 forward = _breathOrigin.forward;
            float halfAngle = _angle * 0.5f;

            // 부채꼴 그리기
            int segments = 20;
            Vector3 prevPoint = origin;
            for (int i = 0; i <= segments; i++)
            {
                float currentAngle = -halfAngle + (halfAngle * 2f * i / segments);
                Vector3 direction = Quaternion.Euler(0, currentAngle, 0) * forward;
                Vector3 point = origin + direction * _range;

                if (i > 0)
                {
                    Gizmos.DrawLine(prevPoint, point);
                }
                Gizmos.DrawLine(origin, point);
                prevPoint = point;
            }
        }
#endif
    }
}
