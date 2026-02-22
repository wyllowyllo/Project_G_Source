using Boss.AI;
using Boss.AI.States;
using UnityEngine;

namespace Boss.Test
{
    /// <summary>
    /// 보스 디버그 시각화 컴포넌트
    /// Scene 뷰와 Game 뷰에서 보스 상태 및 범위를 시각화
    /// </summary>
    public class BossDebugVisualizer : MonoBehaviour
    {
        [Header("시각화 설정")]
        [SerializeField] private bool _showRanges = true;
        [SerializeField] private bool _showState = true;
        [SerializeField] private bool _showCooldowns = true;
        [SerializeField] private bool _showPhaseInfo = true;
        [SerializeField] private bool _showEnrageStatus = true;

        [Header("범위 색상")]
        [SerializeField] private Color _meleeRangeColor = new Color(1f, 0f, 0f, 0.3f);
        [SerializeField] private Color _chargeRangeColor = new Color(1f, 0.5f, 0f, 0.3f);
        [SerializeField] private Color _breathRangeColor = new Color(1f, 1f, 0f, 0.3f);
        [SerializeField] private Color _detectionRangeColor = new Color(0f, 1f, 0f, 0.2f);
        [SerializeField] private Color _summonRangeColor = new Color(0.5f, 0f, 1f, 0.3f);

        private BossController _controller;

        private void Awake()
        {
            _controller = GetComponent<BossController>();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (_controller == null)
            {
                _controller = GetComponent<BossController>();
            }

            if (_controller == null || _controller.Data == null) return;

            if (_showRanges)
            {
                DrawRanges();
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (_controller == null || _controller.Data == null) return;

            // 선택 시 더 자세한 정보 표시
            DrawDetailedRanges();
        }

        private void DrawRanges()
        {
            Vector3 pos = transform.position;
            var data = _controller.Data;

            // 감지 범위
            Gizmos.color = _detectionRangeColor;
            Gizmos.DrawWireSphere(pos, data.DetectionRange);

            // 근접 공격 범위
            Gizmos.color = _meleeRangeColor;
            Gizmos.DrawWireSphere(pos, data.MeleeRange);
        }

        private void DrawDetailedRanges()
        {
            Vector3 pos = transform.position;
            var data = _controller.Data;

            // 돌진 거리
            Gizmos.color = _chargeRangeColor;
            Gizmos.DrawWireSphere(pos, data.ChargeDistance);

            // 브레스 범위 (부채꼴)
            DrawBreathCone(pos, transform.forward, data.BreathAngle, data.BreathRange);

            // 소환 범위
            Gizmos.color = _summonRangeColor;
            Gizmos.DrawWireSphere(pos, data.MinionSpawnRadius);
        }

        private void DrawBreathCone(Vector3 origin, Vector3 forward, float angle, float range)
        {
            Gizmos.color = _breathRangeColor;

            int segments = 20;
            float halfAngle = angle * 0.5f;

            Vector3 prevPoint = origin;
            for (int i = 0; i <= segments; i++)
            {
                float currentAngle = -halfAngle + (angle * i / segments);
                Vector3 direction = Quaternion.Euler(0, currentAngle, 0) * forward;
                Vector3 point = origin + direction * range;

                if (i > 0)
                {
                    Gizmos.DrawLine(prevPoint, point);
                }
                Gizmos.DrawLine(origin, point);
                prevPoint = point;
            }
        }
#endif

        private void OnGUI()
        {
            if (!Application.isPlaying) return;
            if (_controller == null) return;

            // 화면 상단에 디버그 정보 표시
            GUILayout.BeginArea(new Rect(10, 10, 300, 400));
            GUILayout.BeginVertical("box");

            GUILayout.Label("<b>=== BOSS DEBUG ===</b>", CreateRichTextStyle());

            if (_showState)
            {
                DrawStateInfo();
            }

            if (_showPhaseInfo)
            {
                DrawPhaseInfo();
            }

            if (_showEnrageStatus)
            {
                DrawEnrageInfo();
            }

            if (_showCooldowns)
            {
                DrawCooldownInfo();
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void DrawStateInfo()
        {
            GUILayout.Label($"<b>State:</b> {_controller.CurrentStateType}", CreateRichTextStyle());

            if (_controller.Combatant != null)
            {
                float hp = _controller.Combatant.CurrentHealth;
                float maxHp = _controller.Combatant.MaxHealth;
                float hpPercent = (hp / maxHp) * 100f;
                GUILayout.Label($"<b>HP:</b> {hp:F0}/{maxHp:F0} ({hpPercent:F1}%)", CreateRichTextStyle());
            }

            if (_controller.SuperArmor != null)
            {
                float poise = _controller.SuperArmor.CurrentPoise;
                float maxPoise = _controller.SuperArmor.MaxPoise;
                string infinite = _controller.SuperArmor.IsInfinite ? " [INFINITE]" : "";
                GUILayout.Label($"<b>Poise:</b> {poise:F0}/{maxPoise:F0}{infinite}", CreateRichTextStyle());
            }
        }

        private void DrawPhaseInfo()
        {
            GUILayout.Space(5);
            GUILayout.Label("<b>--- Phase Info ---</b>", CreateRichTextStyle());

            if (_controller.PhaseManager != null)
            {
                GUILayout.Label($"<b>Phase:</b> {_controller.PhaseManager.CurrentPhaseNumber}", CreateRichTextStyle());

                var phase = _controller.PhaseManager.CurrentPhase;
                if (phase != null)
                {
                    GUILayout.Label($"Damage Mult: {phase.DamageMultiplier:F2}x", CreateRichTextStyle());
                    GUILayout.Label($"Speed Mult: {phase.AttackSpeedMultiplier:F2}x", CreateRichTextStyle());
                }
            }
        }

        private void DrawEnrageInfo()
        {
            GUILayout.Space(5);
            GUILayout.Label("<b>--- Enrage ---</b>", CreateRichTextStyle());

            if (_controller.EnrageSystem != null)
            {
                string status = _controller.EnrageSystem.IsEnraged ? "<color=red>ENRAGED!</color>" : "Normal";
                GUILayout.Label($"<b>Status:</b> {status}", CreateRichTextStyle());

                if (_controller.EnrageSystem.IsEnraged)
                {
                    GUILayout.Label($"Damage: {_controller.EnrageSystem.DamageMultiplier:F2}x", CreateRichTextStyle());
                    GUILayout.Label($"Speed: {_controller.EnrageSystem.SpeedMultiplier:F2}x", CreateRichTextStyle());
                }
            }
        }

        private void DrawCooldownInfo()
        {
            GUILayout.Space(5);
            GUILayout.Label("<b>--- Cooldowns ---</b>", CreateRichTextStyle());

            if (_controller.PatternSelector != null)
            {
                DrawCooldownBar("Melee", _controller.PatternSelector.GetRemainingCooldown(EBossState.MeleeAttack), _controller.Data.MeleeCooldown);
                DrawCooldownBar("Charge", _controller.PatternSelector.GetRemainingCooldown(EBossState.Charge), _controller.Data.ChargeCooldown);
                DrawCooldownBar("Breath", _controller.PatternSelector.GetRemainingCooldown(EBossState.Breath), _controller.Data.BreathCooldown);
                DrawCooldownBar("Projectile", _controller.PatternSelector.GetRemainingCooldown(EBossState.Projectile), _controller.Data.ProjectileCooldown);
                DrawCooldownBar("Summon", _controller.PatternSelector.GetRemainingCooldown(EBossState.Summon), _controller.Data.SummonCooldown);
            }

            // 잡졸 정보
            if (_controller.MinionManager != null)
            {
                GUILayout.Space(3);
                int minionCount = _controller.MinionManager.AliveMinionCount;
                GUILayout.Label($"<b>Minions:</b> {minionCount}/{_controller.Data.MaxAliveMinions}", CreateRichTextStyle());
            }
        }

        private void DrawCooldownBar(string name, float remaining, float max)
        {
            float percent = max > 0 ? (1f - remaining / max) : 1f;
            string status = remaining <= 0 ? "<color=green>READY</color>" : $"{remaining:F1}s";
            GUILayout.Label($"{name}: {status}", CreateRichTextStyle());
        }

        private GUIStyle CreateRichTextStyle()
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.richText = true;
            return style;
        }
    }
}
