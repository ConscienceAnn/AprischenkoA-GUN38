using GamePrototype.Items.EconomicItems;
using GamePrototype.Items.EquipItems;
using GamePrototype.Utils;
using System.Text;

namespace GamePrototype.Units
{
    public sealed class Player : Unit
    {
        private readonly Dictionary<EquipSlot, EquipItem> _equipment = new();

        public Player(string name, uint health, uint maxHealth, uint baseDamage) : base(name, health, maxHealth, baseDamage)
        {            
        }

        public override uint GetUnitDamage()
        {
            
            if (_equipment.TryGetValue(EquipSlot.Weapon, out var item) && item is Weapon weapon)
            {
                return BaseDamage + weapon.Damage;
            }
           
            if (_equipment.TryGetValue(EquipSlot.RangeWeapon, out var rangeItem) && rangeItem is RangeWeapon rangeWeapon)
            {
                return BaseDamage / 2 + rangeWeapon.Damage;
            }

            return BaseDamage;
        }

        public override void HandleCombatComplete()
        {
            var items = Inventory.Items;
            for (int i = 0; i < items.Count; i++) 
            {
                if (items[i] is EconomicItem economicItem) 
                {
                    UseEconomicItem(economicItem);
                    Inventory.TryRemove(items[i]);
                }
            }
        }

        public override void AddItemToInventory(Item item)
        {
            if (item is EquipItem equipItem)
            {
                if (!_equipment.ContainsKey(equipItem.Slot))
                {
                    _equipment[equipItem.Slot] = equipItem;
                    Console.WriteLine($"Автоматически экипирован: {equipItem.Name}");
                }
                else
                {
                    TryReplaceEquipment(equipItem);
                }
            }
            else
            {
                base.AddItemToInventory(item);
            }
        }

        private void UseEconomicItem(EconomicItem economicItem)
        {
            if (economicItem is HealthPotion healthPotion) 
            {
                Health = Math.Min(Health + healthPotion.HealthRestore, MaxHealth);
                Console.WriteLine($"Восстановлено здоровье: {Health}/{MaxHealth}");
            }
            else if (economicItem is Grindstone grindstone) 
            {
                UseGrindstone(grindstone); 
            }
        }

        protected override uint CalculateAppliedDamage(uint damage)
        {
            uint totalDefence = 0;

            if (_equipment.TryGetValue(EquipSlot.Armour, out var armourItem) && armourItem is Armour armour)
            {
                totalDefence += armour.Defence;
            }

            if (_equipment.TryGetValue(EquipSlot.Helmet, out var helmetItem) && helmetItem is Helmet helmet)
            {
                totalDefence += helmet.Defence;
            }

            if (totalDefence > 0)
            {
                damage -= (uint)(damage * (totalDefence / 100f));
            }

            return damage;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine(Name);
            builder.AppendLine($"Health {Health}/{MaxHealth}");
            builder.AppendLine("Loot:");
            var items = Inventory.Items;
            for (int i = 0; i < items.Count; i++) 
            {
                builder.AppendLine($"[{items[i].Name}] : {items[i].Amount}");
            }
            return builder.ToString();
        }


        public void ReduceArmourDurability()
        {
            if (_equipment.TryGetValue(EquipSlot.Armour, out var item) && item is Armour armour)
            {
                armour.ReduceDurability(1);
                Console.WriteLine($"Прочность брони уменьшена: {armour.Durability}/{armour.MaxDurability}");

                if (armour.Durability == 0)
                {
                    Console.WriteLine($"Броня {armour.Name} сломалась!");
                    _equipment.Remove(EquipSlot.Armour);
                }
            }
        }

        private void UseGrindstone(Grindstone grindstone)
        {
            Console.WriteLine("Что вы хотите починить? (1 - Оружие, 2 - Броня)");
            var choice = Console.ReadLine();

            if (choice == "1" && _equipment.TryGetValue(EquipSlot.Weapon, out var weaponItem))
            {
                weaponItem.Repair(grindstone.RepairPower);
                Console.WriteLine($"Оружие отремонтировано: {weaponItem.Durability}/{weaponItem.MaxDurability}");
            }
            else if (choice == "2" && _equipment.TryGetValue(EquipSlot.Armour, out var armourItem))
            {
                armourItem.Repair(grindstone.RepairPower);
                Console.WriteLine($"Броня отремонтирована: {armourItem.Durability}/{armourItem.MaxDurability}");
            }
            else
            {
                Console.WriteLine("Нечего чинить или неверный выбор!");
                return;
            }

            Inventory.TryRemove(grindstone);
        }

        public bool TryReplaceEquipment(EquipItem newItem)
        {
            if (_equipment.TryGetValue(newItem.Slot, out var currentItem))
            {
                Console.WriteLine($"Заменить {currentItem.Name} на {newItem.Name}? (y/n)");
                var answer = Console.ReadLine();

                if (answer?.ToLower() != "y")
                    return false;

                
                base.AddItemToInventory(currentItem);
                _equipment.Remove(newItem.Slot);
            }

            
            _equipment[newItem.Slot] = newItem;
            Console.WriteLine($"Экипирован: {newItem.Name}");
            return true;
        }

        public uint GetRangeDamage()
        {
            if (_equipment.TryGetValue(EquipSlot.RangeWeapon, out var item) && item is RangeWeapon rangeWeapon)
            {
                return BaseDamage / 2 + rangeWeapon.Damage; 
            }
            return 0; 
        }


        public void ReduceHelmetDurability()
        {
            if (_equipment.TryGetValue(EquipSlot.Helmet, out var item) && item is Helmet helmet)
            {
                helmet.ReduceDurability(1);
                Console.WriteLine($"Прочность шлема уменьшена: {helmet.Durability}/{helmet.MaxDurability}");

                if (helmet.Durability == 0)
                {
                    Console.WriteLine($"Шлем {helmet.Name} сломался!");
                    _equipment.Remove(EquipSlot.Helmet);
                }
            }
        }

    }
}
