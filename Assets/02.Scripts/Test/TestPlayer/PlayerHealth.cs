using Combat.Core;
using UnityEngine;
using CombatDamageable = Combat.Damage.IDamageable;
using MonsterDamageable = Monster.AI.IDamageable;

namespace Test.TestPlayer
{
    /// <summary>
    /// 플레이어의 체력을 관리하는 컴포넌트.
    /// Combat 시스템과 Monster 시스템 모두와 호환됩니다.
    ///
    /// 테스트 키:
    /// - K: 자가 피해 (10 데미지)
    /// - I: 무적 모드 토글
    /// - H: 체력 전체 회복
    /// </summary>
    public class PlayerHealth : MonoBehaviour, CombatDamageable, MonsterDamageable
    {
        [Header("체력 설정")]
        [SerializeField] private float _maxHealth = 100f;
        [SerializeField] private bool _isInvincible = false;

        private float _currentHealth;
        private bool _isAlive = true;

        public bool IsAlive => _isAlive;
        public float CurrentHealth => _currentHealth;
        public float MaxHealth => _maxHealth;

        // Combat.Damage.IDamageable 구현
        public bool CanTakeDamage => _isAlive && !_isInvincible;

        private void Awake()
        {
            _currentHealth = _maxHealth;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.K))
            {
                TakeDamage(10f, transform.position);
            }

            if (Input.GetKeyDown(KeyCode.I))
            {
                _isInvincible = !_isInvincible;
                Debug.Log($"무적 모드: {(_isInvincible ? "ON" : "OFF")}");
            }

            if (Input.GetKeyDown(KeyCode.H))
            {
                Heal(_maxHealth);
            }
        }

        /// <summary>
        /// Monster.AI.IDamageable 구현 - 기존 호환성 유지
        /// </summary>
        public void TakeDamage(float damage, Vector3 attackerPosition)
        {
            if (!CanTakeDamage) return;

            ApplyDamage(damage, false);
        }

        /// <summary>
        /// Combat.Damage.IDamageable 구현 - Combat 시스템 통합용
        /// </summary>
        public void TakeDamage(DamageInfo damageInfo)
        {
            if (!CanTakeDamage) return;

            ApplyDamage(damageInfo.Amount, damageInfo.IsCritical);
        }

        private void ApplyDamage(float damage, bool isCritical)
        {
            _currentHealth -= damage;

            string critText = isCritical ? " (크리티컬!)" : "";
            Debug.Log($"플레이어 피격! 데미지: {damage:F1}{critText}, 남은 체력: {_currentHealth:F1}/{_maxHealth}");

            if (_currentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            _isAlive = false;
            Debug.Log("플레이어 사망!");

            // TODO: 사망 처리
            // 임시: 3초 후 재생성
            Invoke(nameof(Respawn), 3f);
        }

        private void Respawn()
        {
            _currentHealth = _maxHealth;
            _isAlive = true;
            transform.position = Vector3.zero + Vector3.up;
            Debug.Log("플레이어 부활!");
        }

        public void Heal(float amount)
        {
            if (!_isAlive)
            {
                return;
            }

            _currentHealth = Mathf.Min(_currentHealth + amount, _maxHealth);
            Debug.Log($"체력 회복! 현재 체력: {_currentHealth}/{_maxHealth}");
        }

        private void OnGUI()
        {
            // 화면 좌상단에 체력 표시
            GUI.Box(new Rect(10, 10, 200, 80), "");

            string healthText = _isAlive
                ? $"HP: {_currentHealth:F0} / {_maxHealth:F0}"
                : "DEAD";

            GUI.Label(new Rect(20, 20, 180, 20), healthText);

            if (_isInvincible)
            {
                GUI.Label(new Rect(20, 40, 180, 20), "무적 모드: ON");
            }

            GUI.Label(new Rect(20, 60, 180, 20), "K: 자가피해 I: 무적 H: 회복");
        }
    }
}
