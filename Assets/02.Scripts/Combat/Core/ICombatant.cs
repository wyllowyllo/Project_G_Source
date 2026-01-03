using UnityEngine;

namespace Combat.Core
{
    public interface ICombatant
    {
        Transform Transform { get; }
        CombatStats Stats { get; }
        CombatTeam Team { get; }
        CombatantAttackStats GetAttackStats();
    }
}
