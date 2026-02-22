using System.Collections.Generic;
using Boss.AI.States;
using Boss.Data;
using UnityEngine;

namespace Boss.AI
{
    /// <summary>
    /// 보스 패턴 선택 로직
    /// 거리, 쿨다운, 페이즈 조건, 가중치를 고려하여 다음 공격 패턴 결정
    /// </summary>
    public class BossPatternSelector
    {
        private readonly BossController _controller;
        private readonly Dictionary<EBossState, float> _cooldownTimers = new();
        private readonly Dictionary<EBossState, float> _baseCooldowns = new();

        // 콤보 시스템
        private Queue<EBossState> _comboQueue = new();
        private bool _isInCombo;

        // 마지막 사용 패턴 (연속 사용 방지)
        private EBossState _lastPattern = EBossState.Idle;
        private int _samePatternCount;

        public bool IsInCombo => _isInCombo && _comboQueue.Count > 0;

        public BossPatternSelector(BossController controller)
        {
            _controller = controller;
            InitializeCooldowns();
        }

        private void InitializeCooldowns()
        {
            var data = _controller.Data;

            _baseCooldowns[EBossState.MeleeAttack] = data.MeleeCooldown;
            _baseCooldowns[EBossState.Charge] = data.ChargeCooldown;
            _baseCooldowns[EBossState.Breath] = data.BreathCooldown;
            _baseCooldowns[EBossState.Projectile] = data.ProjectileCooldown;
            _baseCooldowns[EBossState.Summon] = data.SummonCooldown;

            // 초기 쿨다운 설정 (MeleeAttack은 바로 사용 가능)
            foreach (var pattern in _baseCooldowns.Keys)
            {
                _cooldownTimers[pattern] = pattern == EBossState.MeleeAttack ? 0f : _baseCooldowns[pattern] * 0.5f;
            }
        }

        /// <summary>
        /// 쿨다운 업데이트 (매 프레임 호출)
        /// </summary>
        public void UpdateCooldowns(float deltaTime)
        {
            var keys = new List<EBossState>(_cooldownTimers.Keys);
            foreach (var key in keys)
            {
                if (_cooldownTimers[key] > 0f)
                {
                    _cooldownTimers[key] -= deltaTime;
                }
            }
        }

        /// <summary>
        /// 다음 패턴 선택
        /// </summary>
        public EBossState SelectNextPattern()
        {
            // 콤보 진행 중이면 큐에서 다음 패턴 반환
            if (_isInCombo && _comboQueue.Count > 0)
            {
                return _comboQueue.Dequeue();
            }

            _isInCombo = false;

            // 사용 가능한 패턴 목록 구성
            var availablePatterns = GetAvailablePatterns();

            if (availablePatterns.Count == 0)
            {
                return EBossState.Idle; // 사용 가능한 패턴 없음 (대기)
            }

            // 가중치 기반 선택
            EBossState selected = SelectByWeight(availablePatterns);

            // 콤보 패턴 확인 및 큐 설정
            TryStartCombo(selected);

            // 쿨다운 적용
            ApplyCooldown(selected);

            // 연속 사용 추적
            TrackPatternUsage(selected);

            return selected;
        }

        private List<PatternCandidate> GetAvailablePatterns()
        {
            var candidates = new List<PatternCandidate>();
            var phaseData = _controller.PhaseManager?.CurrentPhase;
            float distanceToPlayer = GetDistanceToPlayer();
            var data = _controller.Data;

            // 근접 공격 - 항상 가능, 거리 조건만 확인
            if (IsOffCooldown(EBossState.MeleeAttack) && distanceToPlayer <= data.MeleeRange * 1.5f)
            {
                float weight = CalculateMeleeWeight(distanceToPlayer);
                candidates.Add(new PatternCandidate(EBossState.MeleeAttack, weight));
            }

            // 돌진 공격 - 페이즈 조건 + 거리 조건 (중거리 이상)
            if (phaseData != null && phaseData.EnableCharge &&
                IsOffCooldown(EBossState.Charge) &&
                distanceToPlayer > data.MeleeRange && distanceToPlayer <= data.ChargeDistance)
            {
                float weight = CalculateChargeWeight(distanceToPlayer);
                candidates.Add(new PatternCandidate(EBossState.Charge, weight));
            }

            // 브레스 공격 - 페이즈 조건 + 거리 조건 (근~중거리)
            if (phaseData != null && phaseData.EnableBreath &&
                IsOffCooldown(EBossState.Breath) &&
                distanceToPlayer <= data.BreathRange)
            {
                float weight = CalculateBreathWeight(distanceToPlayer);
                candidates.Add(new PatternCandidate(EBossState.Breath, weight));
            }

            // 투사체 공격 - 페이즈 조건 + 거리 조건 (중~원거리)
            if (phaseData != null && phaseData.EnableProjectile &&
                IsOffCooldown(EBossState.Projectile) &&
                distanceToPlayer > data.MeleeRange)
            {
                float weight = CalculateProjectileWeight(distanceToPlayer);
                candidates.Add(new PatternCandidate(EBossState.Projectile, weight));
            }

            // 소환 - 페이즈 조건 + HP 조건 + 잡졸 수 조건
            if (phaseData != null && phaseData.EnableSummon &&
                IsOffCooldown(EBossState.Summon) &&
                CanSummon())
            {
                candidates.Add(new PatternCandidate(EBossState.Summon, 30f));
            }

            return candidates;
        }

        private float GetDistanceToPlayer()
        {
            if (_controller.PlayerTransform == null) return float.MaxValue;
            return Vector3.Distance(_controller.transform.position, _controller.PlayerTransform.position);
        }

        private bool IsOffCooldown(EBossState pattern)
        {
            return !_cooldownTimers.ContainsKey(pattern) || _cooldownTimers[pattern] <= 0f;
        }

        private bool CanSummon()
        {
            var data = _controller.Data;
            float hpRatio = _controller.Combatant.CurrentHealth / _controller.Combatant.MaxHealth;

            // HP 조건
            if (hpRatio > data.SummonHPThreshold) return false;

            // 잡졸 수 조건
            if (_controller.MinionManager != null &&
                !_controller.MinionManager.CanSummonMore(data.MaxAliveMinions))
            {
                return false;
            }

            return true;
        }

        #region 가중치 계산

        private float CalculateMeleeWeight(float distance)
        {
            // 가까울수록 높은 가중치
            float baseWeight = 50f;
            float distanceFactor = 1f - (distance / (_controller.Data.MeleeRange * 1.5f));
            return baseWeight * Mathf.Max(0.5f, distanceFactor);
        }

        private float CalculateChargeWeight(float distance)
        {
            // 중거리에서 최대 가중치
            float baseWeight = 40f;
            float optimalDistance = _controller.Data.ChargeDistance * 0.6f;
            float distanceFactor = 1f - Mathf.Abs(distance - optimalDistance) / optimalDistance;
            return baseWeight * Mathf.Max(0.3f, distanceFactor);
        }

        private float CalculateBreathWeight(float distance)
        {
            // 근~중거리에서 높은 가중치
            float baseWeight = 45f;
            float distanceFactor = 1f - (distance / _controller.Data.BreathRange);
            return baseWeight * Mathf.Max(0.4f, distanceFactor);
        }

        private float CalculateProjectileWeight(float distance)
        {
            // 원거리에서 높은 가중치
            float baseWeight = 35f;
            float minRange = _controller.Data.MeleeRange;
            float distanceFactor = Mathf.Min(1f, (distance - minRange) / 10f);
            return baseWeight * Mathf.Max(0.3f, distanceFactor);
        }

        #endregion

        private EBossState SelectByWeight(List<PatternCandidate> candidates)
        {
            // 연속 사용 패널티 적용
            foreach (var candidate in candidates)
            {
                if (candidate.Pattern == _lastPattern)
                {
                    candidate.Weight *= Mathf.Max(0.3f, 1f - (_samePatternCount * 0.3f));
                }
            }

            // 총 가중치 계산
            float totalWeight = 0f;
            foreach (var candidate in candidates)
            {
                totalWeight += candidate.Weight;
            }

            // 랜덤 선택
            float random = Random.Range(0f, totalWeight);
            float cumulative = 0f;

            foreach (var candidate in candidates)
            {
                cumulative += candidate.Weight;
                if (random <= cumulative)
                {
                    return candidate.Pattern;
                }
            }

            return candidates[candidates.Count - 1].Pattern;
        }

        private void ApplyCooldown(EBossState pattern)
        {
            if (_baseCooldowns.TryGetValue(pattern, out float baseCooldown))
            {
                float multiplier = _controller.PhaseManager?.CurrentPhase?.CooldownMultiplier ?? 1f;

                // 분노 상태면 쿨다운 감소
                if (_controller.EnrageSystem != null && _controller.EnrageSystem.IsEnraged)
                {
                    multiplier *= 0.7f;
                }

                _cooldownTimers[pattern] = baseCooldown * multiplier;
            }
        }

        private void TrackPatternUsage(EBossState pattern)
        {
            if (pattern == _lastPattern)
            {
                _samePatternCount++;
            }
            else
            {
                _samePatternCount = 1;
                _lastPattern = pattern;
            }
        }

        #region 콤보 시스템

        private void TryStartCombo(EBossState initialPattern)
        {
            var phaseData = _controller.PhaseManager?.CurrentPhase;

            // Phase 3 이상에서만 콤보 활성화
            int phaseNumber = _controller.PhaseManager?.CurrentPhaseNumber ?? 1;
            if (phaseNumber < 3) return;

            // 콤보 확률 (Phase 3에서 30%)
            if (Random.value > 0.3f) return;

            // 콤보 패턴 구성
            var combo = GetComboForPattern(initialPattern);
            if (combo != null && combo.Length > 1)
            {
                _comboQueue.Clear();
                // 첫 패턴은 이미 반환되므로 두 번째부터 큐에 추가
                for (int i = 1; i < combo.Length; i++)
                {
                    _comboQueue.Enqueue(combo[i]);
                }
                _isInCombo = true;
            }
        }

        private EBossState[] GetComboForPattern(EBossState pattern)
        {
            switch (pattern)
            {
                case EBossState.MeleeAttack:
                    // 근접 3연타
                    return new[] { EBossState.MeleeAttack, EBossState.MeleeAttack, EBossState.MeleeAttack };

                case EBossState.Charge:
                    // 돌진 → 근접
                    return new[] { EBossState.Charge, EBossState.MeleeAttack };

                case EBossState.Breath:
                    // 브레스 → 투사체 (원거리 도주 대비)
                    if (_controller.PhaseManager?.CurrentPhase?.EnableProjectile == true)
                    {
                        return new[] { EBossState.Breath, EBossState.Projectile };
                    }
                    break;
            }

            return null;
        }

        /// <summary>
        /// 콤보 강제 종료
        /// </summary>
        public void CancelCombo()
        {
            _comboQueue.Clear();
            _isInCombo = false;
        }

        #endregion

        /// <summary>
        /// 페이즈 전환 시 쿨다운 초기화
        /// </summary>
        public void OnPhaseTransition()
        {
            foreach (var key in new List<EBossState>(_cooldownTimers.Keys))
            {
                _cooldownTimers[key] = 0f;
            }

            _comboQueue.Clear();
            _isInCombo = false;
        }

        /// <summary>
        /// 특정 패턴의 남은 쿨다운 조회
        /// </summary>
        public float GetRemainingCooldown(EBossState pattern)
        {
            return _cooldownTimers.TryGetValue(pattern, out float cd) ? Mathf.Max(0f, cd) : 0f;
        }

        private class PatternCandidate
        {
            public EBossState Pattern;
            public float Weight;

            public PatternCandidate(EBossState pattern, float weight)
            {
                Pattern = pattern;
                Weight = weight;
            }
        }
    }
}
