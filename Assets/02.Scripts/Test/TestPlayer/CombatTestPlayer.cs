using Combat.Core;
using UnityEngine;

namespace Test.TestPlayer
{
    /// <summary>
    /// Combat 시스템 통합 테스트용 플레이어 컴포넌트.
    /// Combatant + Health를 사용하여 몬스터의 공격을 받을 수 있습니다.
    ///
    /// 필수 컴포넌트:
    /// - Combatant (Team = Player)
    /// - Health
    /// - Collider (히트 판정용)
    ///
    /// 테스트 키:
    /// - I: 무적 모드 토글
    /// - H: 체력 전체 회복
    /// - K: 자가 피해 (10 데미지)
    /// - R: 스탯 정보 출력
    /// </summary>
    [RequireComponent(typeof(Combatant))]
    public class CombatTestPlayer : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private float _selfDamageAmount = 10f;
        [SerializeField] private float _invincibilityDuration = 2f;

        private Combatant _combatant;
        private bool _isManualInvincible;

        private void Awake()
        {
            _combatant = GetComponent<Combatant>();
        }

        private void OnEnable()
        {
            _combatant.OnDamaged += HandleDamaged;
            _combatant.OnDeath += HandleDeath;
            _combatant.OnHitStunStart += HandleHitStunStart;
            _combatant.OnHitStunEnd += HandleHitStunEnd;
            _combatant.OnInvincibilityStart += HandleInvincibilityStart;
            _combatant.OnInvincibilityEnd += HandleInvincibilityEnd;
        }

        private void OnDisable()
        {
            _combatant.OnDamaged -= HandleDamaged;
            _combatant.OnDeath -= HandleDeath;
            _combatant.OnHitStunStart -= HandleHitStunStart;
            _combatant.OnHitStunEnd -= HandleHitStunEnd;
            _combatant.OnInvincibilityStart -= HandleInvincibilityStart;
            _combatant.OnInvincibilityEnd -= HandleInvincibilityEnd;
        }

        private void Update()
        {
            HandleTestInput();
        }

        private void HandleTestInput()
        {
            // K: 자가 피해
            if (Input.GetKeyDown(KeyCode.K))
            {
                _combatant.TakeDamage(_selfDamageAmount);
                Debug.Log($"[CombatTestPlayer] 자가 피해 {_selfDamageAmount}");
            }

            // I: 무적 모드 토글
            if (Input.GetKeyDown(KeyCode.I))
            {
                _isManualInvincible = !_isManualInvincible;
                if (_isManualInvincible)
                {
                    _combatant.SetInvincible(float.MaxValue);
                    Debug.Log("[CombatTestPlayer] 무적 모드 ON");
                }
                else
                {
                    _combatant.ClearInvincibility();
                    Debug.Log("[CombatTestPlayer] 무적 모드 OFF");
                }
            }

            // H: 체력 전체 회복
            if (Input.GetKeyDown(KeyCode.H))
            {
                _combatant.Heal(_combatant.MaxHealth);
                Debug.Log($"[CombatTestPlayer] 체력 회복! {_combatant.CurrentHealth}/{_combatant.MaxHealth}");
            }

            // R: 스탯 정보 출력
            if (Input.GetKeyDown(KeyCode.R))
            {
                PrintStats();
            }
        }

        private void PrintStats()
        {
            var stats = _combatant.Stats;
            Debug.Log($"[CombatTestPlayer] === 스탯 정보 ===\n" +
                      $"  Team: {_combatant.Team}\n" +
                      $"  HP: {_combatant.CurrentHealth:F1}/{_combatant.MaxHealth:F1}\n" +
                      $"  IsAlive: {_combatant.IsAlive}\n" +
                      $"  IsInvincible: {_combatant.IsInvincible}\n" +
                      $"  IsStunned: {_combatant.IsStunned}\n" +
                      $"  AttackDamage: {stats.AttackDamage.Value:F1}\n" +
                      $"  Defense: {stats.Defense.Value:F1}\n" +
                      $"  CritChance: {stats.CriticalChance.Value:P0}\n" +
                      $"  CritMultiplier: {stats.CriticalMultiplier.Value:F1}x");
        }

        private void HandleDamaged(DamageInfo damageInfo)
        {
            string critText = damageInfo.IsCritical ? " (크리티컬!)" : "";
            Debug.Log($"[CombatTestPlayer] 피격! 데미지: {damageInfo.Amount:F1}{critText}, " +
                      $"남은 체력: {_combatant.CurrentHealth:F1}/{_combatant.MaxHealth:F1}");
        }

        private void HandleDeath()
        {
            Debug.Log("[CombatTestPlayer] 사망!");
        }

        private void HandleHitStunStart()
        {
            Debug.Log("[CombatTestPlayer] 경직 시작");
        }

        private void HandleHitStunEnd()
        {
            Debug.Log("[CombatTestPlayer] 경직 종료");
        }

        private void HandleInvincibilityStart()
        {
            Debug.Log("[CombatTestPlayer] 무적 시작");
        }

        private void HandleInvincibilityEnd()
        {
            if (!_isManualInvincible)
            {
                Debug.Log("[CombatTestPlayer] 무적 종료");
            }
        }

        private void OnGUI()
        {
            // 화면 좌상단에 상태 표시
            GUI.Box(new Rect(10, 10, 250, 130), "");

            string statusText = _combatant.IsAlive ? "ALIVE" : "DEAD";
            GUI.Label(new Rect(20, 20, 230, 20), $"상태: {statusText}");
            GUI.Label(new Rect(20, 40, 230, 20),
                $"HP: {_combatant.CurrentHealth:F0} / {_combatant.MaxHealth:F0}");

            string invincibleText = _combatant.IsInvincible ? "ON" : "OFF";
            GUI.Label(new Rect(20, 60, 230, 20), $"무적: {invincibleText}");

            string stunnedText = _combatant.IsStunned ? "ON" : "OFF";
            GUI.Label(new Rect(20, 80, 230, 20), $"경직: {stunnedText}");

            GUI.Label(new Rect(20, 100, 230, 20), "K:피해 I:무적 H:회복 R:스탯");
            GUI.Label(new Rect(20, 120, 230, 20), $"Defense: {_combatant.Stats.Defense.Value:F1}");
        }
    }
}
