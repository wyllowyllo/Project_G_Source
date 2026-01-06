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
        private Vector3 _retreatTarget;
        private bool _isRetreating = false;

        public EMonsterState StateType => EMonsterState.Recover;

        public RecoverState(MonsterController controller, MonsterStateMachine stateMachine)
        {
            _controller = controller;
            _stateMachine = stateMachine;
            _transform = controller.transform;
        }

        public void Enter()
        {
            _recoverTimer = 0f;

            // 후퇴 시작
            StartRetreat();
        }

        /// <summary>
        /// 플레이어 반대 방향으로 후퇴 시작
        /// </summary>
        private void StartRetreat()
        {
            if (_controller.NavAgent == null || _controller.PlayerTransform == null)
            {
                _isRetreating = false;
                return;
            }

            // 플레이어 반대 방향 계산
            Vector3 directionFromPlayer = (_transform.position - _controller.PlayerTransform.position).normalized;
            directionFromPlayer.y = 0f;

            // 후퇴 목표 지점 설정
            _retreatTarget = _transform.position + directionFromPlayer * _controller.Data.RetreatDistance;

            // NavMesh로 후퇴
            _controller.NavAgent.isStopped = false;
            _controller.NavAgent.SetDestination(_retreatTarget);
            _isRetreating = true;

            Debug.Log($"{_controller.gameObject.name}: 후퇴 시작");
        }

        public void Update()
        {
            if (_controller.PlayerTransform == null)
            {
                _stateMachine.ChangeState(EMonsterState.Idle);
                return;
            }

            // 후퇴 중이면 도착 여부 체크
            if (_isRetreating)
            {
                float distanceToRetreatTarget = Vector3.Distance(_transform.position, _retreatTarget);

                // 후퇴 목표에 도착하면 후퇴 종료
                if (distanceToRetreatTarget <= 0.5f || !_controller.NavAgent.pathPending && _controller.NavAgent.remainingDistance <= 0.5f)
                {
                    _isRetreating = false;
                    _controller.NavAgent.isStopped = true;
                    Debug.Log($"{_controller.gameObject.name}: 후퇴 완료");
                }
            }

            // 회복 시간 체크 (후퇴와 동시에 진행)
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
                _stateMachine.ChangeState(EMonsterState.Idle);
                return;
            }

            // 거리에 따라 상태 전환
            if (distanceToPlayer > _controller.Data.PreferredMaxDistance)
            {
                // 거리 밴드 밖이면 Approach
                _stateMachine.ChangeState(EMonsterState.Approach);
            }
            else
            {
                // 거리 밴드 안이면 Strafe
                _stateMachine.ChangeState(EMonsterState.Strafe);
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
