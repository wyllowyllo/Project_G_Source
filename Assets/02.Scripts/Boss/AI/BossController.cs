using Boss.Ability;
using Boss.AI.States;
using Boss.Core;
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
        [SerializeField] private int _currentPhase;

        [Header("설정")]
        [SerializeField] private BossData _bossData;

        [Header("참조")]
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private BossTelegraph _telegraph;

        // 컴포넌트
        private NavMeshAgent _navAgent;
        private Combatant _combatant;
        private BossStateMachine _stateMachine;
        private Animator _animator;

        // 핵심 시스템
        private BossSuperArmor _superArmor;
        private BossPhaseManager _phaseManager;

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

        // 핵심 시스템 프로퍼티
        public BossSuperArmor SuperArmor => _superArmor;
        public BossPhaseManager PhaseManager => _phaseManager;
        public BossTelegraph Telegraph => _telegraph;

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
            _phaseManager?.Update();
            _stateMachine?.Update();
            UpdateDebugInfo();
        }

        private void UpdateDebugInfo()
        {
            _currentState = _stateMachine?.CurrentStateType ?? EBossState.Idle;
            _currentPhase = _phaseManager?.CurrentPhaseNumber ?? 1;
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

            InitializeCoreSystems();
        }

        private void InitializeCoreSystems()
        {
            // 슈퍼아머 시스템
            _superArmor = new BossSuperArmor(_bossData.MaxPoise);
            _superArmor.OnPoiseBroken += HandlePoiseBroken;

            // 페이즈 시스템
            _phaseManager = new BossPhaseManager(_bossData.Phases, _combatant);
            _phaseManager.OnPhaseTransitionStart += HandlePhaseTransitionStart;

            // Telegraph (자동 검색)
            if (_telegraph == null)
            {
                _telegraph = GetComponentInChildren<BossTelegraph>();
            }
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

        private void HandlePoiseBroken()
        {
            // 슈퍼아머가 활성화된 상태(무한 포이즈)가 아닐 때만 그로기
            if (!_superArmor.IsInfinite)
            {
                _stateMachine?.ChangeState(EBossState.Stagger);
            }
        }

        private void HandlePhaseTransitionStart(BossPhaseData newPhase)
        {
            // 페이즈 전환 상태로 변경
            _stateMachine?.ChangeState(EBossState.PhaseTransition);
        }

        // 외부에서 호출: 포이즈 데미지 처리
        public void TakePoiseDamage(float damage)
        {
            _superArmor?.TakePoiseDamage(damage);
        }

        // 포이즈 회복 (그로기 종료 후)
        public void RecoverPoise()
        {
            _superArmor?.Recover();
        }

        // 슈퍼아머 무한 모드 설정 (특정 패턴 중)
        public void SetSuperArmorInfinite(bool infinite)
        {
            _superArmor?.SetInfinite(infinite);
        }

        // 페이즈 전환 완료 알림
        public void CompletePhaseTransition()
        {
            _phaseManager?.CompleteTransition();
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

            // 이벤트 해제
            if (_superArmor != null)
            {
                _superArmor.OnPoiseBroken -= HandlePoiseBroken;
            }

            if (_phaseManager != null)
            {
                _phaseManager.OnPhaseTransitionStart -= HandlePhaseTransitionStart;
            }
        }
    }
}
