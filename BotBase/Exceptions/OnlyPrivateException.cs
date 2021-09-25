namespace BotBase.Exceptions
{
    public class WrongChatTypeException : BotException
    {
        public override string Message => "this command cannot be used in current chat";
    }
}
