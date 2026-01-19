#if UNITY_EDITOR
using Boss.AI;
using Boss.Combat;
using Boss.Core;
using Boss.Test;
using Combat.Attack;
using Combat.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace Boss.Editor
{
    /// <summary>
    /// 보스 프리팹 설정을 도와주는 에디터 유틸리티
    /// </summary>
    public static class BossSetupEditor
    {
        [MenuItem("GameObject/ProjectG/Boss/Setup Boss Components", false, 10)]
        public static void SetupBossComponents()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                EditorUtility.DisplayDialog("Error", "보스 오브젝트를 선택해주세요.", "OK");
                return;
            }

            Undo.RegisterCompleteObjectUndo(selected, "Setup Boss Components");

            // 필수 컴포넌트 추가
            AddRequiredComponents(selected);

            // 하위 구조 생성
            CreateBossStructure(selected);

            EditorUtility.DisplayDialog("Success", "보스 컴포넌트 설정이 완료되었습니다.\n\n다음 단계:\n1. BossData ScriptableObject 할당\n2. Animator Controller 설정\n3. Hitbox Trigger 설정", "OK");
        }

        [MenuItem("GameObject/ProjectG/Boss/Add Debug Visualizer", false, 11)]
        public static void AddDebugVisualizer()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                EditorUtility.DisplayDialog("Error", "보스 오브젝트를 선택해주세요.", "OK");
                return;
            }

            if (selected.GetComponent<BossDebugVisualizer>() == null)
            {
                Undo.AddComponent<BossDebugVisualizer>(selected);
                Debug.Log("BossDebugVisualizer가 추가되었습니다.");
            }
            else
            {
                Debug.Log("BossDebugVisualizer가 이미 존재합니다.");
            }
        }

        [MenuItem("Assets/Create/ProjectG/Boss/Default Boss Data", false, 100)]
        public static void CreateDefaultBossData()
        {
            var asset = ScriptableObject.CreateInstance<Boss.Data.BossData>();

            string path = EditorUtility.SaveFilePanelInProject(
                "Save Boss Data",
                "NewBossData",
                "asset",
                "보스 데이터를 저장할 경로를 선택하세요."
            );

            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(asset, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = asset;
            }
        }

        private static void AddRequiredComponents(GameObject boss)
        {
            // NavMeshAgent
            if (boss.GetComponent<NavMeshAgent>() == null)
            {
                var agent = boss.AddComponent<NavMeshAgent>();
                agent.speed = 4f;
                agent.angularSpeed = 120f;
                agent.acceleration = 8f;
                agent.stoppingDistance = 0f;
                agent.autoBraking = true;
            }

            // Combatant
            if (boss.GetComponent<Combatant>() == null)
            {
                boss.AddComponent<Combatant>();
            }

            // Health (Combatant가 RequireComponent로 가짐)
            if (boss.GetComponent<Health>() == null)
            {
                boss.AddComponent<Health>();
            }

            // BossController
            if (boss.GetComponent<BossController>() == null)
            {
                boss.AddComponent<BossController>();
            }
        }

        private static void CreateBossStructure(GameObject boss)
        {
            // Combat 컨테이너
            Transform combatContainer = boss.transform.Find("Combat");
            if (combatContainer == null)
            {
                GameObject combatObj = new GameObject("Combat");
                combatObj.transform.SetParent(boss.transform);
                combatObj.transform.localPosition = Vector3.zero;
                combatContainer = combatObj.transform;
            }

            // MeleeHitbox
            if (combatContainer.Find("MeleeHitbox") == null)
            {
                GameObject hitboxObj = new GameObject("MeleeHitbox");
                hitboxObj.transform.SetParent(combatContainer);
                hitboxObj.transform.localPosition = new Vector3(0, 1f, 1.5f);
                hitboxObj.layer = LayerMask.NameToLayer("Hitbox");

                var collider = hitboxObj.AddComponent<BoxCollider>();
                collider.isTrigger = true;
                collider.size = new Vector3(3f, 2f, 3f);

                hitboxObj.AddComponent<HitboxTrigger>();
            }

            // ChargeHitbox
            if (combatContainer.Find("ChargeHitbox") == null)
            {
                GameObject hitboxObj = new GameObject("ChargeHitbox");
                hitboxObj.transform.SetParent(combatContainer);
                hitboxObj.transform.localPosition = new Vector3(0, 1f, 1f);
                hitboxObj.layer = LayerMask.NameToLayer("Hitbox");

                var collider = hitboxObj.AddComponent<BoxCollider>();
                collider.isTrigger = true;
                collider.size = new Vector3(2f, 2f, 2f);

                hitboxObj.AddComponent<HitboxTrigger>();
            }

            // BreathAttacker
            if (combatContainer.Find("BreathAttacker") == null)
            {
                GameObject breathObj = new GameObject("BreathAttacker");
                breathObj.transform.SetParent(combatContainer);
                breathObj.transform.localPosition = new Vector3(0, 1.5f, 1f);

                breathObj.AddComponent<BossBreathAttacker>();
            }

            // ProjectileLauncher
            if (combatContainer.Find("ProjectileLauncher") == null)
            {
                GameObject launcherObj = new GameObject("ProjectileLauncher");
                launcherObj.transform.SetParent(combatContainer);
                launcherObj.transform.localPosition = new Vector3(0, 2f, 1f);

                launcherObj.AddComponent<BossProjectileLauncher>();
            }

            // Telegraph 컨테이너
            Transform telegraphContainer = boss.transform.Find("Telegraph");
            if (telegraphContainer == null)
            {
                GameObject telegraphObj = new GameObject("Telegraph");
                telegraphObj.transform.SetParent(boss.transform);
                telegraphObj.transform.localPosition = Vector3.zero;

                telegraphObj.AddComponent<BossTelegraph>();
            }

            // Body placeholder (모델 위치)
            if (boss.transform.Find("Body") == null)
            {
                GameObject bodyObj = new GameObject("Body");
                bodyObj.transform.SetParent(boss.transform);
                bodyObj.transform.localPosition = Vector3.zero;

                // 애니메이션 이벤트 수신기 추가 (실제 모델에 붙여야 함)
                // bodyObj.AddComponent<BossAnimationEventReceiver>();

                Debug.Log("Body 오브젝트가 생성되었습니다. 여기에 보스 모델을 배치하고 BossAnimationEventReceiver를 Animator가 있는 오브젝트에 추가하세요.");
            }
        }
    }

    /// <summary>
    /// BossController 커스텀 인스펙터
    /// </summary>
    [CustomEditor(typeof(BossController))]
    public class BossControllerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            BossController controller = (BossController)target;

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("=== Quick Actions ===", EditorStyles.boldLabel);

            if (GUILayout.Button("Setup Boss Structure"))
            {
                BossSetupEditor.SetupBossComponents();
            }

            if (GUILayout.Button("Add Debug Visualizer"))
            {
                if (controller.GetComponent<BossDebugVisualizer>() == null)
                {
                    Undo.AddComponent<BossDebugVisualizer>(controller.gameObject);
                }
            }

            if (Application.isPlaying)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("=== Runtime Info ===", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"State: {controller.CurrentStateType}");

                if (controller.PatternSelector != null)
                {
                    EditorGUILayout.LabelField($"In Combo: {controller.PatternSelector.IsInCombo}");
                }

                if (controller.EnrageSystem != null)
                {
                    EditorGUILayout.LabelField($"Enraged: {controller.EnrageSystem.IsEnraged}");
                }

                if (controller.PhaseManager != null)
                {
                    EditorGUILayout.LabelField($"Phase: {controller.PhaseManager.CurrentPhaseNumber}");
                }
            }
        }
    }
}
#endif
