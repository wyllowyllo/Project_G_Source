using Boss.AI;
using Monster.Ability;

namespace Boss.Ability
{
    // Boss 전용 PlayerDetectAbility 확장
    // 공통 기능은 Monster.Ability.PlayerDetectAbility에서 상속
    public class BossPlayerDetectAbility : PlayerDetectAbility
    {
        private BossController BossController => _controller as BossController;

        public bool IsInMeleeRange()
        {
            return BossController != null && IsInRange(BossController.Data.MeleeRange);
        }

        public bool IsInBreathRange()
        {
            return BossController != null && IsInRange(BossController.Data.BreathRange);
        }

        public bool IsInChargeRange()
        {
            return BossController != null && IsInRange(BossController.Data.ChargeDistance);
        }
    }
}
