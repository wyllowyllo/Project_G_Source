using UnityEngine;

namespace Player
{
    public class PlayerAnimationController : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private float _attackAnimationSpeed = 1f;

        private Animator _animator;

        private static readonly int _attackTrigger = Animator.StringToHash("Attack");
        private static readonly int _comboStepParameters = Animator.StringToHash("ComboStep");
        private static readonly int _damageTrigger = Animator.StringToHash("Damage");
        private static readonly int _deathTrigger = Animator.StringToHash("Death");
        private static readonly int _attackSpeedParameters = Animator.StringToHash("AttackSpeed");
        private static readonly int _attackEndTrigger = Animator.StringToHash("AttackEnd");
        private static readonly int _dodgeTrigger = Animator.StringToHash("Dodge");

        private bool HasAnimator => _animator != null;

        private void Awake()
        {
            _animator = GetComponent<Animator>();

            if (!HasAnimator)
            {
                Debug.LogError("[PlayerAnimationController] Animator 컴포넌트 필요");
                return;
            }

            SetAttackSpeed(_attackAnimationSpeed);
        }

        public void PlayAttack(int comboStep)
        {
            if (!HasAnimator) return;

            _animator.SetInteger(_comboStepParameters, comboStep);
            _animator.SetTrigger(_attackTrigger);
        }

        public void PlayDamage()
        {
            if (!HasAnimator) return;

            _animator.SetTrigger(_damageTrigger);
        }

        public void PlayDeath()
        {
            if (!HasAnimator) return;

            _animator.SetTrigger(_deathTrigger);
        }

        public void PlayDodge()
        {
            if (!HasAnimator) return;

            _animator.SetTrigger(_dodgeTrigger);
        }

        public void EndAttack()
        {
            if (!HasAnimator) return;

            _animator.SetTrigger(_attackEndTrigger);
        }

        public void SetAttackSpeed(float speed)
        {
            if (!HasAnimator) return;

            _animator.SetFloat(_attackSpeedParameters, speed);
        }

        public AnimatorStateInfo GetCurrentAnimatorStateInfo(int layerIndex = 0)
        {
            return HasAnimator ? _animator.GetCurrentAnimatorStateInfo(layerIndex) : default;
        }

        public bool IsPlayingAnimation(string stateName, int layerIndex = 0)
        {
            if (!HasAnimator) return false;

            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(layerIndex);
            return stateInfo.IsName(stateName);
        }
    }
}
