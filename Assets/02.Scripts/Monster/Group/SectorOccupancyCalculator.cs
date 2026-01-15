using System.Collections.Generic;
using Monster.AI;
using UnityEngine;

namespace Monster.Group
{
    // 플레이어 주변 섹터별 점유도 계산 (360도를 섹터로 나누어 밀집도 관리)
    public class SectorOccupancyCalculator
    {
        private readonly int _sectorCount;
        private readonly float _sectorScanExtraDist;
        private readonly float _sectorSpreadJitterDeg;
        private readonly float _baseRadius;

        public SectorOccupancyCalculator(int sectorCount, float sectorScanExtraDist, float sectorSpreadJitterDeg, float baseRadius = 0.5f)
        {
            _sectorCount = sectorCount;
            _sectorScanExtraDist = sectorScanExtraDist;
            _sectorSpreadJitterDeg = sectorSpreadJitterDeg;
            _baseRadius = baseRadius;
        }

        public float[] CalculateSectorOccupancy(List<MonsterController> monsters, Vector3 playerPos, Vector3 playerForward)
        {
            float[] occupancy = new float[_sectorCount];

            for (int i = 0; i < monsters.Count; i++)
            {
                var monster = monsters[i];
                if (monster == null || !monster.IsAlive || monster.NavAgent == null)
                {
                    continue;
                }

                Vector3 toMonster = monster.transform.position - playerPos;
                toMonster.y = 0f;
                float distance = toMonster.magnitude;

                float scanDistance = monster.Data.PreferredMaxDistance + _sectorScanExtraDist;
                if (distance > scanDistance || toMonster.sqrMagnitude < 0.001f)
                {
                    continue;
                }

                float angle = Vector3.SignedAngle(playerForward, toMonster.normalized, Vector3.up);
                int sectorIndex = AngleToSector(angle);

                // 거리에 따른 가중치 (가까울수록 더 "차있다"로 취급)
                float distanceWeight = Mathf.Lerp(1.5f, 0.5f, Mathf.Clamp01(distance / scanDistance));

                // 몬스터 크기에 따른 가중치 (큰 몬스터는 더 많은 공간 차지)
                float radiusWeight = monster.NavAgent.radius / _baseRadius;

                occupancy[sectorIndex] += distanceWeight * radiusWeight;
            }

            return occupancy;
        }

        public int SelectLeastOccupiedSector(float[] occupancy, float avoidFrontBias)
        {
            int bestSector = 0;
            float bestScore = float.MaxValue;

            for (int s = 0; s < _sectorCount; s++)
            {
                float sectorAngle = SectorToAngle(s);

                // 전방(0도) 근처 회피 바이어스
                float frontPenalty = Mathf.Abs(Mathf.DeltaAngle(0f, sectorAngle)) / 180f;
                float bias = Mathf.Lerp(0f, (1f - frontPenalty), avoidFrontBias);

                float score = occupancy[s] + bias;

                if (score < bestScore)
                {
                    bestScore = score;
                    bestSector = s;
                }
            }

            return bestSector;
        }

        public float ApplyJitter(float angle)
        {
            return angle + Random.Range(-_sectorSpreadJitterDeg, _sectorSpreadJitterDeg);
        }

        private int AngleToSector(float signedAngle)
        {
            float normalizedAngle = (signedAngle + 180f) / 360f;
            int index = Mathf.FloorToInt(normalizedAngle * _sectorCount);
            return Mathf.Clamp(index, 0, _sectorCount - 1);
        }

        public float SectorToAngle(int sectorIndex)
        {
            float step = 360f / _sectorCount;
            return -180f + step * (sectorIndex + 0.5f);
        }
    }
}
