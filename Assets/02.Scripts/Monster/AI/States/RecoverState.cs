using Monster.Ability;
using UnityEngine;

namespace Monster.AI.States
{
    // 회복 상태: 공격 후 그 자리에서 잠깐 멈춰서 추스른 후 다시 전투로 복귀 (Ability 기반 리팩터링)
    public class RecoverState : IMonsterState
    {
        private readonly MonsterController _controller;
        private readonly MonsterStateMachine _stateMachine;
        private readonly GroupCommandProvider _groupCommandProvider;
        private readonly Transform _transform;

        // Abilities
        private readonly NavAgentAbility _navAgentAbility;
        private readonly PlayerDetectAbility _playerDetectAbility;

        private float _recoverTimer = 0f;
        private float _recoverDuration;

        public EMonsterState StateType => EMonsterState.Recover;

        public RecoverState(MonsterController controller, MonsterStateMachine stateMachine, GroupCommandProvider groupCommandProvider)
        {
            _controller = controller;
            _stateMachine = stateMachine;
            _groupCommandProvider = groupCommandProvider;
            _transform = controller.transform;

           
            _navAgentAbility = controller.GetAbility<NavAgentAbility>();
            _playerDetectAbility = controller.GetAbility<PlayerDetectAbility>();
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
           
            if (_playerDetectAbility.IsTooFar())
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
                ? _controller.Data.HeavyAttackRecoverTime
                : _controller.Data.LightAttackRecoverTime;
        }

        private void StopNavigation()
        {
            
            _navAgentAbility?.Stop();
        }

        private void ResumeNavigation()
        {
            
            _navAgentAbility?.Resume();
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
