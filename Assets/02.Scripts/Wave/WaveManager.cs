using System.Collections.Generic;
using Monster;
using UnityEngine;

namespace Wave
{
    /// <summary>
    /// 몬스터 웨이브 스폰을 관리하는 매니저.
    /// 알파: EnemyGroup 기반 그룹 스폰 시스템
    /// </summary>
    public class WaveManager : MonoBehaviour
    {
        [System.Serializable]
        public class SpawnGroup
        {
            [Header("그룹 설정")]
            [Tooltip("EnemyGroup 프리팹 (파라미터는 프리팹 인스펙터에서 설정)")]
            public EnemyGroup EnemyGroupPrefab;
            public List<Transform> SpawnPoints;
            public GameObject MonsterPrefab;
        }

        [Header("스폰 설정")]
        [SerializeField] private List<SpawnGroup> _spawnGroups;
        [SerializeField] private float _spawnDelay = 1f;
        [SerializeField] private bool _spawnOnStart = true;

        [Header("디버그")]
        [SerializeField] private bool _showDebugGizmos = true;

        private readonly List<EnemyGroup> _enemyGroups = new();
        private readonly List<MonsterController> _spawnedMonsters = new();
        private float _spawnTimer;

        private void Start()
        {
            if (_spawnOnStart)
            {
                SpawnWave();
            }
        }

        /// <summary>
        /// 웨이브 스폰 - 알파: 그룹 단위 스폰
        /// </summary>
        public void SpawnWave()
        {
            if (_spawnGroups == null || _spawnGroups.Count == 0)
            {
                Debug.LogWarning("WaveManager: 스폰 그룹이 설정되지 않았습니다.");
                return;
            }

            foreach (SpawnGroup spawnGroup in _spawnGroups)
            {
                if (spawnGroup.SpawnPoints == null || spawnGroup.SpawnPoints.Count == 0)
                {
                    Debug.LogWarning("WaveManager: 스폰 포인트가 설정되지 않았습니다.");
                    continue;
                }

                if (spawnGroup.MonsterPrefab == null)
                {
                    Debug.LogWarning("WaveManager: 몬스터 프리팹이 null입니다.");
                    continue;
                }

                SpawnEnemyGroup(spawnGroup);
            }
        }

        /// <summary>
        /// 그룹 단위 스폰
        /// </summary>
        private void SpawnEnemyGroup(SpawnGroup spawnGroup)
        {
            if (spawnGroup.EnemyGroupPrefab == null)
            {
                Debug.LogError("WaveManager: EnemyGroup 프리팹이 null입니다.");
                return;
            }

            // EnemyGroup 프리팹 인스턴스 생성
            EnemyGroup enemyGroup = Instantiate(
                spawnGroup.EnemyGroupPrefab,
                transform
            );
            enemyGroup.name = $"EnemyGroup_{_enemyGroups.Count + 1}";

            // 플레이어 찾기
            enemyGroup.FindPlayer();

            // 몬스터 스폰 및 그룹 등록
            List<MonsterController> groupMonsters = new List<MonsterController>();

            foreach (Transform spawnPoint in spawnGroup.SpawnPoints)
            {
                if (spawnPoint == null)
                {
                    continue;
                }

                GameObject monsterObj = Instantiate(
                    spawnGroup.MonsterPrefab,
                    spawnPoint.position,
                    Quaternion.identity,
                    enemyGroup.transform
                );

                MonsterController monster = monsterObj.GetComponent<MonsterController>();
                if (monster != null)
                {
                    // 몬스터에 그룹 설정
                    monster.SetEnemyGroup(enemyGroup);

                    // 그룹에 몬스터 등록
                    enemyGroup.RegisterMonster(monster);

                    groupMonsters.Add(monster);
                    _spawnedMonsters.Add(monster);

                    Debug.Log($"몬스터 스폰: {monsterObj.name}");
                }
                else
                {
                    Debug.LogError($"스폰된 프리팹에 MonsterController가 없습니다: {monsterObj.name}");
                }
            }

            _enemyGroups.Add(enemyGroup);
            Debug.Log($"EnemyGroup 생성: {enemyGroup.name}, 몬스터 수: {groupMonsters.Count}");
        }


        /// <summary>
        /// 모든 살아있는 몬스터 반환
        /// </summary>
        public List<MonsterController> GetAliveMonsters()
        {
            _spawnedMonsters.RemoveAll(m => m == null || !m.IsAlive);
            return _spawnedMonsters;
        }

        /// <summary>
        /// 살아있는 몬스터 수 반환
        /// </summary>
        public int GetAliveMonsterCount()
        {
            return GetAliveMonsters().Count;
        }

        private void OnDrawGizmos()
        {
            if (!_showDebugGizmos || _spawnGroups == null)
            {
                return;
            }

            foreach (SpawnGroup spawnGroup in _spawnGroups)
            {
                if (spawnGroup.SpawnPoints == null)
                {
                    continue;
                }

                // 스폰 포인트
                Gizmos.color = Color.cyan;
                foreach (Transform spawnPoint in spawnGroup.SpawnPoints)
                {
                    if (spawnPoint != null)
                    {
                        Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
                        Gizmos.DrawLine(
                            spawnPoint.position,
                            spawnPoint.position + Vector3.up * 2f
                        );
                    }
                }
            }
        }
    }
}
