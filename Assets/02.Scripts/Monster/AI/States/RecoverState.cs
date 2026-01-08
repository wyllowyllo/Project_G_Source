using UnityEngine;

namespace Monster.AI.States
{
    // 회복 상태: 공격 후 그 자리에서 잠깐 멈춰서 추스른 후 다시 전투로 복귀
    public class RecoverState : IMonsterState
    {
        private readonly MonsterController _controller;
        private readonly MonsterStateMachine _stateMachine;
        private readonly GroupCommandProvider _groupCommandProvider;
        private readonly Transform _transform;

        private float _recoverTimer = 0f;
        private float _recoverDuration; // 동적으로 설정 (강공/약공에 따라)

        private const float HeavyAttackRecoverTime = 2f; // 강공: 2초
        private const float LightAttackRecoverTime = 0.5f; // 약공: 0.5초

        public EMonsterState StateType => EMonsterState.Recover;

        public RecoverState(MonsterController controller, MonsterStateMachine stateMachine, GroupCommandProvider groupCommandProvider)
        {
            _controller = controller;
            _stateMachine = stateMachine;
            _groupCommandProvider = groupCommandProvider;
            _transform = controller.transform;
        }

        public void Enter()
        {
            _recoverTimer = 0f;
            DetermineRecoverDuration();
            StopNavigation();

            string attackType = _groupCommandProvider.CurrentAttackWasHeavy ? "강공" : "약공";
            Debug.Log($"{_controller.gameObject.name}: {attackType} 후 추스르는 중... ({_recoverDuration}초)");
        }

        public void Update()
        {
            _recoverTimer += Time.deltaTime;

            if (_recoverTimer >= _recoverDuration)
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
            ResumeNavigation();
            RestoreMaterialColor();
            ReleaseAttackResources();
        }

        private void DetermineRecoverDuration()
        {
            _recoverDuration = _groupCommandProvider.CurrentAttackWasHeavy
                ? HeavyAttackRecoverTime
                : LightAttackRecoverTime;
        }

        private void StopNavigation()
        {
            if (_controller.NavAgent != null && _controller.NavAgent.isActiveAndEnabled)
            {
                _controller.NavAgent.isStopped = true;
            }
        }

        private void ResumeNavigation()
        {
            if (_controller.NavAgent != null)
            {
                _controller.NavAgent.isStopped = false;
            }
        }

        private void RestoreMaterialColor()
        {
            Renderer renderer = _controller.GetComponentInChildren<Renderer>();
            if (renderer != null && renderer.material != null)
            {
                renderer.material.color = _controller.OriginalMaterialColor;
            }
        }

        private void ReleaseAttackResources()
        {
            if (_groupCommandProvider.CurrentAttackWasHeavy)
            {
                _groupCommandProvider.ReleaseAttackSlot();
            }

            _groupCommandProvider.ClearCurrentAttackHeavy();
        }
    }
}
