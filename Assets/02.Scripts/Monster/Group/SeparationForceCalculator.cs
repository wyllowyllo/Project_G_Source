using System.Collections.Generic;
using Monster.AI;
using UnityEngine;

namespace Monster.Group
{
    // 몬스터 간 분리력 계산 (겹치지 않도록 밀어내는 힘)
    public class SeparationForceCalculator
    {
        private readonly float _separationBuffer;

        public SeparationForceCalculator(float separationBuffer)
        {
            _separationBuffer = separationBuffer;
        }

        public Vector3 ComputeSeparation(MonsterController self, List<MonsterController> allMonsters)
        {
            if (self == null || self.NavAgent == null)
            {
                return Vector3.zero;
            }

            Vector3 selfPosition = self.transform.position;
            Vector3 force = Vector3.zero;
            float selfRadius = self.NavAgent.radius;

            for (int i = 0; i < allMonsters.Count; i++)
            {
                var other = allMonsters[i];
                if (other == null || other == self || other.NavAgent == null)
                {
                    continue;
                }

                Vector3 difference = selfPosition - other.transform.position;
                difference.y = 0f;

                float distanceSquared = difference.sqrMagnitude;

                // 두 몬스터의 반경 합 + 버퍼를 분리 반경으로 사용
                float combinedRadius = selfRadius + other.NavAgent.radius + _separationBuffer;
                float radiusSquared = combinedRadius * combinedRadius;

                // 분리 범위 밖이거나 너무 가까운 경우 스킵
                if (distanceSquared < 0.0001f || distanceSquared > radiusSquared)
                {
                    continue;
                }

                // 가까울수록 강한 힘 적용
                float distance = Mathf.Sqrt(distanceSquared);
                float strength = 1f - (distance / combinedRadius);
                force += difference.normalized * strength;
            }

            return force;
        }
    }
}
