using System;
using Combat.Core;
using Combat.Damage;
using UnityEngine;

namespace Combat.Attack
{
    [RequireComponent(typeof(Combatant))]
    [RequireComponent(typeof(ComboAttackHandler))]
    public class MeleeAttacker : MonoBehaviour, IAttacker
    {
        [Header("References")]
        [SerializeField] private HitboxTrigger _hitbox;


        [Header("Settings")]
        [SerializeField] private DamageType _damageType = DamageType.Normal;

        private Combatant _combatant;
        private ComboAttackHandler _comboHandler;
        private AttackContext _currentAttackContext;
        private AttackSession _currentSession;

        public ICombatant Combatant => _combatant;
        public AttackSession CurrentSession => _currentSession;
        public bool CanAttack => _comboHandler.CurrentState is ComboState.Idle or ComboState.ComboWindow
                                 && _combatant.IsAlive && !_combatant.IsStunned;

        public int CurrentComboStep => _comboHandler.CurrentComboStep;
        public ComboState CurrentState => _comboHandler.CurrentState;
        public float CurrentMultiplier => _comboHandler.CurrentMultiplier;
        public int MaxComboSteps => _comboHandler.MaxComboSteps;

        public event Action<int, float> OnComboAttack;
        public event Action OnComboReset;
        public event Action<IDamageable, DamageInfo> OnHit;

        private void Awake()
        {
            _combatant = GetComponent<Combatant>();
            _comboHandler = GetComponent<ComboAttackHandler>();

#if UNITY_EDITOR
            if (_hitbox == null)
            {
                Debug.LogWarning($"[{nameof(MeleeAttacker)}] HitboxTrigger is not assigned on {gameObject.name}");
            }
#endif
        }

        private void OnEnable()
        {
            _comboHandler.OnComboAttack += HandleComboAttack;
            _comboHandler.OnComboReset += HandleComboReset;

            if (_hitbox != null)
            {
                _hitbox.OnHit += HandleHit;
            }
        }

        private void OnDisable()
        {
            _comboHandler.OnComboAttack -= HandleComboAttack;
            _comboHandler.OnComboReset -= HandleComboReset;

            if (_hitbox != null)
            {
                _hitbox.OnHit -= HandleHit;
            }
        }

        public bool TryAttack()
        {
            if (!CanAttack) return false;
            return _comboHandler.TryAttack();
        }

        public void Attack() => TryAttack();

        public AttackSession StartNewSession(int comboStep)
        {
            // 이전 세션 정리
            if (_currentSession != null && _currentSession.IsActive)
            {
                _hitbox?.DisableHitbox();
            }

            _currentSession?.Invalidate();
            _currentSession = new AttackSession(comboStep);
            return _currentSession;
        }

        public void OnAttackHitStart(AttackSession session)
        {
            if (!IsSessionValid(session)) return;
            if (_hitbox == null) return;

            _currentAttackContext = AttackContext.Scaled(_combatant, _comboHandler.CurrentMultiplier, type: _damageType);
            _hitbox.EnableHitbox(_combatant.Team);
        }

        public void OnAttackHitEnd(AttackSession session)
        {
            if (!IsSessionValid(session)) return;
            _hitbox?.DisableHitbox();
        }

        public void ForceDisableHitbox()
        {
            _hitbox?.DisableHitbox();
        }

        private bool IsSessionValid(AttackSession session)
        {
            return session != null && session == _currentSession && session.IsActive;
        }

        public void OnComboWindowStart()
        {
            if (_comboHandler.CurrentState != ComboState.Attacking)
                return;

            _comboHandler.SetState(ComboState.ComboWindow);
            _comboHandler.StartComboWindowTimer();
        }

        public void ResetCombo()
        {
            _comboHandler.ResetCombo();
        }

        private void HandleComboAttack(int step, float multiplier)
        {
            OnComboAttack?.Invoke(step, multiplier);
        }

        private void HandleComboReset()
        {
            OnComboReset?.Invoke();
        }

        private void HandleHit(HitInfo hitInfo)
        {
            var damageInfo = DamageProcessor.Process(_currentAttackContext, hitInfo, transform.position);
            hitInfo.Target.TakeDamage(damageInfo);
            OnHit?.Invoke(hitInfo.Target, damageInfo);
        }

#if UNITY_INCLUDE_TESTS
        public void SetHitboxForTest(HitboxTrigger hitbox) => _hitbox = hitbox;
#endif
    }
}
