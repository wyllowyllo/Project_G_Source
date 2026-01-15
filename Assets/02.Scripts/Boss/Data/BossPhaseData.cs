using UnityEngine;

namespace Boss.Data
{
    [System.Serializable]
    public class BossPhaseData
    {
        [Header("페이즈 조건")]
        [Tooltip("이 페이즈가 활성화되는 HP 비율 (이하일 때)")]
        [Range(0f, 1f)]
        public float HPThreshold = 1f;

        [Header("스탯 배율")]
        [Tooltip("데미지 배율")]
        public float DamageMultiplier = 1f;

        [Tooltip("공격 속도 배율")]
        public float AttackSpeedMultiplier = 1f;

        [Tooltip("쿨다운 배율 (낮을수록 빠름)")]
        public float CooldownMultiplier = 1f;

        [Header("패턴 활성화")]
        [Tooltip("돌진 공격 활성화")]
        public bool EnableCharge = true;

        [Tooltip("브레스 공격 활성화")]
        public bool EnableBreath = false;

        [Tooltip("투사체 공격 활성화")]
        public bool EnableProjectile = false;

        [Tooltip("소환 활성화")]
        public bool EnableSummon = false;

        [Header("전환 연출")]
        [Tooltip("페이즈 전환 시 포효 연출")]
        public bool PlayRoarOnTransition = true;
    }
}
