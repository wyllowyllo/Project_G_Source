using Skill;
using UnityEngine;

namespace Player
{
    public class DiveBombFlyingState : StateMachineBehaviour
    {
        [Header("Sound Timings (Normalized)")]
        [SerializeField] private float[] _soundTimes = { 0.1f };

        private ISkillAnimationReceiver _receiver;
        private GlideController _glideController;
        private bool[] _soundPlayed;
        private float _lastTime;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            CacheComponents(animator);
            ResetSoundFlags();
            _lastTime = 0f;
            _receiver?.StartSkillTrail();
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (_soundTimes == null || _soundTimes.Length == 0) return;

            float time = stateInfo.normalizedTime % 1f;

            if (time < _lastTime)
            {
                ResetSoundFlags();
            }
            _lastTime = time;

            for (int i = 0; i < _soundTimes.Length; i++)
            {
                if (!_soundPlayed[i] && time >= _soundTimes[i])
                {
                    _soundPlayed[i] = true;
                    _glideController?.PlayDiveBombSound();
                }
            }
        }

        private void ResetSoundFlags()
        {
            if (_soundPlayed == null || _soundPlayed.Length != _soundTimes.Length)
            {
                _soundPlayed = new bool[_soundTimes.Length];
            }

            for (int i = 0; i < _soundPlayed.Length; i++)
            {
                _soundPlayed[i] = false;
            }
        }

        private void CacheComponents(Animator animator)
        {
            _receiver ??= animator.GetComponent<ISkillAnimationReceiver>();
            _glideController ??= animator.GetComponent<GlideController>();
        }
    }
}
