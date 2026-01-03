using UnityEngine;
using UnityEngine.AI;

namespace ProjectG.Monster
{
    /// <summary>
    /// 몬스터의 핵심 동작을 제어하는 컨트롤러.
    /// 프로토타입: 기본 플레이어 추적 및 HP 시스템.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class MonsterController : MonoBehaviour, IDamageable
    {
        [Header("설정")]
        [SerializeField] private MonsterData _monsterData;

        [Header("참조")]
        [SerializeField] private Transform _playerTransform;

        // 컴포넌트
        private NavMeshAgent _navAgent;

        // 상태
        private float _currentHealth;
        private bool _isAlive = true;
        private Vector3 _targetPosition;

        // 프로퍼티
        public bool IsAlive => _isAlive;
        public float CurrentHealth => _currentHealth;
        public MonsterData Data => _monsterData;

        private void Awake()
        {
            _navAgent = GetComponent<NavMeshAgent>();
            InitializeMonster();
        }

        private void Start()
        {
            FindPlayer();
        }

        private void Update()
        {
            if (!_isAlive)
            {
                return;
            }

            UpdateMovement();
        }

        private void InitializeMonster()
        {
            if (_monsterData == null)
            {
                Debug.LogError($"{gameObject.name}: MonsterData가 할당되지 않았습니다.");
                return;
            }

            _currentHealth = _monsterData.MaxHealth;

            // NavMeshAgent 설정
            _navAgent.speed = _monsterData.MoveSpeed;
            _navAgent.angularSpeed = _monsterData.RotationSpeed;
            _navAgent.stoppingDistance = _monsterData.AttackRange;
        }

        private void FindPlayer()
        {
            // TODO: 플레이어 태그나 싱글톤으로 찾기
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerTransform = player.transform;
            }
            else
            {
                Debug.LogWarning($"{gameObject.name}: Player를 찾을 수 없습니다.");
            }
        }

        private void UpdateMovement()
        {
            if (_playerTransform == null)
            {
                return;
            }

            float distanceToPlayer = Vector3.Distance(transform.position, _playerTransform.position);

            // 감지 범위 내에 있으면 플레이어 추적
            if (distanceToPlayer <= _monsterData.DetectionRange)
            {
                _navAgent.SetDestination(_playerTransform.position);
            }
        }

        public void TakeDamage(float damage, Vector3 attackerPosition)
        {
            if (!_isAlive)
            {
                return;
            }

            _currentHealth -= damage;

            if (_currentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            _isAlive = false;
            _navAgent.isStopped = true;

            // TODO: 사망 애니메이션, 보상 드롭
            Debug.Log($"{gameObject.name} 사망");

            // 임시: 3초 후 파괴
            Destroy(gameObject, 3f);
        }

        public void SetTargetPosition(Vector3 targetPosition)
        {
            _targetPosition = targetPosition;
        }

        // 디버그용
        private void OnDrawGizmosSelected()
        {
            if (_monsterData == null)
            {
                return;
            }

            // 감지 범위
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _monsterData.DetectionRange);

            // 공격 범위
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _monsterData.AttackRange);
        }
    }
}
