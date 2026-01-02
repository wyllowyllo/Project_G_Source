using UnityEditor;
using UnityEngine;

/// <summary>
/// 전투 시스템 초기 설정 유틸리티.
/// 레이어 추가 및 Physics 충돌 매트릭스를 자동으로 설정합니다.
/// </summary>
public static class CombatSystemSetup
{
    private const string LAYER_PLAYER = "Player";
    private const string LAYER_ENEMY = "Enemy";
    private const string LAYER_PLAYER_HITBOX = "PlayerHitbox";
    private const string LAYER_ENEMY_HITBOX = "EnemyHitbox";

    [MenuItem("Tools/Combat System/Setup Layers", false, 100)]
    public static void SetupLayers()
    {
        bool anyChanges = false;

        // 레이어 추가
        anyChanges |= AddLayerIfNotExists(LAYER_PLAYER);
        anyChanges |= AddLayerIfNotExists(LAYER_ENEMY);
        anyChanges |= AddLayerIfNotExists(LAYER_PLAYER_HITBOX);
        anyChanges |= AddLayerIfNotExists(LAYER_ENEMY_HITBOX);

        if (!anyChanges)
        {
            Debug.Log("[Combat System] All layers already exist.");
        }

        // Physics 충돌 매트릭스 설정
        SetupPhysicsMatrix();

        AssetDatabase.SaveAssets();
        Debug.Log("[Combat System] Setup complete!");
    }

    [MenuItem("Tools/Combat System/Setup Physics Matrix Only", false, 101)]
    public static void SetupPhysicsMatrixOnly()
    {
        SetupPhysicsMatrix();
        Debug.Log("[Combat System] Physics matrix setup complete!");
    }

    [MenuItem("Tools/Combat System/Validate Setup", false, 200)]
    public static void ValidateSetup()
    {
        bool isValid = true;
            
        // 레이어 확인
        string[] requiredLayers = { LAYER_PLAYER, LAYER_ENEMY, LAYER_PLAYER_HITBOX, LAYER_ENEMY_HITBOX };
        foreach (var layerName in requiredLayers)
        {
            int layer = LayerMask.NameToLayer(layerName);
            if (layer == -1)
            {
                Debug.LogWarning($"[Combat System] Missing layer: {layerName}");
                isValid = false;
            }
        }

        // Physics 매트릭스 확인
        if (isValid)
        {
            int playerLayer = LayerMask.NameToLayer(LAYER_PLAYER);
            int enemyLayer = LayerMask.NameToLayer(LAYER_ENEMY);
            int playerHitboxLayer = LayerMask.NameToLayer(LAYER_PLAYER_HITBOX);
            int enemyHitboxLayer = LayerMask.NameToLayer(LAYER_ENEMY_HITBOX);

            bool playerHitboxToEnemy = !Physics.GetIgnoreLayerCollision(playerHitboxLayer, enemyLayer);
            bool enemyHitboxToPlayer = !Physics.GetIgnoreLayerCollision(enemyHitboxLayer, playerLayer);

            if (!playerHitboxToEnemy)
            {
                Debug.LogWarning("[Combat System] PlayerHitbox ↔ Enemy collision is disabled");
                isValid = false;
            }

            if (!enemyHitboxToPlayer)
            {
                Debug.LogWarning("[Combat System] EnemyHitbox ↔ Player collision is disabled");
                isValid = false;
            }
        }

        if (isValid)
        {
            Debug.Log("[Combat System] ✓ All settings are valid!");
        }
        else
        {
            Debug.LogError("[Combat System] Setup validation failed. Run 'Tools > Combat System > Setup Layers' to fix.");
        }
    }

    private static bool AddLayerIfNotExists(string layerName)
    {
        // 이미 존재하는지 확인
        if (LayerMask.NameToLayer(layerName) != -1)
        {
            return false;
        }

        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAssetAtPath<Object>("ProjectSettings/TagManager.asset"));
        SerializedProperty layers = tagManager.FindProperty("layers");

        // 빈 슬롯 찾기 (8번부터 사용자 레이어)
        for (int i = 8; i < layers.arraySize; i++)
        {
            SerializedProperty layer = layers.GetArrayElementAtIndex(i);
            if (string.IsNullOrEmpty(layer.stringValue))
            {
                layer.stringValue = layerName;
                tagManager.ApplyModifiedProperties();
                Debug.Log($"[Combat System] Added layer '{layerName}' at index {i}");
                return true;
            }
        }

        Debug.LogError($"[Combat System] No empty layer slots available for '{layerName}'");
        return false;
    }

    private static void SetupPhysicsMatrix()
    {
        int playerLayer = LayerMask.NameToLayer(LAYER_PLAYER);
        int enemyLayer = LayerMask.NameToLayer(LAYER_ENEMY);
        int playerHitboxLayer = LayerMask.NameToLayer(LAYER_PLAYER_HITBOX);
        int enemyHitboxLayer = LayerMask.NameToLayer(LAYER_ENEMY_HITBOX);

        if (playerLayer == -1 || enemyLayer == -1 || playerHitboxLayer == -1 || enemyHitboxLayer == -1)
        {
            Debug.LogError("[Combat System] Cannot setup physics matrix: missing layers. Run 'Setup Layers' first.");
            return;
        }

        // 모든 히트박스 충돌 비활성화
        for (int i = 0; i < 32; i++)
        {
            Physics.IgnoreLayerCollision(playerHitboxLayer, i, true);
            Physics.IgnoreLayerCollision(enemyHitboxLayer, i, true);
        }

        // 필요한 충돌만 활성화
        // PlayerHitbox ↔ Enemy
        Physics.IgnoreLayerCollision(playerHitboxLayer, enemyLayer, false);
            
        // EnemyHitbox ↔ Player
        Physics.IgnoreLayerCollision(enemyHitboxLayer, playerLayer, false);

        Debug.Log("[Combat System] Physics collision matrix configured:");
        Debug.Log("  - PlayerHitbox ↔ Enemy: Enabled");
        Debug.Log("  - EnemyHitbox ↔ Player: Enabled");
        Debug.Log("  - All other hitbox collisions: Disabled");
    }
}