using System;
using System.Collections.Generic;
using Boss.AI;
using Combat.Core;
using Common;
using Monster.AI;
using Monster.Group;
using UnityEngine;
using UnityEngine.AI;

namespace Boss.Core
{
    /// <summary>
    /// 보스가 소환한 잡졸을 관리하는 시스템
    /// 기존 MonsterGroupDirector와 연동하여 잡졸 AI 활용
    /// </summary>
    public class BossMinionManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int _maxAttackSlots = 2;
        [SerializeField] private float _aggroRange = 15f;

        private BossController _owner;
        private MonsterGroupDirector _minionGroup;
        private List<MonsterController> _activeMinions = new();
        private Transform _playerTransform;

        public int AliveMinionCount => GetAliveMinionCount();
        public bool HasActiveMinions => AliveMinionCount > 0;

        public event Action OnAllMinionsDead;

        public void Initialize(BossController owner)
        {
            _owner = owner;

            if (PlayerReferenceProvider.Instance != null)
            {
                _playerTransform = PlayerReferenceProvider.Instance.PlayerTransform;
            }
        }

        /// <summary>
        /// 잡졸 소환
        /// </summary>
        public void SpawnMinions(GameObject[] prefabs, int count, float radius)
        {
            if (prefabs == null || prefabs.Length == 0)
            {
                Debug.LogWarning("BossMinionManager: 소환할 프리팹이 없습니다.");
                return;
            }

            // MonsterGroupDirector 생성 (없으면)
            EnsureMinionGroup();

            Vector3 bossPosition = _owner.transform.position;

            // 원형 배치로 소환 위치 계산
            float angleStep = 360f / count;

            for (int i = 0; i < count; i++)
            {
                // 랜덤 프리팹 선택
                GameObject prefab = prefabs[UnityEngine.Random.Range(0, prefabs.Length)];
                if (prefab == null) continue;

                // 소환 위치 계산
                float angle = i * angleStep + UnityEngine.Random.Range(-15f, 15f);
                Vector3 offset = Quaternion.Euler(0, angle, 0) * Vector3.forward * radius;
                Vector3 spawnPos = bossPosition + offset;

                // NavMesh 위치 보정
                if (NavMesh.SamplePosition(spawnPos, out NavMeshHit hit, 3f, NavMesh.AllAreas))
                {
                    spawnPos = hit.position;
                }
                else
                {
                    Debug.LogWarning($"BossMinionManager: NavMesh 위치를 찾을 수 없습니다. ({spawnPos})");
                    continue;
                }

                // 잡졸 인스턴스화
                SpawnSingleMinion(prefab, spawnPos);
            }
        }

        private void SpawnSingleMinion(GameObject prefab, Vector3 position)
        {
            // 보스를 바라보는 방향으로 회전
            Quaternion rotation = Quaternion.LookRotation(_owner.transform.position - position);
            rotation = Quaternion.Euler(0, rotation.eulerAngles.y, 0);

            GameObject minionObj = Instantiate(prefab, position, rotation);
            var minionController = minionObj.GetComponent<MonsterController>();

            if (minionController == null)
            {
                Debug.LogError("BossMinionManager: 소환된 오브젝트에 MonsterController가 없습니다.");
                Destroy(minionObj);
                return;
            }

            // 그룹에 등록
            RegisterMinion(minionController);

            // 사망 이벤트 구독
            var combatant = minionObj.GetComponent<Combatant>();
            if (combatant != null)
            {
                combatant.OnDeath += () => HandleMinionDeath(minionController);
            }
        }

        private void RegisterMinion(MonsterController minion)
        {
            _activeMinions.Add(minion);

            // MonsterGroupDirector에 등록
            _minionGroup?.RegisterMonster(minion);

            // GroupCommandProvider 설정 (MonsterController의 프로퍼티로 접근)
            minion.GroupCommandProvider?.SetEnemyGroup(_minionGroup);

            // 즉시 전투 상태로 전환 (순찰 스킵)
            minion.ForceEnterCombat();
        }

        private void HandleMinionDeath(MonsterController minion)
        {
            _activeMinions.Remove(minion);
            _minionGroup?.UnregisterMonster(minion);

            // 모든 잡졸 사망 체크
            if (_activeMinions.Count == 0 || GetAliveMinionCount() == 0)
            {
                OnAllMinionsDead?.Invoke();
            }
        }

        private void EnsureMinionGroup()
        {
            if (_minionGroup != null) return;

            // 새 MonsterGroupDirector 생성
            GameObject groupObj = new GameObject("Boss_MinionGroup");
            groupObj.transform.SetParent(_owner.transform);
            groupObj.transform.localPosition = Vector3.zero;

            _minionGroup = groupObj.AddComponent<MonsterGroupDirector>();

            // 초기화는 MonsterGroupDirector.Awake()에서 자동 수행
        }

        /// <summary>
        /// 생존 잡졸 수 반환
        /// </summary>
        public int GetAliveMinionCount()
        {
            int count = 0;
            for (int i = _activeMinions.Count - 1; i >= 0; i--)
            {
                if (_activeMinions[i] == null)
                {
                    _activeMinions.RemoveAt(i);
                    continue;
                }

                if (_activeMinions[i].IsAlive)
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// 모든 잡졸이 사망했는지 확인
        /// </summary>
        public bool AreAllMinionsDead()
        {
            return GetAliveMinionCount() == 0;
        }

        /// <summary>
        /// 모든 잡졸 제거 (보스 사망 시)
        /// </summary>
        public void DespawnAllMinions()
        {
            for (int i = _activeMinions.Count - 1; i >= 0; i--)
            {
                var minion = _activeMinions[i];
                if (minion != null && minion.gameObject != null)
                {
                    var combatant = minion.GetComponent<Combatant>();
                    if (combatant != null && combatant.IsAlive)
                    {
                        combatant.TakeDamage(99999f);
                    }
                    else
                    {
                        Destroy(minion.gameObject);
                    }
                }
            }

            _activeMinions.Clear();
        }

        /// <summary>
        /// 최대 소환 가능 여부 확인
        /// </summary>
        public bool CanSummonMore(int maxAlive)
        {
            return GetAliveMinionCount() < maxAlive;
        }

        private void OnDestroy()
        {
            DespawnAllMinions();

            if (_minionGroup != null)
            {
                Destroy(_minionGroup.gameObject);
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_owner == null) return;

            // 소환 범위 시각화
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(_owner.transform.position, _owner.Data?.MinionSpawnRadius ?? 5f);
        }
#endif
    }
}
