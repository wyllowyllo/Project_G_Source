using Monster.Ability;
using UnityEngine;

namespace Monster.AI.States
{
    // 공격 상태: Windup (준비) → Perform (애니메이션 실행) 2단계로 구성
    // NavMeshAgent 이동 없이 애니메이션이 Body를 이동시킴
    public class AttackState : IMonsterState
    {
        private readonly MonsterController _controller;
        private readonly MonsterStateMachine _stateMachine;
        private readonly GroupCommandProvider _groupCommandProvider;
        private readonly Transform _transform;

        // Abilities
        private readonly NavAgentAbility _navAgentAbility;
        private readonly PlayerDetectAbility _playerDetectAbility;
        private readonly FacingAbility _facingAbility;
        private readonly AnimatorAbility _animatorAbility;

        private enum EAttackPhase { Windup, Perform }
        private EAttackPhase _currentPhase;

        private bool _isHeavyAttack;
        private float _phaseTimer;
        private bool _isAttackComplete;

        public EMonsterState StateType => EMonsterState.Attack;

        public AttackState(MonsterController controller, MonsterStateMachine stateMachine, GroupCommandProvider groupCommandProvider)
        {
            _controller = controller;
            _stateMachine = stateMachine;
            _groupCommandProvider = groupCommandProvider;
            _transform = controller.transform;

            _navAgentAbility = controller.GetAbility<NavAgentAbility>();
            _playerDetectAbility = controller.GetAbility<PlayerDetectAbility>();
            _facingAbility = controller.GetAbility<FacingAbility>();
            _animatorAbility = controller.GetAbility<AnimatorAbility>();
        }

        public void Enter()
        {
            // NavAgent 정지
            _navAgentAbility?.Stop();

            DetermineAttackType();

            // 약공일 때: 플레이어와 직접 대면한 경우만 공격 (다른 몬스터에게 가로막힌 경우 Strafe)
            if (!_isHeavyAttack && !HasDirectLineOfSightToPlayer())
            {
                Debug.Log($"{_controller.gameObject.name}: 약공 취소 (플레이어 시야 차단됨) - Strafe로 전환");
                _stateMachine.ChangeState(EMonsterState.Strafe);
                return;
            }

            InitializeAttackPhase();

            Debug.Log($"{_controller.gameObject.name}: 공격 시작 (Windup) - {(_isHeavyAttack ? "강공" : "약공")}");
        }

        public void Update()
        {
            // 강공인데 슬롯 잃으면 취소
            if (_isHeavyAttack && !_groupCommandProvider.CanAttack())
            {
                ReturnToCombat();
                return;
            }

            _phaseTimer += Time.deltaTime;

            switch (_currentPhase)
            {
                case EAttackPhase.Windup:
                    UpdateWindupPhase();
                    break;

                case EAttackPhase.Perform:
                    UpdatePerformPhase();
                    break;
            }
        }

        public void Exit()
        {
            // NavAgent 재개
            _navAgentAbility?.Resume();
        }

        private void UpdateWindupPhase()
        {
            // Windup 중 플레이어가 너무 멀어지면 취소
            float attackRange = _isHeavyAttack ? _controller.Data.HeavyAttackRange : _controller.Data.LightAttackRange;
            if (_playerDetectAbility.DistanceToPlayer > attackRange * 2f)
            {
                ReturnToCombat();
                return;
            }

            // 플레이어 방향으로 회전
            LookAtPlayer();

            // Windup 시간 완료 시 공격 애니메이션 실행
            if (_phaseTimer >= _controller.Data.WindupTime)
            {
                StartPerformPhase();
            }
        }

        private void UpdatePerformPhase()
        {
            // 애니메이션 완료 대기
            if (_isAttackComplete)
            {
                _stateMachine.ChangeState(EMonsterState.Recover);
            }
        }

        private void StartPerformPhase()
        {
            _currentPhase = EAttackPhase.Perform;
            _phaseTimer = 0f;
            _isAttackComplete = false;

            // 공격 애니메이션 트리거
            _animatorAbility?.TriggerAttack(_isHeavyAttack, OnAnimationComplete);

            Debug.Log($"{_controller.gameObject.name}: 공격 실행 (Perform) - {(_isHeavyAttack ? "강공" : "약공")}");
        }

        private void OnAnimationComplete()
        {
            _isAttackComplete = true;
        }

        private void DetermineAttackType()
        {
            _isHeavyAttack = _groupCommandProvider.NextAttackIsHeavy;
            _groupCommandProvider.MarkCurrentAttackHeavy(_isHeavyAttack);
            _groupCommandProvider.SetNextAttackHeavy(false);
        }

        private void InitializeAttackPhase()
        {
            _currentPhase = EAttackPhase.Windup;
            _phaseTimer = 0f;
            _isAttackComplete = false;
        }

        private void LookAtPlayer()
        {
            if (_playerDetectAbility.HasPlayer)
            {
                _facingAbility.FaceTo(_playerDetectAbility.PlayerPosition);
            }
        }

        private void ReturnToCombat()
        {
            if (_playerDetectAbility.IsTooFar())
                _stateMachine.ChangeState(EMonsterState.Approach);
            else
                _stateMachine.ChangeState(EMonsterState.Strafe);
        }

        private bool HasDirectLineOfSightToPlayer()
        {
            if (!_playerDetectAbility.HasPlayer)
                return false;

            Vector3 startPosition = _transform.position + Vector3.up * 1.0f;
            Vector3 playerPosition = _playerDetectAbility.PlayerPosition + Vector3.up * 1.0f;
            Vector3 directionToPlayer = playerPosition - startPosition;
            float distance = directionToPlayer.magnitude;

            if (Physics.Raycast(startPosition, directionToPlayer.normalized, out RaycastHit hit, distance))
            {
                if (hit.transform != _playerDetectAbility.PlayerTransform)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
