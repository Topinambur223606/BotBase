using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotBase.Commands.Abstract
{
    public interface ICommandImplementations
    {
        MethodInfo GetCommandBody(string command, int argsCount, bool hasRepliedMessage, ChatType chatType, bool isAdmin);

        object ExecuteCommandBody(MethodInfo commandBody, List<string> args, Message message);

        Task<string> GetStringFromResult(object commandResult);

        Task HandleButtonCallback(CallbackQuery callbackQuery);

        Task HandleInlineQuery(InlineQuery inlineQuery);
    }
}
