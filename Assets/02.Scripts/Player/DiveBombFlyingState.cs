using Skill;
using UnityEngine;

namespace Player
{
    public class DiveBombFlyingState : StateMachineBehaviour
    {
        private ISkillAnimationReceiver _receiver;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            CacheReceiver(animator);
            _receiver?.StartSkillTrail();
        }

        private void CacheReceiver(Animator animator)
        {
            _receiver ??= animator.GetComponent<ISkillAnimationReceiver>();
        }
    }
}
