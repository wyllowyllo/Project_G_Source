using System.Collections.Generic;
using Common;
using Monster.AI;
using UnityEngine;

namespace Monster.Group
{
    // 몬스터 그룹의 전투를 운영하는 핵심 클래스 (Facade 패턴)
    public class MonsterGroupDirector : MonoBehaviour
    {
        [Header("그룹 설정")]
        [SerializeField] private Vector3 _groupCenter;
        [SerializeField] private float _aggroRange = 12f;
        [SerializeField] private int _maxAttackSlots = 2;

        [Header("디렉터 - 업데이트 주기")]
        [SerializeField] private float _directorTickInterval = 5f;

        [Header("각도 가중치(측면 선호)")]
        [SerializeField] private float _sideAngleCenter = 90f;
        [SerializeField] private float _sideAngleSigma = 35f;
        [SerializeField] private float _sideAngleWeight = 3.0f;

        [Header("Separation")]
        [Tooltip("몬스터 간 분리 시 NavAgent.radius 합에 추가되는 여유 거리")]
        [SerializeField] private float _separationBuffer = 0.5f;
        [SerializeField] private float _separationWeight = 1.0f;
        [Tooltip("섹터 점유도 계산 시 기준 반경 (이보다 큰 몬스터는 더 많은 공간 차지)")]
        [SerializeField] private float _baseRadius = 0.5f;

        [Header("공격자 선정(스코어링)")]
        [SerializeField] private float _minAttackReassignInterval = 0.6f;
        [SerializeField] private float _recentAttackerPenaltySeconds = 2.0f;
        [SerializeField] private float _attackRangeBuffer = 0.75f;
        [SerializeField] private LayerMask _losBlockMask = ~0;

        [Header("Sector occupancy (빈 섹터 탐색)")]
        [SerializeField] private int _sectorCount = 12;
        [SerializeField] private float _sectorScanExtraDist = 2.0f;
        [SerializeField] private int _relocatePerTick = 3;
        [SerializeField] private float _sectorSpreadJitterDeg = 10f;
        [SerializeField] private float _avoidFrontBias = 0.3f;

        [Header("디버그")]
        [SerializeField] private bool _showDebugGizmos = true;

        // 그룹 상태
        private List<MonsterController> _monsters = new List<MonsterController>();
        private readonly Dictionary<MonsterController, float> _desiredAngleDeg = new();
        private readonly Dictionary<MonsterController, Vector3> _desiredPosition = new();
        private readonly Dictionary<MonsterController, float> _lastAttackTime = new();

        // 컴포넌트들 (Composition)
        private AttackSlotManager _slotManager;
        private LineOfSightChecker _losChecker;
        private SeparationForceCalculator _separationCalculator;
        private AttackScoreCalculator _scoreCalculator;
        private SectorOccupancyCalculator _sectorCalculator;
        private PositionAssigner _positionAssigner;
        private PositionDirector _positionDirector;
        private AttackerSelector _attackerSelector;
        private PushbackCoordinator _pushbackCoordinator;

        private float _nextDirectorTickTime;

        // 프로퍼티
        public Vector3 GroupCenter => _groupCenter;
        public int MonsterCount => _monsters.Count;

        private void Awake()
        {
            InitializeGroup();
        }

        private void Update()
        {
            if (PlayerReferenceProvider.Instance == null || PlayerReferenceProvider.Instance.PlayerTransform == null)
            {
                return;
            }

            float now = Time.time;

            // 디렉터 틱
            if (now >= _nextDirectorTickTime)
            {
                _nextDirectorTickTime = now + _directorTickInterval;

                // 위치 업데이트
                _positionDirector.UpdatePositions(_monsters, HasSlot);

                // 공격자 선정
                _attackerSelector.AssignAttackersByScore(_monsters, RequestAttackSlot, now);
            }
        }

        private void InitializeGroup()
        {
            if (_groupCenter == Vector3.zero)
            {
                _groupCenter = transform.position;
            }

            // 컴포넌트 생성
            _slotManager = new AttackSlotManager(_maxAttackSlots);
            _losChecker = new LineOfSightChecker(_losBlockMask);
            _separationCalculator = new SeparationForceCalculator(_separationBuffer);

            _scoreCalculator = new AttackScoreCalculator(
                _losChecker,
                _lastAttackTime,
                _monsters,
                _attackRangeBuffer,
                _sideAngleCenter,
                _sideAngleSigma,
                _sideAngleWeight,
                _recentAttackerPenaltySeconds);

            _sectorCalculator = new SectorOccupancyCalculator(
                _sectorCount,
                _sectorScanExtraDist,
                _sectorSpreadJitterDeg,
                _baseRadius);

            _positionAssigner = new PositionAssigner(
                _desiredAngleDeg,
                _desiredPosition,
                _separationCalculator,
                _separationWeight);

            _positionDirector = new PositionDirector(
                _sectorCalculator,
                _positionAssigner,
                _desiredAngleDeg,
                _relocatePerTick,
                _avoidFrontBias);

            _attackerSelector = new AttackerSelector(
                _slotManager,
                _scoreCalculator,
                _lastAttackTime,
                _minAttackReassignInterval);

            _pushbackCoordinator = new PushbackCoordinator(
                _monsters,
                _desiredPosition,
                HasSlot);
        }

        // ===== 몬스터 등록/해제 =====

        public void RegisterMonster(MonsterController monster)
        {
            if (monster == null) return;

            if (!_monsters.Contains(monster))
            {
                _monsters.Add(monster);

                // 초기값 설정
                if (!_desiredAngleDeg.ContainsKey(monster))
                    _desiredAngleDeg[monster] = 0f;

                if (!_desiredPosition.ContainsKey(monster))
                    _desiredPosition[monster] = monster.transform.position;

                if (!_lastAttackTime.ContainsKey(monster))
                    _lastAttackTime[monster] = -9999f;
            }
        }

        public void UnregisterMonster(MonsterController monster)
        {
            if (monster == null) return;

            if (_monsters.Remove(monster))
            {
                _slotManager?.ReleaseSlot(monster);
                _desiredAngleDeg.Remove(monster);
                _desiredPosition.Remove(monster);
                _lastAttackTime.Remove(monster);
            }
        }

        // ===== 플레이어 참조 =====

        public void FindPlayer()
        {
            if (PlayerReferenceProvider.Instance != null)
            {
                _scoreCalculator.SetPlayerTransform(PlayerReferenceProvider.Instance.PlayerTransform);
                Debug.Log("EnemyGroup: PlayerReferenceProvider로부터 플레이어 참조를 설정했습니다.");
            }
            else
            {
                Debug.LogWarning("EnemyGroup: PlayerReferenceProvider를 찾을 수 없습니다.");
            }
        }

        // ===== 공격 슬롯 API =====

        public bool RequestAttackSlot(MonsterController monster)
        {
            if (_slotManager == null) return false;
            return _slotManager.RequestSlot(monster);
        }

        public void ReleaseAttackSlot(MonsterController monster)
        {
            _slotManager?.ReleaseSlot(monster);
        }

        public bool CanAttack(MonsterController monster)
        {
            return _slotManager?.HasSlot(monster) ?? false;
        }

        private bool HasSlot(MonsterController monster)
        {
            return _slotManager?.HasSlot(monster) ?? false;
        }

        // ===== 위치 API =====

        public Vector3 GetDesiredPosition(MonsterController monster)
        {
            return _positionDirector.GetDesiredPosition(monster);
        }

        public void RequestPushback(MonsterController pusher, Vector3 retreatDirection, float distance)
        {
            _pushbackCoordinator?.ProcessPushback(pusher, retreatDirection, distance);
        }

        // ===== 디버그 =====

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
        }
    }
}
