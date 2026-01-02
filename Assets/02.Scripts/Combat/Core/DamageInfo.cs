using UnityEngine;

namespace Combat.Core
{
    public readonly struct DamageInfo
{
    public float Amount { get; }
    public bool IsCritical { get; }
    public ICombatant Attacker { get; }
    public Vector3 HitPoint { get; }
    public Vector3 HitDirection { get; }
    public DamageType Type { get; }

    public DamageInfo(float amount, bool isCritical, ICombatant attacker, HitContext hitContext)
    {
        Amount = amount;
        IsCritical = isCritical;
        Attacker = attacker;
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

    public HitContext(Vector3 hitPoint, Vector3 hitDirection, DamageType damageType)
    {
        HitPoint = hitPoint;
        HitDirection = hitDirection;
        DamageType = damageType;
    }
}
}
