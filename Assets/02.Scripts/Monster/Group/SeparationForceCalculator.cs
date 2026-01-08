using System.Collections.Generic;
using Monster.AI;
using UnityEngine;

namespace Monster.Group
{
    /// <summary>
    /// 몬스터 간 분리력(Separation Force)을 계산하는 클래스.
    /// 몬스터들이 서로 겹치지 않도록 밀어내는 힘을 계산합니다.
    /// </summary>
    public class SeparationForceCalculator
    {
        private readonly float _separationRadius;

        public SeparationForceCalculator(float separationRadius)
        {
            _separationRadius = separationRadius;
        }

        /// <summary>
        /// 특정 몬스터에 대한 분리력을 계산합니다.
        /// </summary>
        public Vector3 ComputeSeparation(MonsterController self, List<MonsterController> allMonsters)
        {
            if (self == null)
            {
                return Vector3.zero;
            }

            Vector3 selfPosition = self.transform.position;
            Vector3 force = Vector3.zero;
            float radiusSquared = _separationRadius * _separationRadius;

            for (int i = 0; i < allMonsters.Count; i++)
            {
                var other = allMonsters[i];
                if (other == null || other == self)
                {
                    continue;
                }

                Vector3 difference = selfPosition - other.transform.position;
                difference.y = 0f;

                float distanceSquared = difference.sqrMagnitude;

                // 분리 범위 밖이거나 너무 가까운 경우 스킵
                if (distanceSquared < 0.0001f || distanceSquared > radiusSquared)
                {
                    continue;
                }

                // 가까울수록 강한 힘 적용
                float distance = Mathf.Sqrt(distanceSquared);
                float strength = 1f - (distance / _separationRadius);
                force += difference.normalized * strength;
            }

            return force;
        }
    }
}
