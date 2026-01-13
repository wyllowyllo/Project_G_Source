using Skill;
using UnityEngine;

namespace Player
{
    public class SkillState : StateMachineBehaviour
    {
        [Header("Damage Timing (Normalized)")]
        [Range(0f, 1f)]
        [SerializeField] private float _damageFrameTime = 0.3f;

        [Header("Trail Timing (Normalized)")]
        [Range(0f, 1f)]
        [SerializeField] private float _trailStartTime = 0.15f;
        [Range(0f, 1f)]
        [SerializeField] private float _trailEndTime = 0.65f;

        [Header("Animation End Timing (Normalized)")]
        [Range(0f, 1f)]
        [SerializeField] private float _animEndTime = 0.9f;

        private ISkillAnimationReceiver _receiver;

        private bool _damageApplied;
        private bool _trailStarted;
        private bool _trailEnded;
        private bool _animEnded;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            CacheReceiver(animator);
            ResetFlags();
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            float time = stateInfo.normalizedTime;

            if (!_trailStarted && time >= _trailStartTime)
            {
                _trailStarted = true;
                _receiver?.StartSkillTrail();
            }

            if (!_damageApplied && time >= _damageFrameTime)
            {
                _damageApplied = true;
                _receiver?.OnSkillDamageFrame();
            }

            if (!_trailEnded && _trailStarted && time >= _trailEndTime)
            {
                _trailEnded = true;
                _receiver?.StopSkillTrail();
            }

            if (!_animEnded && time >= _animEndTime)
            {
                _animEnded = true;
                _receiver?.OnSkillComplete();
            }
        }

        private void CacheReceiver(Animator animator)
        {
            _receiver ??= animator.GetComponent<ISkillAnimationReceiver>();
        }

        private void ResetFlags()
        {
            _damageApplied = false;
            _trailStarted = false;
            _trailEnded = false;
            _animEnded = false;
        }
    }
}
