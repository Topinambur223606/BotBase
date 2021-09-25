namespace BotBase.Exceptions
{
    public class ReplyOnlyCommandException : BotException
    {
        public override string Message => "reply to message to use this command";
    }
}
