namespace Combat.Core
{
    public interface IHealthProvider
    {
        float CurrentHealth { get; }
        float MaxHealth { get; }
    }
}
