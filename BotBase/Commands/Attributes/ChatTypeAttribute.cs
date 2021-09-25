using System;
using Telegram.Bot.Types.Enums;

namespace BotBase.Commands.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ChatTypeAttribute : Attribute
    {
        public ChatTypeAttribute(ChatType chatType)
        {
            ChatType = chatType;
        }

        public ChatType ChatType { get; }
    }
}
