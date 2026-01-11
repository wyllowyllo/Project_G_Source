using System.Collections;
using Combat.Attack;
using Combat.Core;
using Combat.Damage;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Combat.Sample
{
    /// <summary>
    /// 전투 시스템을 활용한 플레이어 샘플 코드입니다.
    /// 
    /// 필요한 컴포넌트:
    /// - Combatant: 전투 참여자 (자동으로 Health 추가됨)
    /// - MeleeAttacker: 근접 공격 (자동으로 ComboAttackHandler 추가됨)
    /// 
    /// 필요한 자식 오브젝트:
    /// - WeaponHitbox: HitboxTrigger 컴포넌트가 있는 자식 오브젝트
    /// 
    /// 테스트 모드:
    /// - AutoEndAttack을 켜면 애니메이션 이벤트 없이도 공격이 자동 종료됩니다.
    /// </summary>
    [RequireComponent(typeof(Combatant))]
    [RequireComponent(typeof(MeleeAttacker))]
    public class PlayerCombatSample : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Animator _animator;
        
        [Header("Test Mode")]
        [Tooltip("애니메이션 이벤트 없이 공격 자동 종료 (테스트용)")]
        [SerializeField] private bool _autoEndAttack;
        [SerializeField] private float _attackDuration = 0.5f;
        
        private Combatant _combatant;
        private MeleeAttacker _attacker;
        
        private static readonly int AttackTrigger = Animator.StringToHash("Attack");
        private static readonly int DamageTrigger = Animator.StringToHash("Damage");
        private static readonly int DeathTrigger = Animator.StringToHash("Death");
        private static readonly int ComboStep = Animator.StringToHash("ComboStep");

        private void Awake()
        {
            _combatant = GetComponent<Combatant>();
            _attacker = GetComponent<MeleeAttacker>();
        }

        private void Start()
        {
            SubscribeEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }

        private void Update()
        {
            HandleAttackInput();
        }

        private void HandleAttackInput()
        {
            if (!CanPerformAction())
                return;

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            if (Input.GetButtonDown("Fire1"))
            {
                TryAttack();
            }
        }

        private bool CanPerformAction()
        {
            return _combatant.IsAlive && !_combatant.IsStunned;
        }

        private void TryAttack()
        {
            if (!_attacker.TryAttack())
                return;

            PlayAttackAnimation();
            
            if (_autoEndAttack)
            {
                StartCoroutine(AutoEndAttackRoutine());
            }
        }

        private IEnumerator AutoEndAttackRoutine()
        {
            float hitStart = _attackDuration * 0.3f;
            float hitDuration = _attackDuration * 0.4f;
            float hitEnd = _attackDuration * 0.3f;
            
            yield return new WaitForSeconds(hitStart);
            _attacker.OnAttackHitStart();
            
            yield return new WaitForSeconds(hitDuration);
            _attacker.ForceDisableHitbox();
            
            yield return new WaitForSeconds(hitEnd);
            _attacker.OnComboWindowStart();
        }

        private void PlayAttackAnimation()
        {
            if (_animator == null)
                return;

            _animator.SetInteger(ComboStep, _attacker.CurrentComboStep);
            _animator.SetTrigger(AttackTrigger);
        }

        private void SubscribeEvents()
        {
            _combatant.OnDamaged += HandleDamaged;
            _combatant.OnDeath += HandleDeath;
            _attacker.OnHit += HandleHit;
            _attacker.OnComboAttack += HandleComboAttack;
            _attacker.OnComboReset += HandleComboReset;
        }

        private void UnsubscribeEvents()
        {
            _combatant.OnDamaged -= HandleDamaged;
            _combatant.OnDeath -= HandleDeath;
            _attacker.OnHit -= HandleHit;
            _attacker.OnComboAttack -= HandleComboAttack;
            _attacker.OnComboReset -= HandleComboReset;
        }

        private void HandleDamaged(DamageInfo info)
        {
            Debug.Log($"[Player] 피격: {info.Amount} 데미지 (크리티컬: {info.IsCritical})");
            
            PlayDamageAnimation();
            UpdateHealthUI();
            SpawnHitEffect(info.HitPoint);
        }

        private void HandleDeath()
        {
            Debug.Log("[Player] 사망");
            
            PlayDeathAnimation();
            DisableInput();
        }

        private void HandleHit(IDamageable target, DamageInfo info)
        {
            Debug.Log($"[Player] 적중: {info.Amount} 데미지 (크리티컬: {info.IsCritical})");
            
            SpawnHitEffect(info.HitPoint);
            ApplyHitStop();
        }

        private void HandleComboAttack(int step, float multiplier)
        {
            Debug.Log($"[Player] 콤보 {step}단계 (배율: {multiplier:F1}x)");
        }

        private void HandleComboReset()
        {
            Debug.Log("[Player] 콤보 리셋");
        }

        private void PlayDamageAnimation()
        {
            if (_animator != null)
            {
                _animator.SetTrigger(DamageTrigger);
            }
        }

        private void PlayDeathAnimation()
        {
            if (_animator != null)
            {
                _animator.SetTrigger(DeathTrigger);
            }
        }

        private void UpdateHealthUI()
        {
            // TODO: UI 시스템과 연동
            // UIManager.UpdateHealthBar(_combatant.CurrentHealth, _combatant.MaxHealth);
        }

        private void SpawnHitEffect(Vector3 position)
        {
            // TODO: 이펙트 시스템과 연동
            // EffectManager.Spawn("HitEffect", position);
        }

        private void ApplyHitStop()
        {
            // TODO: 히트스탑 구현
            // TimeManager.HitStop(0.05f);
        }

        private void DisableInput()
        {
            enabled = false;
        }
    }
}
