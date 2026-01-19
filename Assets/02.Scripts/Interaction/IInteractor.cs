using Equipment;

namespace Interaction
{
    public interface IInteractor
    {
        PlayerEquipment Equipment { get; }
        bool TryEquip(EquipmentData equipment);
    }
}
