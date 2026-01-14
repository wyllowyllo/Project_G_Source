using Skill;
using UnityEngine;

namespace Player
{
    public class GlideSkillState : StateMachineBehaviour
    {
        [Header("Jump Timing (Normalized)")]
        [Range(0f, 1f)]
        [SerializeField] private float _jumpExecuteTime = 0.2f;

        private IGlideAnimationReceiver _receiver;
        private bool _jumpExecuted;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            CacheReceiver(animator);
            ResetFlags();
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            float time = stateInfo.normalizedTime;

            if (!_jumpExecuted && time >= _jumpExecuteTime)
            {
                _jumpExecuted = true;
                _receiver?.OnJumpExecute();
            }
        }

        private void CacheReceiver(Animator animator)
        {
            _receiver ??= animator.GetComponent<IGlideAnimationReceiver>();
        }

        private void ResetFlags()
        {
            _jumpExecuted = false;
        }
    }
}
