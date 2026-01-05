using System;
using UnityEngine;

namespace Combat.Core
{
    public class Health : MonoBehaviour, IHealthProvider
    {
        [SerializeField] private float _maxHealth = 100f;

        private float _currentHealth;

        public float CurrentHealth => _currentHealth;
        public float MaxHealth => _maxHealth;
        public bool IsAlive => _currentHealth > 0;

        public event Action<float> OnDamaged;
        public event Action<float> OnHealed;
        public event Action OnDeath;

        private void Awake()
        {
            _currentHealth = _maxHealth;
        }

        public void TakeDamage(float amount)
        {
            if (!IsAlive) return;
            if (amount <= 0) return;

            _currentHealth = Mathf.Max(0, _currentHealth - amount);
            OnDamaged?.Invoke(amount);

            if (!IsAlive)
            {
                OnDeath?.Invoke();
            }
        }

        public void Heal(float amount)
        {
            if (!IsAlive) return;
            if (amount <= 0) return;

            float previousHealth = _currentHealth;
            _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);

            float actualHeal = _currentHealth - previousHealth;
            if (actualHeal > 0)
            {
                OnHealed?.Invoke(actualHeal);
            }
        }

        public void SetMaxHealth(float maxHealth, bool healToFull = false)
        {
            _maxHealth = Mathf.Max(1f, maxHealth);
            _currentHealth = Mathf.Min(_currentHealth, _maxHealth);

            if (healToFull)
            {
                _currentHealth = _maxHealth;
            }
        }
    }
}
