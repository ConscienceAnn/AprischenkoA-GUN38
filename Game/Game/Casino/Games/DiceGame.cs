using System;
using System.Collections.Generic;
using System.Linq;
using Game.Casino.GameEntities;

namespace Game.Casino.Games
{
    public class DiceGame : CasinoGameBas
    {
        private readonly int _diceCount;
        private readonly int _minValue;
        private readonly int _maxValue;


        public DiceGame(int diceCount, int minValue, int maxValue)
        {
            if (diceCount <= 0)
                throw new ArgumentException("Количество костей должно быть больше 0.", nameof(diceCount));
            _diceCount = diceCount;
            _minValue = minValue;
            _maxValue = maxValue;
        }


        protected override void FactoryMethod()
        {

        }

        public override void PlayGame()
        {

            var playerDices = new List<Dice>();
            var dealerDices = new List<Dice>();

            for (int i = 0; i < _diceCount; i++)
            {
                playerDices.Add(new Dice(_minValue, _maxValue));
                dealerDices.Add(new Dice(_minValue, _maxValue));
            }

            var playerResults = playerDices.Select(d => d.Number).ToList();
            var dealerResults = dealerDices.Select(d => d.Number).ToList();

            int playerSum = playerResults.Sum();
            int dealerSum = dealerResults.Sum();

            Console.WriteLine($"Ваши кости: {string.Join(", ", playerResults)}. Сумма: {playerSum}");
            Console.WriteLine($"Кости дилера: {string.Join(", ", dealerResults)}. Сумма: {dealerSum}");

            if (playerSum > dealerSum)
            {
                Console.WriteLine("Вы выиграли!");
                OnWinInvoke(1);
            }
            else if (dealerSum > playerSum)
            {
                Console.WriteLine("Вы проиграли!");
                OnLoseInvoke(1);
            }
            else
            {
                Console.WriteLine("Ничья!");
                OnDrawInvoke();
            }
        }
    }
}
