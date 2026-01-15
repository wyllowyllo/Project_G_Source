using Boss.Ability;
using Boss.AI.States;
using Boss.Data;
using Combat.Core;
using Common;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Boss.AI
{
    [RequireComponent(typeof(NavMeshAgent), typeof(Combatant))]
    public class BossController : MonoBehaviour
    {
        [SerializeField] private EBossState _currentState;

        [Header("설정")]
        [SerializeField] private BossData _bossData;

        [Header("참조")]
        [SerializeField] private Transform _playerTransform;

        // 컴포넌트
        private NavMeshAgent _navAgent;
        private Combatant _combatant;
        private BossStateMachine _stateMachine;
        private Animator _animator;

        // Ability 시스템
        private Dictionary<System.Type, BossAbility> _abilities;
        private List<BossAbility> _abilityList;

        // 프로퍼티
        public bool IsAlive => _combatant != null && _combatant.IsAlive;
        public BossData Data => _bossData;
        public Combatant Combatant => _combatant;
        public NavMeshAgent NavAgent => _navAgent;
        public Transform PlayerTransform => _playerTransform;
        public Animator Animator => _animator;
        public BossStateMachine StateMachine => _stateMachine;
        public EBossState CurrentStateType => _stateMachine?.CurrentStateType ?? EBossState.Idle;

        private void Awake()
        {
            _navAgent = GetComponent<NavMeshAgent>();
            _combatant = GetComponent<Combatant>();
            _animator = GetComponentInChildren<Animator>();

            InitializeBoss();
            InitializeAbilities();
            InitializeStateMachine();
        }

        private void Start()
        {
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

            UpdateAbilities();
            _stateMachine?.Update();
            UpdateDebugInfo();
        }

        private void UpdateDebugInfo()
        {
            _currentState = _stateMachine?.CurrentStateType ?? EBossState.Idle;
        }

        private void InitializeBoss()
        {
            if (_bossData == null)
            {
                Debug.LogError($"{gameObject.name}: BossData가 할당되지 않았습니다.");
                enabled = false;
                return;
            }

            _navAgent.speed = _bossData.MoveSpeed;
            _navAgent.angularSpeed = _bossData.RotationSpeed;
        }

        private void InitializeStateMachine()
        {
            _stateMachine = new BossStateMachine(this);

            // 상태 등록은 Phase 3에서 구현
            // 임시로 Idle 상태만 등록
            // _stateMachine.RegisterState(EBossState.Idle, new BossIdleState(this, _stateMachine));

            // 초기 상태 설정
            // _stateMachine.Initialize(EBossState.Idle);
        }

        private void InitializeAbilities()
        {
            _abilities = new Dictionary<System.Type, BossAbility>();
            _abilityList = new List<BossAbility>();

            RegisterAbility(new BossNavAgentAbility());
            RegisterAbility(new BossPlayerDetectAbility());
            RegisterAbility(new BossFacingAbility());
            RegisterAbility(new BossAnimatorAbility());
        }

        private void RegisterAbility(BossAbility ability)
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

        public T GetAbility<T>() where T : BossAbility
        {
            if (_abilities.TryGetValue(typeof(T), out BossAbility ability))
            {
                return ability as T;
            }
            return null;
        }

        // 애니메이션 콜백
        public void OnMeleeAttackAnimationComplete()
        {
            GetAbility<BossAnimatorAbility>()?.OnMeleeAttackComplete();
        }

        public void OnChargeAnimationComplete()
        {
            GetAbility<BossAnimatorAbility>()?.OnChargeComplete();
        }

        public void OnBreathAnimationComplete()
        {
            GetAbility<BossAnimatorAbility>()?.OnBreathComplete();
        }

        public void OnProjectileAnimationComplete()
        {
            GetAbility<BossAnimatorAbility>()?.OnProjectileComplete();
        }

        public void OnSummonAnimationComplete()
        {
            GetAbility<BossAnimatorAbility>()?.OnSummonComplete();
        }

        public void OnStaggerAnimationComplete()
        {
            GetAbility<BossAnimatorAbility>()?.OnStaggerComplete();
        }

        public void OnHitAnimationComplete()
        {
            GetAbility<BossAnimatorAbility>()?.OnHitComplete();
        }

        public void OnDeathAnimationComplete()
        {
            GetAbility<BossAnimatorAbility>()?.OnDeathComplete();
        }

        public void OnPhaseTransitionAnimationComplete()
        {
            GetAbility<BossAnimatorAbility>()?.OnPhaseTransitionComplete();
        }

        private void HandleDeath()
        {
            _stateMachine?.ChangeState(EBossState.Dead);
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
