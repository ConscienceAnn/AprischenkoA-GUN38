

namespace Classes
{
    public class Unit
    {
        public string Name { get; }
        private float _health; //а не нужно устанавливать какое то значение? в таком формате будет по умолчанию 0, и тогда пациент скорее мертв.. а на старте то должен быть полон сил...


        public Unit(string name)
        {
            Name = name;
        }

        public float Health => _health;

        public int Damage { get; } = 5;
        public float Armor { get; } = 0.6f;

        public Unit() : this("Unknown Unit") { }

        public float GetRealHealth()
        {
            return Health * (1f + Armor);
        }

        public bool SetDamage(float value) 
        {
            if (value < 0f) //вроде не требуется, но есть смысл добавить проверку значения value
            {
                return false; //не поняла только что возращать, вроде так игнорируется если будет отрицательное значение
            }
            else
            {
                _health -= value * Armor;
                return _health <= 0f;
            }
        }
    }
}
