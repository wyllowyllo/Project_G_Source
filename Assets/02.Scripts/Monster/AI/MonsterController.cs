using UnityEngine;
using UnityEngine.AI;

namespace ProjectG.Monster
{
    /// <summary>
    /// 몬스터의 핵심 동작을 제어하는 컨트롤러.
    /// FSM을 사용하여 상태별 동작을 관리합니다.
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
        private MonsterStateMachine _stateMachine;

        // 상태
        private float _currentHealth;
        private bool _isAlive = true;

        // 프로퍼티
        public bool IsAlive => _isAlive;
        public float CurrentHealth => _currentHealth;
        public MonsterData Data => _monsterData;
        
        public NavMeshAgent NavAgent => _navAgent;
        public Transform PlayerTransform => _playerTransform;
        public MonsterStateMachine StateMachine => _stateMachine;

        private void Awake()
        {
            _navAgent = GetComponent<NavMeshAgent>();
            InitializeMonster();
            InitializeStateMachine();
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

            _stateMachine?.Update();
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

        private void InitializeStateMachine()
        {
            _stateMachine = new MonsterStateMachine(this);

            // 상태 등록
            _stateMachine.RegisterState(MonsterState.Idle, new IdleState(this, _stateMachine));
            _stateMachine.RegisterState(MonsterState.Engage, new EngageState(this, _stateMachine));
            _stateMachine.RegisterState(MonsterState.Attack, new AttackState(this, _stateMachine));
            _stateMachine.RegisterState(MonsterState.Dead, new DeadState(this));

            // 초기 상태 설정
            _stateMachine.Initialize(MonsterState.Idle);
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

        public void TakeDamage(float damage, Vector3 attackerPosition)
        {
            if (!_isAlive)
            {
                return;
            }

            _currentHealth -= damage;

            // TODO: Hit 상태 추가 시 피격 처리

            if (_currentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            _isAlive = false;
            _stateMachine?.ChangeState(MonsterState.Dead);
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
