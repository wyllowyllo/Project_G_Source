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

        private Health _health;
        private CombatStats _stats;
        
        public Transform Transform => transform;
        public CombatStats Stats => _stats;
        public CombatTeam Team => _team;
        
        public float CurrentHealth => _health.CurrentHealth;
        public float MaxHealth => _health.MaxHealth;
        
        public bool CanTakeDamage => _health.IsAlive;
        
        public event Action<DamageInfo> OnDamaged;
        public event Action OnDeath;

        private void Awake()
        {
            _health = GetComponent<Health>();
            _stats = _statsData != null
                ? CombatStats.FromData(_statsData)
                : new CombatStats(10f, 0.1f, 1.5f, 0f);
        }

        private void Start()
        {
            _health.OnDeath += HandleDeath;
        }

        private void OnDestroy()
        {
            if (_health != null)
                _health.OnDeath -= HandleDeath;
        }

        public void TakeDamage(DamageInfo damageInfo)
        {
            if (!CanTakeDamage) return;

            _health.TakeDamage(damageInfo.Amount);
            OnDamaged?.Invoke(damageInfo);
        }

        private void HandleDeath()
        {
            _stats.ClearAllModifiers();
            OnDeath?.Invoke();
        }

        public void Heal(float amount)
        {
            _health.Heal(amount);
        }
    }
}
