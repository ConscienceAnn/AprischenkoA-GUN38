

using System.Diagnostics;
using System.Threading.Tasks;

namespace Collections
{
    public class Program
    {
        //Задача 1
        private class Task1
        {
            List<string> _list = new List<string>() {"строка1", "строка2", "строка3"};

            public void TaskLoop()
            {

                Console.WriteLine("Введите строку:");
                _list.Add(Console.ReadLine()); 

                PrintList();

                Console.WriteLine("Введите еще одну строку, для добавления в середину списка:");
                string middle = Console.ReadLine();
                _list.Insert(_list.Count / 2, middle);
                PrintList();

                Console.WriteLine("Для выхода введите '-exit'");
                while (Console.ReadLine() != "-exit") { }

            }
                private void PrintList()
            {
                Console.WriteLine("Текущий список:");
                foreach (var item in _list)
                {
                    Console.WriteLine(item);
                }
            }

        }

        //Задача 2
        private class Task2
        {
            private Dictionary<string, int> students = new Dictionary<string, int>();
           

        public void TaskLoop()
        {
           
            // Добавления студентов:
            Console.WriteLine("Добавление студентов (для завершения введите '-exit'):");
            while (true)
            {
                Console.Write("Введите имя студента: ");
                string name = Console.ReadLine();
            
                if (name == "-exit") 
                    break;
                    
                int grade;
                    while (true)
                    {
                        Console.Write("Введите оценку (2-5): ");
                        string input = Console.ReadLine();

                        if (!int.TryParse(input, out grade))
                        {
                            Console.WriteLine("Ошибка: введите целое число!");
                            continue;
                        }

                        if (grade < 2 || grade > 5)
                        {
                            Console.WriteLine("Ошибка: оценка должна быть от 2 до 5!");
                            continue;
                        }

                        break;
                       
                    }

                    students[name] = grade; //не описано что делать если повторно вводим того же студента, будем считать что исправил свою оценку и просто перезаписывается его результат
                    Console.WriteLine($"Студент {name} с оценкой {grade} добавлен.\n");
                }

            // Поиск студентов:
            Console.WriteLine("\nПоиск студентов (для завершения введите '-exit'):");
            while (true)
            {
                Console.Write("Введите имя студента для поиска: ");
                string name = Console.ReadLine();
            
                if (name == "-exit") 
                    break;

                if (students.TryGetValue(name, out int grade)) //в задаче не указано про регистр...
                {
                    Console.WriteLine($"Студент {name}: оценка {grade}.\n");
                }
                else
                {
                    Console.WriteLine($"Студент {name} не найден.\n");
                }
        }
        }

            }

        

        //Задача 3
        private class Task3
        {

            private class Node
            {
                public string Data;
                public Node Next;
                public Node Previous;
            }

            private Node _head;
            private Node _tail;

            public void TaskLoop()
            {
                Console.WriteLine("Введите от 3 до 6 элементов списка (каждый с новой строки):");
                int count = 0;
                bool exit = false;

                while (count < 6 && !exit)
                {
                    string input = Console.ReadLine();

                    if (input == "-exit")
                    {
                        if (count < 3)
                        {
                            Console.WriteLine("Нужно ввести минимум 3 элемента!");
                            continue;
                        }
                        exit = true;
                        break;
                    }

                    var newNode = new Node { Data = input };

                    if (_head == null)
                    {
                        _head = newNode;
                        _tail = newNode;
                    }
                    else
                    {
                        _tail.Next = newNode;
                        newNode.Previous = _tail;
                        _tail = newNode;
                    }

                    count++;

                    if (count >= 3 && count < 6)
                    {
                        Console.WriteLine($"Введено {count}/6. Добавить ещё? (Введите элемент или '-exit' для завершения ввода)");
                    }
                }

                Console.WriteLine("Список в прямом порядке:");
                Node current = _head;
                while (current != null)
                {
                    Console.WriteLine(current.Data);
                    current = current.Next;
                }

                Console.WriteLine("Список в обратном порядке:");
                current = _tail;
                while (current != null)
                {
                    Console.WriteLine(current.Data);
                    current = current.Previous;
                }

                Console.WriteLine("Для выхода введите '-exit'");
                while (Console.ReadLine() != "-exit") { }
            }
        }



        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Выберите задачу 1, 2, 3 или 4 для выхода из программы:");
                if (!int.TryParse(Console.ReadLine(), out int taskNumber))
                {
                    Console.WriteLine("Ошибка: Введен некорректный номер задачи");
                    continue;
                }
                switch (taskNumber)
                {
                    case 1:
                        new Task1().TaskLoop();
                        break;
                    case 2:
                        new Task2().TaskLoop();
                        break;
                    case 3:
                        new Task3().TaskLoop();
                        break;
                    case 4: 
                        return;
                    default:
                        Console.WriteLine("Ошибка: Введен некорректный номер задачи");
                        break;
                }
            }
        }
    }
}
