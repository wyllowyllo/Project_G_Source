using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Monster.MonsterTracker
{
    /// <summary>
    /// 전체 몬스터를 추적하고 클리어 조건을 체크하는 매니저
    /// </summary>
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

        /// <summary>
        /// 몬스터 등록 (SpawnGroup에서 호출)
        /// </summary>
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

        /// <summary>
        /// 몬스터 제거 (몬스터 사망 시 호출)
        /// </summary>
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

        /// <summary>
        /// 모든 살아있는 몬스터 반환
        /// </summary>
        public List<MonsterController> GetAliveMonsters()
        {
            _allMonsters.RemoveAll(m => m == null || !m.IsAlive);
            return new List<MonsterController>(_allMonsters);
        }

        /// <summary>
        /// 살아있는 몬스터 수 반환
        /// </summary>
        public int GetAliveMonsterCount()
        {
            return GetAliveMonsters().Count;
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

        /// <summary>
        /// 클리어 상태 초기화 (재시작 시 호출)
        /// </summary>
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
