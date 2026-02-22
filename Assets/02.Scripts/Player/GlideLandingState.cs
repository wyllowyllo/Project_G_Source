using Skill;
using UnityEngine;

namespace Player
{
    public class GlideLandingState : StateMachineBehaviour
    {
        [Header("Animation End Timing (Normalized)")]
        [Range(0f, 1f)]
        [SerializeField] private float _landingCompleteTime = 0.9f;

        [Header("Sound Timing (Normalized)")]
        [Range(0f, 1f)]
        [SerializeField] private float _soundTime = 0.1f;

        private IGlideAnimationReceiver _receiver;
        private GlideController _glideController;
        private bool _landingCompleted;
        private bool _soundPlayed;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            CacheComponents(animator);
            ResetFlags();
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            float time = stateInfo.normalizedTime;

            if (!_soundPlayed && time >= _soundTime)
            {
                _soundPlayed = true;
                _glideController?.PlayLandingSound();
            }

            if (!_landingCompleted && time >= _landingCompleteTime)
            {
                _landingCompleted = true;
                _receiver?.OnLandingComplete();
            }
        }

        private void CacheComponents(Animator animator)
        {
            _receiver ??= animator.GetComponent<IGlideAnimationReceiver>();
            _glideController ??= animator.GetComponent<GlideController>();
        }

        private void ResetFlags()
        {
            _landingCompleted = false;
            _soundPlayed = false;
        }
    }
}
