
using Game.Casino;

namespace Game
{
    internal class Program
    {
        static void Main(string[] args)
        {


            string saveDirectoryPath = "Saves";
            var saveLoadService = new FileSystemSaveLoadService<PlayerProfile>(saveDirectoryPath);


            IGame casino = new Game.Casino.Casino(saveLoadService);

            casino.StartGame();


        }
    }
}
