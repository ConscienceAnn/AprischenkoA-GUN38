//ЗАДАЧА A: создать 4 массива внутри метода Main

//ЗАДАНИЕ 1 (Числа Фибоначчи)
    int[] fibonacci = new int[8];
    fibonacci[0] = 0;
    fibonacci[1] = 1;
    for (int i = 2; i < fibonacci.Length; i++)
    {
        fibonacci[i] = fibonacci[i - 1] + fibonacci[i - 2];
    }

        //Вывод для проверки
        //Console.WriteLine("Массив 8 первых чисел Фибоначчи:");
        //for (int i = 0; i < fibonacci.Length; i++)
        //{
        //    Console.WriteLine($"fibonacci[{i}] = {fibonacci[i]}");
        //}

//ЗАДАНИЕ 2 (массив 12 месяцев)
string[] months = new string[] {"January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"};

        //Вывод для проверки
        //foreach (var month in months)
        //{
        //    Console.WriteLine(month);
        //}

//ЗАДАНИЕ 3 (Создайте двумерный массив (матрицу) 3x3)

//Версия 3.1 Самостоятельная (долгая)
    int[,] array = new int[3, 3];
    // Первая строка: в степени 1, без изменений
    array[0, 0] = 2;  
    array[0, 1] = 3;  
    array[0, 2] = 4; 

    // Вторая строка: числа в квадрате
    array[1, 0] = 2 * 2;  
    array[1, 1] = 3 * 3;  
    array[1, 2] = 4 * 4;  

    // Третья строка: числа в кубе
    array[2, 0] = 2 * 2 * 2;  
    array[2, 1] = 3 * 3 * 3;  
    array[2, 2] = 4 * 4 * 4;

        //Вывод для проверки
        //for (int i = 0; i < 3; i++)
        //        {
        //            for (int j = 0; j < 3; j++)
        //            {
        //                Console.Write(array[i, j] + "\t");
        //            }
        //            Console.WriteLine(); 
        //        }



//Версия 3.2 Версия лаконичная, проверка навыка гуглить
    int[,] array2 = new int[3, 3];
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                array2[i, j] = (int)Math.Pow(j + 2, i + 1);
            }
        }

        //Вывод для проверки
        //for (int i = 0; i < 3; i++)
        //{
        //    for (int j = 0; j < 3; j++)
        //    {
        //        Console.Write(array2[i, j] + "\t");
        //    }
        //    Console.WriteLine();
        //}

//ЗАДАНИЕ 4 (Ломанный массив)
    double[][] jaggedArray = new double[3][];
    jaggedArray[0] = new double[] { 1, 2, 3, 4, 5 };
    jaggedArray[1] = new double[] { Math.E, Math.PI };
    jaggedArray[2] = new double[] {
                Math.Log10(1),
                Math.Log10(10),
                Math.Log10(100),
                Math.Log10(1000)
            };


        //Вывод для проверки
        //Console.WriteLine("Jagged Array:");
        //foreach (var a in jaggedArray)
        //{
        //    Console.WriteLine(string.Join(", ", a));
        //}




//ЗАДАЧА Б: дано два массива
    int[] arrayA = { 1, 2, 3, 4, 5 }; //переименовала т.к. уже используется
    int[] arrayB = { 7, 8, 9, 10, 11, 12, 13 }; //переименовала т.к. уже используется

//ЗАДАНИЕ 5 (Скопируйте первые 3 элемента первого массива во второй. Воспользуйтесь классом Array.)
Array.Copy(arrayA, arrayB, 3);
Console.WriteLine("arrayB после копирования: " + string.Join(", ", arrayB));

// ЗАДАНИЕ 6 (Измените размер первого массива, в 2 раза больше элементов)
Array.Resize(ref arrayA, arrayA.Length * 2);
Console.WriteLine("arrayA после Resize: " + string.Join(", ", arrayA));
