using UnityEngine;

namespace Combat.Core
{
    public readonly struct DamageInfo
    {
        public float Amount { get; }
        public bool IsCritical { get; }
        public Vector3 HitPoint { get; }
        public Vector3 HitDirection { get; }
        public DamageType Type { get; }

        public DamageInfo(float amount, bool isCritical, HitContext hitContext)
        {
            Amount = amount;
            IsCritical = isCritical;
            HitPoint = hitContext.HitPoint;
            HitDirection = hitContext.HitDirection;
            Type = hitContext.DamageType;
        }
    }

    public readonly struct HitContext
    {
        public Vector3 HitPoint { get; }
        public Vector3 HitDirection { get; }
        public DamageType DamageType { get; }

        private HitContext(Vector3 hitPoint, Vector3 hitDirection, DamageType damageType)
        {
            HitPoint = hitPoint;
            HitDirection = hitDirection;
            DamageType = damageType;
        }

        // 히트박스 충돌 기반 (일반 공격)
        public static HitContext FromCollision(Vector3 hitPoint, Vector3 hitDirection, DamageType damageType)
        {
            return new HitContext(hitPoint, hitDirection, damageType);
        }

        // 공격자-타겟 위치 기반 (DOT, 투사체 없는 스킬)
        public static HitContext FromPositions(Vector3 attackerPosition, Vector3 targetPosition, DamageType damageType)
        {
            Vector3 direction = (targetPosition - attackerPosition).normalized;
            return new HitContext(targetPosition, direction, damageType);
        }

        // 방향 없는 데미지 (환경, 함정, 낙하)
        public static HitContext FromEnvironment(Vector3 targetPosition, DamageType damageType)
        {
            return new HitContext(targetPosition, Vector3.zero, damageType);
        }

        // 정보 없는 기본값 (테스트/디버그용)
        public static HitContext Empty => new HitContext(Vector3.zero, Vector3.zero, DamageType.True);
    }
}
