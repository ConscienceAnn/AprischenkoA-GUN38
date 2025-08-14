

namespace Classes
{
    public struct Interval
    {
        private static Random _random = new Random(); 

        public int Min { get; }
        public int Max { get; }

        public Interval(int minValue, int maxValue)
        {
            // Проверка 1: min > max - меняем местами
            if (minValue > maxValue)
            {
                (minValue, maxValue) = (maxValue, minValue);
                Console.WriteLine("Некорректные входные данные,минимальное значение больше максимального!");
            }

            // Проверка 2: отрицательные числа - заменяем на 0
            if (minValue < 0)
            {
                minValue = 0;
                Console.WriteLine("Значение отрицательное, изменено на 0.");
            }
            if (maxValue < 0)
            {
                maxValue = 0;
                Console.WriteLine("Значение отрицательное, изменено на 0.");
            }

            // Проверка 3: min == max → увеличиваем max на 10
            if (minValue == maxValue)
            {
                maxValue += 10;
                Console.WriteLine("min == max. Максимальное значение увеличено на 10.");
            }

            Min = minValue;
            Max = maxValue;
        }

        public int Get()
        {
            return _random.Next(Min, Max + 1);
        }

    }
}
