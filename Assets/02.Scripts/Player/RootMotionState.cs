using UnityEngine;

namespace Player
{
    public class RootMotionState : StateMachineBehaviour, IRootMotionRequester
    {
        public bool ForwardOnly => _forwardOnly;

        [Range(0.5f, 1f)]
        [SerializeField] private float _releaseThreshold = 0.85f;

        [Header("Position Multiplier")]
        [SerializeField] private float _positionMultiplier = 1f;

        [Header("Direction Constraint")]
        [Tooltip("체크 시 뒤로 가는 루트 모션을 무시합니다")]
        [SerializeField] private bool _forwardOnly;

        private PlayerMovement _movement;
        private bool _released;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _movement ??= animator.GetComponent<PlayerMovement>();
            _movement?.RequestRootMotion(this, _positionMultiplier);
            _released = false;
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (!_released && stateInfo.normalizedTime >= _releaseThreshold)
            {
                _movement?.ReleaseRootMotion(this);
                _released = true;
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (!_released)
            {
                _movement?.ReleaseRootMotion(this);
            }
        }
    }
}
