using Skill;
using UnityEngine;

namespace Player
{
    public class GlidingState : StateMachineBehaviour
    {
        [Header("DiveBomb Timing (Normalized)")]
        [Range(0f, 1f)]
        [SerializeField] private float _diveBombReadyTime = 0.3f;

        private IGlideAnimationReceiver _receiver;
        private bool _diveBombReady;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            CacheReceiver(animator);
            ResetFlags();
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            float time = stateInfo.normalizedTime;

            if (!_diveBombReady && time >= _diveBombReadyTime)
            {
                _diveBombReady = true;
                _receiver?.OnDiveBombReady();
            }
        }

        private void CacheReceiver(Animator animator)
        {
            _receiver ??= animator.GetComponent<IGlideAnimationReceiver>();
        }

        private void ResetFlags()
        {
            _diveBombReady = false;
        }
    }
}
