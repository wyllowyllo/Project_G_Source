using Combat.Core;
using Common;
using Monster.AI.States;
using Monster.Ability;
using Monster.Combat;
using Monster.Data;
using Monster.Group;
using Monster.Manager;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using EMonsterAttackType = Monster.Data.EMonsterAttackType;

namespace Monster.AI
{
    
    [RequireComponent(typeof(NavMeshAgent),typeof(Combatant))]
    public class MonsterController : MonoBehaviour
    {

        [SerializeField] private EMonsterState _currentState;

        [Header("설정")]
        [SerializeField] private MonsterData _monsterData;

        [Header("참조")]
        [SerializeField] private Transform _playerTransform;

        
        private Vector3 _homePosition;
        
        // 컴포넌트
        private NavMeshAgent _navAgent;
        private Combatant _combatant;
        private MonsterAttacker _monsterAttacker;
        private MonsterRangedAttacker _rangedAttacker;
        private MonsterStateMachine _stateMachine;
        private GroupCommandProvider _groupCommandProvider;
        private Animator _animator;

        // Ability 시스템
        private Dictionary<System.Type, EntityAbility> _abilities;
        private List<EntityAbility> _abilityList;

        // 프로퍼티
        public bool IsAlive => _combatant != null && _combatant.IsAlive;
        public MonsterData Data => _monsterData;
        public Combatant Combatant => _combatant;
        public MonsterAttacker Attacker => _monsterAttacker;
        public MonsterRangedAttacker RangedAttacker => _rangedAttacker;
        public bool IsRangedType => _monsterData != null && _monsterData.IsRanged;

        public NavMeshAgent NavAgent => _navAgent;
        public Transform PlayerTransform => _playerTransform;

        public GroupCommandProvider GroupCommandProvider => _groupCommandProvider;

        public Vector3 HomePosition => _homePosition;

        public Animator Animator => _animator;
        
       

        private void Awake()
        {
            _navAgent = GetComponent<NavMeshAgent>();
            _combatant = GetComponent<Combatant>();
            _monsterAttacker = GetComponent<MonsterAttacker>();
            _rangedAttacker = GetComponent<MonsterRangedAttacker>();
            _groupCommandProvider = new GroupCommandProvider(this);


            _animator = GetComponentInChildren<Animator>();

            InitializeMonster();
            InitializeAttackers();
            InitializeAbilities();
            InitializeStateMachine();
        }

        private void InitializeAttackers()
        {
            _monsterAttacker?.Initialize();

            _rangedAttacker?.Initialize();
        }
        
        private void Start()
        {
            if (PlayerReferenceProvider.Instance != null)
            {
                _playerTransform = PlayerReferenceProvider.Instance.PlayerTransform;
                _rangedAttacker?.SetPlayerTransform(_playerTransform);
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
            _stateMachine.RegisterState(EMonsterState.Patrol, new PatrolState(this, _stateMachine));
            _stateMachine.RegisterState(EMonsterState.Alert, new AlertState(this, _stateMachine));
            _stateMachine.RegisterState(EMonsterState.Approach, new ApproachState(this, _stateMachine));
            _stateMachine.RegisterState(EMonsterState.Strafe, new StrafeState(this, _stateMachine, _groupCommandProvider));

            // 공격 타입에 따라 AttackState 등록
            switch (_monsterData.AttackType)
            {
                case EMonsterAttackType.Melee:
                    _stateMachine.RegisterState(EMonsterState.Attack, new AttackState(this, _stateMachine, _groupCommandProvider));
                    break;

                case EMonsterAttackType.Ranged:
                    _stateMachine.RegisterState(EMonsterState.RangedAttack, new RangedAttackState(this, _stateMachine, _groupCommandProvider));
                    break;

                case EMonsterAttackType.Hybrid:
                    _stateMachine.RegisterState(EMonsterState.Attack, new AttackState(this, _stateMachine, _groupCommandProvider));
                    _stateMachine.RegisterState(EMonsterState.RangedAttack, new RangedAttackState(this, _stateMachine, _groupCommandProvider));
                    break;
            }

            _stateMachine.RegisterState(EMonsterState.Recover, new RecoverState(this, _stateMachine, _groupCommandProvider));
            _stateMachine.RegisterState(EMonsterState.ReturnHome, new ReturnHomeState(this, _stateMachine));
            _stateMachine.RegisterState(EMonsterState.Hit, new HitState(this, _stateMachine));
            _stateMachine.RegisterState(EMonsterState.Dead, new DeadState(this));

            // 초기 상태 설정 (순찰 모드 ON이면 Patrol, OFF면 Idle)
            EMonsterState initialState = _monsterData.EnablePatrol ? EMonsterState.Patrol : EMonsterState.Idle;
            _stateMachine.Initialize(initialState);
        }

        private void InitializeAbilities()
        {
            _abilities = new Dictionary<System.Type, EntityAbility>();
            _abilityList = new List<EntityAbility>();

            // Ability 생성 및 등록
            RegisterAbility(new NavAgentAbility());
            RegisterAbility(new PlayerDetectAbility());
            RegisterAbility(new FacingAbility());
            RegisterAbility(new AnimatorAbility());
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
        
        public bool IsCurrentAttackHeavy() => _groupCommandProvider?.CurrentAttackWasHeavy ?? false;
        
        public void OnAttackAnimationComplete()
        {
            GetAbility<AnimatorAbility>()?.OnAttackComplete();
        }

       
        public void OnAlertAnimationComplete()
        {
            GetAbility<AnimatorAbility>()?.OnAlertComplete();
        }

        
        public void OnHitAnimationComplete()
        {
            GetAbility<AnimatorAbility>()?.OnHitComplete();
        }

       
        public void OnDeathAnimationComplete()
        {
            GetAbility<AnimatorAbility>()?.OnDeathComplete();
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
                _combatant.OnHitStunStart += HandleHitStunStart;
                _combatant.OnHitStunEnd += HandleHitStunEnd;
            }
        }

        private void OnDisable()
        {
            if (_combatant != null)
            {
                _combatant.OnDeath -= HandleDeath;
                _combatant.OnHitStunStart -= HandleHitStunStart;
                _combatant.OnHitStunEnd -= HandleHitStunEnd;
            }
        }

        private void HandleHitStunStart()
        {
            if (_stateMachine.CurrentStateType == EMonsterState.Dead)
            {
                return;
            }

            // 이미 Hit 상태면 ReEnter로 애니메이션 재시작
            if (_stateMachine.CurrentStateType == EMonsterState.Hit)
            {
                _stateMachine.TryReEnterCurrentState();
                return;
            }

            _stateMachine.ChangeState(EMonsterState.Hit);
        }

        private void HandleHitStunEnd()
        {
            // 경직 종료 - 필요시 추가 로직
        }
    }
}
