namespace BotBase.Exceptions
{
    public class NotAdminException : BotException
    {
        public override string Message => "you are not admin";
    }

}
