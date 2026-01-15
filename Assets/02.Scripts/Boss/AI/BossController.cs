using Boss.Ability;
using Boss.AI.States;
using Boss.Combat;
using Boss.Core;
using Boss.Data;
using Combat.Attack;
using Combat.Core;
using Common;
using Monster.Ability;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Boss.AI
{
    [RequireComponent(typeof(NavMeshAgent), typeof(Combatant))]
    public class BossController : MonoBehaviour, IEntityController
    {
        [SerializeField] private EBossState _currentState;
        [SerializeField] private int _currentPhase;

        [Header("설정")]
        [SerializeField] private BossData _bossData;

        [Header("참조")]
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private BossTelegraph _telegraph;

        [Header("Combat 컴포넌트")]
        [SerializeField] private HitboxTrigger _meleeHitbox;
        [SerializeField] private HitboxTrigger _chargeHitbox;
        [SerializeField] private BossBreathAttacker _breathAttacker;
        [SerializeField] private BossProjectileLauncher _projectileLauncher;

        // 컴포넌트
        private NavMeshAgent _navAgent;
        private Combatant _combatant;
        private BossStateMachine _stateMachine;
        private Animator _animator;

        // 핵심 시스템
        private BossSuperArmor _superArmor;
        private BossPhaseManager _phaseManager;
        private BossMinionManager _minionManager;

        // Ability 시스템 (Monster.Ability 재사용)
        private Dictionary<System.Type, EntityAbility> _abilities;
        private List<EntityAbility> _abilityList;

        // 프로퍼티
        public bool IsAlive => _combatant != null && _combatant.IsAlive;
        public BossData Data => _bossData;
        public Combatant Combatant => _combatant;
        public NavMeshAgent NavAgent => _navAgent;
        public Transform PlayerTransform => _playerTransform;
        public Animator Animator => _animator;
        public float RotationSpeed => _bossData != null ? _bossData.RotationSpeed : 0f;
        public BossStateMachine StateMachine => _stateMachine;
        public EBossState CurrentStateType => _stateMachine?.CurrentStateType ?? EBossState.Idle;

        // 핵심 시스템 프로퍼티
        public BossSuperArmor SuperArmor => _superArmor;
        public BossPhaseManager PhaseManager => _phaseManager;
        public BossTelegraph Telegraph => _telegraph;
        public BossMinionManager MinionManager => _minionManager;

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

            // 잡졸 관리 시스템
            _minionManager = GetComponentInChildren<BossMinionManager>();
            if (_minionManager == null)
            {
                GameObject minionManagerObj = new GameObject("MinionManager");
                minionManagerObj.transform.SetParent(transform);
                _minionManager = minionManagerObj.AddComponent<BossMinionManager>();
            }
            _minionManager.Initialize(this);
            _minionManager.OnAllMinionsDead += HandleAllMinionsDead;

            // Telegraph (자동 검색)
            if (_telegraph == null)
            {
                _telegraph = GetComponentInChildren<BossTelegraph>();
            }

            // Combat 컴포넌트 자동 검색
            if (_breathAttacker == null)
            {
                _breathAttacker = GetComponentInChildren<BossBreathAttacker>();
            }
            if (_projectileLauncher == null)
            {
                _projectileLauncher = GetComponentInChildren<BossProjectileLauncher>();
            }

            // 투사체 발사기 설정
            if (_projectileLauncher != null)
            {
                _projectileLauncher.SetProjectileData(
                    _bossData.ProjectilePrefab,
                    _bossData.ProjectileDamage,
                    _bossData.ProjectileSpeed
                );
            }
        }

        private void InitializeStateMachine()
        {
            _stateMachine = new BossStateMachine(this);

            // Phase 3: 기본 상태 등록
            _stateMachine.RegisterState(EBossState.Idle, new BossIdleState(this, _stateMachine));
            _stateMachine.RegisterState(EBossState.Hit, new BossHitState(this, _stateMachine));
            _stateMachine.RegisterState(EBossState.Stagger, new BossStaggerState(this, _stateMachine));
            _stateMachine.RegisterState(EBossState.Dead, new BossDeadState(this));
            _stateMachine.RegisterState(EBossState.PhaseTransition, new BossPhaseTransitionState(this, _stateMachine));

            // Phase 4: 공격 상태 등록
            _stateMachine.RegisterState(EBossState.MeleeAttack, new BossMeleeAttackState(this, _stateMachine));
            _stateMachine.RegisterState(EBossState.Charge, new BossChargeState(this, _stateMachine));
            _stateMachine.RegisterState(EBossState.Breath, new BossBreathState(this, _stateMachine));
            _stateMachine.RegisterState(EBossState.Projectile, new BossProjectileState(this, _stateMachine));
            _stateMachine.RegisterState(EBossState.Summon, new BossSummonState(this, _stateMachine));

            // 초기 상태: Idle
            _stateMachine.Initialize(EBossState.Idle);
        }

        private void InitializeAbilities()
        {
            _abilities = new Dictionary<System.Type, EntityAbility>();
            _abilityList = new List<EntityAbility>();

            // Monster.Ability 재사용
            RegisterAbility(new NavAgentAbility());
            RegisterAbility(new BossPlayerDetectAbility());
            RegisterAbility(new FacingAbility());
            RegisterAbility(new BossAnimatorAbility());
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

        public T GetAbility<T>() where T : EntityAbility
        {
            if (_abilities.TryGetValue(typeof(T), out EntityAbility ability))
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

        #region Combat API

        // 근접 공격 히트박스
        public void EnableMeleeHitbox()
        {
            _meleeHitbox?.EnableHitbox(_combatant.Team);
        }

        public void DisableMeleeHitbox()
        {
            _meleeHitbox?.DisableHitbox();
        }

        // 돌진 히트박스
        public void EnableChargeHitbox()
        {
            _chargeHitbox?.EnableHitbox(_combatant.Team);
        }

        public void DisableChargeHitbox()
        {
            _chargeHitbox?.DisableHitbox();
        }

        // 브레스 공격
        public void StartBreathAttack()
        {
            _breathAttacker?.StartBreath(
                _bossData.BreathAngle,
                _bossData.BreathRange,
                _bossData.BreathDamage
            );
        }

        public void StopBreathAttack()
        {
            _breathAttacker?.StopBreath();
        }

        // 투사체 발사
        public void FireProjectile()
        {
            _projectileLauncher?.Fire();
        }

        public void FireProjectile(int index, int totalCount)
        {
            _projectileLauncher?.Fire(index, totalCount);
        }

        // 잡졸 소환
        public void SpawnMinions()
        {
            if (_minionManager == null) return;

            // 최대 수 제한 확인
            if (!_minionManager.CanSummonMore(_bossData.MaxAliveMinions))
            {
                Debug.Log($"{gameObject.name}: 최대 잡졸 수에 도달하여 소환 불가");
                return;
            }

            _minionManager.SpawnMinions(
                _bossData.SummonPrefabs,
                _bossData.SummonCount,
                _bossData.MinionSpawnRadius
            );
        }

        #endregion

        private void HandleAllMinionsDead()
        {
            // 분노 시스템 연동 (Phase 5에서 구현)
            // _enrageSystem?.OnMinionsDead();
            Debug.Log($"{gameObject.name}: 모든 잡졸이 사망했습니다.");
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

            if (_minionManager != null)
            {
                _minionManager.OnAllMinionsDead -= HandleAllMinionsDead;
            }
        }
    }
}
