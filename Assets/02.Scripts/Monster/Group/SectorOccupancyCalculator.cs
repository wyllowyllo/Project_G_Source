using System.Collections.Generic;
using Monster.AI;
using UnityEngine;

namespace Monster.Group
{
    /// <summary>
    /// 플레이어 주변 섹터별 점유도를 계산하는 클래스.
    /// 360도를 여러 섹터로 나누어 각 섹터의 밀집도를 관리합니다.
    /// </summary>
    public class SectorOccupancyCalculator
    {
        private readonly int _sectorCount;
        private readonly float _sectorScanExtraDist;
        private readonly float _sectorSpreadJitterDeg;

        public SectorOccupancyCalculator(int sectorCount, float sectorScanExtraDist, float sectorSpreadJitterDeg)
        {
            _sectorCount = sectorCount;
            _sectorScanExtraDist = sectorScanExtraDist;
            _sectorSpreadJitterDeg = sectorSpreadJitterDeg;
        }

        /// <summary>
        /// 각 섹터의 점유도를 계산합니다.
        /// </summary>
        public float[] CalculateSectorOccupancy(List<MonsterController> monsters, Vector3 playerPos, Vector3 playerForward)
        {
            float[] occupancy = new float[_sectorCount];

            for (int i = 0; i < monsters.Count; i++)
            {
                var monster = monsters[i];
                if (monster == null || !monster.IsAlive)
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
                float weight = Mathf.Lerp(1.5f, 0.5f, Mathf.Clamp01(distance / scanDistance));
                occupancy[sectorIndex] += weight;
            }

            return occupancy;
        }

        /// <summary>
        /// 가장 점유도가 낮은 섹터를 선택합니다 (전방 편향 포함).
        /// </summary>
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

        /// <summary>
        /// 섹터 각도에 지터(랜덤 오프셋)를 적용합니다.
        /// </summary>
        public float ApplyJitter(float angle)
        {
            return angle + Random.Range(-_sectorSpreadJitterDeg, _sectorSpreadJitterDeg);
        }

        /// <summary>
        /// 각도를 섹터 인덱스로 변환 ([-180, 180] → [0, sectorCount))
        /// </summary>
        private int AngleToSector(float signedAngle)
        {
            float normalizedAngle = (signedAngle + 180f) / 360f;
            int index = Mathf.FloorToInt(normalizedAngle * _sectorCount);
            return Mathf.Clamp(index, 0, _sectorCount - 1);
        }

        /// <summary>
        /// 섹터 인덱스를 각도로 변환 (섹터 중심 각도 반환: [-180, 180])
        /// </summary>
        public float SectorToAngle(int sectorIndex)
        {
            float step = 360f / _sectorCount;
            return -180f + step * (sectorIndex + 0.5f);
        }
    }
}
