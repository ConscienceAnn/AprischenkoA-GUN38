using System;


namespace Game.Casino.GameEntities
{
    public readonly struct Dice
    {
        private static readonly Random _random = new Random();

        public int Number { get; } 

        public Dice(int min, int max)
        {
           
            if (min < 1)
                throw new WrongDiceNumberException(
                    $"Минимальное значение ({min}) не может быть меньше 1. Допустимый диапазон: 1 - {int.MaxValue}.");
            if (max > int.MaxValue)
                throw new WrongDiceNumberException(
                    $"Максимальное значение ({max}) превышает допустимый максимум ({int.MaxValue}).");

            Number = _random.Next(min, max + 1);
        }
    }
}
