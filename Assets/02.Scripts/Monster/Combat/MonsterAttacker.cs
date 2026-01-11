using System;
using Combat.Attack;
using Combat.Core;
using Combat.Damage;
using Monster.Data;
using UnityEngine;

namespace Monster.Combat
{
    /// <summary>
    /// 몬스터의 공격을 관리하는 컴포넌트.
    /// Combat 시스템의 HitboxTrigger를 사용하여 데미지를 전달합니다.
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
        private MonsterData _monsterData;
        private AttackContext _currentAttackContext;
        private bool _isInitialized;

        public event Action<IDamageable, DamageInfo> OnHit;

        public void Initialize(MonsterData monsterData)
        {
            _combatant = GetComponent<Combatant>();
            _monsterData = monsterData;

            if (_hitbox != null)
            {
                _hitbox.OnHit += HandleHit;
            }

            _isInitialized = true;
        }

        private void Start()
        {
            // Combatant.Awake()가 완료된 후 Stats 동기화
            SyncStatsFromMonsterData();
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

            // MonsterData의 AttackDamage가 Combatant.Stats에 동기화되어 있으므로
            // AttackContext.Scaled를 사용하면 자동으로 MonsterData 값이 적용됨
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

        /// <summary>
        /// MonsterData의 스탯을 Combatant.Stats에 동기화합니다.
        /// </summary>
        private void SyncStatsFromMonsterData()
        {
            if (_combatant == null || _monsterData == null) return;

            // Combatant.Stats.AttackDamage.BaseValue는 setter가 있어서 런타임에 변경 가능
            _combatant.Stats.AttackDamage.BaseValue = _monsterData.AttackDamage;

            // 몬스터는 기본적으로 크리티컬 없음
            _combatant.Stats.CriticalChance.BaseValue = 0f;
            _combatant.Stats.CriticalMultiplier.BaseValue = 1f;

            // Defense는 필요시 MonsterData에 추가
            _combatant.Stats.Defense.BaseValue = 0f;
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
