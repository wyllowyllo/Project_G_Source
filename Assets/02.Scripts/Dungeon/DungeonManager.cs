using System;
using System.Collections.Generic;
using Progression;
using UnityEngine;
using UnityEngine.SceneManagement;

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

        public event Action DungeonCleared;
        public event Action DungeonFailed;
        public event Action GameCompleted;

        public DungeonData CurrentDungeon => _currentDungeon;
        public bool IsInDungeon => _currentDungeon != null;
        public IReadOnlyList<DungeonData> AllDungeons => _allDungeons;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadProgress();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public bool IsDungeonCleared(string dungeonId) => _clearedDungeons.Contains(dungeonId);

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

            var asyncLoad = SceneManager.LoadSceneAsync(dungeon.SceneName);
            if (asyncLoad == null)
            {
                Debug.LogError($"[DungeonManager] Failed to load scene: {dungeon.SceneName}");
                _currentDungeon = null;
            }
        }

        public void CompleteDungeon()
        {
            if (_currentDungeon == null)
            {
                Debug.LogWarning("[DungeonManager] CompleteDungeon called but not in dungeon");
                return;
            }

            if (_clearedDungeons.Contains(_currentDungeon.DungeonId))
                return;

            _clearedDungeons.Add(_currentDungeon.DungeonId);

            PlayerPrefs.SetInt($"Dungeon_{_currentDungeon.DungeonId}_Cleared", 1);
            PlayerPrefs.Save();

            FindFirstObjectByType<PlayerProgression>()?.AddExperience(_currentDungeon.ClearXpReward);

            DungeonCleared?.Invoke();

            if (_clearedDungeons.Count >= _allDungeons.Length)
            {
                GameCompleted?.Invoke();
            }
        }

        public void FailDungeon()
        {
            DungeonFailed?.Invoke();
        }

        public void ReturnToTown()
        {
            _currentDungeon = null;
            SceneManager.LoadScene(_townSceneName);
        }

        private void LoadProgress()
        {
            foreach (var dungeon in _allDungeons)
            {
                if (PlayerPrefs.GetInt($"Dungeon_{dungeon.DungeonId}_Cleared", 0) == 1)
                {
                    _clearedDungeons.Add(dungeon.DungeonId);
                }
            }
        }
    }
}
