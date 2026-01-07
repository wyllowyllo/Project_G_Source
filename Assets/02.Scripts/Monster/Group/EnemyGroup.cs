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
        [SerializeField] private float _directorTickInterval = 5f;   // 공격자 선정/자리 갱신 주기
        
        [Header("각도 가중치(측면 선호)")]
        [SerializeField] private float _sideAngleCenter = 90f;   // 측면 기준 각도(90도)
        [SerializeField] private float _sideAngleSigma = 35f;    // 허용 폭(작을수록 엄격)
        [SerializeField] private float _sideAngleWeight = 3.0f;  // 영향력(크면 측면 우선)
        
        [Header("Separation")]
        [SerializeField] private float _separationRadius = 1.5f;
        [SerializeField] private float _separationWeight = 1.0f;

        [Header("공격자 선정(스코어링)")]
        [SerializeField] private float _minAttackReassignInterval = 0.6f; // 슬롯이 비었을 때 새 공격자 배정 최소 간격
        [SerializeField] private float _recentAttackerPenaltySeconds = 2.0f;
        [SerializeField] private float _attackRangeBuffer = 0.75f;        // AttackRange + buffer 내 후보 선호
        [SerializeField] private LayerMask _losBlockMask = ~0;            // 필요 시 장애물 마스크로 조절
        
        [Header("Front crowding (빈 공간 감지)")]
        [SerializeField] private float _frontConeAngle = 55f;      // 전방 콘(±각도)
        [SerializeField] private int _frontCrowdCount = 4;         // 이 이상이면 전방 포화로 판단
        [SerializeField] private float _frontCrowdExtraDist = 1.0f;// PreferredMaxDistance에 더할 거리
        [SerializeField] private int _frontKeepCount = 2;          // 포화 시에도 전방 유지할 전열 수(가까운 순)

// 포화 시 배치 후보 각도(도). 전방(0도)은 의도적으로 제외.
        [SerializeField] private float[] _crowdReliefAngles =
        {
            70f, -70f,
            110f, -110f,
            150f, -150f,
            180f
        };

        [SerializeField] private float _angleOccupancySigma = 20f; // "그 방향이 차있다" 판단 폭
        
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
            if (_playerTransform == null) return;
            
            // 죽은 참조 정리
            _monsters.RemoveAll(m => m == null || !m.IsAlive);

            float now = Time.time;
            
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
        
         

       
        private void UpdateDesiredPositions()
        {
            if (_playerTransform == null) return;

            Vector3 playerPos = _playerTransform.position;

            bool frontCrowded = IsFrontCrowded(playerPos);

            if (frontCrowded)
                UpdateDesiredAnglesWhenCrowded(playerPos);
            
            for (int i = 0; i < _monsters.Count; i++)
            {
                var monster = _monsters[i];
                if (monster == null) continue;

                Vector3 monsterPosition = monster.transform.position;
                Vector3 toMonster = monsterPosition - playerPos;
                toMonster.y = 0f;

                float dist = toMonster.magnitude;
                float minDistance = monster.Data.PreferredMinDistance;
                float maxDistance = monster.Data.PreferredMaxDistance;
                
                Vector3 target = monsterPosition;
                
                if (dist < minDistance)
                {
                    Vector3 dirOut = toMonster.normalized; 
                    target = playerPos + dirOut * minDistance;
                }
                else if (dist > maxDistance)
                {
                    Vector3 dirIn = (-toMonster).normalized; 
                    target = monsterPosition + dirIn * Mathf.Min(2.0f, dist - maxDistance); 
                }
                
                // 전방이 포화일 때만: 각도 기반 목표점으로 "빈 공간 파고들기"
                if (frontCrowded && _desiredAngleDeg.TryGetValue(monster, out float angDeg))
                {
                    // 반경은 밴드 안에서 유지하되 약간 랜덤/분산 가능
                    float radius = Mathf.Clamp(dist, minDistance, maxDistance);

                    Vector3 fwd = _playerTransform.forward;
                    fwd.y = 0f;
                    if (fwd.sqrMagnitude < 0.001f) fwd = Vector3.forward;
                    fwd.Normalize();

                    Vector3 dir = Quaternion.Euler(0f, angDeg, 0f) * fwd;
                    target = playerPos + dir * radius;
                }
                
                // 몬스터 살짝 퍼져서 배치되도록 조정
                Vector3 sep = ComputeSeparation(monster);
                target += sep * _separationWeight;

                target.y = monsterPosition.y;
                _desiredPosition[monster] = target;
            }
        }

        private bool IsFrontCrowded(Vector3 playerPos)
        {
            if (_playerTransform == null) return false;

            Vector3 fwd = _playerTransform.forward;
            fwd.y = 0f;
            if (fwd.sqrMagnitude < 0.001f) fwd = Vector3.forward;
            fwd.Normalize();

            int count = 0;
            for (int i = 0; i < _monsters.Count; i++)
            {
                var m = _monsters[i];
                if (m == null || !m.IsAlive) continue;

                Vector3 toM = m.transform.position - playerPos;
                toM.y = 0f;

                float dist = toM.magnitude;
                float nearDist = m.Data.PreferredMaxDistance + _frontCrowdExtraDist;
                if (dist > nearDist) continue;

                if (toM.sqrMagnitude < 0.001f) continue;
                float ang = Vector3.Angle(fwd, toM.normalized); // 0=전방
                if (ang <= _frontConeAngle)
                {
                    count++;
                    if (count >= _frontCrowdCount) return true;
                }
            }
            return false;
        }

        private void UpdateDesiredAnglesWhenCrowded(Vector3 playerPos)
{
    if (_playerTransform == null) return;

    Vector3 fwd = _playerTransform.forward;
    fwd.y = 0f;
    if (fwd.sqrMagnitude < 0.001f) fwd = Vector3.forward;
    fwd.Normalize();

    // 1) 살아있는 몬스터 정리 + 거리순 정렬
    List<MonsterController> alive = new();
    for (int i = 0; i < _monsters.Count; i++)
    {
        var m = _monsters[i];
        if (m == null || !m.IsAlive) continue;
        alive.Add(m);
    }

    alive.Sort((a, b) =>
    {
        float da = (a.transform.position - playerPos).sqrMagnitude;
        float db = (b.transform.position - playerPos).sqrMagnitude;
        return da.CompareTo(db);
    });

    // 2) "전열 유지"는 현재 각도(혹은 0 근처)로 유지
    int keep = Mathf.Clamp(_frontKeepCount, 0, alive.Count);

    // 현재 점유(각도 히스토그램 비슷한) 정보: 몬스터들의 "현재 각도"를 기준으로 혼잡도를 측정
    List<float> occupiedAngles = new(alive.Count);
    for (int i = 0; i < alive.Count; i++)
    {
        Vector3 toM = alive[i].transform.position - playerPos;
        toM.y = 0f;
        if (toM.sqrMagnitude < 0.001f) continue;

        float signed = SignedAngleOnY(fwd, toM.normalized); // [-180,180]
        occupiedAngles.Add(signed);
    }

    // 3) 전열(가까운 애)은 현재 각도 유지(혹은 전방 쪽으로 당겨도 됨)
    for (int i = 0; i < keep; i++)
    {
        var m = alive[i];
        if (m == null) continue;

        Vector3 toM = m.transform.position - playerPos;
        toM.y = 0f;

        float ang = 0f;
        if (toM.sqrMagnitude > 0.001f)
            ang = SignedAngleOnY(fwd, toM.normalized);

        _desiredAngleDeg[m] = ang; // "전방에서 싸우려는 애"는 그대로 둠
    }

    // 4) 후열은 빈 방향 선택
    for (int i = keep; i < alive.Count; i++)
    {
        var m = alive[i];
        if (m == null) continue;

        // 후보 각도 중 "점유도가 최소"인 방향을 선택
        float bestAng = _crowdReliefAngles[0];
        float bestOcc = float.MaxValue;

        for (int k = 0; k < _crowdReliefAngles.Length; k++)
        {
            float cand = _crowdReliefAngles[k];
            float occ = AngleOccupancy(cand, occupiedAngles, _angleOccupancySigma);
            if (occ < bestOcc)
            {
                bestOcc = occ;
                bestAng = cand;
            }
        }

        _desiredAngleDeg[m] = bestAng;

        // 배정한 각도는 점유 목록에 추가해서 다음 애가 같은 곳만 가는 걸 완화
        occupiedAngles.Add(bestAng);
    }
}

private float AngleOccupancy(float candidateDeg, List<float> occupied, float sigma)
{
    // candidate 근처(±sigma)에 많을수록 값이 커짐
    float sum = 0f;
    float denom = 2f * sigma * sigma;

    for (int i = 0; i < occupied.Count; i++)
    {
        float d = Mathf.DeltaAngle(candidateDeg, occupied[i]);
        sum += Mathf.Exp(-(d * d) / Mathf.Max(0.0001f, denom));
    }

    return sum;
}

private float SignedAngleOnY(Vector3 from, Vector3 to)
{
    // from->to signed angle around Y axis
    float ang = Vector3.SignedAngle(from, to, Vector3.up);
    return ang; // [-180, 180]
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
                var monster = _monsters[i];
                if (monster == null || !monster.IsAlive) continue;

                // 이미 슬롯 보유 중이면 후보로 볼 필요 없음
                if (CanAttack(monster)) continue;

                float s = ComputeAttackScore(monster, now);
                if (s > 0f)
                {
                    candidates.Add((monster, s));
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

        private float ComputeAttackScore(MonsterController monster, float now)
        {
            if (monster == null || _playerTransform == null) return 0f;

            Vector3 monsterPosition = monster.transform.position;
            Vector3 targetPosition = _playerTransform.position;

            float dist = Vector3.Distance(monsterPosition, targetPosition);

            // 너무 멀면 탈락
            float hardMax = monster.Data.AttackRange + _attackRangeBuffer + 2.0f;
            if (dist > hardMax) return 0f;

            // LOS 체크
            if (!HasLineOfSight(monsterPosition, targetPosition)) return 0f;

            float score = 0f;

            // 거리 선호
            float ideal = monster.Data.AttackRange + 0.25f;
            float distScore = Mathf.Clamp01(1f - Mathf.Abs(dist - ideal) / (monster.Data.AttackRange + 1f));
            score += distScore * 3.0f;
            
            // 각도(측면) 점수: 플레이어 forward 대비 몬스터 방향이 90도 근처일수록 가점
            Vector3 pf = _playerTransform.forward; pf.y = 0f;
            Vector3 toM = (monsterPosition - targetPosition); toM.y = 0f;

            if (pf.sqrMagnitude > 0.001f && toM.sqrMagnitude > 0.001f)
            {
                pf.Normalize();
                toM.Normalize();

                float angle = Vector3.Angle(pf, toM);  // 0=정면, 90=측면, 180=후방
                float sideScore = Gaussian01(angle, _sideAngleCenter, _sideAngleSigma); // 0..1
                score += sideScore * _sideAngleWeight;
            }

            // 최근 공격자 페널티/공정성
            float last = _lastAttackTime.TryGetValue(monster, out var t) ? t : -9999f;
            float since = now - last;
            if (since < _recentAttackerPenaltySeconds) score *= 0.35f;
            else score += Mathf.Clamp01(since / 5f) * 0.5f;

            // 혼잡도 페널티
            score *= ComputeCrowdPenalty(monster);

            return score;
        }
        private float Gaussian01(float x, float mean, float sigma)
        {
            // exp(-((x-mean)^2)/(2*sigma^2)) in [0,1]
            float d = x - mean;
            float denom = 2f * sigma * sigma;
            return Mathf.Exp(-(d * d) / Mathf.Max(0.0001f, denom));
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
