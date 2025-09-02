using GamePrototype.Utils;

namespace GamePrototype.Items.EquipItems
{
    public class Helmet : EquipItem
    {
        public Helmet(uint defence, uint maxDurability, string name) : base(maxDurability, name)
            => Defence = defence;

        public uint Defence { get; }

        public override EquipSlot Slot => EquipSlot.Helmet;

        public void TakeDamage()
        {
            if (Durability > 0)
                ReduceDurability(1);
        }
    }
}
