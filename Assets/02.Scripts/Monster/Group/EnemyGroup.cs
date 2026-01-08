using System.Collections.Generic;
using Common;
using Monster.AI;
using UnityEngine;

namespace Monster.Group
{
    /// <summary>
    /// 몬스터 그룹의 전투를 운영하는 핵심 클래스 (Facade 패턴).
    /// 각 전문 컴포넌트를 조율하여 그룹 전술을 구현합니다.
    /// </summary>
    public class EnemyGroup : MonoBehaviour
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
        [SerializeField] private float _separationRadius = 1.5f;
        [SerializeField] private float _separationWeight = 1.0f;

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

            // 죽은 참조 정리
            _monsters.RemoveAll(m => m == null || !m.IsAlive);

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
            _separationCalculator = new SeparationForceCalculator(_separationRadius);

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
                _sectorSpreadJitterDeg);

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

        /// <summary>
        /// 몬스터를 그룹에 등록
        /// </summary>
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

        /// <summary>
        /// 몬스터를 그룹에서 제거
        /// </summary>
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

        /// <summary>
        /// 플레이어 참조를 설정합니다.
        /// </summary>
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

        /// <summary>
        /// 공격 슬롯 요청
        /// </summary>
        public bool RequestAttackSlot(MonsterController monster)
        {
            if (_slotManager == null) return false;
            return _slotManager.RequestSlot(monster);
        }

        /// <summary>
        /// 공격 슬롯 반환
        /// </summary>
        public void ReleaseAttackSlot(MonsterController monster)
        {
            _slotManager?.ReleaseSlot(monster);
        }

        /// <summary>
        /// 공격 가능 여부 확인
        /// </summary>
        public bool CanAttack(MonsterController monster)
        {
            return _slotManager?.HasSlot(monster) ?? false;
        }

        /// <summary>
        /// 슬롯 보유 여부 확인 (내부용)
        /// </summary>
        private bool HasSlot(MonsterController monster)
        {
            return _slotManager?.HasSlot(monster) ?? false;
        }

        // ===== 위치 API =====

        /// <summary>
        /// 원하는 위치 제공
        /// </summary>
        public Vector3 GetDesiredPosition(MonsterController monster)
        {
            return _positionDirector.GetDesiredPosition(monster);
        }

        /// <summary>
        /// Cascading push-back 요청 (Law of Demeter 준수)
        /// </summary>
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
