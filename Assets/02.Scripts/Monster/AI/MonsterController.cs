using Common;
using Monster.AI;
using Monster.AI.States;
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

        [SerializeField] private EMonsterState _currentState;

        [Header("설정")]
        [SerializeField] private MonsterData _monsterData;

        [Header("참조")]
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private EnemyGroup _enemyGroup;

        // 컴포넌트
        private NavMeshAgent _navAgent;
        private MonsterStateMachine _stateMachine;

        // 상태
        private float _currentHealth;
        private bool _isAlive = true;
        
        // 개체 쿨다운/공격 타입
        private float _nextLightAttackTime;
        private float _nextHeavyAttackTime;
        private bool _nextAttackIsHeavy;
        private bool _currentAttackWasHeavy;

        // 테더 시스템
        private Vector3 _homePosition;
        private bool _isTethered = false;

        // 공격 시각화
        private Color _originalMaterialColor;

        // 프로퍼티
        public bool IsAlive => _isAlive;
        public float CurrentHealth => _currentHealth;
        public Color OriginalMaterialColor => _originalMaterialColor;
        public MonsterData Data => _monsterData;

        public NavMeshAgent NavAgent => _navAgent;
        public Transform PlayerTransform => _playerTransform;
        public MonsterStateMachine StateMachine => _stateMachine;

        // 프로퍼티
        public Vector3 HomePosition => _homePosition;
        public bool IsTethered => _isTethered;
        public bool NextAttackIsHeavy => _nextAttackIsHeavy;
        public bool CurrentAttackWasHeavy => _currentAttackWasHeavy;

        private void Awake()
        {
            _navAgent = GetComponent<NavMeshAgent>();

            // 원래 머티리얼 색상 저장
            Renderer renderer = GetComponentInChildren<Renderer>();
            if (renderer != null && renderer.material != null)
            {
                _originalMaterialColor = renderer.material.color;
            }

            InitializeMonster();
            InitializeStateMachine();
        }

        private void Start()
        {
            // PlayerReferenceProvider로부터 플레이어 참조 가져오기
            if (PlayerReferenceProvider.Instance != null)
            {
                _playerTransform = PlayerReferenceProvider.Instance.PlayerTransform;
            }
            else
            {
                Debug.LogWarning($"{gameObject.name}: PlayerReferenceProvider를 찾을 수 없습니다.");
            }
        }

        private void Update()
        {
            if (!_isAlive)
            {
                return;
            }

            _stateMachine?.Update();

            // 디버그 정보 업데이트 (인스펙터 표시용)
            UpdateDebugInfo();
        }

        private void UpdateDebugInfo()
        {
            _currentState = _stateMachine?.CurrentStateType ?? EMonsterState.Idle;
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
            
            _homePosition = transform.position;
            
            _nextLightAttackTime = 0f;
            _nextHeavyAttackTime = 0f;
        }

        private void InitializeStateMachine()
        {
            _stateMachine = new MonsterStateMachine(this);

            // 상태 등록
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


        /// <summary>
        /// EnemyGroup 설정
        /// </summary>
        public void SetEnemyGroup(EnemyGroup group)
        {
            _enemyGroup = group;
        }

        // ===== Wrapper 메서드: 디미터 법칙 준수 =====
        // States가 EnemyGroup에 직접 접근하지 않도록 캡슐화

        /// <summary>
        /// 공격 슬롯 요청 (EnemyGroup에 위임)
        /// </summary>
        public bool RequestAttackSlot()
        {
            return _enemyGroup?.RequestAttackSlot(this) ?? false;
        }

        /// <summary>
        /// 공격 슬롯 반환 (EnemyGroup에 위임)
        /// </summary>
        public void ReleaseAttackSlot()
        {
            _enemyGroup?.ReleaseAttackSlot(this);
        }

        /// <summary>
        /// 공격 가능 여부 확인 (EnemyGroup에 위임)
        /// </summary>
        public bool CanAttack()
        {
            return _enemyGroup?.CanAttack(this) ?? false;
        }

        /// <summary>
        /// 원하는 위치 가져오기 (EnemyGroup에 위임)
        /// </summary>
        public Vector3 GetDesiredPosition()
        {
            return _enemyGroup?.GetDesiredPosition(this) ?? transform.position;
        }

        /// <summary>
        /// 후퇴 시 cascading push-back 요청 (EnemyGroup에 위임)
        /// </summary>
        public void RequestPushback(Vector3 retreatDirection, float distance)
        {
            _enemyGroup?.RequestPushback(this, retreatDirection, distance);
        }


        /// <summary>
        /// 테더 리셋 (홈 복귀 완료 시 호출)
        /// </summary>
        public void ResetTether()
        {
            _isTethered = false;
        }
        
        public bool CanLightAttack(float now) => now >= _nextLightAttackTime;
        public bool CanHeavyAttack(float now) => now >= _nextHeavyAttackTime;
        public void ConsumeLightAttack(float now, float cd)
        {
            _nextLightAttackTime = now + Mathf.Max(0.01f, cd);
        }
        public void ConsumeHeavyAttack(float now, float cd)
        {
            _nextHeavyAttackTime = now + Mathf.Max(0.01f, cd);
        }
    
        public void SetNextAttackHeavy(bool heavy)
        {
            _nextAttackIsHeavy = heavy;
        }
    
        public void MarkCurrentAttackHeavy(bool heavy)
        {
            _currentAttackWasHeavy = heavy;
        }
    
        public void ClearCurrentAttackHeavy()
        {
            _currentAttackWasHeavy = false;
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

        
    }
}
