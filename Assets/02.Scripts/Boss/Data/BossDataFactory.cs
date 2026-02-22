#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Boss.Data
{
    /// <summary>
    /// 기본 BossData 및 PhaseData 템플릿 생성 팩토리
    /// </summary>
    public static class BossDataFactory
    {
        [MenuItem("Assets/Create/ProjectG/Boss/Standard Boss Data (3 Phases)", false, 101)]
        public static void CreateStandardBossData()
        {
            var bossData = ScriptableObject.CreateInstance<BossData>();

            // 기본값은 BossData의 SerializeField 기본값 사용
            // 페이즈 데이터 설정
            var phases = new BossPhaseData[3];

            // Phase 1: 100% ~ 70% HP
            phases[0] = new BossPhaseData
            {
                HPThreshold = 1.0f,
                DamageMultiplier = 1.0f,
                AttackSpeedMultiplier = 1.0f,
                CooldownMultiplier = 1.0f,
                EnableCharge = true,
                EnableBreath = false,
                EnableProjectile = false,
                EnableSummon = false,
                PlayRoarOnTransition = false
            };

            // Phase 2: 70% ~ 40% HP
            phases[1] = new BossPhaseData
            {
                HPThreshold = 0.7f,
                DamageMultiplier = 1.2f,
                AttackSpeedMultiplier = 1.1f,
                CooldownMultiplier = 0.9f,
                EnableCharge = true,
                EnableBreath = true,
                EnableProjectile = true,
                EnableSummon = false,
                PlayRoarOnTransition = true
            };

            // Phase 3: 40% ~ 0% HP
            phases[2] = new BossPhaseData
            {
                HPThreshold = 0.4f,
                DamageMultiplier = 1.5f,
                AttackSpeedMultiplier = 1.3f,
                CooldownMultiplier = 0.7f,
                EnableCharge = true,
                EnableBreath = true,
                EnableProjectile = true,
                EnableSummon = true,
                PlayRoarOnTransition = true
            };

            // Reflection으로 private 필드 설정 (에디터 전용)
            SetPrivateField(bossData, "_phases", phases);

            string path = EditorUtility.SaveFilePanelInProject(
                "Save Standard Boss Data",
                "StandardBossData",
                "asset",
                "보스 데이터를 저장할 경로를 선택하세요."
            );

            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(bossData, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = bossData;

                Debug.Log($"Standard Boss Data가 생성되었습니다: {path}");
            }
        }

        [MenuItem("Assets/Create/ProjectG/Boss/Mini Boss Data (2 Phases)", false, 102)]
        public static void CreateMiniBossData()
        {
            var bossData = ScriptableObject.CreateInstance<BossData>();

            var phases = new BossPhaseData[2];

            // Phase 1: 100% ~ 50% HP
            phases[0] = new BossPhaseData
            {
                HPThreshold = 1.0f,
                DamageMultiplier = 1.0f,
                AttackSpeedMultiplier = 1.0f,
                CooldownMultiplier = 1.0f,
                EnableCharge = true,
                EnableBreath = false,
                EnableProjectile = false,
                EnableSummon = false,
                PlayRoarOnTransition = false
            };

            // Phase 2: 50% ~ 0% HP
            phases[1] = new BossPhaseData
            {
                HPThreshold = 0.5f,
                DamageMultiplier = 1.3f,
                AttackSpeedMultiplier = 1.2f,
                CooldownMultiplier = 0.8f,
                EnableCharge = true,
                EnableBreath = true,
                EnableProjectile = false,
                EnableSummon = false,
                PlayRoarOnTransition = true
            };

            SetPrivateField(bossData, "_phases", phases);

            // 미니보스용 기본값 조정
            SetPrivateField(bossData, "_maxHP", 500f);
            SetPrivateField(bossData, "_maxPoise", 50f);
            SetPrivateField(bossData, "_staggerDuration", 2f);

            string path = EditorUtility.SaveFilePanelInProject(
                "Save Mini Boss Data",
                "MiniBossData",
                "asset",
                "미니보스 데이터를 저장할 경로를 선택하세요."
            );

            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(bossData, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = bossData;

                Debug.Log($"Mini Boss Data가 생성되었습니다: {path}");
            }
        }

        private static void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (field != null)
            {
                field.SetValue(obj, value);
            }
            else
            {
                Debug.LogWarning($"Field '{fieldName}' not found.");
            }
        }
    }
}
#endif
