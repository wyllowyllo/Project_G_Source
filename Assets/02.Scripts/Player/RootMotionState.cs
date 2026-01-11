using UnityEngine;

namespace Player
{
    public class RootMotionState : StateMachineBehaviour
    {
        [Range(0.5f, 1f)]
        [SerializeField] private float _releaseThreshold = 0.85f;

        private PlayerMovement _movement;
        private bool _released;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _movement ??= animator.GetComponent<PlayerMovement>();
            _movement?.RequestRootMotion();
            _released = false;
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (!_released && stateInfo.normalizedTime >= _releaseThreshold)
            {
                _movement?.ReleaseRootMotion();
                _released = true;
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (!_released)
            {
                _movement?.ReleaseRootMotion();
            }
        }
    }
}
