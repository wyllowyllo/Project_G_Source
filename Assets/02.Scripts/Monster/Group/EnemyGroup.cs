using System.Collections.Generic;
using UnityEngine;

namespace Monster
{
    /// <summary>
    /// 몬스터 그룹의 전투를 운영하는 핵심 클래스.
    /// 링 포지셔닝, Aggro 관리, 공격 슬롯 분배를 담당합니다.
    /// </summary>
    public class 
        EnemyGroup : MonoBehaviour
    {
        [Header("그룹 설정")]
        [SerializeField] private Vector3 _groupCenter;
        [SerializeField] private float _aggroRange = 12f;
        [SerializeField] private float _ringRadius = 3.5f;
        [SerializeField] private int _maxAttackSlots = 2;

        [Header("업데이트 설정")]
        [SerializeField] private float _updateInterval = 0.3f;

        [Header("디버그")]
        [SerializeField] private bool _showDebugGizmos = true;

        // 참조
        private Transform _playerTransform;
        private AttackSlotManager _attackSlotManager;

        // 그룹 상태
        private List<MonsterController> _monsters = new List<MonsterController>();
        private bool _isInCombat = false;
        private float _updateTimer = 0f;

        // 프로퍼티
        public Vector3 GroupCenter => _groupCenter;
        public bool IsInCombat => _isInCombat;
        public int MonsterCount => _monsters.Count;
        public Transform PlayerTransform => _playerTransform;

        private void Awake()
        {
            InitializeGroup();
        }

        private void Update()
        {
            if (_monsters.Count == 0)
            {
                return;
            }

            // Aggro 체크
            if (!_isInCombat)
            {
                CheckAggro();
            }

            // 주기적 업데이트 (0.3초)
            _updateTimer += Time.deltaTime;
            if (_updateTimer >= _updateInterval)
            {
                _updateTimer = 0f;

                if (_isInCombat)
                {
                    UpdateEngagePositions();
                }
            }
        }

        private void InitializeGroup()
        {
            // 그룹 센터가 설정되지 않았으면 현재 위치 사용
            if (_groupCenter == Vector3.zero)
            {
                _groupCenter = transform.position;
            }

            // AttackSlotManager 생성
            _attackSlotManager = new AttackSlotManager(_maxAttackSlots);
        }

        /// <summary>
        /// 몬스터를 그룹에 등록
        /// </summary>
        public void RegisterMonster(MonsterController monster)
        {
            if (monster == null)
            {
                Debug.LogWarning("EnemyGroup: 등록하려는 몬스터가 null입니다.");
                return;
            }

            if (!_monsters.Contains(monster))
            {
                _monsters.Add(monster);
                Debug.Log($"EnemyGroup: {monster.name} 등록됨. 현재 그룹 크기: {_monsters.Count}");
            }
        }

        /// <summary>
        /// 몬스터를 그룹에서 제거
        /// </summary>
        public void UnregisterMonster(MonsterController monster)
        {
            if (monster == null)
            {
                return;
            }

            if (_monsters.Remove(monster))
            {
                // 공격 슬롯 반환
                _attackSlotManager?.ReleaseSlot(monster);
                Debug.Log($"EnemyGroup: {monster.name} 제거됨. 현재 그룹 크기: {_monsters.Count}");
            }
        }

        /// <summary>
        /// 플레이어를 찾아서 설정
        /// </summary>
        public void FindPlayer()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerTransform = player.transform;
            }
            else
            {
                Debug.LogWarning("EnemyGroup: Player를 찾을 수 없습니다.");
            }
        }

        /// <summary>
        /// Aggro 범위 체크 - 플레이어가 범위 안에 들어오면 전투 시작
        /// </summary>
        private void CheckAggro()
        {
            if (_playerTransform == null)
            {
                FindPlayer();
                return;
            }

            float distanceToPlayer = Vector3.Distance(_groupCenter, _playerTransform.position);

            if (distanceToPlayer <= _aggroRange)
            {
                TransitionToCombat();
            }
        }

        /// <summary>
        /// 그룹 전체를 전투 상태로 전환
        /// </summary>
        private void TransitionToCombat()
        {
            if (_isInCombat)
            {
                return;
            }

            _isInCombat = true;
            Debug.Log($"EnemyGroup: 전투 시작! 그룹 크기: {_monsters.Count}");

            // 모든 몬스터를 Engage 상태로 전환
            foreach (MonsterController monster in _monsters)
            {
                if (monster != null && monster.IsAlive)
                {
                    monster.StateMachine.ChangeState(MonsterState.Engage);
                }
            }
        }

        /// <summary>
        /// 링 포지셔닝 업데이트 - 각 몬스터에게 원형 배치 위치 할당
        /// </summary>
        public void UpdateEngagePositions()
        {
            if (_playerTransform == null || _monsters.Count == 0)
            {
                return;
            }

            // 살아있는 몬스터만 필터링
            List<MonsterController> aliveMonsters = _monsters.FindAll(m => m != null && m.IsAlive);

            // 원형으로 균등하게 배치
            for (int i = 0; i < aliveMonsters.Count; i++)
            {
                Vector3 ringPosition = CalculateRingPosition(i, aliveMonsters.Count);
                aliveMonsters[i].NavAgent.SetDestination(ringPosition);
            }
        }

        /// <summary>
        /// 링 위의 특정 인덱스 위치 계산
        /// </summary>
        public Vector3 CalculateRingPosition(int index, int totalCount)
        {
            if (_playerTransform == null || totalCount == 0)
            {
                return _groupCenter;
            }

            // 각도 계산 (균등 분배)
            float angleStep = 360f / totalCount;
            float angle = angleStep * index;
            float radian = angle * Mathf.Deg2Rad;

            // 플레이어 중심으로 원형 위치 계산
            Vector3 offset = new Vector3(
                Mathf.Cos(radian) * _ringRadius,
                0f,
                Mathf.Sin(radian) * _ringRadius
            );

            return _playerTransform.position + offset;
        }

        /// <summary>
        /// 공격 슬롯 요청
        /// </summary>
        public bool RequestAttackSlot(MonsterController monster)
        {
            if (_attackSlotManager == null)
            {
                return false;
            }

            return _attackSlotManager.RequestSlot(monster);
        }

        /// <summary>
        /// 공격 슬롯 반환
        /// </summary>
        public void ReleaseAttackSlot(MonsterController monster)
        {
            _attackSlotManager?.ReleaseSlot(monster);
        }

        /// <summary>
        /// 공격 가능 여부 확인
        /// </summary>
        public bool CanAttack(MonsterController monster)
        {
            if (_attackSlotManager == null)
            {
                return false;
            }

            return _attackSlotManager.HasSlot(monster);
        }

        private void OnDrawGizmos()
        {
            if (!_showDebugGizmos)
            {
                return;
            }

            // 그룹 센터
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(_groupCenter, 0.5f);

            // Aggro 범위
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_groupCenter, _aggroRange);

            // 링 반경
            if (_playerTransform != null)
            {
                Gizmos.color = Color.green;
                DrawCircle(_playerTransform.position, _ringRadius, 32);
            }
        }

        private void DrawCircle(Vector3 center, float radius, int segments)
        {
            float angleStep = 360f / segments;
            Vector3 prevPoint = center + new Vector3(radius, 0, 0);

            for (int i = 1; i <= segments; i++)
            {
                float angle = angleStep * i * Mathf.Deg2Rad;
                Vector3 newPoint = center + new Vector3(
                    Mathf.Cos(angle) * radius,
                    0,
                    Mathf.Sin(angle) * radius
                );

                Gizmos.DrawLine(prevPoint, newPoint);
                prevPoint = newPoint;
            }
        }
    }
}
