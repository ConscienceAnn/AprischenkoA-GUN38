//ЗАДАНИЕ 1 (Числа Фибоначчи, вывести первые 10)

Console.WriteLine("Первые 10 чисел Фибоначчи:");
int a = 0;
int b = 1;
Console.Write(a + ", " + b);

for (int i = 2; i < 10; i++)
{
    int next = a + b;
    Console.Write(", " + next);
    a = b;
    b = next;
}

Console.WriteLine();
Console.WriteLine(); //пустые строки, чтобы в консоли разделить визуально вывод результатов задач



//ЗАДАНИЕ 2 (Цикл for, вывести все четные числа <=20)

Console.WriteLine("Четные числа от 2 до 20 (включительно):");

for (int i = 2; i <= 20; i += 2)
{
   if(i % 2 == 0)
    {
        Console.Write(i);
        if (i < 20) Console.Write(", ");
    }
   
}

Console.WriteLine();
Console.WriteLine(); //пустые строки, чтобы в консоли разделить визуально вывод результатов задач

// ЗАДАНИЕ 3 (Таблица умножения от 1 до 5)
Console.WriteLine("Таблица умножения от 1 до 5: ");

for (int i = 1; i <= 5; i++)
{
    for (int j = 1; j <= 5; j++)
    {
        Console.Write($"{i} * {j} = {i * j,2}   "); //про 2 нагуглила, очень смущал вид
    }
    Console.WriteLine();
}

Console.WriteLine();
Console.WriteLine(); //пустые строки, чтобы в консоли разделить визуально вывод результатов задач


//ЗАДАНИЕ 4 (Ввод пароля)

string password = "qwerty";
string input;
do
{
    Console.Write("Введите пароль: ");
    input = Console.ReadLine();
} while (input != password);
Console.WriteLine("Введен правильный пароль.");
