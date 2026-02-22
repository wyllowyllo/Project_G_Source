using Dialogue;
using Equipment;
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

        [Header("Progression")]
        [Tooltip("이전에 클리어해야 하는 던전. null이면 처음부터 열림")]
        [SerializeField] private DungeonData _requiredDungeon;

        [Header("Rewards")]
        [SerializeField] private int _clearXpReward;
        [Tooltip("던전 기본 드랍 테이블 (몬스터가 자체 테이블 없을 시 적용)")]
        [SerializeField] private DropTableData _dropTable;

        [Header("Dialogue")]
        [Tooltip("던전 클리어 후 대화")]
        [SerializeField] private DialogueData _clearDialogue;

        public string DungeonId => name;
        public string DisplayName => _displayName;
        public string SceneName => _sceneName;
        public int RecommendedLevel => _recommendedLevel;
        public int ClearXpReward => _clearXpReward;
        public DropTableData DropTable => _dropTable;
        public DungeonData RequiredDungeon => _requiredDungeon;
        public bool IsFirstDungeon => _requiredDungeon == null;
        public DialogueData ClearDialogue => _clearDialogue;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(_sceneName))
                Debug.LogWarning($"[{name}] SceneName is empty!", this);

            if (_requiredDungeon == this)
            {
                Debug.LogError($"[{name}] Cannot set self as required dungeon!", this);
                _requiredDungeon = null;
            }
        }
#endif
    }
}
