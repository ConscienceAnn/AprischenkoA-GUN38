using System;
using System.Collections.Generic;
using Game.Casino.GameEntities;

namespace Game.Casino.Games
{
    public class BlackjackGame : CasinoGameBas
    {
        private readonly int _numberOfCards;
        private Queue<Card> _deck;
        private List<Card> _playerHand;
        private List<Card> _dealerHand;

        public BlackjackGame(int numberOfCards)
        {
            if (numberOfCards <= 0)
                throw new ArgumentException("Количество карт должно быть больше 0.", nameof(numberOfCards));
            if (numberOfCards % 4 != 0)
                throw new ArgumentException("Количество карт должно быть кратно 4 для создания колоды.");
            _numberOfCards = numberOfCards;
        }
        protected override void FactoryMethod()
        {
            CreateDeck();
        }

        private void CreateDeck()
        {
            var cards = new List<Card>();
            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            {
                foreach (Rank rank in Enum.GetValues(typeof(Rank)))
                {
                    cards.Add(new Card(suit, rank));
                    if (cards.Count >= _numberOfCards) break;
                }
                if (cards.Count >= _numberOfCards) break;
            }
            Shuffle(cards);
            _deck = new Queue<Card>(cards);
        }


        private void Shuffle(List<Card> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                Card value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        private int CalculateScore(List<Card> hand)
        {
            int score = 0;
            int acesCount = 0;

            foreach (var card in hand)
            {
                if (card.Rank == Rank.Ace)
                {
                    acesCount++;
                    score += 11;
                }
                else if (card.Rank >= Rank.Jack)
                {
                    score += 10;
                }
                else
                {
                    score += (int)card.Rank;
                }
            }

            while (score > 21 && acesCount > 0)
            {
                score -= 10;
                acesCount--;
            }
            return score;
        }

        public override void PlayGame()
        {
            if (_deck == null || _deck.Count < 4)
            {
                CreateDeck();
            }

            _playerHand = new List<Card> { _deck.Dequeue(), _deck.Dequeue() };
            _dealerHand = new List<Card> { _deck.Dequeue(), _deck.Dequeue() };

            int playerScore = CalculateScore(_playerHand);
            int dealerScore = CalculateScore(_dealerHand);

            Console.WriteLine($"Ваши карты: {string.Join(", ", _playerHand)}");
            Console.WriteLine($"Ваши очки: {playerScore}");
            Console.WriteLine($"Карты дилера: {string.Join(", ", _dealerHand)}");
            Console.WriteLine($"Очки дилера: {dealerScore}");

          
            if (playerScore > 21 && dealerScore > 21)
            {
                Console.WriteLine("Ничья! Оба перебрали.");
                OnDrawInvoke();
            }
            else if (playerScore > 21)
            {
                Console.WriteLine("Вы проиграли! Перебор.");
                OnLoseInvoke(1);
            }
            else if (dealerScore > 21)
            {
                Console.WriteLine("Вы выиграли! Дилер перебрал.");
                OnWinInvoke(1);
            }
            else if (playerScore == dealerScore)
            {
                Console.WriteLine("Ничья! Достаем по дополнительной карте...");

                if (_deck.Count < 2)
                {
                    Console.WriteLine("Не хватает карт для дополнительного раунда.");
                    OnDrawInvoke();
                    return;
                }

                _playerHand.Add(_deck.Dequeue());
                _dealerHand.Add(_deck.Dequeue());

                playerScore = CalculateScore(_playerHand);
                dealerScore = CalculateScore(_dealerHand);

                Console.WriteLine($"Ваша дополнительная карта: {_playerHand[2]}");
                Console.WriteLine($"Дополнительная карта дилера: {_dealerHand[2]}");
                Console.WriteLine($"Новые очки: Вы - {playerScore}, Дилер - {dealerScore}");

             
                if (playerScore > 21 && dealerScore > 21)
                {
                    Console.WriteLine("Ничья после дополнительной карты!");
                    OnDrawInvoke();
                }
                else if (playerScore > 21)
                {
                    Console.WriteLine("Вы проиграли после дополнительной карты!");
                    OnLoseInvoke(1);
                }
                else if (dealerScore > 21)
                {
                    Console.WriteLine("Вы выиграли после дополнительной карты!");
                    OnWinInvoke(1);
                }
                else if (playerScore == dealerScore)
                {
                    Console.WriteLine("Ничья после дополнительной карты!");
                    OnDrawInvoke();
                }
                else if (playerScore > dealerScore)
                {
                    Console.WriteLine("Вы выиграли после дополнительной карты!");
                    OnWinInvoke(1);
                }
                else
                {
                    Console.WriteLine("Вы проиграли после дополнительной карты!");
                    OnLoseInvoke(1);
                }
            }
            else if (playerScore > dealerScore)
            {
                Console.WriteLine("Вы выиграли!");
                OnWinInvoke(1);
            }
            else
            {
                Console.WriteLine("Вы проиграли!");
                OnLoseInvoke(1);
            }
        }
    }
}
