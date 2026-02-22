using Combat.Core;
using Combat.Data;
using UnityEngine;

namespace Combat.Damage
{
    public static class DamageProcessor
    {
        public static DamageInfo Process(AttackContext attack, HitInfo hitInfo, Vector3 currentAttackerPosition)
        {
            var defenderInfo = new DefenderInfo(hitInfo.TargetCombatant, hitInfo.TargetHealth);
            var damageResult = DamageCalculator.Calculate(attack, defenderInfo);

            Vector3 hitPoint = hitInfo.GetAdjustedHitPoint(currentAttackerPosition);
            Vector3 hitDirection = hitInfo.GetHitDirectionFrom(attack.AttackerPosition);

            var hitContext = HitContext.FromCollision(hitPoint, hitDirection, attack.DamageType);
            return new DamageInfo(damageResult.FinalDamage, damageResult.IsCritical, hitContext);
        }
    }
}
