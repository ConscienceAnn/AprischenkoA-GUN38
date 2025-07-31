
Console.Write("Enter first number: "); 
if (!Int32.TryParse(Console.ReadLine(), out var a))
{
    Console.WriteLine("Not a number! Try again.");
    return;
}

Console.Write("Enter second number: ");
if (!Int32.TryParse(Console.ReadLine(), out var b))
{
    Console.WriteLine("Not a number! Try again.");
    return;
}

Console.Write("Enter one of the sign &, | or ^ : ");
var s = Console.ReadLine();
var result = 0;
if (s.Length != 1 || (s[0] != '&' && s[0] != '|' && s[0] != '^'))
{
    Console.WriteLine("Wrong sign! Try again.");
    return;
}

switch (s[0])
{
    case '&':
        result = a & b;
        break;
    case '|':
        result = a | b;
        break;
    case '^':
        result = a ^ b;
        break;
    default:
        Console.WriteLine("Wrong sign!"); // в операторе if исключила все кроме правильного, и по логике сюда не попаду.. но оставила
        break;
}

Console.WriteLine("In decimal system: " +  result);
Console.WriteLine("In hexadecimal system: 0x" + result.ToString("X"));
Console.WriteLine("In binary system: " + Convert.ToString(result, 2)); //все нули перед сознательно исключила
