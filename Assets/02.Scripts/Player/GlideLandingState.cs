using Skill;
using UnityEngine;

namespace Player
{
    public class GlideLandingState : StateMachineBehaviour
    {
        [Header("Animation End Timing (Normalized)")]
        [Range(0f, 1f)]
        [SerializeField] private float _landingCompleteTime = 0.9f;

        private IGlideAnimationReceiver _receiver;
        private bool _landingCompleted;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            CacheReceiver(animator);
            ResetFlags();
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            float time = stateInfo.normalizedTime;

            if (!_landingCompleted && time >= _landingCompleteTime)
            {
                _landingCompleted = true;
                _receiver?.OnLandingComplete();
            }
        }

        private void CacheReceiver(Animator animator)
        {
            _receiver ??= animator.GetComponent<IGlideAnimationReceiver>();
        }

        private void ResetFlags()
        {
            _landingCompleted = false;
        }
    }
}
