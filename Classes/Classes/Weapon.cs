

namespace Classes
{
    public class Weapon
    {
        public string Name { get; }
        public int MinDamage { get; private set; }

        public int MaxDamage { get; private set; }
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
            return (MinDamage + MaxDamage) / 2;  
        }

        public void SetDamageParams(int minDamage, int maxDamage)
        {
            // Проверка 1: min > max → меняем местами
            if (minDamage > maxDamage)
            {
                (minDamage, maxDamage) = (maxDamage, minDamage); 
                Console.WriteLine($"Некорректные входные данные для {Name}, минимальное значение больше максимального!");
            }

            // Проверка 2: min < 1 → ставим 1
            if (minDamage < 1)
            {
                minDamage = 1;
                Console.WriteLine($"Минимальный урон {Name} = 1.");
            }

            // Проверка 3: max <= 1 → ставим 10
            if (maxDamage <= 1)
            {
                maxDamage = 10;
                Console.WriteLine($"Максимальный урон {Name} = 10.");
            }

            // Устанавливаем значения
            MinDamage = minDamage;
            MaxDamage = maxDamage;
        }

    }
}
