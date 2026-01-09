using Monster.AI;
using UnityEngine;

namespace Test.TestPlayer
{
    /// <summary>
    /// 플레이어의 체력을 관리하는 컴포넌트.
    /// 테스트용: 몬스터가 플레이어를 공격할 수 있도록 IDamageable 구현.
    /// </summary>
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        [Header("체력 설정")]
        [SerializeField] private float _maxHealth = 100f;
        [SerializeField] private bool _isInvincible = false;

        private float _currentHealth;
        private bool _isAlive = true;

        public bool IsAlive => _isAlive;
        public float CurrentHealth => _currentHealth;
        public float MaxHealth => _maxHealth;

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

        public void TakeDamage(float damage, Vector3 attackerPosition)
        {
            if (!_isAlive || _isInvincible)
            {
                return;
            }

            _currentHealth -= damage;

            Debug.Log($"플레이어 피격! 데미지: {damage}, 남은 체력: {_currentHealth}/{_maxHealth}");

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
