using System.Collections.Generic;
using Monster.AI;
using UnityEngine;

namespace Monster.Group
{
    /// <summary>
    /// 공격자 선정을 위한 점수를 계산하는 클래스.
    /// 거리, 각도, 공정성, 혼잡도 등을 종합하여 점수를 산출합니다.
    /// </summary>
    public class AttackScoreCalculator
    {
        private readonly LineOfSightChecker _losChecker;
        private readonly Dictionary<MonsterController, float> _lastAttackTimes;
        private readonly List<MonsterController> _allMonsters;

        // 설정 값들
        private readonly float _attackRangeBuffer;
        private readonly float _sideAngleCenter;
        private readonly float _sideAngleSigma;
        private readonly float _sideAngleWeight;
        private readonly float _recentAttackerPenaltySeconds;

        private Transform _playerTransform;

        public AttackScoreCalculator(
            LineOfSightChecker losChecker,
            Dictionary<MonsterController, float> lastAttackTimes,
            List<MonsterController> allMonsters,
            float attackRangeBuffer,
            float sideAngleCenter,
            float sideAngleSigma,
            float sideAngleWeight,
            float recentAttackerPenaltySeconds)
        {
            _losChecker = losChecker;
            _lastAttackTimes = lastAttackTimes;
            _allMonsters = allMonsters;
            _attackRangeBuffer = attackRangeBuffer;
            _sideAngleCenter = sideAngleCenter;
            _sideAngleSigma = sideAngleSigma;
            _sideAngleWeight = sideAngleWeight;
            _recentAttackerPenaltySeconds = recentAttackerPenaltySeconds;
        }

        /// <summary>
        /// 플레이어 Transform 설정
        /// </summary>
        public void SetPlayerTransform(Transform playerTransform)
        {
            _playerTransform = playerTransform;
        }

        /// <summary>
        /// 특정 몬스터의 공격 점수를 계산합니다.
        /// </summary>
        public float CalculateScore(MonsterController monster, float currentTime)
        {
            if (monster == null || _playerTransform == null)
            {
                return 0f;
            }

            Vector3 monsterPosition = monster.transform.position;
            Vector3 playerPosition = _playerTransform.position;
            float distance = Vector3.Distance(monsterPosition, playerPosition);

            // 거리 범위 체크
            if (!IsWithinAttackRange(monster, distance))
            {
                return 0f;
            }

            // LoS 체크
            if (!_losChecker.HasLineOfSight(monsterPosition, playerPosition))
            {
                return 0f;
            }

            float score = 0f;

            // 거리 점수
            score += CalculateDistanceScore(distance, monster.Data.AttackRange);

            // 각도 점수 (측면 선호)
            score += CalculateAngleScore(monsterPosition, playerPosition);

            // 공정성 점수
            score *= CalculateFairnessModifier(monster, currentTime);

            // 혼잡도 페널티
            score *= CalculateCrowdPenalty(monster);

            return score;
        }

        /// <summary>
        /// 공격 범위 내에 있는지 확인
        /// </summary>
        private bool IsWithinAttackRange(MonsterController monster, float distance)
        {
            float maxDistance = monster.Data.AttackRange + _attackRangeBuffer + 2.0f;
            return distance <= maxDistance;
        }

        /// <summary>
        /// 거리 기반 점수 계산
        /// </summary>
        private float CalculateDistanceScore(float distance, float attackRange)
        {
            float idealDistance = attackRange + 0.25f;
            float distanceScore = Mathf.Clamp01(1f - Mathf.Abs(distance - idealDistance) / (attackRange + 1f));
            return distanceScore * 3.0f;
        }

        /// <summary>
        /// 각도 기반 점수 계산 (측면 공격 선호)
        /// </summary>
        private float CalculateAngleScore(Vector3 monsterPosition, Vector3 playerPosition)
        {
            Vector3 playerForward = _playerTransform.forward;
            playerForward.y = 0f;

            Vector3 toMonster = monsterPosition - playerPosition;
            toMonster.y = 0f;

            if (playerForward.sqrMagnitude < 0.001f || toMonster.sqrMagnitude < 0.001f)
            {
                return 0f;
            }

            playerForward.Normalize();
            toMonster.Normalize();

            float angle = Vector3.Angle(playerForward, toMonster);
            float sideScore = CalculateGaussian(angle, _sideAngleCenter, _sideAngleSigma);

            return sideScore * _sideAngleWeight;
        }

        /// <summary>
        /// 공정성 보정 값 계산 (최근 공격자는 페널티, 오래 대기한 몬스터는 보너스)
        /// </summary>
        private float CalculateFairnessModifier(MonsterController monster, float currentTime)
        {
            float lastAttackTime = _lastAttackTimes.TryGetValue(monster, out var time) ? time : -9999f;
            float timeSinceLastAttack = currentTime - lastAttackTime;

            // 최근 공격자 페널티
            if (timeSinceLastAttack < _recentAttackerPenaltySeconds)
            {
                return 0.35f;
            }

            // 오래 대기한 몬스터 보너스
            float bonus = Mathf.Clamp01(timeSinceLastAttack / 5f) * 0.5f;
            return 1f + bonus;
        }

        /// <summary>
        /// 혼잡도 페널티 계산 (주변 몬스터가 많을수록 감점)
        /// </summary>
        private float CalculateCrowdPenalty(MonsterController monster)
        {
            int nearbyCount = CountNearbyMonsters(monster);

            if (nearbyCount <= 0) return 1f;
            if (nearbyCount == 1) return 0.9f;
            if (nearbyCount == 2) return 0.8f;
            return 0.7f;
        }

        /// <summary>
        /// 주변 몬스터 수 계산
        /// </summary>
        private int CountNearbyMonsters(MonsterController monster)
        {
            const float NearbyRadius = 1.2f;
            float radiusSquared = NearbyRadius * NearbyRadius;

            Vector3 position = monster.transform.position;
            int count = 0;

            for (int i = 0; i < _allMonsters.Count; i++)
            {
                var other = _allMonsters[i];
                if (other == null || other == monster)
                {
                    continue;
                }

                Vector3 difference = position - other.transform.position;
                difference.y = 0f;

                if (difference.sqrMagnitude <= radiusSquared)
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// 가우시안 함수 계산 (0~1 범위)
        /// </summary>
        private float CalculateGaussian(float x, float mean, float sigma)
        {
            float difference = x - mean;
            float denominator = 2f * sigma * sigma;
            return Mathf.Exp(-(difference * difference) / Mathf.Max(0.0001f, denominator));
        }
    }
}
