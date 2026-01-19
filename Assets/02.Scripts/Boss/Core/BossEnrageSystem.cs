using System;
using Boss.AI;
using Boss.Data;
using Combat.Core;
using UnityEngine;

namespace Boss.Core
{
    /// <summary>
    /// 보스 분노 시스템
    /// 특정 조건에서 보스가 강화되는 시스템
    /// </summary>
    public class BossEnrageSystem
    {
        private readonly BossController _controller;
        private readonly BossData _data;
        private readonly Combatant _combatant;

        private bool _isEnraged;
        private float _enrageStartTime;
        private float _combatStartTime;

        // 분노 발동 조건 추적
        private bool _triggeredByHP;
        private bool _triggeredByTime;
        private bool _triggeredByMinionDeath;

        // 분노 효과
        private float _damageMultiplier = 1f;
        private float _speedMultiplier = 1f;

        public bool IsEnraged => _isEnraged;
        public float DamageMultiplier => _isEnraged ? _damageMultiplier : 1f;
        public float SpeedMultiplier => _isEnraged ? _speedMultiplier : 1f;
        public float EnrageDuration => _isEnraged ? Time.time - _enrageStartTime : 0f;

        public event Action OnEnrageStart;
        public event Action OnEnrageEnd;

        public BossEnrageSystem(BossController controller)
        {
            _controller = controller;
            _data = controller.Data;
            _combatant = controller.Combatant;
            _combatStartTime = Time.time;
        }

        /// <summary>
        /// 분노 조건 확인 (매 프레임 호출)
        /// </summary>
        public void Update()
        {
            if (_isEnraged) return;

            // HP 기반 분노
            if (!_triggeredByHP && CheckHPCondition())
            {
                _triggeredByHP = true;
                TriggerEnrage(EEnrageReason.LowHP);
                return;
            }

            // 시간 기반 분노 (전투 시간 2분 이상)
            if (!_triggeredByTime && CheckTimeCondition())
            {
                _triggeredByTime = true;
                TriggerEnrage(EEnrageReason.CombatTimeout);
                return;
            }
        }

        private bool CheckHPCondition()
        {
            if (_combatant == null) return false;
            float hpRatio = _combatant.CurrentHealth / _combatant.MaxHealth;
            return hpRatio <= _data.EnrageHPThreshold;
        }

        private bool CheckTimeCondition()
        {
            // 전투 시작 후 2분 경과
            const float COMBAT_TIMEOUT = 120f;
            return Time.time - _combatStartTime >= COMBAT_TIMEOUT;
        }

        /// <summary>
        /// 잡졸 전멸 시 호출 (BossMinionManager에서 호출)
        /// </summary>
        public void OnAllMinionsDead()
        {
            if (_isEnraged) return;
            if (_triggeredByMinionDeath) return;

            _triggeredByMinionDeath = true;
            TriggerEnrage(EEnrageReason.MinionsDead);
        }

        private void TriggerEnrage(EEnrageReason reason)
        {
            if (_isEnraged) return;

            _isEnraged = true;
            _enrageStartTime = Time.time;

            // 분노 효과 적용
            _damageMultiplier = _data.EnrageDamageMultiplier;
            _speedMultiplier = _data.EnrageSpeedMultiplier;

            // NavAgent 속도 증가
            if (_controller.NavAgent != null)
            {
                _controller.NavAgent.speed = _data.MoveSpeed * _speedMultiplier;
            }

            Debug.Log($"[BossEnrageSystem] 분노 발동! 이유: {reason}");
            OnEnrageStart?.Invoke();
        }

        /// <summary>
        /// 분노 종료 (선택적 - 일반적으로 사망까지 유지)
        /// </summary>
        public void EndEnrage()
        {
            if (!_isEnraged) return;

            _isEnraged = false;
            _damageMultiplier = 1f;
            _speedMultiplier = 1f;

            // NavAgent 속도 복원
            if (_controller.NavAgent != null)
            {
                _controller.NavAgent.speed = _data.MoveSpeed;
            }

            OnEnrageEnd?.Invoke();
        }

        /// <summary>
        /// 전투 시작 시간 리셋 (전투 재시작 시)
        /// </summary>
        public void ResetCombatTimer()
        {
            _combatStartTime = Time.time;
        }

        /// <summary>
        /// 분노 상태에 따른 데미지 계산
        /// </summary>
        public float ApplyEnrageDamage(float baseDamage)
        {
            return baseDamage * DamageMultiplier;
        }
    }

    public enum EEnrageReason
    {
        LowHP,          // HP가 임계점 이하
        CombatTimeout,  // 전투 시간 초과
        MinionsDead     // 소환한 잡졸 전멸
    }
}
