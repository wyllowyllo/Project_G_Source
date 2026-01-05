using System.Collections.Generic;
using UnityEngine;

namespace Monster
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
            public Vector3 groupCenter;
            public List<Transform> spawnPoints;
            public GameObject monsterPrefab;

            [Header("그룹 파라미터 (BDO 스타일)")]
            public float aggroRange = 12f;
            [Tooltip("동시 공격 가능한 몬스터 수 (BDO 권장: 2~3명)")]
            public int maxAttackSlots = 2;
        }

        [Header("스폰 설정")]
        [SerializeField] private List<SpawnGroup> _spawnGroups;
        [SerializeField] private float _spawnDelay = 1f;
        [SerializeField] private bool _spawnOnStart = true;

        [Header("디버그")]
        [SerializeField] private bool _showDebugGizmos = true;

        private List<EnemyGroup> _enemyGroups = new List<EnemyGroup>();
        private List<MonsterController> _spawnedMonsters = new List<MonsterController>();
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
                if (spawnGroup.spawnPoints == null || spawnGroup.spawnPoints.Count == 0)
                {
                    Debug.LogWarning("WaveManager: 스폰 포인트가 설정되지 않았습니다.");
                    continue;
                }

                if (spawnGroup.monsterPrefab == null)
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
            // EnemyGroup 생성
            GameObject groupObj = new GameObject($"EnemyGroup_{_enemyGroups.Count + 1}");
            groupObj.transform.SetParent(transform);
            groupObj.transform.position = spawnGroup.groupCenter;

            EnemyGroup enemyGroup = groupObj.AddComponent<EnemyGroup>();

            // EnemyGroup 설정은 리플렉션을 통해 설정
            SetEnemyGroupParameters(enemyGroup, spawnGroup);

            // 플레이어 찾기
            enemyGroup.FindPlayer();

            // 몬스터 스폰 및 그룹 등록
            List<MonsterController> groupMonsters = new List<MonsterController>();

            foreach (Transform spawnPoint in spawnGroup.spawnPoints)
            {
                if (spawnPoint == null)
                {
                    continue;
                }

                GameObject monsterObj = Instantiate(
                    spawnGroup.monsterPrefab,
                    spawnPoint.position,
                    Quaternion.identity,
                    groupObj.transform
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
            Debug.Log($"EnemyGroup 생성: {groupObj.name}, 몬스터 수: {groupMonsters.Count}");
        }

        /// <summary>
        /// EnemyGroup 파라미터 설정 (리플렉션 사용)
        /// </summary>
        private void SetEnemyGroupParameters(EnemyGroup enemyGroup, SpawnGroup spawnGroup)
        {
            // SerializedField는 리플렉션으로 접근
            var type = typeof(EnemyGroup);

            var groupCenterField = type.GetField("_groupCenter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (groupCenterField != null)
            {
                groupCenterField.SetValue(enemyGroup, spawnGroup.groupCenter);
            }

            var aggroRangeField = type.GetField("_aggroRange", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (aggroRangeField != null)
            {
                aggroRangeField.SetValue(enemyGroup, spawnGroup.aggroRange);
            }

            var maxAttackSlotsField = type.GetField("_maxAttackSlots", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (maxAttackSlotsField != null)
            {
                maxAttackSlotsField.SetValue(enemyGroup, spawnGroup.maxAttackSlots);
            }
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
                if (spawnGroup.spawnPoints == null)
                {
                    continue;
                }

                // 그룹 센터
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(spawnGroup.groupCenter, 0.8f);

                // Aggro 범위
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(spawnGroup.groupCenter, spawnGroup.aggroRange);

                // 스폰 포인트
                Gizmos.color = Color.cyan;
                foreach (Transform spawnPoint in spawnGroup.spawnPoints)
                {
                    if (spawnPoint != null)
                    {
                        Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
                        Gizmos.DrawLine(
                            spawnPoint.position,
                            spawnPoint.position + Vector3.up * 2f
                        );

                        // 그룹 센터와 연결선
                        Gizmos.color = Color.gray;
                        Gizmos.DrawLine(spawnGroup.groupCenter, spawnPoint.position);
                        Gizmos.color = Color.cyan;
                    }
                }
            }
        }
    }
}
