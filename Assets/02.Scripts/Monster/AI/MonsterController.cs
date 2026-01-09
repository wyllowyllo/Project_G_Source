using Combat.Core;
using Common;
using Monster.AI.States;
using Monster.Ability;
using Monster.Data;
using Monster.Group;
using Monster.Manager;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Monster.AI
{
    // 몬스터의 핵심 동작을 제어 (FSM 기반 상태별 동작 관리)
    [RequireComponent(typeof(NavMeshAgent),typeof(Combatant))]
    public class MonsterController : MonoBehaviour
    {

        [SerializeField] private EMonsterState _currentState;

        [Header("설정")]
        [SerializeField] private MonsterData _monsterData;

        [Header("참조")]
        [SerializeField] private Transform _playerTransform;

        // 몬스터 기본 위치
        private Vector3 _homePosition;
        
        // 컴포넌트
        private NavMeshAgent _navAgent;
        private Combatant _combatant;
        private MonsterStateMachine _stateMachine;
        private GroupCommandProvider _groupCommandProvider;
        private Animator _animator;
        private MonsterAnimationEventReceiver _animEventReceiver;

        // Ability 시스템
        private Dictionary<System.Type, EntityAbility> _abilities;
        private List<EntityAbility> _abilityList;

        // 애니메이션 콜백
        private System.Action _onAttackComplete;
        private System.Action _onAlertComplete;

        // 프로퍼티
        public bool IsAlive => _combatant != null && _combatant.IsAlive;
        public MonsterData Data => _monsterData;

        public NavMeshAgent NavAgent => _navAgent;
        public Transform PlayerTransform => _playerTransform;
      
        public GroupCommandProvider GroupCommandProvider => _groupCommandProvider;

        public Vector3 HomePosition => _homePosition;

        public Animator Animator => _animator;
        
       

        private void Awake()
        {
            _navAgent = GetComponent<NavMeshAgent>();
            _combatant = GetComponent<Combatant>();
            _groupCommandProvider = new GroupCommandProvider(this);

            // Animator 찾기 (Model 하위의 프리팹에 있음)
            _animator = GetComponentInChildren<Animator>();

            InitializeMonster();
            InitializeAbilities();
            InitializeAnimationEventReceiver();
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
            if (!IsAlive)
            {
                return;
            }

            // Ability 업데이트
            UpdateAbilities();

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
                enabled = false;
                return;
            }

            // NavMeshAgent 설정
            _navAgent.speed = _monsterData.MoveSpeed;
            _navAgent.angularSpeed = _monsterData.RotationSpeed;

            _homePosition = transform.position;
        }

        private void InitializeStateMachine()
        {
            _stateMachine = new MonsterStateMachine(this);

            // 상태 등록
            _stateMachine.RegisterState(EMonsterState.Idle, new IdleState(this, _stateMachine));
            _stateMachine.RegisterState(EMonsterState.Alert, new AlertState(this, _stateMachine));
            _stateMachine.RegisterState(EMonsterState.Approach, new ApproachState(this, _stateMachine));
            _stateMachine.RegisterState(EMonsterState.Strafe, new StrafeState(this, _stateMachine, _groupCommandProvider));
            _stateMachine.RegisterState(EMonsterState.Attack, new AttackState(this, _stateMachine, _groupCommandProvider));
            _stateMachine.RegisterState(EMonsterState.Recover, new RecoverState(this, _stateMachine, _groupCommandProvider));
            _stateMachine.RegisterState(EMonsterState.ReturnHome, new ReturnHomeState(this, _stateMachine));
            _stateMachine.RegisterState(EMonsterState.Dead, new DeadState(this));

            // 초기 상태 설정
            _stateMachine.Initialize(EMonsterState.Idle);
        }

        private void InitializeAbilities()
        {
            _abilities = new Dictionary<System.Type, EntityAbility>();
            _abilityList = new List<EntityAbility>();

            // Ability 생성 및 등록
            RegisterAbility(new NavAgentAbility());
            RegisterAbility(new PlayerDetectAbility());
            RegisterAbility(new FacingAbility());
        }

        private void RegisterAbility(EntityAbility ability)
        {
            ability.Initialize(this);
            _abilities[ability.GetType()] = ability;
            _abilityList.Add(ability);
        }

        private void UpdateAbilities()
        {
            foreach (var ability in _abilityList)
            {
                ability.Update();
            }
        }

        // State에서 Ability를 가져오기 위한 메서드
        public T GetAbility<T>() where T : EntityAbility
        {
            if (_abilities.TryGetValue(typeof(T), out EntityAbility ability))
            {
                return ability as T;
            }
            return null;
        }

        private void InitializeAnimationEventReceiver()
        {
            if (_animator == null)
            {
                Debug.LogWarning($"{gameObject.name}: Animator를 찾을 수 없습니다.");
                return;
            }

            // Animator가 있는 오브젝트에 EventReceiver 추가
            _animEventReceiver = _animator.gameObject.GetComponent<MonsterAnimationEventReceiver>();
            if (_animEventReceiver == null)
            {
                _animEventReceiver = _animator.gameObject.AddComponent<MonsterAnimationEventReceiver>();
            }

            // 공격 콜라이더 찾기 (Body 하위의 AttackCollider)
            Collider attackCollider = null;
            Transform body = _animator.transform.Find("Body");
            if (body != null)
            {
                Transform attackColliderTransform = body.Find("AttackCollider");
                if (attackColliderTransform != null)
                {
                    attackCollider = attackColliderTransform.GetComponent<Collider>();
                }
            }

            _animEventReceiver.Initialize(this, attackCollider);
        }

        // AttackState에서 호출: 공격 애니메이션 트리거
        public void TriggerAttackAnimation(string triggerName, System.Action onComplete)
        {
            _onAttackComplete = onComplete;

            if (_animator != null)
            {
                _animator.SetTrigger(triggerName);
            }
            else
            {
                // Animator가 없으면 즉시 완료 처리
                onComplete?.Invoke();
            }
        }

        // MonsterAnimationEventReceiver에서 호출
        public void OnAttackAnimationComplete()
        {
            _onAttackComplete?.Invoke();
            _onAttackComplete = null;
        }

        // AlertState에서 호출: 경계 애니메이션 트리거
        public void TriggerAlertAnimation(System.Action onComplete)
        {
            _onAlertComplete = onComplete;

            if (_animator != null)
            {
                _animator.SetTrigger("Alert");
            }
            else
            {
                // Animator가 없으면 즉시 완료 처리
                onComplete?.Invoke();
            }
        }

        // MonsterAnimationEventReceiver에서 호출
        public void OnAlertAnimationComplete()
        {
            _onAlertComplete?.Invoke();
            _onAlertComplete = null;
        }

        private void HandleDeath()
        {
            _stateMachine?.ChangeState(EMonsterState.Dead);

            // EnemyGroup에서 제거 (GroupCommandProvider에 위임)
            _groupCommandProvider?.UnregisterFromGroup();

            // MonsterTracker에서 제거
            MonsterTracker.Instance?.UnregisterMonster(this);
        }
        
        private void OnEnable()
        {
            if (_combatant != null)
            {
                _combatant.OnDeath += HandleDeath;
            }
        }

        private void OnDisable()
        {
            if (_combatant != null)
            {
                _combatant.OnDeath -= HandleDeath;
            }
        }
    }
}
