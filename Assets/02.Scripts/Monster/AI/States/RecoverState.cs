using UnityEngine;

namespace Monster
{
    /// <summary>
    /// BDO 스타일 - 회복 상태.
    /// 공격 후 짧은 후딜 시간을 가진 후 다시 전투로 복귀합니다.
    /// </summary>
    public class RecoverState : IMonsterState
    {
        private readonly MonsterController _controller;
        private readonly MonsterStateMachine _stateMachine;
        private readonly Transform _transform;

        private float _recoverTimer = 0f;

        public MonsterState StateType => MonsterState.Recover;

        public RecoverState(MonsterController controller, MonsterStateMachine stateMachine)
        {
            _controller = controller;
            _stateMachine = stateMachine;
            _transform = controller.transform;
        }

        public void Enter()
        {
            // 이동 정지
            if (_controller.NavAgent != null)
            {
                _controller.NavAgent.isStopped = true;
            }

            _recoverTimer = 0f;
        }

        public void Update()
        {
            if (_controller.PlayerTransform == null)
            {
                _stateMachine.ChangeState(MonsterState.Idle);
                return;
            }

            // 회복 시간 체크
            _recoverTimer += Time.deltaTime;

            if (_recoverTimer >= _controller.Data.RecoverTime)
            {
                // 회복 완료 - 다시 전투로 복귀
                TransitionBackToCombat();
            }
        }

        /// <summary>
        /// 전투 상태로 복귀 (거리에 따라 Approach 또는 Strafe)
        /// </summary>
        private void TransitionBackToCombat()
        {
            float distanceToPlayer = Vector3.Distance(
                _transform.position,
                _controller.PlayerTransform.position
            );

            // 감지 범위를 벗어나면 Idle로 복귀 (그룹 시스템이 있으면 이 체크는 무시)
            if (_controller.EnemyGroup == null && distanceToPlayer > _controller.Data.DetectionRange)
            {
                _stateMachine.ChangeState(MonsterState.Idle);
                return;
            }

            // 거리에 따라 상태 전환
            if (distanceToPlayer > _controller.Data.PreferredMaxDistance)
            {
                // 거리 밴드 밖이면 Approach
                _stateMachine.ChangeState(MonsterState.Approach);
            }
            else
            {
                // 거리 밴드 안이면 Strafe
                _stateMachine.ChangeState(MonsterState.Strafe);
            }
        }

        public void Exit()
        {
            // NavAgent 재개
            if (_controller.NavAgent != null)
            {
                _controller.NavAgent.isStopped = false;
            }
        }
    }
}
