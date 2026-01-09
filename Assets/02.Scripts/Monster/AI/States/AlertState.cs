using Monster.Ability;
using UnityEngine;

namespace Monster.AI.States
{
    // 경계 상태: 플레이어 감지 시 활성화, 경계 애니메이션 재생 후 Approach로 전이
    public class AlertState : IMonsterState
    {
        private readonly MonsterController _controller;
        private readonly MonsterStateMachine _stateMachine;

        // Abilities
        private readonly NavAgentAbility _navAgentAbility;
        private readonly PlayerDetectAbility _playerDetectAbility;
        private readonly FacingAbility _facingAbility;

        private bool _isAlertComplete;

        public EMonsterState StateType => EMonsterState.Alert;

        public AlertState(MonsterController controller, MonsterStateMachine stateMachine)
        {
            _controller = controller;
            _stateMachine = stateMachine;

            _navAgentAbility = controller.GetAbility<NavAgentAbility>();
            _playerDetectAbility = controller.GetAbility<PlayerDetectAbility>();
            _facingAbility = controller.GetAbility<FacingAbility>();
        }

        public void Enter()
        {
            _isAlertComplete = false;

            // 이동 정지
            _navAgentAbility?.Stop();

            // 경계 애니메이션 트리거
            _controller.TriggerAlertAnimation(OnAlertComplete);

            Debug.Log($"{_controller.gameObject.name}: 플레이어 감지! 경계 상태 진입");
        }

        public void Update()
        {
            // 플레이어 방향으로 회전
            if (_playerDetectAbility.HasPlayer)
            {
                _facingAbility?.FaceTo(_playerDetectAbility.PlayerPosition);
            }

            // 애니메이션 완료 시 Approach로 전이
            if (_isAlertComplete)
            {
                _stateMachine.ChangeState(EMonsterState.Approach);
            }
        }

        public void Exit()
        {
            _navAgentAbility?.Resume();
        }

        private void OnAlertComplete()
        {
            _isAlertComplete = true;
        }
    }
}
