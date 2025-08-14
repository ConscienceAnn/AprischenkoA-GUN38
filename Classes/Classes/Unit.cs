

namespace Classes
{
    public class Unit
    {
        public string Name { get; }
        private float _health = 100f;
        public float Armor { get; }
        public Interval Damage { get; } = new Interval(0, 10);

        public Unit(string name)
        {
            Name = name;
            Armor = 0.6f;
        }

        public Unit(string name, int minDamage, int maxDamage) : this(name)
        {
            Damage = new Interval(minDamage, maxDamage);
        }


        public float Health => _health;


        

        public float GetRealHealth()
        {
            return Health * (1f + Armor);
        }

        public bool SetDamage(float value) 
        {
            if (value <= 0f) 
            {
                return false; 
            }
            
            if (_health <= 0f)
            {
                return true;
            }
            
            _health -= value * Armor;
            return _health <= 0f;
            
        }
    }
}
