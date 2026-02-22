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

        [Header("Animation End Timing (Normalized)")]
        [Range(0f, 1f)]
        [SerializeField] private float _animEndTime = 1f;

        [Header("Sound Timing (Normalized)")]
        [Range(0f, 1f)]
        [SerializeField] private float _soundTime = 0.1f;

        private MeleeAttacker _attacker;
        private PlayerVFXController _vfxController;
        private PlayerAnimationEventReceiver _eventReceiver;

        private AttackSession _session;
        private bool _hitStarted;
        private bool _hitEnded;
        private bool _trailStarted;
        private bool _trailEnded;
        private bool _animEnded;
        private bool _soundPlayed;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            CacheComponents(animator);
            ResetFlags();

            int comboStep = _attacker?.CurrentComboStep ?? 0;
            _session = _attacker?.StartNewSession(comboStep);
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (_session == null || !_session.IsActive)
                return;

            float time = stateInfo.normalizedTime;

            // Hitbox
            if (!_hitStarted && time >= _hitStartTime)
            {
                _hitStarted = true;
                _attacker?.OnAttackHitStart(_session);
            }

            if (!_hitEnded && _hitStarted && time >= _hitEndTime)
            {
                _hitEnded = true;
                _attacker?.OnAttackHitEnd(_session);
            }

            // Trail
            if (!_trailStarted && time >= _trailStartTime)
            {
                _trailStarted = true;
                _vfxController?.StartTrail(_session);
            }

            if (!_trailEnded && _trailStarted && time >= _trailEndTime)
            {
                _trailEnded = true;
                _vfxController?.StopTrail(_session);
            }

            // AnimationEnd
            if (!_animEnded && time >= _animEndTime)
            {
                _animEnded = true;
                _eventReceiver?.OnAttackAnimationEnd(_session);
            }

            // Sound
            if (!_soundPlayed && time >= _soundTime)
            {
                _soundPlayed = true;
                int comboStep = _attacker?.CurrentComboStep ?? 0;
                _vfxController?.PlayAttackSFX(comboStep);
            }
        }

        // OnStateExit에서는 아무것도 하지 않음
        // 정리는 다음 상태의 OnStateEnter 또는 콤보 종료 시 수행

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

            if (_eventReceiver == null)
            {
                _eventReceiver = animator.GetComponent<PlayerAnimationEventReceiver>();
            }
        }

        private void ResetFlags()
        {
            _hitStarted = false;
            _hitEnded = false;
            _trailStarted = false;
            _trailEnded = false;
            _animEnded = false;
            _soundPlayed = false;
        }
    }
}
