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
        [SerializeField] private GameObject _beamPrefab;
        [SerializeField] private GameObject _impactPrefab;

        [Header("Settings")]
        [SerializeField] private LayerMask _targetLayers;
        [SerializeField] private LayerMask _obstacleLayer;
        [SerializeField] private float _tickInterval = 0.5f;
        [SerializeField] private float _beamRadius = 0.5f;

        private Combatant _combatant;
        private bool _isBreathing;
        private float _range;
        private float _damagePerTick;
        private float _tickTimer;

        private GameObject _beamInstance;
        private GameObject _impactInstance;
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

        public void StartBreath(float range, float damage)
        {
            _range = range;
            _damagePerTick = damage * _tickInterval;
            _tickTimer = 0f;
            _isBreathing = true;

            if (_beamPrefab != null)
            {
                _beamInstance = Instantiate(_beamPrefab, _breathOrigin.position, _breathOrigin.rotation, _breathOrigin);
            }
        }

        public void StopBreath()
        {
            _isBreathing = false;

            if (_beamInstance != null)
            {
                Destroy(_beamInstance);
                _beamInstance = null;
            }

            if (_impactInstance != null)
            {
                Destroy(_impactInstance);
                _impactInstance = null;
            }
        }

        private void ProcessBreathDamage()
        {
            _hitTargetsThisTick.Clear();

            Vector3 origin = _breathOrigin.position;
            Vector3 direction = _breathOrigin.forward;
            float beamLength = _range;

            // 장애물 체크 - 빔 길이 제한
            if (Physics.Raycast(origin, direction, out RaycastHit obstacleHit, _range, _obstacleLayer))
            {
                beamLength = obstacleHit.distance;
                UpdateImpactEffect(obstacleHit.point);
            }
            else
            {
                UpdateImpactEffect(null);
            }

            // 전방으로 SphereCast하여 대상 검색
            RaycastHit[] hits = Physics.SphereCastAll(origin, _beamRadius, direction, beamLength, _targetLayers);

            foreach (var hit in hits)
            {
                var damageable = hit.collider.GetComponentInParent<IDamageable>();
                if (damageable == null || !damageable.CanTakeDamage) continue;
                if (_hitTargetsThisTick.Contains(damageable)) continue;

                var combatant = hit.collider.GetComponentInParent<ICombatant>();
                if (combatant == null) continue;

                // 같은 팀이면 무시
                if (_combatant != null && combatant.Team == _combatant.Team) continue;

                _hitTargetsThisTick.Add(damageable);
                ApplyBreathDamage(damageable, hit.point);
            }
        }

        private void ApplyBreathDamage(IDamageable target, Vector3 hitPoint)
        {
            var hitContext = HitContext.FromCollision(
                hitPoint,
                -_breathOrigin.forward,
                DamageType.Skill
            );

            var damageInfo = new DamageInfo(_damagePerTick, false, hitContext);
            target.TakeDamage(damageInfo);
        }

        private void UpdateImpactEffect(Vector3? hitPoint)
        {
            if (!hitPoint.HasValue)
            {
                if (_impactInstance != null)
                {
                    _impactInstance.SetActive(false);
                }
                return;
            }

            if (_impactPrefab == null) return;

            if (_impactInstance == null)
            {
                _impactInstance = Instantiate(_impactPrefab);
            }

            _impactInstance.SetActive(true);
            _impactInstance.transform.position = hitPoint.Value;
            _impactInstance.transform.rotation = Quaternion.LookRotation(-_breathOrigin.forward);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_breathOrigin == null) return;

            Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);

            Vector3 origin = _breathOrigin.position;
            Vector3 endPoint = origin + _breathOrigin.forward * _range;

            // 빔 중심선
            Gizmos.DrawLine(origin, endPoint);

            // 빔 반경 표시
            Gizmos.DrawWireSphere(origin, _beamRadius);
            Gizmos.DrawWireSphere(endPoint, _beamRadius);
        }
#endif
    }
}
