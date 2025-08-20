using System.Text;

namespace Strings
{
    internal class Program
    {

        //Задача 1: Конкатенация строк
        public static string ConcatenateStrings(string firstString, string secondString)
        {
            return firstString + secondString;
        }

        //Задача 2: Приветствие пользователя 

        public static string GreetUser(string name, int age)
        {
            return $"Hello, {name}!\nYou are {age} years old.";
        }

        //Задача 3: Информация о строке
        public static string StringInfo (string inputString)
        {
            int length = inputString.Length;
            string upperCase = inputString.ToUpper();
            string lowerCase = inputString.ToLower();

            return $"Длина строки: {length} символов\nВ верхнем регистре: {upperCase}\nВ нижнем регистре: {lowerCase}";
        }

        //Задача 4: Первые 5 символов
        public static string FirstFiveCharacters(string inputString)
        {
            
            if (string.IsNullOrEmpty(inputString))
            {
                return "";
            }
            
            if (inputString.Length <= 5)
            {
                return inputString; 
            }
            else
            {
                return inputString.Substring(0, 5);
            }
        }

        // Задание 5: Объединение массива строк в StringBuilder
        public static StringBuilder ConcatenateArrayToStringBuilder(string[] stringArray)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Join(" ", stringArray)); 
            return sb;
        }

        //Задание 6: ReplaceWords
        public static string ReplaceWords(string inputString, string wordToReplace, string replacementWord)
        {
            return inputString.Replace(wordToReplace, replacementWord);
        }

        static void Main(string[] args)
        {
            // Тестирование метода 1
            Console.WriteLine("1. Конкатенация строк:");
            string result1 = ConcatenateStrings("Hello, ", "World!");
            Console.WriteLine($"Результат: {result1}");

            // Тестирование метода 2
            Console.WriteLine("\n2. Приветствие пользователя:");
            string result2 = GreetUser("Иван", 30);
            Console.WriteLine(result2);

            // Тестирование метода 3
            Console.WriteLine("\n3. Анализ строки:");
            string result3 = StringInfo("Hello World!");
            Console.WriteLine(result3);

            // Тестирование метода 4
            Console.WriteLine("\n4. Первые 5 символов:");
            string result4 = FirstFiveCharacters("Hello World!");
            Console.WriteLine($"Результат: '{result4}'");

            // Тестирование метода 5
            Console.WriteLine("\n5. Объединение массива строк:");
            string[] words = { "Массив", "из", "четырех", "слов" };
            StringBuilder result5 = ConcatenateArrayToStringBuilder(words);
            Console.WriteLine($"Результат: '{result5}'");

            // Тестирование метода 6
            Console.WriteLine("\n6. Замена слов:");
            string result6 = ReplaceWords("Hello, world!", "world", "teacher");
            Console.WriteLine($"Результат: {result6}");
        }
    }
}
