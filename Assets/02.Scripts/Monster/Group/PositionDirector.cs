using System;
using System.Collections.Generic;
using Common;
using Monster.AI;
using UnityEngine;

namespace Monster.Group
{
    /// <summary>
    /// 몬스터들의 위치 할당을 조율하는 디렉터 클래스.
    /// 섹터 기반 밀집도 관리, 재배치, 최종 위치 계산을 담당합니다.
    /// </summary>
    public class PositionDirector
    {
        private readonly SectorOccupancyCalculator _sectorCalculator;
        private readonly PositionAssigner _positionAssigner;
        private readonly Dictionary<MonsterController, float> _desiredAngleDeg;

        private readonly int _relocatePerTick;
        private readonly float _avoidFrontBias;

        public PositionDirector(
            SectorOccupancyCalculator sectorCalculator,
            PositionAssigner positionAssigner,
            Dictionary<MonsterController, float> desiredAngleDeg,
            int relocatePerTick,
            float avoidFrontBias)
        {
            _sectorCalculator = sectorCalculator;
            _positionAssigner = positionAssigner;
            _desiredAngleDeg = desiredAngleDeg;
            _relocatePerTick = relocatePerTick;
            _avoidFrontBias = avoidFrontBias;
        }

        /// <summary>
        /// 모든 몬스터의 원하는 위치를 업데이트합니다.
        /// </summary>
        public void UpdatePositions(
            List<MonsterController> allMonsters,
            Func<MonsterController, bool> hasSlotPredicate)
        {
            if (PlayerReferenceProvider.Instance == null || PlayerReferenceProvider.Instance.PlayerTransform == null)
            {
                return;
            }

            Transform playerTransform = PlayerReferenceProvider.Instance.PlayerTransform;
            Vector3 playerPos = playerTransform.position;
            Vector3 playerForward = playerTransform.forward;
            playerForward.y = 0f;

            if (playerForward.sqrMagnitude < 0.001f)
            {
                playerForward = Vector3.forward;
            }
            playerForward.Normalize();

            // 1. 섹터별 각도 할당
            UpdateDesiredAnglesBySectorOccupancy(allMonsters, hasSlotPredicate, playerPos, playerForward);

            // 2. 3D 위치 계산
            _positionAssigner.CalculatePositions(allMonsters, playerPos, playerForward);
        }

        /// <summary>
        /// 섹터 점유도를 기반으로 몬스터들의 원하는 각도를 업데이트합니다.
        /// </summary>
        private void UpdateDesiredAnglesBySectorOccupancy(
            List<MonsterController> allMonsters,
            Func<MonsterController, bool> hasSlotPredicate,
            Vector3 playerPos,
            Vector3 playerForward)
        {
            // 1. 섹터 점유도 계산
            float[] occupancy = _sectorCalculator.CalculateSectorOccupancy(allMonsters, playerPos, playerForward);

            // 2. 공격권 보유자는 현재 각도 유지, 비보유자는 재배치 대상
            var relocationCandidates = _positionAssigner.SelectRelocationCandidates(allMonsters, hasSlotPredicate);

            // 공격권 보유자의 현재 각도 유지
            PreserveAttackerAngles(allMonsters, hasSlotPredicate, playerPos, playerForward);

            // 3. 재배치 대상을 거리 순으로 정렬
            _positionAssigner.SortByDistanceToPlayer(relocationCandidates, playerPos);

            // 4. 제한된 수만큼 재배치
            int relocateCount = Mathf.Min(_relocatePerTick, relocationCandidates.Count);

            for (int i = 0; i < relocateCount; i++)
            {
                var monster = relocationCandidates[i];

                // 가장 비어있는 섹터 선택
                int bestSector = _sectorCalculator.SelectLeastOccupiedSector(occupancy, _avoidFrontBias);
                float chosenAngle = _sectorCalculator.SectorToAngle(bestSector);

                // 지터 적용
                chosenAngle = _sectorCalculator.ApplyJitter(chosenAngle);

                _desiredAngleDeg[monster] = chosenAngle;

                // 선택된 섹터의 점유도 즉시 증가
                occupancy[bestSector] += 1.0f;
            }
        }

        /// <summary>
        /// 공격권 보유 몬스터의 현재 각도를 유지합니다 (전열 안정성).
        /// </summary>
        private void PreserveAttackerAngles(
            List<MonsterController> allMonsters,
            Func<MonsterController, bool> hasSlotPredicate,
            Vector3 playerPos,
            Vector3 playerForward)
        {
            for (int i = 0; i < allMonsters.Count; i++)
            {
                var monster = allMonsters[i];
                if (monster == null || !monster.IsAlive)
                {
                    continue;
                }

                if (hasSlotPredicate(monster))
                {
                    // 공격권 보유자는 현재 각도 유지
                    Vector3 toMonster = monster.transform.position - playerPos;
                    toMonster.y = 0f;

                    float currentAngle = 0f;
                    if (toMonster.sqrMagnitude > 0.001f)
                    {
                        currentAngle = Vector3.SignedAngle(playerForward, toMonster.normalized, Vector3.up);
                    }

                    _desiredAngleDeg[monster] = currentAngle;
                }
            }
        }

        /// <summary>
        /// 원하는 위치를 가져옵니다 (외부 API).
        /// </summary>
        public Vector3 GetDesiredPosition(MonsterController monster)
        {
            return _positionAssigner.GetDesiredPosition(monster);
        }
    }
}
