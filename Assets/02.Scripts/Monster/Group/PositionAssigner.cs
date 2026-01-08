using System;
using System.Collections.Generic;
using Monster.AI;
using UnityEngine;

namespace Monster.Group
{
    // 몬스터들의 원하는 위치를 할당 (거리 밴드, 각도, 분리력 종합)
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

        public void SortByDistanceToPlayer(List<MonsterController> candidates, Vector3 playerPos)
        {
            candidates.Sort((a, b) =>
            {
                float distA = (a.transform.position - playerPos).sqrMagnitude;
                float distB = (b.transform.position - playerPos).sqrMagnitude;
                return distA.CompareTo(distB);
            });
        }

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

        private Vector3 CalculatePositionFromAngle(Vector3 playerPos, Vector3 playerForward, float angle, float radius)
        {
            Quaternion rotation = Quaternion.Euler(0f, angle, 0f);
            Vector3 direction = rotation * playerForward;
            return playerPos + direction * radius;
        }

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
