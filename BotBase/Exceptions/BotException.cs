using System;

namespace BotBase.Exceptions
{
    public class BotException : ApplicationException
    {
        public BotException()
        { }

        public BotException(string message) : base(message)
        { }
    }
}
