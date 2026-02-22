#if UNITY_EDITOR
using Monster.Data;
using UnityEditor;
using UnityEngine;

namespace Monster.Editor
{
    // MonsterData의 거리 설정값 검증
    [CustomEditor(typeof(MonsterData))]
    public class MonsterDataValidator : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var data = (MonsterData)target;

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("거리 설정 검증", EditorStyles.boldLabel);

            ValidateAndDrawWarnings(data);
        }

        private void ValidateAndDrawWarnings(MonsterData data)
        {
            bool hasError = false;

            // 밴드 거리 기본 규칙
            if (data.PreferredMinDistance >= data.PreferredMaxDistance)
            {
                DrawError("PreferMin >= PreferMax: 최소 거리가 최대 거리보다 작아야 합니다.");
                hasError = true;
            }

            // 타입별 검증
            switch (data.AttackType)
            {
                case EMonsterAttackType.Melee:
                    hasError |= ValidateMelee(data);
                    break;

                case EMonsterAttackType.Ranged:
                    hasError |= ValidateRanged(data);
                    break;

                case EMonsterAttackType.Hybrid:
                    hasError |= ValidateMelee(data);
                    hasError |= ValidateRanged(data);
                    hasError |= ValidateHybrid(data);
                    break;
            }

            if (!hasError)
            {
                DrawSuccess("거리 설정이 올바릅니다.");
            }
        }

        private bool ValidateMelee(MonsterData data)
        {
            bool hasError = false;

            if (data.LightAttackRange < data.PreferredMinDistance)
            {
                DrawError($"LightRange({data.LightAttackRange}) < PreferMin({data.PreferredMinDistance}): 약공 시 후퇴하여 공격 불가");
                hasError = true;
            }

            if (data.LightAttackRange > data.PreferredMaxDistance)
            {
                DrawWarning($"LightRange({data.LightAttackRange}) > PreferMax({data.PreferredMaxDistance}): Strafe 밖에서 약공 시도");
            }

            if (data.HeavyAttackRange < data.PreferredMinDistance)
            {
                DrawError($"HeavyRange({data.HeavyAttackRange}) < PreferMin({data.PreferredMinDistance}): 강공 시 후퇴하여 공격 불가");
                hasError = true;
            }

            return hasError;
        }

        private bool ValidateRanged(MonsterData data)
        {
            bool hasError = false;

            if (data.RangedMinDistance >= data.RangedAttackRange)
            {
                DrawError($"RangedMin({data.RangedMinDistance}) >= RangedMax({data.RangedAttackRange}): 원거리 범위 설정 오류");
                hasError = true;
            }

            if (data.RangedMinDistance > data.PreferredMaxDistance)
            {
                DrawError($"RangedMin({data.RangedMinDistance}) > PreferMax({data.PreferredMaxDistance}): Strafe 내 원거리 공격 불가");
                hasError = true;
            }

            if (data.RangedAttackRange < data.PreferredMaxDistance)
            {
                DrawWarning($"RangedMax({data.RangedAttackRange}) < PreferMax({data.PreferredMaxDistance}): Strafe 진입 전 사거리 이탈");
            }

            return hasError;
        }

        private bool ValidateHybrid(MonsterData data)
        {
            if (data.LightAttackRange < data.RangedMinDistance)
            {
                float gap = data.RangedMinDistance - data.LightAttackRange;
                if (gap > 0.5f)
                {
                    DrawWarning($"공격 불가 구간: {data.LightAttackRange} ~ {data.RangedMinDistance} (간격 {gap:F1})");
                }
            }

            return false;
        }

        private void DrawError(string message)
        {
            EditorGUILayout.HelpBox(message, MessageType.Error);
        }

        private void DrawWarning(string message)
        {
            EditorGUILayout.HelpBox(message, MessageType.Warning);
        }

        private void DrawSuccess(string message)
        {
            EditorGUILayout.HelpBox(message, MessageType.Info);
        }
    }
}
#endif