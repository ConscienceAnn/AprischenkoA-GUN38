using GamePrototype.Utils;

namespace GamePrototype.Items.EquipItems
{
    public class RangeWeapon : EquipItem
    {
        public RangeWeapon(uint damage, uint range, uint maxDurability, string name) : base(maxDurability, name)
        {
            Damage = damage;
            Range = range;
        }

        public uint Damage { get; }
        public uint Range { get; }

        public override EquipSlot Slot => EquipSlot.RangeWeapon;

        public void Use()
        {
            if (Durability > 0)
                ReduceDurability(1);
        }
    }
}
