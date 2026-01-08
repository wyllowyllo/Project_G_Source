using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float _attackAnimationSpeed = 1f;

    private Animator _animator;

    private static readonly int _attackTrigger = Animator.StringToHash("Attack");
    private static readonly int _comboStepParameters = Animator.StringToHash("ComboStep");
    private static readonly int _isAttackingParameters = Animator.StringToHash("IsAttacking");
    private static readonly int _damageTrigger = Animator.StringToHash("Damage");
    private static readonly int _deathTrigger = Animator.StringToHash("Death");
    private static readonly int _attackSpeedParameters = Animator.StringToHash("AttackSpeed");

    private void Awake()
    {
        _animator = GetComponent<Animator>();

        if(_animator == null)
        {
            Debug.Log($"Animator 컴포넌트를 넣어주세요");
            return;
        }

        SetAttackSpeed(_attackAnimationSpeed);
    }

    public void PlayAttack(int comboStep)
    {
        if(_animator == null)
        {
            return;
        }
        Debug.Log($"PlayAttack ComboStep: {comboStep}");
        _animator.SetInteger(_comboStepParameters, comboStep);
        _animator.SetTrigger(_attackTrigger);
        
        
    }

    public void PlayDamage() // 피격 애니메이션
    {
        if(_animator == null)
        {
            return;
        }

        _animator.SetTrigger(_damageTrigger);
    }

    public void PlayDeath()
    {
        if(_animator == null)
        {
            return;
        }

        _animator.SetTrigger(_deathTrigger);
    }

    public void EndAttack()
    {
        if(_animator == null)
        {
            return;
        }
      
        _animator.SetTrigger("AttackEnd");
    }

    public void SetAttackSpeed(float speed)
    {         if(_animator == null)
        {
            return;
        }
        _animator.SetFloat(_attackSpeedParameters, speed);
    }

    public AnimatorStateInfo GetCurrentAnimatorStateInfo(int layerIndex = 0)
    {
        return _animator != null ? _animator.GetCurrentAnimatorStateInfo(layerIndex) : default;
    }

    public bool IsPlayingAnimation(string stateName, int layerIndex = 0)
    {
        if(_animator == null)
        {
            return false;
        }

        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(layerIndex);
        return stateInfo.IsName(stateName);

    }


}
