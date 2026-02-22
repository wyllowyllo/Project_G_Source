#if UNITY_EDITOR
using Dungeon;
using UnityEditor;
using UnityEngine;

namespace ProjectG.Editor
{
    public static class DungeonDataCreator
    {
        [MenuItem("ProjectG/Create Dungeon Data Assets")]
        public static void CreateDungeonDataAssets()
        {
            string path = "Assets/09.ScriptableObjects/Dungeon";

            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder("Assets/09.ScriptableObjects", "Dungeon");
            }

            CreateDungeonAsset(path, "Dungeon_Forest", "Forest Dungeon", "Dungeon_Forest", 5, 100);
            CreateDungeonAsset(path, "Dungeon_Cave", "Cave Dungeon", "Dungeon_Cave", 10, 200);
            CreateDungeonAsset(path, "Dungeon_Castle", "Castle Dungeon", "Dungeon_Castle", 20, 400);
            CreateDungeonAsset(path, "Dungeon_Abyss", "Abyss Dungeon", "Dungeon_Abyss", 25, 800);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[DungeonDataCreator] Created 4 dungeon data assets");
        }

        private static void CreateDungeonAsset(string path, string assetName, string displayName, string sceneName, int level, int xp)
        {
            string fullPath = $"{path}/{assetName}.asset";

            if (AssetDatabase.LoadAssetAtPath<DungeonData>(fullPath) != null)
            {
                Debug.Log($"[DungeonDataCreator] {assetName} already exists, skipping");
                return;
            }

            var asset = ScriptableObject.CreateInstance<DungeonData>();

            var so = new SerializedObject(asset);
            so.FindProperty("_displayName").stringValue = displayName;
            so.FindProperty("_sceneName").stringValue = sceneName;
            so.FindProperty("_recommendedLevel").intValue = level;
            so.FindProperty("_clearXpReward").intValue = xp;
            so.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.CreateAsset(asset, fullPath);
            Debug.Log($"[DungeonDataCreator] Created {assetName}");
        }
    }
}
#endif
