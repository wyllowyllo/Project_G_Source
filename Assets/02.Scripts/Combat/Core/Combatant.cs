using System;
using Combat.Damage;
using Combat.Data;
using UnityEngine;

namespace Combat.Core
{
    [RequireComponent(typeof(Health))]
    public class Combatant : MonoBehaviour, ICombatant, IDamageable, IHealthProvider
    {
        [SerializeField] private CombatTeam _team;
        [SerializeField] private CombatStatsData _statsData;
        [SerializeField] private HitReactionSettings _hitReactionSettings;

        private Health _health;
        private CombatStats _stats;
        private float _invincibilityEndTime;
        private float _hitStunEndTime;
        private bool _wasInvincible;
        private bool _wasStunned;

        public Transform Transform => transform;
        public CombatStats Stats => _stats;

        public CombatantAttackStats GetAttackStats()
            => new CombatantAttackStats(
                Stats.AttackDamage.Value,
                Stats.CriticalChance.Value,
                Stats.CriticalMultiplier.Value);

        public float GetDefense() => Stats.Defense.Value;

        public bool IsAlly(CombatTeam team) => Team == team;
        public CombatTeam Team => _team;

        public float CurrentHealth => _health.CurrentHealth;
        public float MaxHealth => _health.MaxHealth;

        public bool IsAlive => _health.IsAlive;
        public bool IsInvincible => Time.time < _invincibilityEndTime;
        public bool IsStunned => Time.time < _hitStunEndTime;
        public bool CanTakeDamage => IsAlive && !IsInvincible;

        public event Action<DamageInfo> OnDamaged;
        public event Action OnDeath;
        public event Action OnInvincibilityStart;
        public event Action OnInvincibilityEnd;
        public event Action OnHitStunStart;
        public event Action OnHitStunEnd;

        private void Awake()
        {
            _health = GetComponent<Health>();
            _stats = _statsData != null
                ? CombatStats.FromData(_statsData)
                : new CombatStats(
                    CombatConstants.DefaultAttackDamage, 
                    CombatConstants.DefaultCriticalChance, 
                    CombatConstants.DefaultCriticalMultiplier, 
                    CombatConstants.DefaultDefense
                );
        }

        private void OnEnable()
        {
            if (_health != null)
                _health.OnDeath += HandleDeath;
        }

        private void Update()
        {
            if (!IsAlive) return;

            if (_wasInvincible && !IsInvincible)
            {
                _wasInvincible = false;
                OnInvincibilityEnd?.Invoke();
            }

            if (_wasStunned && !IsStunned)
            {
                _wasStunned = false;
                OnHitStunEnd?.Invoke();
            }
        }

        private void OnDisable()
        {
            if (_health != null)
                _health.OnDeath -= HandleDeath;
        }

        public void TakeDamage(float damage)
        {
            TakeDamage(new DamageInfo(damage, false, new HitContext()));
        }

        public void TakeDamage(DamageInfo damageInfo)
        {
            if (!CanTakeDamage) return;

            _health.TakeDamage(damageInfo.Amount);
            OnDamaged?.Invoke(damageInfo);

            if (_hitReactionSettings != null)
            {
                if (_hitReactionSettings.AutoInvincibilityOnHit)
                    SetInvincible(_hitReactionSettings.InvincibilityDuration);

                if (_hitReactionSettings.AutoHitStunOnHit)
                    SetStunned(_hitReactionSettings.HitStunDuration);
            }
        }

        public void TakeDamage(AttackContext attackContext, HitContext hitContext)
        {
            if (!CanTakeDamage) return;

            var defenderInfo = new DefenderInfo(this, this);
            var result = DamageCalculator.Calculate(attackContext, defenderInfo);
            var damageInfo = new DamageInfo(result.FinalDamage, result.IsCritical, hitContext);

            TakeDamage(damageInfo);
        }

        public void Heal(float amount)
        {
            _health.Heal(amount);
        }

        public void SetInvincible(float duration)
        {
            if (!IsAlive || duration <= 0) return;

            float newEndTime = Time.time + duration;
            bool wasAlreadyInvincible = IsInvincible;

            _invincibilityEndTime = Mathf.Max(_invincibilityEndTime, newEndTime);
            _wasInvincible = true;

            if (!wasAlreadyInvincible)
            {
                OnInvincibilityStart?.Invoke();
            }
        }

        public void SetStunned(float duration)
        {
            if (!IsAlive || duration <= 0) return;

            float newEndTime = Time.time + duration;
            bool wasAlreadyStunned = IsStunned;

            _hitStunEndTime = Mathf.Max(_hitStunEndTime, newEndTime);
            _wasStunned = true;

            if (!wasAlreadyStunned)
            {
                OnHitStunStart?.Invoke();
            }
        }

        public void ClearInvincibility()
        {
            if (_wasInvincible)
            {
                _invincibilityEndTime = 0;
                _wasInvincible = false;
                OnInvincibilityEnd?.Invoke();
            }
        }

        public void ClearHitStun()
        {
            if (_wasStunned)
            {
                _hitStunEndTime = 0;
                _wasStunned = false;
                OnHitStunEnd?.Invoke();
            }
        }

        private void HandleDeath()
        {
            _stats.ClearAllModifiers();
            ClearInvincibility();
            ClearHitStun();
            OnDeath?.Invoke();
        }

#if UNITY_INCLUDE_TESTS
        public void SetTeamForTest(CombatTeam team) => _team = team;
        public void SetStatsDataForTest(CombatStatsData data)
        {
            _statsData = data;
            _stats = data != null
                ? CombatStats.FromData(data)
                : new CombatStats(
                    CombatConstants.DefaultAttackDamage,
                    CombatConstants.DefaultCriticalChance,
                    CombatConstants.DefaultCriticalMultiplier,
                    CombatConstants.DefaultDefense);
        }
        public void SetHitReactionSettingsForTest(HitReactionSettings settings) => _hitReactionSettings = settings;
#endif
    }
}
