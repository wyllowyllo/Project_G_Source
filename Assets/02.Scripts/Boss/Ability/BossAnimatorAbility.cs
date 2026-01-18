using Common;
using Monster.Ability;
using UnityEngine;

namespace Boss.Ability
{
    // Boss 전용 AnimatorAbility
    // Monster의 EntityAbility를 상속하여 Boss 특화 기능 구현
    public class BossAnimatorAbility : EntityAbility
    {
        private Animator _animator;

        // 애니메이터 파라미터 해시
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int MoveXHash = Animator.StringToHash("MoveX");
        private static readonly int MoveYHash = Animator.StringToHash("MoveY");
        private static readonly int InCombatHash = Animator.StringToHash("InCombat");

        // Bool 파라미터 해시
        private static readonly int IsStaggeredHash = Animator.StringToHash("IsStaggered");

        // 트리거 해시
        private static readonly int MeleeAttackHash = Animator.StringToHash("MeleeAttack");
        private static readonly int ChargeHash = Animator.StringToHash("Charge");
        private static readonly int BreathHash = Animator.StringToHash("Breath");
        private static readonly int ProjectileHash = Animator.StringToHash("Projectile");
        private static readonly int SummonHash = Animator.StringToHash("Summon");
        private static readonly int HitHash = Animator.StringToHash("Hit");
        private static readonly int DeathHash = Animator.StringToHash("Death");
        private static readonly int PhaseTransitionHash = Animator.StringToHash("PhaseTransition");

        // 콜백
        private System.Action _onMeleeAttackComplete;
        private System.Action _onChargeComplete;
        private System.Action _onBreathComplete;
        private System.Action _onProjectileComplete;
        private System.Action _onSummonComplete;
        private System.Action _onHitComplete;
        private System.Action _onDeathComplete;
        private System.Action _onPhaseTransitionComplete;

        public bool IsActive => _animator != null && _animator.isActiveAndEnabled;

        public override void Initialize(IEntityController controller)
        {
            base.Initialize(controller);
            _animator = controller.Animator;
        }

        // Float 파라미터
        public void SetSpeed(float speed)
        {
            if (IsActive)
            {
                _animator.SetFloat(SpeedHash, speed);
            }
        }

        public void SetMoveDirection(float x, float y)
        {
            if (IsActive)
            {
                _animator.SetFloat(MoveXHash, x);
                _animator.SetFloat(MoveYHash, y);
            }
        }

        // Bool 파라미터
        public void SetInCombat(bool inCombat)
        {
            if (IsActive)
            {
                _animator.SetBool(InCombatHash, inCombat);
            }
        }

        // 트리거들
        public void TriggerMeleeAttack(System.Action onComplete)
        {
            _onMeleeAttackComplete = onComplete;

            if (IsActive)
            {
                _animator.SetTrigger(MeleeAttackHash);
            }
            else
            {
                onComplete?.Invoke();
            }
        }

        public void TriggerCharge(System.Action onComplete)
        {
            _onChargeComplete = onComplete;

            if (IsActive)
            {
                _animator.SetTrigger(ChargeHash);
            }
            else
            {
                onComplete?.Invoke();
            }
        }

        public void TriggerBreath(System.Action onComplete)
        {
            _onBreathComplete = onComplete;

            if (IsActive)
            {
                _animator.SetTrigger(BreathHash);
            }
            else
            {
                onComplete?.Invoke();
            }
        }

        public void TriggerProjectile(System.Action onComplete)
        {
            _onProjectileComplete = onComplete;

            if (IsActive)
            {
                _animator.SetTrigger(ProjectileHash);
            }
            else
            {
                onComplete?.Invoke();
            }
        }

        public void TriggerSummon(System.Action onComplete)
        {
            _onSummonComplete = onComplete;

            if (IsActive)
            {
                _animator.SetTrigger(SummonHash);
            }
            else
            {
                onComplete?.Invoke();
            }
        }

        public void SetStagger(bool isStaggered)
        {
            if (IsActive)
            {
                _animator.SetBool(IsStaggeredHash, isStaggered);
            }
        }

        public void TriggerHit(System.Action onComplete = null)
        {
            _onHitComplete = onComplete;

            if (IsActive)
            {
                // CrossFade로 즉시 Hit 애니메이션 재생 (연타 대응)
                // 0.05초의 짧은 블렌딩으로 자연스러우면서도 빠른 반응
                _animator.CrossFadeInFixedTime("Hit", 0.05f, 0);
            }
            else
            {
                onComplete?.Invoke();
            }
        }

        public void TriggerDeath(System.Action onComplete = null)
        {
            _onDeathComplete = onComplete;

            if (IsActive)
            {
                _animator.SetTrigger(DeathHash);
            }
            else
            {
                onComplete?.Invoke();
            }
        }

        public void TriggerPhaseTransition(System.Action onComplete)
        {
            _onPhaseTransitionComplete = onComplete;

            if (IsActive)
            {
                _animator.SetTrigger(PhaseTransitionHash);
            }
            else
            {
                onComplete?.Invoke();
            }
        }

        // 콜백 수신 (BossController에서 호출)
        public void OnMeleeAttackComplete()
        {
            _onMeleeAttackComplete?.Invoke();
            _onMeleeAttackComplete = null;
        }

        public void OnChargeComplete()
        {
            _onChargeComplete?.Invoke();
            _onChargeComplete = null;
        }

        public void OnBreathComplete()
        {
            _onBreathComplete?.Invoke();
            _onBreathComplete = null;
        }

        public void OnProjectileComplete()
        {
            _onProjectileComplete?.Invoke();
            _onProjectileComplete = null;
        }

        public void OnSummonComplete()
        {
            _onSummonComplete?.Invoke();
            _onSummonComplete = null;
        }

        public void OnHitComplete()
        {
            _onHitComplete?.Invoke();
            _onHitComplete = null;
        }

        public void OnDeathComplete()
        {
            _onDeathComplete?.Invoke();
            _onDeathComplete = null;
        }

        public void OnPhaseTransitionComplete()
        {
            _onPhaseTransitionComplete?.Invoke();
            _onPhaseTransitionComplete = null;
        }

        /// <summary>
        /// 모든 공격/상태 트리거 리셋 (PhaseTransition 진입 시 사용)
        /// </summary>
        public void ResetAllAttackTriggers()
        {
            if (!IsActive) return;

            _animator.ResetTrigger(MeleeAttackHash);
            _animator.ResetTrigger(ChargeHash);
            _animator.ResetTrigger(BreathHash);
            _animator.ResetTrigger(ProjectileHash);
            _animator.ResetTrigger(SummonHash);
            _animator.ResetTrigger(HitHash);
        }

        // 유틸리티
        public bool IsInState(string stateName, int layer = 0)
        {
            if (!IsActive) return false;
            return _animator.GetCurrentAnimatorStateInfo(layer).IsName(stateName);
        }

        public float GetNormalizedTime(int layer = 0)
        {
            if (!IsActive) return 0f;
            return _animator.GetCurrentAnimatorStateInfo(layer).normalizedTime;
        }
    }
}
