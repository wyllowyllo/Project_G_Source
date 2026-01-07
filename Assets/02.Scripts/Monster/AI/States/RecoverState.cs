using UnityEngine;

namespace Monster.AI.States
{
    /// <summary>
    /// 회복 상태
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
        
        private void StartRetreat()
        {
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

           
            _recoverTimer += Time.deltaTime;

            if (_recoverTimer >= _controller.Data.RecoverTime)
            {
                TransitionBackToCombat();
            }
        }
        
        private void TransitionBackToCombat()
        {
            
            float distanceToPlayer = Vector3.Distance(_transform.position, _controller.PlayerTransform.position);

            
            if (distanceToPlayer > _controller.Data.PreferredMaxDistance)
            {
                
                _stateMachine.ChangeState(EMonsterState.Approach);
            }
            else
            {
                
                _stateMachine.ChangeState(EMonsterState.Strafe);
            }
        }

        public void Exit()
        {
            if (_controller.NavAgent != null)
            {
                _controller.NavAgent.isStopped = false;
            }

            // 머티리얼 색상을 원래대로 복원
            Renderer renderer = _controller.GetComponentInChildren<Renderer>();
            if (renderer != null && renderer.material != null)
            {
                renderer.material.color = _controller.OriginalMaterialColor;
            }

            // 공격 슬롯 반환
            if (_controller.EnemyGroup != null)
            {
                _controller.EnemyGroup.ReleaseAttackSlot(_controller);
            }
        }
    }
}
