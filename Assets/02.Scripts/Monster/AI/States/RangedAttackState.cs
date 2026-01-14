using Monster.Ability;
using Monster.Combat;
using UnityEngine;

namespace Monster.AI.States
{
    // 원거리 공격 상태: Aim (조준) → Fire (발사) 2단계로 구성
    public class RangedAttackState : IMonsterState
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

        private enum ERangedPhase { Aim, Fire }
        private ERangedPhase _currentPhase;

        private bool _isHeavyAttack;
        private float _phaseTimer;
        private bool _isAttackComplete;

        public EMonsterState StateType => EMonsterState.Attack;

        public RangedAttackState(MonsterController controller, MonsterStateMachine stateMachine, GroupCommandProvider groupCommandProvider)
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
            _navAgentAbility?.Stop();

            DetermineAttackType();

            // 시야 체크 - 플레이어가 보이지 않으면 이동
            if (!HasLineOfSightToPlayer())
            {
                Debug.Log($"{_controller.gameObject.name}: 원거리 공격 취소 (시야 차단됨) - Approach로 전환");
                _stateMachine.ChangeState(EMonsterState.Approach);
                return;
            }

            // 너무 가까우면 후퇴 (원거리는 최소 거리 유지 필요)
            if (_playerDetectAbility.DistanceToPlayer < _controller.Data.RangedMinDistance)
            {
                Debug.Log($"{_controller.gameObject.name}: 원거리 공격 취소 (너무 가까움) - Strafe로 전환");
                _stateMachine.ChangeState(EMonsterState.Strafe);
                return;
            }

            InitializePhase();
            Debug.Log($"{_controller.gameObject.name}: 원거리 공격 시작 (Aim) - {(_isHeavyAttack ? "강공" : "약공")}");
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
                case ERangedPhase.Aim:
                    UpdateAimPhase();
                    break;

                case ERangedPhase.Fire:
                    UpdateFirePhase();
                    break;
            }
        }

        public void Exit()
        {
            _navAgentAbility?.Resume();
        }

        private void UpdateAimPhase()
        {
            // 조준 중 플레이어가 너무 멀어지면 취소
            if (_playerDetectAbility.DistanceToPlayer > _controller.Data.RangedAttackRange)
            {
                ReturnToCombat();
                return;
            }

            // 조준 중 시야 상실 시 취소
            if (!HasLineOfSightToPlayer())
            {
                ReturnToCombat();
                return;
            }

            // 플레이어 방향으로 회전
            LookAtPlayer();

            // Aim 시간 완료 시 발사
            if (_phaseTimer >= _controller.Data.WindupTime)
            {
                StartFirePhase();
            }
        }

        private void UpdateFirePhase()
        {
            // 발사 완료 대기
            if (_isAttackComplete)
            {
                _stateMachine.ChangeState(EMonsterState.Recover);
            }
        }

        private void StartFirePhase()
        {
            _currentPhase = ERangedPhase.Fire;
            _phaseTimer = 0f;
            _isAttackComplete = false;

            // 공격 애니메이션 트리거 + 투사체 발사는 애니메이션 이벤트로 처리
            _animatorAbility?.TriggerAttack(_isHeavyAttack, OnAnimationComplete);

            Debug.Log($"{_controller.gameObject.name}: 원거리 발사 (Fire) - {(_isHeavyAttack ? "강공" : "약공")}");
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

        private void InitializePhase()
        {
            _currentPhase = ERangedPhase.Aim;
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
            if (_playerDetectAbility.DistanceToPlayer > _controller.Data.RangedAttackRange)
                _stateMachine.ChangeState(EMonsterState.Approach);
            else
                _stateMachine.ChangeState(EMonsterState.Strafe);
        }

        private bool HasLineOfSightToPlayer()
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
