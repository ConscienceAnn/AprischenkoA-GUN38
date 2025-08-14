

namespace Classes
{
    public class Weapon
    {
        public string Name { get; }
        public Interval Damage { get; private set; }
        public float Durability { get; } 

        public Weapon(string name)
        {
            Name = name;
            Durability = 1f;
        }

        public Weapon(string name, int minDamage, int maxDamage) : this(name)  
        {
            SetDamageParams(minDamage, maxDamage);  
        }

        public int GetDamage()
        {
            return Damage.Get();  
        }

        public void SetDamageParams(int minDamage, int maxDamage)
        {
            Damage = new Interval(minDamage, maxDamage);
        }

    }
}
