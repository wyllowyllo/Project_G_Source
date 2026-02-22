using UnityEngine;

namespace Player
{
    public class DodgeState : StateMachineBehaviour
    {
        [Header("Sound Timing (Normalized)")]
        [Range(0f, 1f)]
        [SerializeField] private float _soundTime = 0.1f;

        private PlayerVFXController _vfxController;
        private bool _soundPlayed;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            CacheComponents(animator);
            _soundPlayed = false;
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            float time = stateInfo.normalizedTime;

            if (!_soundPlayed && time >= _soundTime)
            {
                _soundPlayed = true;
                _vfxController?.PlayDodgeSFX();
            }
        }

        private void CacheComponents(Animator animator)
        {
            _vfxController ??= animator.GetComponent<PlayerVFXController>();
        }
    }
}
