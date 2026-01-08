using System.Collections.Generic;
using System;
using Monster.AI;
using UnityEngine;

namespace Monster.Group
{
    /// <summary>
    /// 공격자 선정을 담당하는 클래스.
    /// 점수 기반으로 공격 슬롯을 할당합니다.
    /// </summary>
    public class AttackerSelector
    {
        private readonly AttackSlotManager _slotManager;
        private readonly AttackScoreCalculator _scoreCalculator;
        private readonly Dictionary<MonsterController, float> _lastAttackTimes;

        private readonly float _minAttackReassignInterval;
        private float _nextAttackAssignTime;

        public AttackerSelector(
            AttackSlotManager slotManager,
            AttackScoreCalculator scoreCalculator,
            Dictionary<MonsterController, float> lastAttackTimes,
            float minAttackReassignInterval)
        {
            _slotManager = slotManager;
            _scoreCalculator = scoreCalculator;
            _lastAttackTimes = lastAttackTimes;
            _minAttackReassignInterval = minAttackReassignInterval;
            _nextAttackAssignTime = 0f;
        }

        /// <summary>
        /// 공격자를 점수 기반으로 선정하고 슬롯을 할당합니다.
        /// </summary>
        public void AssignAttackersByScore(
            List<MonsterController> allMonsters,
            Func<MonsterController, bool> requestSlotFunc,
            float currentTime)
        {
            if (currentTime < _nextAttackAssignTime)
            {
                return;
            }

            int availableSlots = _slotManager.AvailableSlots;
            if (availableSlots <= 0)
            {
                return;
            }

            // 후보 평가
            var candidates = new List<(MonsterController monster, float score)>();

            for (int i = 0; i < allMonsters.Count; i++)
            {
                var monster = allMonsters[i];
                if (monster == null || !monster.IsAlive)
                {
                    continue;
                }

                // 이미 슬롯 보유 중이면 후보에서 제외
                if (_slotManager.HasSlot(monster))
                {
                    continue;
                }

                float score = _scoreCalculator.CalculateScore(monster, currentTime);
                if (score > 0f)
                {
                    candidates.Add((monster, score));
                }
            }

            if (candidates.Count == 0)
            {
                return;
            }

            // 점수 내림차순 정렬
            candidates.Sort((a, b) => b.score.CompareTo(a.score));

            // 상위 후보에게 슬롯 할당
            int assignCount = Mathf.Min(availableSlots, candidates.Count);
            for (int i = 0; i < assignCount; i++)
            {
                var monster = candidates[i].monster;
                if (requestSlotFunc(monster))
                {
                    _lastAttackTimes[monster] = currentTime;
                }
            }

            _nextAttackAssignTime = currentTime + _minAttackReassignInterval;
        }
    }
}
