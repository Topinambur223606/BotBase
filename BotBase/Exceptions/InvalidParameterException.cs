using System;

namespace BotBase.Exceptions
{
    public class InvalidParameterException : BotException
    {
        public InvalidParameterException(string paramName, string paramValue, Type expectedType)
            : base($"invalid parameter \"{paramName}\": expected type \"{expectedType.Name}\", got value\n{paramValue}")
        { }
    }
}
