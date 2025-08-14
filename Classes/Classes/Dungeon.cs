namespace Classes
{
    public class Dungeon
    {
        private Room[] _rooms;

        public Dungeon()
        {
            // Инициализация массива комнат (3-5 элементов)
            _rooms = new Room[]
            {
            new Room(new Unit("Человек"), new Weapon("Меч", 15, 25)),
            new Room(new Unit("Эльф"), new Weapon("Лук", 12, 18)),
            new Room(new Unit("Дварф"), new Weapon("Топор", 10, 20)),
            new Room(new Unit("Хафлинг"), new Weapon("Камень", 8, 12))
            };
        }

        public void ShowRooms()
        {
            for (int i = 0; i < _rooms.Length; i++)
            {
                var room = _rooms[i];
                Console.WriteLine($"Комната {i + 1}:");
                Console.WriteLine($"Юнит: {room.Unit.Name}, Здоровье: {room.Unit.Health}");
                Console.WriteLine($"Оружие: {room.Weapon.Name}, Урон: {room.Weapon.Damage.Min}-{room.Weapon.Damage.Max}");
                Console.WriteLine("---");
            }
        }


    }
}
