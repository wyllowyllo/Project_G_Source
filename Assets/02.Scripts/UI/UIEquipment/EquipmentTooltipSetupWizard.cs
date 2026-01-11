#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

namespace Equipment.Editor
{
    /// <summary>
    /// DroppedEquipment에 툴팁 UI를 자동으로 생성하는 에디터 유틸리티
    /// </summary>
    public class EquipmentTooltipSetupWizard : EditorWindow
    {
        private GameObject targetObject;
        private bool createCanvas = true;
        private bool createUI = true;
        private bool attachScript = true;

        [MenuItem("Tools/Equipment/Setup Tooltip UI")]
        public static void ShowWindow()
        {
            var window = GetWindow<EquipmentTooltipSetupWizard>("Tooltip Setup");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("장비 툴팁 UI 자동 생성", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "DroppedEquipment 게임 오브젝트에 툴팁 UI를 자동으로 생성합니다.\n" +
                "1. DroppedEquipment Prefab을 선택하세요\n" +
                "2. '툴팁 UI 생성' 버튼을 클릭하세요",
                MessageType.Info
            );

            GUILayout.Space(10);

            // 대상 오브젝트 선택
            targetObject = EditorGUILayout.ObjectField(
                "대상 오브젝트",
                targetObject,
                typeof(GameObject),
                true
            ) as GameObject;

            GUILayout.Space(10);

            // 옵션
            createCanvas = EditorGUILayout.Toggle("Canvas 생성", createCanvas);
            createUI = EditorGUILayout.Toggle("UI 요소 생성", createUI);
            attachScript = EditorGUILayout.Toggle("스크립트 연결", attachScript);

            GUILayout.Space(20);

            // 생성 버튼
            GUI.enabled = targetObject != null;
            if (GUILayout.Button("툴팁 UI 생성", GUILayout.Height(40)))
            {
                CreateTooltipUI();
            }
            GUI.enabled = true;

            GUILayout.Space(10);

            // 선택된 오브젝트로 적용 버튼
            if (Selection.activeGameObject != null)
            {
                if (GUILayout.Button($"선택된 오브젝트에 적용: {Selection.activeGameObject.name}"))
                {
                    targetObject = Selection.activeGameObject;
                    CreateTooltipUI();
                }
            }
        }

        private void CreateTooltipUI()
        {
            if (targetObject == null)
            {
                EditorUtility.DisplayDialog("오류", "대상 오브젝트를 선택해주세요.", "확인");
                return;
            }

            // DroppedEquipment 컴포넌트 확인
            var droppedEquipment = targetObject.GetComponent<DroppedEquipment>();
            if (droppedEquipment == null)
            {
                EditorUtility.DisplayDialog(
                    "오류",
                    "선택한 오브젝트에 DroppedEquipment 컴포넌트가 없습니다.",
                    "확인"
                );
                return;
            }

            Undo.RegisterCompleteObjectUndo(targetObject, "Create Tooltip UI");

            Canvas canvas = null;
            GameObject tooltipPanel = null;

            try
            {
                // Canvas 생성
                if (createCanvas)
                {
                    canvas = CreateCanvas(targetObject);
                }

                // UI 요소 생성
                if (createUI && canvas != null)
                {
                    tooltipPanel = CreateUIElements(canvas);
                }

                // 스크립트 연결
                if (attachScript)
                {
                    AttachAndConfigureScript(targetObject, canvas, tooltipPanel);
                }

                EditorUtility.DisplayDialog(
                    "완료",
                    "툴팁 UI가 성공적으로 생성되었습니다!",
                    "확인"
                );

                // Prefab 저장
                if (PrefabUtility.IsPartOfPrefabAsset(targetObject) || 
                    PrefabUtility.IsPartOfPrefabInstance(targetObject))
                {
                    PrefabUtility.RecordPrefabInstancePropertyModifications(targetObject);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"툴팁 UI 생성 중 오류 발생: {e.Message}");
                EditorUtility.DisplayDialog("오류", $"생성 중 오류가 발생했습니다:\n{e.Message}", "확인");
            }
        }

        private Canvas CreateCanvas(GameObject parent)
        {
            // 기존 Canvas 확인
            var existingCanvas = parent.GetComponentInChildren<Canvas>();
            if (existingCanvas != null)
            {
                if (!EditorUtility.DisplayDialog(
                    "기존 Canvas 발견",
                    "이미 Canvas가 존재합니다. 새로 생성하시겠습니까?",
                    "새로 생성",
                    "기존 사용"
                ))
                {
                    return existingCanvas;
                }
            }

            // Canvas 생성
            GameObject canvasObj = new GameObject("TooltipCanvas");
            canvasObj.transform.SetParent(parent.transform, false);
            canvasObj.transform.localPosition = Vector3.zero;

            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            var canvasScaler = canvasObj.AddComponent<CanvasScaler>();
            canvasScaler.scaleFactor = 0.01f;
            canvasScaler.dynamicPixelsPerUnit = 10;

            canvasObj.AddComponent<GraphicRaycaster>();

            // RectTransform 설정
            var rectTransform = canvasObj.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(500, 300);
            rectTransform.localPosition = new Vector3(0, 1.5f, 0);

            return canvas;
        }

        private GameObject CreateUIElements(Canvas canvas)
        {
            // 메인 패널
            GameObject panel = CreatePanel(canvas.transform, "TooltipPanel", 400, 250);
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;

            // 배경
            var background = CreateImage(panel.transform, "Background", new Color(0.2f, 0.2f, 0.2f, 0.9f));
            StretchRectTransform(background.rectTransform);

            // 등급 텍스트
            var gradeText = CreateText(panel.transform, "GradeText", "등급: 노말", 24);
            var gradeRect = gradeText.rectTransform;
            gradeRect.anchorMin = new Vector2(0, 1);
            gradeRect.anchorMax = new Vector2(1, 1);
            gradeRect.pivot = new Vector2(0.5f, 1);
            gradeRect.anchoredPosition = new Vector2(0, 0);
            gradeRect.sizeDelta = new Vector2(0, 50);
            gradeText.alignment = TextAlignmentOptions.Center;

            // 컨텐츠 패널
            GameObject contentPanel = CreatePanel(panel.transform, "ContentPanel", 0, 0);
            var contentRect = contentPanel.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 0);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 0.5f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.offsetMin = new Vector2(10, 10);
            contentRect.offsetMax = new Vector2(-10, -60);

            var horizontalLayout = contentPanel.AddComponent<HorizontalLayoutGroup>();
            horizontalLayout.spacing = 10;
            horizontalLayout.padding = new RectOffset(10, 10, 10, 10);
            horizontalLayout.childForceExpandWidth = true;
            horizontalLayout.childForceExpandHeight = true;

            // 왼쪽 패널 (아이콘)
            GameObject leftPanel = CreatePanel(contentPanel.transform, "LeftPanel", 0, 0);
            var leftLayout = leftPanel.AddComponent<LayoutElement>();
            leftLayout.preferredWidth = 150;
            leftLayout.preferredHeight = 150;

            var itemIcon = CreateImage(leftPanel.transform, "ItemIcon", Color.white);
            StretchRectTransform(itemIcon.rectTransform);
            itemIcon.preserveAspect = true;

            // 오른쪽 패널 (스탯)
            GameObject rightPanel = CreatePanel(contentPanel.transform, "RightPanel", 0, 0);
            var verticalLayout = rightPanel.AddComponent<VerticalLayoutGroup>();
            verticalLayout.spacing = 10;
            verticalLayout.padding = new RectOffset(5, 5, 5, 5);
            verticalLayout.childAlignment = TextAnchor.UpperLeft;
            verticalLayout.childForceExpandWidth = true;
            verticalLayout.childForceExpandHeight = false;

            // 스탯 텍스트들
            var itemNameText = CreateText(rightPanel.transform, "ItemNameText", "장비이름: 헬멧", 18);
            itemNameText.alignment = TextAlignmentOptions.Left;
            AddLayoutElement(itemNameText.gameObject, 30);

            var attackText = CreateText(rightPanel.transform, "AttackText", "공격력: 75", 18);
            attackText.alignment = TextAlignmentOptions.Left;
            AddLayoutElement(attackText.gameObject, 30);

            var defenseText = CreateText(rightPanel.transform, "DefenseText", "방어력: 35", 18);
            defenseText.alignment = TextAlignmentOptions.Left;
            AddLayoutElement(defenseText.gameObject, 30);

            return panel;
        }

        private void AttachAndConfigureScript(GameObject target, Canvas canvas, GameObject tooltipPanel)
        {
            var tooltipController = target.GetComponent<EquipmentTooltipController>();
            if (tooltipController == null)
            {
                tooltipController = target.AddComponent<EquipmentTooltipController>();
            }

            if (canvas != null && tooltipPanel != null)
            {
                var serializedObject = new SerializedObject(tooltipController);

                serializedObject.FindProperty("_tooltipCanvas").objectReferenceValue = canvas;
                serializedObject.FindProperty("_tooltipPanel").objectReferenceValue = tooltipPanel;

                // UI 요소들 찾아서 연결
                serializedObject.FindProperty("_gradeText").objectReferenceValue = 
                    tooltipPanel.transform.Find("GradeText")?.GetComponent<TextMeshProUGUI>();
                
                serializedObject.FindProperty("_itemIcon").objectReferenceValue = 
                    tooltipPanel.transform.Find("ContentPanel/LeftPanel/ItemIcon")?.GetComponent<Image>();
                
                serializedObject.FindProperty("_itemNameText").objectReferenceValue = 
                    tooltipPanel.transform.Find("ContentPanel/RightPanel/ItemNameText")?.GetComponent<TextMeshProUGUI>();
                
                serializedObject.FindProperty("_attackText").objectReferenceValue = 
                    tooltipPanel.transform.Find("ContentPanel/RightPanel/AttackText")?.GetComponent<TextMeshProUGUI>();
                
                serializedObject.FindProperty("_defenseText").objectReferenceValue = 
                    tooltipPanel.transform.Find("ContentPanel/RightPanel/DefenseText")?.GetComponent<TextMeshProUGUI>();
                
                serializedObject.FindProperty("_backgroundImage").objectReferenceValue = 
                    tooltipPanel.transform.Find("Background")?.GetComponent<Image>();

                // 기본 설정
                serializedObject.FindProperty("_tooltipOffset").vector3Value = new Vector3(0, 1.5f, 0);
                serializedObject.FindProperty("_showDistance").floatValue = 5f;
                serializedObject.FindProperty("_enableBillboard").boolValue = true;

                serializedObject.ApplyModifiedProperties();
            }
        }

        // Helper Methods
        private GameObject CreatePanel(Transform parent, string name, float width, float height)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform));
            panel.transform.SetParent(parent, false);
            
            var image = panel.AddComponent<Image>();
            image.color = new Color(0, 0, 0, 0);
            image.raycastTarget = false;

            var rect = panel.GetComponent<RectTransform>();
            if (width > 0) rect.sizeDelta = new Vector2(width, height);

            return panel;
        }

        private Image CreateImage(Transform parent, string name, Color color)
        {
            GameObject imageObj = new GameObject(name, typeof(RectTransform));
            imageObj.transform.SetParent(parent, false);

            var image = imageObj.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;

            return image;
        }

        private TextMeshProUGUI CreateText(Transform parent, string name, string text, float fontSize)
        {
            GameObject textObj = new GameObject(name, typeof(RectTransform));
            textObj.transform.SetParent(parent, false);

            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = Color.white;
            tmp.raycastTarget = false;

            return tmp;
        }

        private void StretchRectTransform(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
        }

        private void AddLayoutElement(GameObject obj, float preferredHeight)
        {
            var layoutElement = obj.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = preferredHeight;
        }
    }
}
#endif
