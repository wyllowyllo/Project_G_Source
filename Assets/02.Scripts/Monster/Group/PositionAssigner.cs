using System;
using System.Collections.Generic;
using UnityEngine;

namespace Monster.Group
{
    /// <summary>
    /// 몬스터들의 원하는 위치를 할당하는 클래스.
    /// 거리 밴드, 각도, 분리력 등을 종합하여 최종 위치를 계산합니다.
    /// </summary>
    public class PositionAssigner
    {
        private readonly Dictionary<MonsterController, float> _desiredAngleDeg;
        private readonly Dictionary<MonsterController, Vector3> _desiredPosition;
        private readonly SeparationForceCalculator _separationCalculator;
        private readonly float _separationWeight;

        public PositionAssigner(
            Dictionary<MonsterController, float> desiredAngleDeg,
            Dictionary<MonsterController, Vector3> desiredPosition,
            SeparationForceCalculator separationCalculator,
            float separationWeight)
        {
            _desiredAngleDeg = desiredAngleDeg;
            _desiredPosition = desiredPosition;
            _separationCalculator = separationCalculator;
            _separationWeight = separationWeight;
        }

        /// <summary>
        /// 재배치 대상 몬스터를 선정합니다 (공격 슬롯이 없는 몬스터만).
        /// </summary>
        public List<MonsterController> SelectRelocationCandidates(
            List<MonsterController> allMonsters,
            Func<MonsterController, bool> hasSlotPredicate)
        {
            var candidates = new List<MonsterController>();

            for (int i = 0; i < allMonsters.Count; i++)
            {
                var monster = allMonsters[i];
                if (monster == null || !monster.IsAlive)
                {
                    continue;
                }

                if (!hasSlotPredicate(monster))
                {
                    candidates.Add(monster);
                }
            }

            return candidates;
        }

        /// <summary>
        /// 후보를 플레이어와의 거리 순으로 정렬합니다 (가까운 순).
        /// </summary>
        public void SortByDistanceToPlayer(List<MonsterController> candidates, Vector3 playerPos)
        {
            candidates.Sort((a, b) =>
            {
                float distA = (a.transform.position - playerPos).sqrMagnitude;
                float distB = (b.transform.position - playerPos).sqrMagnitude;
                return distA.CompareTo(distB);
            });
        }

        /// <summary>
        /// 몬스터의 3D 위치를 계산합니다 (거리 밴드, 각도, 분리력 적용).
        /// </summary>
        public void CalculatePositions(
            List<MonsterController> allMonsters,
            Vector3 playerPos,
            Vector3 playerForward)
        {
            for (int i = 0; i < allMonsters.Count; i++)
            {
                var monster = allMonsters[i];
                if (monster == null)
                {
                    continue;
                }

                Vector3 monsterPosition = monster.transform.position;
                Vector3 toMonster = monsterPosition - playerPos;
                toMonster.y = 0f;

                float currentDistance = toMonster.magnitude;
                float minDistance = monster.Data.PreferredMinDistance;
                float maxDistance = monster.Data.PreferredMaxDistance;

                Vector3 targetPosition = monsterPosition;

                // 거리 밴드 조정
                targetPosition = ApplyDistanceBand(
                    playerPos,
                    monsterPosition,
                    toMonster,
                    currentDistance,
                    minDistance,
                    maxDistance);

                // 각도 기반 위치 조정
                if (_desiredAngleDeg.TryGetValue(monster, out float angle))
                {
                    float radius = Mathf.Clamp(currentDistance, minDistance, maxDistance);
                    targetPosition = CalculatePositionFromAngle(playerPos, playerForward, angle, radius);
                }

                // 분리력 적용
                Vector3 separation = _separationCalculator.ComputeSeparation(monster, allMonsters);
                targetPosition += separation * _separationWeight;

                // Y 좌표 유지
                targetPosition.y = monsterPosition.y;

                _desiredPosition[monster] = targetPosition;
            }
        }

        /// <summary>
        /// 거리 밴드를 적용하여 위치를 조정합니다.
        /// </summary>
        private Vector3 ApplyDistanceBand(
            Vector3 playerPos,
            Vector3 monsterPosition,
            Vector3 toMonster,
            float currentDistance,
            float minDistance,
            float maxDistance)
        {
            if (currentDistance < minDistance)
            {
                // 너무 가까움 → 밀어냄
                Vector3 directionOut = toMonster.normalized;
                return playerPos + directionOut * minDistance;
            }

            if (currentDistance > maxDistance)
            {
                // 너무 멀음 → 당김
                Vector3 directionIn = -toMonster.normalized;
                float pullDistance = Mathf.Min(2.0f, currentDistance - maxDistance);
                return monsterPosition + directionIn * pullDistance;
            }

            return monsterPosition;
        }

        /// <summary>
        /// 각도와 반경으로부터 3D 위치를 계산합니다.
        /// </summary>
        private Vector3 CalculatePositionFromAngle(Vector3 playerPos, Vector3 playerForward, float angle, float radius)
        {
            Quaternion rotation = Quaternion.Euler(0f, angle, 0f);
            Vector3 direction = rotation * playerForward;
            return playerPos + direction * radius;
        }

        /// <summary>
        /// 원하는 위치를 가져옵니다.
        /// </summary>
        public Vector3 GetDesiredPosition(MonsterController monster)
        {
            if (monster == null)
            {
                return Vector3.zero;
            }

            if (_desiredPosition.TryGetValue(monster, out var position))
            {
                return position;
            }

            return monster.transform.position;
        }
    }
}
