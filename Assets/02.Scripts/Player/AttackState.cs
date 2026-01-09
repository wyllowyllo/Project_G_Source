using Combat.Attack;
using UnityEngine;

namespace Player
{
    public class AttackState : StateMachineBehaviour
    {
        [Header("Hitbox Timing (Normalized)")]
        [Range(0f, 1f)]
        [SerializeField] private float _hitStartTime = 0.2f;
        [Range(0f, 1f)]
        [SerializeField] private float _hitEndTime = 0.6f;

        [Header("Trail Timing (Normalized)")]
        [Range(0f, 1f)]
        [SerializeField] private float _trailStartTime = 0.15f;
        [Range(0f, 1f)]
        [SerializeField] private float _trailEndTime = 0.65f;

        private MeleeAttacker _attacker;
        private PlayerVFXController _vfxController;

        private bool _hitStarted;
        private bool _hitEnded;
        private bool _trailStarted;
        private bool _trailEnded;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            CacheComponents(animator);
            ResetFlags();
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            float time = stateInfo.normalizedTime;
            
            if (!_hitStarted && time >= _hitStartTime)
            {
                _hitStarted = true;
                _attacker?.OnAttackHitStart();
            }
            
            if (!_hitEnded && time >= _hitEndTime)
            {
                _hitEnded = true;
                _attacker?.ForceDisableHitbox();
            }
            
            if (!_trailStarted && time >= _trailStartTime)
            {
                _trailStarted = true;
                _vfxController?.StartTrail();
            }
            
            if (!_trailEnded && time >= _trailEndTime)
            {
                _trailEnded = true;
                _vfxController?.StopAllEffects();
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (!_hitEnded)
            {
                _attacker?.ForceDisableHitbox();
            }

            if (!_trailEnded)
            {
                _vfxController?.StopAllEffects();
            }
        }

        private void CacheComponents(Animator animator)
        {
            if (_attacker == null)
            {
                _attacker = animator.GetComponent<MeleeAttacker>();
            }

            if (_vfxController == null)
            {
                _vfxController = animator.GetComponent<PlayerVFXController>();
            }
        }

        private void ResetFlags()
        {
            _hitStarted = false;
            _hitEnded = false;
            _trailStarted = false;
            _trailEnded = false;
        }
    }
}
