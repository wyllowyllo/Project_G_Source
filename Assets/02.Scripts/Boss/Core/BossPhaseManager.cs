using System;
using Boss.Data;
using Combat.Core;
using UnityEngine;

namespace Boss.Core
{
    // 보스 페이즈 관리 시스템
    // HP 구간별로 페이즈 전환 및 스탯 변경 적용
    public class BossPhaseManager
    {
        // 보스 공격 패턴 
        public enum EBossPattern
        {
            MeleeAttack,
            Charge,
            Breath,
            Projectile,
            Summon
        }
        
        private readonly BossPhaseData[] _phases;
        private readonly Combatant _combatant;
        private int _currentPhaseIndex;
        private bool _isTransitioning;

        public int CurrentPhaseIndex => _currentPhaseIndex;
        public int CurrentPhaseNumber => _currentPhaseIndex + 1;
        public BossPhaseData CurrentPhase => _phases != null && _currentPhaseIndex < _phases.Length
            ? _phases[_currentPhaseIndex]
            : null;
        public bool IsTransitioning => _isTransitioning;
        public int TotalPhases => _phases?.Length ?? 0;

        public event Action<int, int> OnPhaseChanged;
        public event Action<BossPhaseData> OnPhaseTransitionStart;
        public event Action<BossPhaseData> OnPhaseTransitionEnd;

        public BossPhaseManager(BossPhaseData[] phases, Combatant combatant)
        {
            _phases = phases ?? Array.Empty<BossPhaseData>();
            _combatant = combatant;
            _currentPhaseIndex = 0;
            _isTransitioning = false;

            SortPhasesByThreshold();
        }

        // HP 임계점 기준 내림차순 정렬 (Phase 1이 가장 높은 HP)
        private void SortPhasesByThreshold()
        {
            if (_phases == null || _phases.Length <= 1)
                return;

            Array.Sort(_phases, (a, b) => b.HPThreshold.CompareTo(a.HPThreshold));
        }

        // 매 프레임 호출하여 페이즈 전환 체크
        public void Update()
        {
            if (_isTransitioning || _combatant == null || _phases == null || _phases.Length == 0)
                return;

            float hpRatio = _combatant.MaxHealth > 0
                ? _combatant.CurrentHealth / _combatant.MaxHealth
                : 1f;
            int nextPhaseIndex = GetPhaseIndexForHP(hpRatio);

            if (nextPhaseIndex > _currentPhaseIndex)
            {
                TriggerPhaseTransition(nextPhaseIndex);
            }
        }

        // HP 비율에 해당하는 페이즈 인덱스 반환
        private int GetPhaseIndexForHP(float hpRatio)
        {
            for (int i = _phases.Length - 1; i >= 0; i--)
            {
                if (hpRatio <= _phases[i].HPThreshold)
                {
                    return i;
                }
            }
            return 0;
        }

        // 페이즈 전환 시작
        private void TriggerPhaseTransition(int newPhaseIndex)
        {
            if (newPhaseIndex <= _currentPhaseIndex || newPhaseIndex >= _phases.Length)
                return;

            int previousPhase = _currentPhaseIndex;
            _currentPhaseIndex = newPhaseIndex;
            _isTransitioning = true;

            OnPhaseChanged?.Invoke(previousPhase, _currentPhaseIndex);
            OnPhaseTransitionStart?.Invoke(CurrentPhase);
        }

        // 페이즈 전환 완료 (애니메이션 종료 후 호출)
        public void CompleteTransition()
        {
            if (!_isTransitioning)
                return;

            _isTransitioning = false;
            OnPhaseTransitionEnd?.Invoke(CurrentPhase);
        }

        // 현재 페이즈에서 특정 패턴이 활성화되어 있는지 확인
        public bool IsPatternEnabled(EBossPattern pattern)
        {
            if (CurrentPhase == null)
                return false;

            return pattern switch
            {
                EBossPattern.Charge => CurrentPhase.EnableCharge,
                EBossPattern.Breath => CurrentPhase.EnableBreath,
                EBossPattern.Projectile => CurrentPhase.EnableProjectile,
                EBossPattern.Summon => CurrentPhase.EnableSummon,
                _ => true
            };
        }

        // 현재 페이즈의 데미지 배율 반환
        public float GetDamageMultiplier()
        {
            return CurrentPhase?.DamageMultiplier ?? 1f;
        }

        // 현재 페이즈의 공격 속도 배율 반환
        public float GetAttackSpeedMultiplier()
        {
            return CurrentPhase?.AttackSpeedMultiplier ?? 1f;
        }

        // 현재 페이즈의 쿨다운 배율 반환
        public float GetCooldownMultiplier()
        {
            return CurrentPhase?.CooldownMultiplier ?? 1f;
        }

        // 전환 연출 필요 여부
        public bool ShouldPlayRoarOnTransition()
        {
            return CurrentPhase?.PlayRoarOnTransition ?? false;
        }
    }

   
}
