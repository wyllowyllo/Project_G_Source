using Skill;
using UnityEngine;

namespace Player
{
    public class SuperJumpState : StateMachineBehaviour
    {
        [Header("Jump Timing (Normalized)")]
        [Range(0f, 1f)]
        [SerializeField] private float _jumpExecuteTime = 0.2f;

        [Header("Sound Timing (Normalized)")]
        [Range(0f, 1f)]
        [SerializeField] private float _soundTime = 0.1f;

        [Header("Continuous Sound")]
        [SerializeField] private bool _startContinuousSound = true;
        [Range(0f, 1f)]
        [SerializeField] private float _continuousSoundStartTime = 0.2f;

        private IGlideAnimationReceiver _receiver;
        private GlideController _glideController;
        private bool _jumpExecuted;
        private bool _soundPlayed;
        private bool _continuousSoundStarted;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            CacheComponents(animator);
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

            if (!_soundPlayed && time >= _soundTime)
            {
                _soundPlayed = true;
                _glideController?.PlaySuperJumpSound();
            }

            if (_startContinuousSound && !_continuousSoundStarted && time >= _continuousSoundStartTime)
            {
                _continuousSoundStarted = true;
                _glideController?.StartContinuousSound();
            }
        }

        private void CacheComponents(Animator animator)
        {
            _receiver ??= animator.GetComponent<IGlideAnimationReceiver>();
            _glideController ??= animator.GetComponent<GlideController>();
        }

        private void ResetFlags()
        {
            _jumpExecuted = false;
            _soundPlayed = false;
            _continuousSoundStarted = false;
        }
    }
}
