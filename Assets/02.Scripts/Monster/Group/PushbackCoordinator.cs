using System;
using System.Collections.Generic;
using Monster.AI;
using UnityEngine;

namespace Monster.Group
{
    /// <summary>
    /// 몬스터가 후퇴할 때 뒤쪽 몬스터들을 cascading 방식으로 밀어내는 컴포넌트.
    /// 단일 책임 원칙(SRP)을 준수하여 cascading push-back만 담당합니다.
    /// </summary>
    public class PushbackCoordinator
    {
        private readonly List<MonsterController> _allMonsters;
        private readonly Dictionary<MonsterController, Vector3> _desiredPosition;
        private readonly Func<MonsterController, bool> _hasSlotPredicate;

        // 튜닝 파라미터
        private const float ConeSemiAngle = 30f;           // 영향 범위 각도 (±30도 원뿔)
        private const float MaxPushbackRange = 3.0f;       // 최대 영향 거리
        private const float DampeningFactor = 0.7f;        // 거리별 감쇠율 

        private bool _isProcessing = false;  // 재귀 방지

        /// <summary>
        /// PushbackCoordinator 생성자.
        /// </summary>
        /// <param name="allMonsters">그룹 내 모든 몬스터 리스트</param>
        /// <param name="desiredPosition">몬스터별 목표 위치 Dictionary (EnemyGroup 공유)</param>
        /// <param name="hasSlotPredicate">공격 슬롯 보유 여부 체크 함수</param>
        public PushbackCoordinator(
            List<MonsterController> allMonsters,
            Dictionary<MonsterController, Vector3> desiredPosition,
            Func<MonsterController, bool> hasSlotPredicate)
        {
            _allMonsters = allMonsters;
            _desiredPosition = desiredPosition;
            _hasSlotPredicate = hasSlotPredicate;
        }

        /// <summary>
        /// Cascading push-back을 처리합니다.
        /// </summary>
        /// <param name="pusher">후퇴하는 몬스터</param>
        /// <param name="retreatDirection">후퇴 방향 (정규화 필요 없음, 내부에서 처리)</param>
        /// <param name="retreatDistance">후퇴 거리</param>
        public void ProcessPushback(MonsterController pusher, Vector3 retreatDirection, float retreatDistance)
        {
            if (_isProcessing)
            {
                return;  // 무한 연쇄 방지
            }

            if (pusher == null || retreatDistance < 0.01f)
            {
                return;
            }

            _isProcessing = true;
            try
            {
                // 1. 영향받을 몬스터 탐색
                var affectedMonsters = FindAffectedMonsters(pusher, retreatDirection);

                // 2. 각 몬스터에 밀림 적용
                for (int i = 0; i < affectedMonsters.Count; i++)
                {
                    var monster = affectedMonsters[i];
                    if (monster == null)
                    {
                        continue;
                    }

                    float distance = Vector3.Distance(pusher.transform.position, monster.transform.position);
                    float pushAmount = CalculatePushAmount(distance, retreatDistance, monster);

                    // 유의미한 밀림만 적용
                    if (pushAmount > 0.01f)
                    {
                        Vector3 pushOffset = retreatDirection.normalized * pushAmount;

                        // _desiredPosition 즉시 업데이트
                        if (_desiredPosition.ContainsKey(monster))
                        {
                            _desiredPosition[monster] += pushOffset;
                        }
                    }
                }
            }
            finally
            {
                _isProcessing = false;
            }
        }

        /// <summary>
        /// 원뿔형 범위 내에서 영향받을 몬스터들을 찾습니다.
        /// </summary>
        private List<MonsterController> FindAffectedMonsters(MonsterController pusher, Vector3 retreatDirection)
        {
            var affected = new List<MonsterController>();

            Vector3 pusherPos = pusher.transform.position;
            Vector3 retreatDir = retreatDirection;
            retreatDir.y = 0f;
            retreatDir.Normalize();

            float maxRangeSqr = MaxPushbackRange * MaxPushbackRange;  // 최적화: 제곱 비교

            for (int i = 0; i < _allMonsters.Count; i++)
            {
                var monster = _allMonsters[i];

                // 자기 자신 제외
                if (monster == null || monster == pusher || !monster.IsAlive)
                {
                    continue;
                }

                Vector3 toMonster = monster.transform.position - pusherPos;
                toMonster.y = 0f;

                // 거리 체크 (제곱 비교로 최적화)
                float distanceSqr = toMonster.sqrMagnitude;
                if (distanceSqr < 0.0001f || distanceSqr > maxRangeSqr)
                {
                    continue;  // 범위 밖
                }

                // 각도 체크 (원뿔형 범위)
                float angle = Vector3.Angle(retreatDir, toMonster.normalized);
                if (angle > ConeSemiAngle)
                {
                    continue;  // 원뿔 밖
                }

                affected.Add(monster);
            }

            return affected;
        }

        /// <summary>
        /// 거리와 공격 상태에 따라 밀림 강도를 계산합니다.
        /// </summary>
        /// <param name="distance">pusher로부터의 거리</param>
        /// <param name="retreatDistance">pusher의 후퇴 거리</param>
        /// <param name="monster">밀리는 몬스터</param>
        /// <returns>밀림 강도 (0 이상)</returns>
        private float CalculatePushAmount(float distance, float retreatDistance, MonsterController monster)
        {
            // 공격 슬롯 보유자는 밀리지 않음 (전열 안정성 유지)
            if (_hasSlotPredicate != null && _hasSlotPredicate(monster))
            {
                return 0f;
            }

            // 거리 기반 감쇠 계산
            float distanceRatio = Mathf.Clamp01(distance / MaxPushbackRange);
            float pushAmount = retreatDistance * (1f - distanceRatio) * DampeningFactor;

            return Mathf.Max(0f, pushAmount);
        }
    }
}
