using UnityEngine;

namespace Dungeon
{
    [CreateAssetMenu(fileName = "DungeonData", menuName = "ProjectG/Dungeon Data")]
    public class DungeonData : ScriptableObject
    {
        [Header("Basic Info")]
        [SerializeField] private string _displayName;
        [SerializeField] private string _sceneName;
        [SerializeField] private int _recommendedLevel;

        [Header("Rewards")]
        [SerializeField] private int _clearXpReward;

        public string DungeonId => name;
        public string DisplayName => _displayName;
        public string SceneName => _sceneName;
        public int RecommendedLevel => _recommendedLevel;
        public int ClearXpReward => _clearXpReward;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(_sceneName))
                Debug.LogWarning($"[{name}] SceneName is empty!", this);
        }
#endif
    }
}
