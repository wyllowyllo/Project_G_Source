using System.Collections.Generic;
using Monster.AI;
using UnityEngine;

namespace Monster.Group
{
    /// <summary>
    /// 독립적으로 몬스터 그룹을 스폰하는 컴포넌트
    /// 씬에 배치하여 구역별로 몬스터를 관리
    /// </summary>
    public class SpawnGroup : MonoBehaviour
    {
        [System.Serializable]
        public class MonsterSpawn
        {
            [Tooltip("몬스터가 스폰될 위치")]
            public Transform spawnPoint;
            [Tooltip("스폰할 몬스터 프리팹")]
            public GameObject monsterPrefab;
        }

        [Header("그룹 설정")]
        [Tooltip("EnemyGroup 프리팹 (aggroRange, maxAttackSlots 등은 프리팹 인스펙터에서 설정)")]
        [SerializeField] private EnemyGroup _enemyGroupPrefab;

        [Header("몬스터 스폰 설정")]
        [Tooltip("각 위치에 스폰할 몬스터")]
        [SerializeField] private List<MonsterSpawn> _monsterSpawns;

        [Header("스폰 조건")]
        [SerializeField] private bool _spawnOnStart = true;

        [Header("디버그")]
        [SerializeField] private bool _showDebugGizmos = true;

        private EnemyGroup _enemyGroup;
        private readonly List<MonsterController> _spawnedMonsters = new();
       

        private void Start()
        {
            if (_spawnOnStart)
            {
                Spawn();
            }
        }
        
        private void Spawn()
        {
            if (_monsterSpawns == null || _monsterSpawns.Count == 0)
            {
                Debug.LogWarning($"SpawnGroup [{gameObject.name}]: 몬스터 스폰 설정이 없습니다.");
                return;
            }

            if (_enemyGroupPrefab == null)
            {
                Debug.LogError($"SpawnGroup [{gameObject.name}]: EnemyGroup 프리팹이 null입니다.");
                return;
            }

            SpawnEnemyGroup();
        }

        private void SpawnEnemyGroup()
        {
            // EnemyGroup 프리팹 인스턴스 생성
            _enemyGroup = Instantiate(_enemyGroupPrefab, transform);
            _enemyGroup.name = $"{gameObject.name}_EnemyGroup";

            // 플레이어 찾기
            _enemyGroup.FindPlayer();

            // 몬스터 스폰 및 그룹 등록
            foreach (MonsterSpawn monsterSpawn in _monsterSpawns)
            {
                if (monsterSpawn.spawnPoint == null)
                {
                    Debug.LogWarning($"SpawnGroup [{gameObject.name}]: 스폰 포인트가 null입니다.");
                    continue;
                }

                if (monsterSpawn.monsterPrefab == null)
                {
                    Debug.LogWarning($"SpawnGroup [{gameObject.name}]: 몬스터 프리팹이 null입니다.");
                    continue;
                }

                GameObject monsterObj = Instantiate(
                    monsterSpawn.monsterPrefab,
                    monsterSpawn.spawnPoint.position,
                    Quaternion.identity,
                    _enemyGroup.transform
                );

                MonsterController monster = monsterObj.GetComponent<MonsterController>();
                if (monster != null)
                {
                    // 몬스터에 그룹 설정
                    monster.GroupCommandProvider.SetEnemyGroup(_enemyGroup);

                    // 그룹에 몬스터 등록
                    _enemyGroup.RegisterMonster(monster);

                    _spawnedMonsters.Add(monster);

                    // MonsterTracker에 등록
                    MonsterTracker.MonsterTracker.Instance?.RegisterMonster(monster);

                    Debug.Log($"SpawnGroup [{gameObject.name}]: 몬스터 스폰 - {monsterObj.name}");
                }
               
            }
            
        }

       
        private List<MonsterController> GetAliveMonsters()
        {
            _spawnedMonsters.RemoveAll(m => m == null || !m.IsAlive);
            return _spawnedMonsters;
        }

        /// <summary>
        /// 이 그룹의 살아있는 몬스터 수 반환
        /// </summary>
        public int GetAliveMonsterCount()
        {
            return GetAliveMonsters().Count;
        }

        private void OnDrawGizmos()
        {
            if (!_showDebugGizmos || _monsterSpawns == null)
            {
                return;
            }

            // 스폰 포인트
            Gizmos.color = Color.cyan;
            foreach (MonsterSpawn monsterSpawn in _monsterSpawns)
            {
                if (monsterSpawn.spawnPoint != null)
                {
                    Gizmos.DrawWireSphere(monsterSpawn.spawnPoint.position, 0.5f);
                    Gizmos.DrawLine(
                        monsterSpawn.spawnPoint.position,
                        monsterSpawn.spawnPoint.position + Vector3.up * 2f
                    );
                }
            }

            // 그룹 범위 표시 
            if (_enemyGroup != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, 1f);
            }
        }
    }
}
