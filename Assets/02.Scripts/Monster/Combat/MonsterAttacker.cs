using System;
using Combat.Attack;
using Combat.Core;
using Combat.Damage;
using UnityEngine;

namespace Monster.Combat
{
    /// <summary>
    /// 몬스터의 공격을 관리하는 컴포넌트.
    /// Combat 시스템의 HitboxTrigger를 사용하여 데미지를 전달합니다.
    ///
    /// 스탯은 Combatant의 CombatStatsData에서 관리합니다.
    /// </summary>
    [RequireComponent(typeof(Combatant))]
    public class MonsterAttacker : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private HitboxTrigger _hitbox;

        [Header("Attack Settings")]
        [SerializeField] private float _lightAttackMultiplier = 1.0f;
        [SerializeField] private float _heavyAttackMultiplier = 1.5f;

        private Combatant _combatant;
        private AttackContext _currentAttackContext;
        private bool _isInitialized;

        public event Action<IDamageable, DamageInfo> OnHit;

        public void Initialize()
        {
            _combatant = GetComponent<Combatant>();

            if (_hitbox != null)
            {
                _hitbox.OnHit += HandleHit;
            }

            _isInitialized = true;
        }

        private void OnDestroy()
        {
            if (_hitbox != null)
            {
                _hitbox.OnHit -= HandleHit;
            }
        }

        /// <summary>
        /// 히트박스를 활성화합니다. Animation Event에서 호출됩니다.
        /// </summary>
        /// <param name="isHeavy">강공 여부</param>
        public void EnableHitbox(bool isHeavy)
        {
            if (!_isInitialized || _hitbox == null) return;

            float multiplier = isHeavy ? _heavyAttackMultiplier : _lightAttackMultiplier;

            // Combatant.Stats는 CombatStatsData에서 초기화됨
            _currentAttackContext = AttackContext.Scaled(
                _combatant,
                baseMultiplier: 1f,
                buffMultiplier: multiplier,
                type: DamageType.Normal
            );

            _hitbox.EnableHitbox(_combatant.Team);
        }

        /// <summary>
        /// 히트박스를 비활성화합니다. Animation Event에서 호출됩니다.
        /// </summary>
        public void DisableHitbox()
        {
            _hitbox?.DisableHitbox();
        }

        private void HandleHit(HitInfo hitInfo)
        {
            var damageInfo = DamageProcessor.Process(
                _currentAttackContext,
                hitInfo,
                transform.position
            );

            hitInfo.Target.TakeDamage(damageInfo);
            OnHit?.Invoke(hitInfo.Target, damageInfo);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_hitbox == null)
            {
                _hitbox = GetComponentInChildren<HitboxTrigger>();
            }
        }
#endif
    }
}
