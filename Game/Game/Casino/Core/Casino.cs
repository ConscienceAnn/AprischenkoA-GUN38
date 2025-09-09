using System;
using System.Text.Json;
using Game.Casino.Games;


namespace Game.Casino
{
        public class Casino : IGame
        {
        private readonly PlayerProfile _player;
        private readonly ISaveLoadService<PlayerProfile> _saveLoadService;
        private readonly BlackjackGame _blackjackGame;
        private readonly DiceGame _diceGame;
        private const int MaxBankValue = int.MaxValue;
        private int _currentBet;

        public Casino(ISaveLoadService<PlayerProfile> saveLoadService)
        {
            _saveLoadService = saveLoadService ?? throw new ArgumentNullException(nameof(saveLoadService));
            _player = LoadPlayerProfile() ?? new PlayerProfile { PlayerName = "Player", Bank = 100 };

            _blackjackGame = new BlackjackGame(36);
            _diceGame = new DiceGame(2, 1, 6);

            _blackjackGame.OnWin += OnGameWin;
            _blackjackGame.OnLose += OnGameLose;
            _blackjackGame.OnDraw += OnGameDraw;

            _diceGame.OnWin += OnGameWin;
            _diceGame.OnLose += OnGameLose;
            _diceGame.OnDraw += OnGameDraw;
        }

        private PlayerProfile LoadPlayerProfile()
        {
            try
            {
                var loaded = _saveLoadService.LoadData("player_profile");
                if (loaded != null && !string.IsNullOrWhiteSpace(loaded.PlayerName))
                {
                    Console.WriteLine($"С возвращением, {loaded.PlayerName}!");
                    return loaded;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки профиля: {ex.Message}");
            }

            Console.Write("Приветствую! Введите ваше имя: ");
            string playerName = Console.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(playerName))
                playerName = "Игрок";

            return new PlayerProfile { PlayerName = playerName, Bank = 100 };
        }


        private void SavePlayerProfile()
        {
            try
            {
                _saveLoadService.SaveData(_player, "player_profile");
                Console.WriteLine("Профиль успешно сохранён.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка сохранения профиля: {ex.Message}");
            }
        }


        private void OnGameWin(int signal)
        {

            int winAmount = _currentBet * 2;
            _player.Bank += winAmount;
            Console.WriteLine($"Поздравляем! Вы выиграли ${winAmount}. Ваш банк: ${_player.Bank}");
            CheckBankLimit();
        }

        private void OnGameLose(int signal)
        {
            Console.WriteLine($"Вы проиграли ставку. Ваш банк: ${_player.Bank}");
        }

        private void OnGameDraw()
        {
            _player.Bank += _currentBet;
            Console.WriteLine($"Ничья! Ставка возвращена. Ваш банк: ${_player.Bank}");
        }

        private void CheckBankLimit()
        {
            if (_player.Bank > MaxBankValue)
            {
                int oldBank = _player.Bank;
                _player.Bank = MaxBankValue;
                int lostMoney = oldBank - MaxBankValue;
                Console.WriteLine($"Вы разорили казино! Ваш выигрыш ${lostMoney} пошел на строительство нового.");
                Console.WriteLine($"Ваш банк теперь: ${_player.Bank}");
            }
        }

        public void StartGame()
        {
            Console.WriteLine($"Добро пожаловать в казино, {_player.PlayerName}!");
            bool continuePlaying = true;

            while (continuePlaying && _player.Bank > 0)
            {
                Console.WriteLine($"\nВаш текущий банк: ${_player.Bank}");
                Console.WriteLine("Выберите игру:");
                Console.WriteLine("1 - Блэкджек (21)");
                Console.WriteLine("2 - Кости");
                Console.WriteLine("3 - Выход");
                Console.Write("Ваш выбор: ");

                string choice = Console.ReadLine();
                CasinoGameBas chosenGame = null;



                switch (choice)
                {
                    case "1": 
                        chosenGame = _blackjackGame; 
                        break;
                    case "2": 
                        chosenGame = _diceGame; 
                        break;
                    case "3":
                        continuePlaying = false;
                        Console.WriteLine("Спасибо за игру! До свидания!");
                        break;
                    default:
                        Console.WriteLine("Неверный выбор. Попробуйте снова.");
                        continue;
                }

                if (chosenGame != null)
                {
                    Console.Write($"Сделайте ставку (максимум ${_player.Bank}): ");
                    if (int.TryParse(Console.ReadLine(), out int bet) && bet > 0 && bet <= _player.Bank)
                    {
                        _currentBet = bet;
                        _player.Bank -= bet;
                        Console.WriteLine($"Ставка ${bet} принята. Удачи!\n");

                        chosenGame.PlayGame();
                    }
                    else
                    {
                        Console.WriteLine("Неверная ставка.");
                        continue;
                    }

                      
                }
                
            }

            if (_player.Bank <= 0)
            {
                Console.WriteLine("“No money? Kicked!”");
            }
            


            SavePlayerProfile();

        }
    }
}
