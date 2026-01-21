using System;
using System.Collections.Generic;
using Core;
using Dialogue;
using UnityEngine;

namespace Dungeon
{
    public class DungeonManager : MonoBehaviour
    {
        public static DungeonManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private DungeonData[] _allDungeons;
        [SerializeField] private string _townSceneName = "TownScene";

        private DungeonData _currentDungeon;
        private HashSet<string> _clearedDungeons = new();

        public event Action<int, bool> DungeonCleared;
        public event Action DungeonFailed;
        public event Action GameCompleted;
        public event Action<DungeonData> DungeonUnlocked;
        public event Action DungeonEntered;
        public event Action DungeonExited;

        public DungeonData CurrentDungeon => _currentDungeon;
        public bool IsInDungeon => _currentDungeon != null;
        public IReadOnlyList<DungeonData> AllDungeons => _allDungeons;
        public bool IsFreeModeUnlocked => _clearedDungeons.Count >= _allDungeons.Length;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public bool IsDungeonCleared(string dungeonId) => _clearedDungeons.Contains(dungeonId);

        public DialogueData GetCurrentDungeonClearDialogue() => _currentDungeon?.ClearDialogue;

        public bool IsDungeonUnlocked(DungeonData dungeon)
        {
            if (dungeon == null) return false;
            if (dungeon.IsFirstDungeon) return true;
            return IsDungeonCleared(dungeon.RequiredDungeon.DungeonId);
        }

        public DungeonData GetNextDungeon(DungeonData clearedDungeon)
        {
            foreach (var dungeon in _allDungeons)
            {
                if (dungeon.RequiredDungeon == clearedDungeon)
                    return dungeon;
            }
            return null;
        }

        public void EnterDungeon(DungeonData dungeon)
        {
            if (dungeon == null)
            {
                Debug.LogWarning("[DungeonManager] Cannot enter null dungeon");
                return;
            }
            if (_currentDungeon != null)
            {
                Debug.LogWarning("[DungeonManager] Already in dungeon");
                return;
            }

            _currentDungeon = dungeon;
            DungeonEntered?.Invoke();
            SceneLoader.LoadScene(dungeon.SceneName);
        }

        public void CompleteDungeon()
        {
            Debug.Log("CompleteDungeon");
            if (_currentDungeon == null)
            {
                Debug.LogWarning("[DungeonManager] CompleteDungeon called but not in dungeon");
                return;
            }

            bool isFirstClear = !_clearedDungeons.Contains(_currentDungeon.DungeonId);

            if (isFirstClear)
            {
                _clearedDungeons.Add(_currentDungeon.DungeonId);

                var nextDungeon = GetNextDungeon(_currentDungeon);
                if (nextDungeon != null)
                {
                    DungeonUnlocked?.Invoke(nextDungeon);
                }

                if (_clearedDungeons.Count >= _allDungeons.Length)
                {
                    GameCompleted?.Invoke();
                }
            }

            // 첫 클리어: XP 보상, 재클리어: 0
            int xpReward = isFirstClear ? _currentDungeon.ClearXpReward : 0;
            DungeonCleared?.Invoke(xpReward, isFirstClear);
            
        }

        public void FailDungeon()
        {
            DungeonFailed?.Invoke();
        }

        public void ReturnToTown()
        {
            _currentDungeon = null;
            DungeonExited?.Invoke();
            SceneLoader.LoadScene(_townSceneName);
        }

#if UNITY_EDITOR
        public void Editor_SetDungeonCleared(string dungeonId, bool cleared)
        {
            if (cleared)
                _clearedDungeons.Add(dungeonId);
            else
                _clearedDungeons.Remove(dungeonId);
        }
#endif
    }
}
