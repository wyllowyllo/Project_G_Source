using Monster.AI;
using Monster.Combat;
using UnityEngine;

namespace Monster.Ability
{
    // 애니메이터를 제어하는 Ability
    // FSM State에서 애니메이션 파라미터와 트리거를 제어
    public class AnimatorAbility : EntityAbility
    {
        private Animator _animator;
        private MonsterAnimationEventReceiver _eventReceiver;

        // 애니메이터 파라미터 해시 (성능 최적화)
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int MoveXHash = Animator.StringToHash("MoveX");
        private static readonly int MoveYHash = Animator.StringToHash("MoveY");
        private static readonly int InCombatHash = Animator.StringToHash("InCombat");
        private static readonly int AlertHash = Animator.StringToHash("Alert");
        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int AttackHeavyHash = Animator.StringToHash("Attack_Heavy");
        private static readonly int HitHash = Animator.StringToHash("Hit");
        private static readonly int DeathHash = Animator.StringToHash("Death");

        // 콜백
        private System.Action _onAlertComplete;
        private System.Action _onAttackComplete;
        private System.Action _onHitComplete;
        private System.Action _onDeathComplete;

        // 프로퍼티
        public bool IsActive => _animator != null && _animator.isActiveAndEnabled;

        public override void Initialize(AI.MonsterController controller)
        {
            base.Initialize(controller);
            _animator = controller.Animator;

            InitializeEventReceiver();
        }

        private void InitializeEventReceiver()
        {
            if (_animator == null) return;

            _eventReceiver = _animator.gameObject.GetComponent<MonsterAnimationEventReceiver>();
            if (_eventReceiver == null)
            {
                _eventReceiver = _animator.gameObject.AddComponent<MonsterAnimationEventReceiver>();
            }

            // Attacker 컴포넌트 찾기 (MonsterController와 같은 오브젝트에 있음)
            MonsterAttacker monsterAttacker = _controller.GetComponent<MonsterAttacker>();
            MonsterRangedAttacker rangedAttacker = _controller.GetComponent<MonsterRangedAttacker>();

            _eventReceiver.Initialize(_controller, monsterAttacker, rangedAttacker);
        }

        // ===== Float 파라미터 =====

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

        // ===== Bool 파라미터 =====

        public void SetInCombat(bool inCombat)
        {
            if (IsActive)
            {
                _animator.SetBool(InCombatHash, inCombat);
            }
        }

        // ===== 트리거 =====

        public void TriggerAlert(System.Action onComplete)
        {
            _onAlertComplete = onComplete;

            if (IsActive)
            {
                _animator.SetTrigger(AlertHash);
            }
            else
            {
                onComplete?.Invoke();
            }
        }

        public void TriggerAttack(bool isHeavy, System.Action onComplete)
        {
            _onAttackComplete = onComplete;

            if (IsActive)
            {
                _animator.SetTrigger(isHeavy ? AttackHeavyHash : AttackHash);
            }
            else
            {
                onComplete?.Invoke();
            }
        }

        public void TriggerHit(System.Action onComplete = null)
        {
            _onHitComplete = onComplete;

            if (IsActive)
            {
                _animator.SetTrigger(HitHash);
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

        // ===== 콜백 수신 (MonsterController에서 호출) =====

        public void OnAlertComplete()
        {
            _onAlertComplete?.Invoke();
            _onAlertComplete = null;
        }

        public void OnAttackComplete()
        {
            _onAttackComplete?.Invoke();
            _onAttackComplete = null;
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

        // ===== 유틸리티 =====

        // 현재 애니메이션 상태 이름 확인
        public bool IsInState(string stateName, int layer = 0)
        {
            if (!IsActive) return false;
            return _animator.GetCurrentAnimatorStateInfo(layer).IsName(stateName);
        }

        // 현재 애니메이션 정규화 시간 (0~1)
        public float GetNormalizedTime(int layer = 0)
        {
            if (!IsActive) return 0f;
            return _animator.GetCurrentAnimatorStateInfo(layer).normalizedTime;
        }
    }
}
