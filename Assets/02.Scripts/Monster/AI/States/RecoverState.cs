using UnityEngine;

namespace Monster.AI.States
{
    /// <summary>
    /// 회복 상태
    /// 공격 후 그 자리에서 잠깐 멈춰서 추스른 후 다시 전투로 복귀합니다.
    /// </summary>
    public class RecoverState : IMonsterState
    {
        private readonly MonsterController _controller;
        private readonly MonsterStateMachine _stateMachine;
        private readonly Transform _transform;

        private float _recoverTimer = 0f;
        private const float RecoverDuration = 2f; // 2초간 추스름

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

            // 그 자리에서 멈춤
            if (_controller.NavAgent != null && _controller.NavAgent.isActiveAndEnabled)
            {
                _controller.NavAgent.isStopped = true;
            }

            Debug.Log($"{_controller.gameObject.name}: 공격 후 추스르는 중...");
        }

        public void Update()
        {
            _recoverTimer += Time.deltaTime;

            if (_recoverTimer >= RecoverDuration)
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
