#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

/// <summary>
/// 캐릭터 뷰어 자동 설정 도구
/// Tools 메뉴에서 실행 가능
/// </summary>
public class CharacterViewerSetupWizard : EditorWindow
{
    private const string LAYER_NAME = "CharacterViewer";
    
    private GameObject targetObject;
    private GameObject viewerPanel;
    private Camera viewerCamera;
    private Transform player;

    [MenuItem("Tools/Character Viewer/Setup Wizard")]
    public static void ShowWindow()
    {
        var window = GetWindow<CharacterViewerSetupWizard>("Character Viewer Setup");
        window.minSize = new Vector2(450, 400);
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("캐릭터 뷰어 자동 설정", EditorStyles.boldLabel);
        GUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "배틀그라운드 스타일 캐릭터 뷰어를 자동으로 설정합니다.\n\n" +
            "설정 내용:\n" +
            "1. CharacterViewer Layer 생성\n" +
            "2. RenderTexture 및 Camera 설정\n" +
            "3. UI RawImage 생성 및 연결\n" +
            "4. CharacterViewer 스크립트 설정",
            MessageType.Info
        );

        GUILayout.Space(15);

        // 필수 참조
        EditorGUILayout.LabelField("필수 참조", EditorStyles.boldLabel);
        
        viewerPanel = EditorGUILayout.ObjectField(
            "Viewer Panel (Canvas)",
            viewerPanel,
            typeof(GameObject),
            true
        ) as GameObject;

        viewerCamera = EditorGUILayout.ObjectField(
            "Viewer Camera",
            viewerCamera,
            typeof(Camera),
            true
        ) as Camera;

        player = EditorGUILayout.ObjectField(
            "Player",
            player,
            typeof(Transform),
            true
        ) as Transform;

        GUILayout.Space(15);

        // 자동 찾기 버튼
        if (GUILayout.Button("📡 자동으로 참조 찾기"))
        {
            AutoFindReferences();
        }

        GUILayout.Space(20);

        // 설정 버튼
        GUI.enabled = viewerPanel != null && viewerCamera != null && player != null;
        
        if (GUILayout.Button("✨ 캐릭터 뷰어 자동 설정", GUILayout.Height(50)))
        {
            SetupCharacterViewer();
        }
        
        GUI.enabled = true;

        GUILayout.Space(10);

        // Layer만 생성 버튼
        if (GUILayout.Button("🔧 CharacterViewer Layer만 생성"))
        {
            CreateLayer();
        }
    }

    private void AutoFindReferences()
    {
        Debug.Log("[Setup] 자동으로 참조를 찾는 중...");

        // Viewer Panel 찾기
        if (viewerPanel == null)
        {
            var canvases = FindObjectsOfType<Canvas>();
            foreach (var canvas in canvases)
            {
                if (canvas.name.Contains("Viewer") || canvas.name.Contains("Character"))
                {
                    viewerPanel = canvas.gameObject;
                    Debug.Log($"[Setup] Viewer Panel 찾음: {viewerPanel.name}");
                    break;
                }
            }
        }

        // Viewer Camera 찾기
        if (viewerCamera == null)
        {
            var cameras = FindObjectsOfType<Camera>();
            foreach (var cam in cameras)
            {
                if (cam.name.Contains("Viewer") || cam.name.Contains("Character"))
                {
                    viewerCamera = cam;
                    Debug.Log($"[Setup] Viewer Camera 찾음: {viewerCamera.name}");
                    break;
                }
            }
        }

        // Player 찾기
        if (player == null)
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                Debug.Log($"[Setup] Player 찾음: {player.name}");
            }
        }

        if (viewerPanel == null || viewerCamera == null || player == null)
        {
            EditorUtility.DisplayDialog(
                "일부 참조를 찾을 수 없음",
                "모든 참조를 자동으로 찾을 수 없었습니다.\n수동으로 할당해주세요.",
                "확인"
            );
        }
        else
        {
            EditorUtility.DisplayDialog(
                "완료",
                "모든 참조를 찾았습니다!",
                "확인"
            );
        }
    }

    private void SetupCharacterViewer()
    {
        if (!EditorUtility.DisplayDialog(
            "캐릭터 뷰어 설정",
            "캐릭터 뷰어를 자동으로 설정하시겠습니까?\n\n" +
            "다음 작업이 수행됩니다:\n" +
            "- CharacterViewer Layer 생성\n" +
            "- Camera 및 RenderTexture 설정\n" +
            "- UI RawImage 생성\n" +
            "- CharacterViewer 스크립트 추가 및 설정",
            "설정 시작",
            "취소"))
        {
            return;
        }

        try
        {
            // 1. Layer 생성
            CreateLayer();

            // 2. RenderTexture 생성
            var renderTexture = CreateRenderTexture();

            // 3. Camera 설정
            SetupCamera(viewerCamera, renderTexture);

            // 4. RawImage 생성
            var rawImage = CreateRawImage(viewerPanel, renderTexture);

            // 5. CharacterViewer 스크립트 설정
            SetupCharacterViewerScript(rawImage);

            EditorUtility.DisplayDialog(
                "설정 완료!",
                "캐릭터 뷰어가 성공적으로 설정되었습니다!\n\n" +
                "Tab키를 눌러서 테스트해보세요.",
                "확인"
            );

            Debug.Log("[Setup] ✅ 캐릭터 뷰어 설정 완료!");
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog(
                "오류",
                $"설정 중 오류가 발생했습니다:\n{e.Message}",
                "확인"
            );
            Debug.LogError($"[Setup] 오류: {e.Message}\n{e.StackTrace}");
        }
    }

    private void CreateLayer()
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        
        SerializedProperty layers = tagManager.FindProperty("layers");

        // Layer가 이미 존재하는지 확인
        for (int i = 8; i < layers.arraySize; i++)
        {
            SerializedProperty layer = layers.GetArrayElementAtIndex(i);
            if (layer.stringValue == LAYER_NAME)
            {
                Debug.Log($"[Setup] '{LAYER_NAME}' Layer가 이미 존재합니다.");
                return;
            }
        }

        // 빈 Layer 슬롯 찾기
        for (int i = 8; i < layers.arraySize; i++)
        {
            SerializedProperty layer = layers.GetArrayElementAtIndex(i);
            if (string.IsNullOrEmpty(layer.stringValue))
            {
                layer.stringValue = LAYER_NAME;
                tagManager.ApplyModifiedProperties();
                Debug.Log($"[Setup] ✅ '{LAYER_NAME}' Layer 생성 완료 (Layer {i})");
                return;
            }
        }

        Debug.LogWarning("[Setup] ⚠️ 빈 Layer 슬롯이 없습니다!");
    }

    private RenderTexture CreateRenderTexture()
    {
        var rt = new RenderTexture(1024, 1024, 24, RenderTextureFormat.ARGB32);
        rt.antiAliasing = 4;
        rt.name = "CharacterViewerRenderTexture";
        
        // Assets 폴더에 저장
        string path = "Assets/CharacterViewerRenderTexture.renderTexture";
        AssetDatabase.CreateAsset(rt, path);
        AssetDatabase.SaveAssets();
        
        Debug.Log($"[Setup] ✅ RenderTexture 생성: {path}");
        return rt;
    }

    private void SetupCamera(Camera cam, RenderTexture rt)
    {
        Undo.RecordObject(cam, "Setup Viewer Camera");

        cam.targetTexture = rt;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0, 0, 0, 0); // 투명
        
        int layerMask = LayerMask.GetMask(LAYER_NAME);
        if (layerMask != 0)
        {
            cam.cullingMask = layerMask;
        }
        
        cam.depth = -1;
        cam.enabled = false; // 일단 비활성화

        EditorUtility.SetDirty(cam);
        Debug.Log("[Setup] ✅ Camera 설정 완료");
    }

    private RawImage CreateRawImage(GameObject panel, RenderTexture rt)
    {
        // 기존 CharacterDisplay 찾기
        var existing = panel.transform.Find("CharacterDisplay");
        if (existing != null)
        {
            var existingRawImage = existing.GetComponent<RawImage>();
            if (existingRawImage != null)
            {
                existingRawImage.texture = rt;
                Debug.Log("[Setup] ✅ 기존 RawImage에 RenderTexture 할당");
                return existingRawImage;
            }
        }

        // 새로 생성
        GameObject rawImageObj = new GameObject("CharacterDisplay");
        rawImageObj.transform.SetParent(panel.transform, false);

        var rawImage = rawImageObj.AddComponent<RawImage>();
        rawImage.texture = rt;
        rawImage.color = Color.white;

        // RectTransform 설정
        var rect = rawImageObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(700, 900);

        Debug.Log("[Setup] ✅ RawImage 생성 완료");
        return rawImage;
    }

    private void SetupCharacterViewerScript(RawImage rawImage)
    {
        // CharacterViewer 스크립트 찾기 또는 추가
        var characterViewer = FindObjectOfType<CharacterViewer>();
        GameObject scriptHolder = null;

        if (characterViewer == null)
        {
            // GameManager 또는 적절한 오브젝트 찾기
            scriptHolder = GameObject.Find("GameManager");
            if (scriptHolder == null)
            {
                scriptHolder = new GameObject("CharacterViewerManager");
            }

            characterViewer = scriptHolder.AddComponent<CharacterViewer>();
            Debug.Log("[Setup] CharacterViewer 스크립트 추가");
        }
        else
        {
            scriptHolder = characterViewer.gameObject;
            Debug.Log("[Setup] 기존 CharacterViewer 스크립트 발견");
        }

        // SerializedObject로 private 필드 설정
        SerializedObject so = new SerializedObject(characterViewer);
        
        so.FindProperty("_viewerPanel").objectReferenceValue = viewerPanel;
        so.FindProperty("_viewerCamera").objectReferenceValue = viewerCamera;
        so.FindProperty("_characterDisplay").objectReferenceValue = rawImage;
        so.FindProperty("_player").objectReferenceValue = player;
        
        var playerEquipment = player.GetComponent<Equipment.PlayerEquipment>();
        if (playerEquipment != null)
        {
            so.FindProperty("_playerEquipment").objectReferenceValue = playerEquipment;
        }

        so.FindProperty("_renderTextureWidth").intValue = 1024;
        so.FindProperty("_renderTextureHeight").intValue = 1024;
        so.FindProperty("_backgroundColor").colorValue = new Color(0, 0, 0, 0);
        so.FindProperty("_characterViewerLayer").stringValue = LAYER_NAME;
        so.FindProperty("_autoSetupLayers").boolValue = true;

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(characterViewer);

        Debug.Log("[Setup] ✅ CharacterViewer 스크립트 설정 완료");
    }
}

/// <summary>
/// CharacterViewer Layer 빠른 토글
/// </summary>
public class CharacterViewerLayerHelper : EditorWindow
{
    [MenuItem("Tools/Character Viewer/Toggle Player Layer")]
    public static void TogglePlayerLayer()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            EditorUtility.DisplayDialog("오류", "Player를 찾을 수 없습니다!", "확인");
            return;
        }

        int layerIndex = LayerMask.NameToLayer("CharacterViewer");
        if (layerIndex == -1)
        {
            EditorUtility.DisplayDialog(
                "오류",
                "CharacterViewer Layer가 존재하지 않습니다!\n" +
                "Tools → Character Viewer → Setup Wizard를 먼저 실행하세요.",
                "확인"
            );
            return;
        }

        if (player.layer == layerIndex)
        {
            // 원래 Layer로 복원 (Default = 0)
            SetLayerRecursively(player.transform, 0);
            Debug.Log("[Helper] Player Layer를 Default로 복원");
        }
        else
        {
            // CharacterViewer Layer로 변경
            SetLayerRecursively(player.transform, layerIndex);
            Debug.Log("[Helper] Player Layer를 CharacterViewer로 변경");
        }
    }

    private static void SetLayerRecursively(Transform obj, int layer)
    {
        obj.gameObject.layer = layer;
        foreach (Transform child in obj)
        {
            SetLayerRecursively(child, layer);
        }
    }
}
#endif
