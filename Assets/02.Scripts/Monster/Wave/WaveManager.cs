using System.Collections.Generic;
using UnityEngine;

namespace ProjectG.Monster
{
    /// <summary>
    /// 몬스터 웨이브 스폰을 관리하는 매니저.
    /// 프로토타입: 지정된 스폰 포인트에서 몬스터를 생성합니다.
    /// </summary>
    public class WaveManager : MonoBehaviour
    {
        [System.Serializable]
        public class SpawnPoint
        {
            public Transform spawnTransform;
            public GameObject monsterPrefab;
        }

        [Header("스폰 설정")]
        [SerializeField] private List<SpawnPoint> _spawnPoints;
        [SerializeField] private float _spawnDelay = 1f;
        [SerializeField] private bool _spawnOnStart = true;

        [Header("디버그")]
        [SerializeField] private bool _showDebugGizmos = true;

        private List<MonsterController> _spawnedMonsters;
        private float _spawnTimer;

        private void Start()
        {
            if (_spawnOnStart)
            {
                SpawnWave();
            }
        }

        /// <summary>
        /// 웨이브를 스폰합니다.
        /// </summary>
        public void SpawnWave()
        {
            if (_spawnPoints.Count == 0)
            {
                Debug.LogWarning("WaveManager: 스폰 포인트가 설정되지 않았습니다.");
                return;
            }

            foreach (SpawnPoint spawnPoint in _spawnPoints)
            {
                if (spawnPoint.spawnTransform == null || spawnPoint.monsterPrefab == null)
                {
                    Debug.LogWarning("WaveManager: 스폰 포인트 또는 프리팹이 null입니다.");
                    continue;
                }

                SpawnMonster(spawnPoint);
            }
        }

        private void SpawnMonster(SpawnPoint spawnPoint)
        {
            GameObject monsterObj = Instantiate(
                spawnPoint.monsterPrefab,
                spawnPoint.spawnTransform.position,
                spawnPoint.spawnTransform.rotation
            );

            MonsterController monster = monsterObj.GetComponent<MonsterController>();
            if (monster != null)
            {
                _spawnedMonsters.Add(monster);
                Debug.Log($"몬스터 스폰: {monsterObj.name}");
            }
            else
            {
                Debug.LogError($"스폰된 프리팹에 MonsterController가 없습니다: {monsterObj.name}");
            }
        }

        /// <summary>
        /// 모든 살아있는 몬스터를 반환합니다.
        /// </summary>
        public List<MonsterController> GetAliveMonsters()
        {
            _spawnedMonsters.RemoveAll(m => m == null || !m.IsAlive);
            return _spawnedMonsters;
        }

        /// <summary>
        /// 살아있는 몬스터 수를 반환합니다.
        /// </summary>
        public int GetAliveMonsterCount()
        {
            return GetAliveMonsters().Count;
        }

        private void OnDrawGizmos()
        {
            if (!_showDebugGizmos || _spawnPoints == null)
            {
                return;
            }

            Gizmos.color = Color.cyan;
            foreach (SpawnPoint spawnPoint in _spawnPoints)
            {
                if (spawnPoint.spawnTransform != null)
                {
                    Gizmos.DrawWireSphere(spawnPoint.spawnTransform.position, 0.5f);
                    Gizmos.DrawLine(
                        spawnPoint.spawnTransform.position,
                        spawnPoint.spawnTransform.position + Vector3.up * 2f
                    );
                }
            }
        }
    }
}
