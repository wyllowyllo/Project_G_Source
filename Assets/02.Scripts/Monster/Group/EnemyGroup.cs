using System.Collections.Generic;
using Monster.AI;
using UnityEngine;

namespace Monster.Group
{
    /// <summary>
    /// 몬스터 그룹의 전투를 운영하는 핵심 클래스.
    /// Aggro 관리, 공격 슬롯 분배를 담당합니다. 
    /// </summary>
    public class EnemyGroup : MonoBehaviour
    {
        [Header("그룹 설정")]
        [SerializeField] private Vector3 _groupCenter;
        [SerializeField] private float _aggroRange = 12f;
        [SerializeField] private int _maxAttackSlots = 2;

        [Header("디렉터 - 업데이트 주기")]
        [SerializeField] private float _directorTickInterval = 0.25f;   // 공격자 선정/자리 갱신 주기
        [SerializeField] private float _positionTickInterval = 0.5f;    // 각도 슬롯 재배치 주기 (너무 잦으면 춤춤)

        [Header("포지션 - 각도 슬롯")]
        [SerializeField] private float _frontArcDegrees = 220f;         // 플레이어 전방 중심 부채꼴로 배치(자연스러움 ↑)
        [SerializeField] private float _slotRadiusLerp = 0.6f;          // PreferredMin~Max 사이 목표 반경 선택 비율

        [Header("Separation")]
        [SerializeField] private float _separationRadius = 1.5f;
        [SerializeField] private float _separationWeight = 1.0f;

        [Header("공격자 선정(스코어링)")]
        [SerializeField] private float _minAttackReassignInterval = 0.6f; // 슬롯이 비었을 때 새 공격자 배정 최소 간격
        [SerializeField] private float _recentAttackerPenaltySeconds = 2.0f;
        [SerializeField] private float _attackRangeBuffer = 0.75f;        // AttackRange + buffer 내 후보 선호
        [SerializeField] private LayerMask _losBlockMask = ~0;            // 필요 시 장애물 마스크로 조절
        
        [Header("디버그")]
        [SerializeField] private bool _showDebugGizmos = true;

        // 참조
        private Transform _playerTransform;
        private AttackSlotManager _attackSlotManager;

        // 그룹 상태
        private List<MonsterController> _monsters = new List<MonsterController>();

        // 디렉터 상태 캐시
        private readonly Dictionary<MonsterController, float> _desiredAngleDeg = new();
        private readonly Dictionary<MonsterController, float> _desiredRadius = new();
        private readonly Dictionary<MonsterController, Vector3> _desiredPosition = new();
        
        // 공정성/리듬
        private readonly Dictionary<MonsterController, float> _lastAttackTime = new();
        private float _nextDirectorTickTime;
        private float _nextPositionTickTime;
        private float _nextAttackAssignTime;
        
        // 프로퍼티
        public Vector3 GroupCenter => _groupCenter;
        public int MonsterCount => _monsters.Count;
        public Transform PlayerTransform => _playerTransform;

        private void Awake()
        {
            InitializeGroup();
        }
        
        private void Update()
        {
            if (_playerTransform == null)
            {
                return;
            }

            // 죽은 참조 정리
            _monsters.RemoveAll(m => m == null || !m.IsAlive);

            float now = Time.time;

            // 자리 갱신(각도/반경)
            if (now >= _nextPositionTickTime)
            {
                _nextPositionTickTime = now + _positionTickInterval;
                RebuildAngleSlots();
            }

            // 디렉터 틱(DesiredPosition + 공격자 선정)
            if (now >= _nextDirectorTickTime)
            {
                _nextDirectorTickTime = now + _directorTickInterval;
                UpdateDesiredPositions();
                AssignAttackersByScore();
            }
        }
        private void InitializeGroup()
        {
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
            if (monster == null) return;
            

            if (!_monsters.Contains(monster))
            {
                _monsters.Add(monster);
               
                // 초기 자리값 세팅
                if (!_desiredAngleDeg.ContainsKey(monster))
                    _desiredAngleDeg[monster] = 0f;

                if (!_desiredRadius.ContainsKey(monster))
                    _desiredRadius[monster] = 0f;

                if (!_desiredPosition.ContainsKey(monster))
                    _desiredPosition[monster] = monster.transform.position;

                if (!_lastAttackTime.ContainsKey(monster))
                    _lastAttackTime[monster] = -9999f;

                // 등록 즉시 슬롯 재배치 한번
                RebuildAngleSlots();
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
                // 공격 슬롯 반환
                _attackSlotManager?.ReleaseSlot(monster);
               
                _desiredAngleDeg.Remove(monster);
                _desiredRadius.Remove(monster);
                _desiredPosition.Remove(monster);
                _lastAttackTime.Remove(monster);
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
        /// 공격 슬롯 요청
        /// </summary>
        public bool RequestAttackSlot(MonsterController monster)
        {
            if (_attackSlotManager == null)  return false;
            

            bool ok = _attackSlotManager.RequestSlot(monster);
            if (ok)
            {
                _lastAttackTime[monster] = Time.time; // 공격자 선정 시각 기록
            }
            return ok;
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
            if (_attackSlotManager == null) return false;
            return _attackSlotManager.HasSlot(monster);
        }

        // ---------------------------
        // DesiredPosition 제공
        // ---------------------------
        public Vector3 GetDesiredPosition(MonsterController monster)
        {
            if (monster == null) return transform.position;

            if (_desiredPosition.TryGetValue(monster, out var pos))
                return pos;

            return monster.transform.position;
        }
        
         private void RebuildAngleSlots()
        {
            if (_playerTransform == null) return;

            int n = _monsters.Count;
            if (n <= 0) return;

            // 플레이어 전방을 중심으로 부채꼴 배치
            Vector3 fwd = _playerTransform.forward;
            fwd.y = 0f;
            if (fwd.sqrMagnitude < 0.0001f) fwd = Vector3.forward;
            fwd.Normalize();

            float halfArc = _frontArcDegrees * 0.5f;
            float step = (n <= 1) ? 0f : (_frontArcDegrees / (n - 1));

            for (int i = 0; i < n; i++)
            {
                var m = _monsters[i];
                if (m == null) continue;

                float angleOffset = -halfArc + step * i; // [-halfArc, +halfArc]
                float desiredAngle = Mathf.Atan2(fwd.x, fwd.z) * Mathf.Rad2Deg + angleOffset;

                _desiredAngleDeg[m] = desiredAngle;

                // 반경은 몬스터 데이터 밴드에서 선택
                float rMin = Mathf.Max(0.5f, m.Data.PreferredMinDistance);
                float rMax = Mathf.Max(rMin + 0.5f, m.Data.PreferredMaxDistance);
                float r = Mathf.Lerp(rMin, rMax, _slotRadiusLerp);

                _desiredRadius[m] = r;
            }
        }

        // ---------------------------
        // 2) DesiredPosition 업데이트 + 4) Separation 적용
        // ---------------------------
        private void UpdateDesiredPositions()
        {
            if (_playerTransform == null) return;

            Vector3 playerPos = _playerTransform.position;

            for (int i = 0; i < _monsters.Count; i++)
            {
                var m = _monsters[i];
                if (m == null) continue;

                float ang = _desiredAngleDeg.TryGetValue(m, out var a) ? a : 0f;
                float r = _desiredRadius.TryGetValue(m, out var rr) ? rr : m.Data.PreferredMaxDistance;

                Vector3 dir = Quaternion.AngleAxis(ang, Vector3.up) * Vector3.forward;
                Vector3 basePos = playerPos + dir * r;

                // Separation 벡터를 살짝 추가
                Vector3 sep = ComputeSeparation(m);
                Vector3 finalPos = basePos + sep * _separationWeight;

                // NavMeshAgent가 갈 수 없는 위치가 될 수 있으니, 일단 y는 현재로 유지
                finalPos.y = m.transform.position.y;

                _desiredPosition[m] = finalPos;
            }
        }

        private Vector3 ComputeSeparation(MonsterController self)
        {
            if (self == null) return Vector3.zero;

            Vector3 pos = self.transform.position;
            Vector3 force = Vector3.zero;

            float radiusSqr = _separationRadius * _separationRadius;

            for (int i = 0; i < _monsters.Count; i++)
            {
                var other = _monsters[i];
                if (other == null || other == self) continue;

                Vector3 d = pos - other.transform.position;
                d.y = 0f;

                float ds = d.sqrMagnitude;
                if (ds < 0.0001f || ds > radiusSqr) continue;

                // 가까울수록 강하게
                float t = 1f - (Mathf.Sqrt(ds) / _separationRadius);
                force += d.normalized * t;
            }

            return force;
        }

        // ---------------------------
        // 3) 공격자 선정: 스코어링 기반 배정
        // ---------------------------
        private void AssignAttackersByScore()
        {
            if (_attackSlotManager == null) return;

            float now = Time.time;
            if (now < _nextAttackAssignTime) return;

            int available = _attackSlotManager.AvailableSlots;
            if (available <= 0) return;

            // 후보 평가
            List<(MonsterController m, float score)> candidates = new();

            for (int i = 0; i < _monsters.Count; i++)
            {
                var m = _monsters[i];
                if (m == null || !m.IsAlive) continue;

                // 이미 슬롯 보유 중이면 후보로 볼 필요 없음
                if (CanAttack(m)) continue;

                float s = ComputeAttackScore(m, now);
                if (s > 0f)
                {
                    candidates.Add((m, s));
                }
            }

            if (candidates.Count == 0) return;

            // 점수 내림차순
            candidates.Sort((a, b) => b.score.CompareTo(a.score));

            int assignCount = Mathf.Min(available, candidates.Count);
            for (int i = 0; i < assignCount; i++)
            {
                RequestAttackSlot(candidates[i].m);
            }

            _nextAttackAssignTime = now + _minAttackReassignInterval;
        }

        private float ComputeAttackScore(MonsterController m, float now)
        {
            if (m == null || _playerTransform == null) return 0f;

            Vector3 mp = m.transform.position;
            Vector3 pp = _playerTransform.position;

            float dist = Vector3.Distance(mp, pp);

            // 너무 멀면 후보 탈락 (접근 중인 애가 너무 이르게 슬롯 잡는 것 방지)
            float hardMax = m.Data.AttackRange + _attackRangeBuffer + 2.0f;
            if (dist > hardMax) return 0f;

            // 라인오브사이트(간단 버전): 몬스터 가슴 -> 플레이어 가슴
            if (!HasLineOfSight(mp, pp)) return 0f;

            float score = 0f;

            // 거리: AttackRange 근처를 선호(가우시안 비슷한 모양)
            float ideal = m.Data.AttackRange + 0.25f;
            float distScore = Mathf.Clamp01(1f - Mathf.Abs(dist - ideal) / (m.Data.AttackRange + 1f));
            score += distScore * 3.0f;

            // 자리 도달: 내 DesiredPosition에 가까울수록 가점(“측면에서 각 잡고 들어오는” 느낌)
            Vector3 dp = GetDesiredPosition(m);
            float dpDist = Vector3.Distance(mp, dp);
            float posScore = Mathf.Clamp01(1f - (dpDist / 2.0f));
            score += posScore * 2.0f;

            // 최근 공격자 페널티
            float last = _lastAttackTime.TryGetValue(m, out var t) ? t : -9999f;
            float since = now - last;
            if (since < _recentAttackerPenaltySeconds)
            {
                score *= 0.35f;
            }
            else
            {
                // 오래 공격 못 했으면 약간 가점
                score += Mathf.Clamp01(since / 5f) * 0.5f;
            }

            // 혼잡도 페널티(주변이 너무 빽빽하면 공격자 우선순위 감소)
            float crowd = ComputeCrowdPenalty(m);
            score *= crowd;

            return score;
        }

        private float ComputeCrowdPenalty(MonsterController m)
        {
            // 주변에 가까운 동료가 많으면 공격자 선정이 덜 되도록
            int near = 0;
            Vector3 pos = m.transform.position;

            float r = 1.2f;
            float rs = r * r;

            for (int i = 0; i < _monsters.Count; i++)
            {
                var o = _monsters[i];
                if (o == null || o == m) continue;

                Vector3 d = pos - o.transform.position;
                d.y = 0f;
                if (d.sqrMagnitude <= rs) near++;
            }

            // 0명: 1.0, 1명:0.9, 2명:0.8, 3+:0.7
            if (near <= 0) return 1f;
            if (near == 1) return 0.9f;
            if (near == 2) return 0.8f;
            return 0.7f;
        }

        private bool HasLineOfSight(Vector3 from, Vector3 to)
        {
            Vector3 a = from + Vector3.up * 1.2f;
            Vector3 b = to + Vector3.up * 1.2f;

            Vector3 dir = (b - a);
            float len = dir.magnitude;
            if (len < 0.01f) return true;

            dir /= len;

            // 플레이어/몬스터 레이어 구성이 정해지지 않았으니, 일단 Raycast 성공 시 막힌 것으로 처리.
            // 필요하면 _losBlockMask를 “벽/지형 레이어”로 좁히세요.
            if (Physics.Raycast(a, dir, out RaycastHit hit, len, _losBlockMask, QueryTriggerInteraction.Ignore))
            {
                // hit이 플레이어면 시야 확보라고 보고 싶으면 여기서 태그 체크로 예외 처리 가능
                if (hit.collider != null && hit.collider.CompareTag("Player")) return true;
                return false;
            }

            return true;
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
        }
    }
}
