namespace BotBase.Exceptions
{
    public class InvalidParameterCountException : BotException
    {
        public override string Message => "wrong parameter count";
    }
}
