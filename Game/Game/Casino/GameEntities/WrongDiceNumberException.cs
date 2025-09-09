using System;

namespace Game.Casino.GameEntities
{
    public class WrongDiceNumberException : Exception
    {
        public WrongDiceNumberException(string message) : base(message) { }
        public WrongDiceNumberException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
