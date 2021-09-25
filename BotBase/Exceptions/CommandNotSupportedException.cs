namespace BotBase.Exceptions
{
    public class CommandNotSupportedException : BotException
    {
        public override string Message => "this command does not exist";
    }
}
