using Combat.Core;

namespace Combat.Attack
{
    public interface IAttacker
    {
        ICombatant Combatant { get; }
        bool CanAttack { get; }
        void Attack();
    }
}
