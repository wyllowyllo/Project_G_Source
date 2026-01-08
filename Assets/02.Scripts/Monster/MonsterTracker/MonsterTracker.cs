using System.Collections.Generic;
using Monster.AI;
using UnityEngine;
using UnityEngine.Events;

namespace Monster.MonsterTracker
{
    // 전체 몬스터를 추적하고 클리어 조건을 체크하는 매니저
    public class MonsterTracker : MonoBehaviour
    {
        public static MonsterTracker Instance { get; private set; }

        [Header("이벤트")]
        [Tooltip("모든 몬스터가 처치되었을 때 호출")]
        public UnityEvent OnAllMonstersDefeated;

        [Header("디버그")]
        [SerializeField] private bool _showDebugInfo = true;

        private readonly List<MonsterController> _allMonsters = new();
        private bool _isCleared;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("MonsterTracker: 이미 인스턴스가 존재합니다. 중복 제거합니다.");
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void RegisterMonster(MonsterController monster)
        {
            if (monster == null)
            {
                Debug.LogWarning("MonsterTracker: null 몬스터를 등록하려고 했습니다.");
                return;
            }

            if (!_allMonsters.Contains(monster))
            {
                _allMonsters.Add(monster);

                if (_showDebugInfo)
                {
                    Debug.Log($"MonsterTracker: 몬스터 등록 - {monster.name} (총 {_allMonsters.Count}마리)");
                }
            }
        }

        public void UnregisterMonster(MonsterController monster)
        {
            if (_allMonsters.Remove(monster))
            {
                if (_showDebugInfo)
                {
                    Debug.Log($"MonsterTracker: 몬스터 제거 - {monster.name} (남은 몬스터: {GetAliveMonsterCount()}마리)");
                }

                CheckClearCondition();
            }
        }

        public List<MonsterController> GetAliveMonsters()
        {
            return new List<MonsterController>(_allMonsters);
        }

        public int GetAliveMonsterCount()
        {
            return _allMonsters.Count;
        }

        public void CleanupDestroyedMonsters()
        {
            int removed = _allMonsters.RemoveAll(m => m == null || !m.IsAlive);
            if (removed > 0 && _showDebugInfo)
            {
                Debug.LogWarning($"MonsterTracker: {removed}개의 비정상 제거된 몬스터 정리");
            }
        }

       
        private void CheckClearCondition()
        {
            if (_isCleared)
            {
                return;
            }

            if (GetAliveMonsterCount() == 0)
            {
                _isCleared = true;

                if (_showDebugInfo)
                {
                    Debug.Log("MonsterTracker: 모든 몬스터 처치! 클리어!");
                }

                OnAllMonstersDefeated?.Invoke();
            }
        }

        public void ResetClearState()
        {
            _isCleared = false;
            _allMonsters.Clear();

            if (_showDebugInfo)
            {
                Debug.Log("MonsterTracker: 클리어 상태 초기화");
            }
        }

        private void OnGUI()
        {
            if (!_showDebugInfo)
            {
                return;
            }

            GUILayout.BeginArea(new Rect(10, 10, 300, 100));
            GUILayout.Label($"남은 몬스터: {GetAliveMonsterCount()}마리");
            if (_isCleared)
            {
                GUILayout.Label("[ 클리어! ]");
            }
            GUILayout.EndArea();
        }
    }
}
