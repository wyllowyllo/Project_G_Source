using Monster.AI;
using Monster.Data;
using Monster.Group;
using UnityEngine;
using UnityEngine.AI;

namespace Monster
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
        [SerializeField] private EnemyGroup _enemyGroup;

        [Header("디버그 정보")]
        [SerializeField] private EMonsterState _currentState;
        [SerializeField] private float _distanceToPlayer;
        [SerializeField] private float _distanceToHome;

        // 컴포넌트
        private NavMeshAgent _navAgent;
        private MonsterStateMachine _stateMachine;

        // 상태
        private float _currentHealth;
        private bool _isAlive = true;

        // BDO 스타일 - 테더 시스템
        private Vector3 _homePosition;
        private bool _isTethered = false;

        // 프로퍼티
        public bool IsAlive => _isAlive;
        public float CurrentHealth => _currentHealth;
        public MonsterData Data => _monsterData;

        public NavMeshAgent NavAgent => _navAgent;
        public Transform PlayerTransform => _playerTransform;
        public MonsterStateMachine StateMachine => _stateMachine;
        public EnemyGroup EnemyGroup => _enemyGroup;

        // BDO 스타일 프로퍼티
        public Vector3 HomePosition => _homePosition;
        public bool IsTethered => _isTethered;

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

            // BDO 스타일 - 테더 체크
            CheckTether();

            _stateMachine?.Update();

            // 디버그 정보 업데이트 (인스펙터 표시용)
            UpdateDebugInfo();
        }

        /// <summary>
        /// 인스펙터에 표시할 디버그 정보 업데이트
        /// </summary>
        private void UpdateDebugInfo()
        {
            _currentState = _stateMachine?.CurrentStateType ?? EMonsterState.Idle;

            if (_playerTransform != null)
            {
                _distanceToPlayer = Vector3.Distance(transform.position, _playerTransform.position);
            }

            _distanceToHome = Vector3.Distance(transform.position, _homePosition);
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

            // BDO 스타일 - 홈 포지션 설정 (스폰 위치)
            _homePosition = transform.position;
        }

        private void InitializeStateMachine()
        {
            _stateMachine = new MonsterStateMachine(this);

            // BDO 스타일 - 상태 등록
            _stateMachine.RegisterState(EMonsterState.Idle, new IdleState(this, _stateMachine));
            _stateMachine.RegisterState(EMonsterState.Approach, new ApproachState(this, _stateMachine));
            _stateMachine.RegisterState(EMonsterState.Strafe, new StrafeState(this, _stateMachine));
            _stateMachine.RegisterState(EMonsterState.Attack, new AttackState(this, _stateMachine));
            _stateMachine.RegisterState(EMonsterState.Recover, new RecoverState(this, _stateMachine));
            _stateMachine.RegisterState(EMonsterState.ReturnHome, new ReturnHomeState(this, _stateMachine));
            _stateMachine.RegisterState(EMonsterState.Dead, new DeadState(this));

            // 초기 상태 설정
            _stateMachine.Initialize(EMonsterState.Idle);
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

        /// <summary>
        /// EnemyGroup 설정
        /// </summary>
        public void SetEnemyGroup(EnemyGroup group)
        {
            _enemyGroup = group;
        }

        /// <summary>
        /// BDO 스타일 - 테더 체크 (홈 포지션으로부터 너무 멀어지면 복귀)
        /// </summary>
        private void CheckTether()
        {
            if (_monsterData == null || _isTethered)
            {
                return;
            }

            float distanceFromHome = Vector3.Distance(transform.position, _homePosition);

            // 테더 범위를 벗어나면 복귀 상태로 전환
            if (distanceFromHome > _monsterData.TetherRadius)
            {
                _isTethered = true;
                _stateMachine?.ChangeState(EMonsterState.ReturnHome);
            }
        }

        /// <summary>
        /// 테더 리셋 (홈 복귀 완료 시 호출)
        /// </summary>
        public void ResetTether()
        {
            _isTethered = false;
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
            _stateMachine?.ChangeState(EMonsterState.Dead);

            // EnemyGroup에서 제거
            _enemyGroup?.UnregisterMonster(this);

            // MonsterTracker에서 제거
            MonsterTracker.MonsterTracker.Instance?.UnregisterMonster(this);
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

            // BDO 스타일 - 테더 범위 (홈 포지션 기준)
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(_homePosition, _monsterData.TetherRadius);

            // 홈 포지션
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(_homePosition, 0.5f);

            // 거리 밴드 (선호 거리)
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, _monsterData.PreferredMinDistance);
            Gizmos.DrawWireSphere(transform.position, _monsterData.PreferredMaxDistance);
        }
    }
}
