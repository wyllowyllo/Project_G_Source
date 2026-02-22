using Combat.Core;
using Monster.Combat;
using UnityEngine;

namespace Test.TestPlayer
{
    /// <summary>
    /// 몬스터 전투 시스템 테스트를 위한 헬퍼 컴포넌트.
    /// 런타임에서 몬스터 공격 테스트 및 디버그 기능을 제공합니다.
    ///
    /// 테스트 키:
    /// - F1: 가장 가까운 몬스터 정보 출력
    /// - F2: 가장 가까운 몬스터 히트박스 수동 활성화 (1초)
    /// - F3: 모든 몬스터 체력 50% 감소
    /// - F4: 모든 몬스터 즉시 처치
    /// - F5: 씬 내 모든 Combatant 정보 출력
    /// </summary>
    public class MonsterCombatTestHelper : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _searchRadius = 50f;
        [SerializeField] private LayerMask _monsterLayer = -1;

        [Header("Debug")]
        [SerializeField] private bool _showGizmos = true;

        private void Update()
        {
            HandleTestInput();
        }

        private void HandleTestInput()
        {
            // F1: 가장 가까운 몬스터 정보 출력
            if (Input.GetKeyDown(KeyCode.F1))
            {
                PrintNearestMonsterInfo();
            }

            // F2: 가장 가까운 몬스터 히트박스 수동 활성화
            if (Input.GetKeyDown(KeyCode.F2))
            {
                TriggerNearestMonsterAttack();
            }

            // F3: 모든 몬스터 체력 50% 감소
            if (Input.GetKeyDown(KeyCode.F3))
            {
                DamageAllMonsters(0.5f);
            }

            // F4: 모든 몬스터 즉시 처치
            if (Input.GetKeyDown(KeyCode.F4))
            {
                KillAllMonsters();
            }

            // F5: 씬 내 모든 Combatant 정보 출력
            if (Input.GetKeyDown(KeyCode.F5))
            {
                PrintAllCombatants();
            }
        }

        private void PrintNearestMonsterInfo()
        {
            var combatant = FindNearestCombatant(CombatTeam.Enemy);
            if (combatant == null)
            {
                Debug.Log("[TestHelper] 근처에 몬스터가 없습니다.");
                return;
            }

            float distance = Vector3.Distance(transform.position, combatant.Transform.position);
            var stats = combatant.Stats;

            Debug.Log($"[TestHelper] === 가장 가까운 몬스터 ===\n" +
                      $"  이름: {combatant.Transform.name}\n" +
                      $"  거리: {distance:F1}m\n" +
                      $"  Team: {combatant.Team}\n" +
                      $"  HP: {combatant.CurrentHealth:F1}/{combatant.MaxHealth:F1}\n" +
                      $"  IsAlive: {combatant.IsAlive}\n" +
                      $"  IsInvincible: {combatant.IsInvincible}\n" +
                      $"  IsStunned: {combatant.IsStunned}\n" +
                      $"  AttackDamage: {stats.AttackDamage.Value:F1}\n" +
                      $"  Defense: {stats.Defense.Value:F1}");
        }

        private void TriggerNearestMonsterAttack()
        {
            var combatant = FindNearestCombatant(CombatTeam.Enemy);
            if (combatant == null)
            {
                Debug.Log("[TestHelper] 근처에 몬스터가 없습니다.");
                return;
            }

            var attacker = combatant.Transform.GetComponent<MonsterAttacker>();
            if (attacker == null)
            {
                Debug.Log($"[TestHelper] {combatant.Transform.name}에 MonsterAttacker가 없습니다.");
                return;
            }

            attacker.EnableHitbox(false);
            Debug.Log($"[TestHelper] {combatant.Transform.name} 히트박스 활성화!");

            // 1초 후 비활성화
            StartCoroutine(DisableHitboxAfterDelay(attacker, 1f));
        }

        private System.Collections.IEnumerator DisableHitboxAfterDelay(MonsterAttacker attacker, float delay)
        {
            yield return new WaitForSeconds(delay);
            attacker.DisableHitbox();
            Debug.Log("[TestHelper] 히트박스 비활성화");
        }

        private void DamageAllMonsters(float healthPercent)
        {
            var combatants = FindObjectsByType<Combatant>(FindObjectsSortMode.None);
            int count = 0;

            foreach (var combatant in combatants)
            {
                if (combatant.Team != CombatTeam.Enemy) continue;
                if (!combatant.IsAlive) continue;

                float damage = combatant.MaxHealth * healthPercent;
                combatant.TakeDamage(damage);
                count++;
            }

            Debug.Log($"[TestHelper] {count}마리 몬스터에게 {healthPercent * 100}% 데미지 적용");
        }

        private void KillAllMonsters()
        {
            var combatants = FindObjectsByType<Combatant>(FindObjectsSortMode.None);
            int count = 0;

            foreach (var combatant in combatants)
            {
                if (combatant.Team != CombatTeam.Enemy) continue;
                if (!combatant.IsAlive) continue;

                combatant.TakeDamage(combatant.CurrentHealth + 1);
                count++;
            }

            Debug.Log($"[TestHelper] {count}마리 몬스터 처치 완료");
        }

        private void PrintAllCombatants()
        {
            var combatants = FindObjectsByType<Combatant>(FindObjectsSortMode.None);

            Debug.Log($"[TestHelper] === 씬 내 모든 Combatant ({combatants.Length}개) ===");

            foreach (var combatant in combatants)
            {
                string status = combatant.IsAlive ? "Alive" : "Dead";
                Debug.Log($"  - {combatant.name} [{combatant.Team}] " +
                          $"HP: {combatant.CurrentHealth:F1}/{combatant.MaxHealth:F1} ({status})");
            }
        }

        private Combatant FindNearestCombatant(CombatTeam team)
        {
            var combatants = FindObjectsByType<Combatant>(FindObjectsSortMode.None);

            Combatant nearest = null;
            float nearestDistance = float.MaxValue;

            foreach (var combatant in combatants)
            {
                if (combatant.Team != team) continue;
                if (!combatant.IsAlive) continue;

                float distance = Vector3.Distance(transform.position, combatant.Transform.position);
                if (distance < nearestDistance && distance <= _searchRadius)
                {
                    nearestDistance = distance;
                    nearest = combatant;
                }
            }

            return nearest;
        }

        private void OnGUI()
        {
            // 화면 우상단에 테스트 키 안내 표시
            float boxWidth = 280f;
            float boxHeight = 120f;
            float x = Screen.width - boxWidth - 10;

            GUI.Box(new Rect(x, 10, boxWidth, boxHeight), "");
            GUI.Label(new Rect(x + 10, 20, boxWidth - 20, 20), "[Monster Combat Test Helper]");
            GUI.Label(new Rect(x + 10, 40, boxWidth - 20, 20), "F1: 가장 가까운 몬스터 정보");
            GUI.Label(new Rect(x + 10, 60, boxWidth - 20, 20), "F2: 가장 가까운 몬스터 공격 트리거");
            GUI.Label(new Rect(x + 10, 80, boxWidth - 20, 20), "F3: 모든 몬스터 50% 데미지");
            GUI.Label(new Rect(x + 10, 100, boxWidth - 20, 20), "F4: 모든 몬스터 처치");
        }

        private void OnDrawGizmosSelected()
        {
            if (!_showGizmos) return;

            // 검색 반경 표시
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, _searchRadius);
        }
    }
}
