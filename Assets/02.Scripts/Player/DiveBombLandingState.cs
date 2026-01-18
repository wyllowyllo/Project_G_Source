using Skill;
using UnityEngine;

namespace Player
{
    public class DiveBombLandingState : StateMachineBehaviour
    {
        [Header("Trail Timing (Normalized)")]
        [Range(0f, 1f)]
        [SerializeField] private float _trailEndTime = 0.3f;

        [Header("Animation End Timing (Normalized)")]
        [Range(0f, 1f)]
        [SerializeField] private float _landingCompleteTime = 0.9f;

        private IGlideAnimationReceiver _glideReceiver;
        private ISkillAnimationReceiver _skillReceiver;
        private bool _trailEnded;
        private bool _landingCompleted;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            CacheReceiver(animator);
            ResetFlags();
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            float time = stateInfo.normalizedTime;

            if (!_trailEnded && time >= _trailEndTime)
            {
                _trailEnded = true;
                _skillReceiver?.StopSkillTrail();
            }

            if (!_landingCompleted && time >= _landingCompleteTime)
            {
                _landingCompleted = true;
                _glideReceiver?.OnDiveBombLandingComplete();
            }
        }

        private void CacheReceiver(Animator animator)
        {
            _glideReceiver ??= animator.GetComponent<IGlideAnimationReceiver>();
            _skillReceiver ??= animator.GetComponent<ISkillAnimationReceiver>();
        }

        private void ResetFlags()
        {
            _trailEnded = false;
            _landingCompleted = false;
        }
    }
}
