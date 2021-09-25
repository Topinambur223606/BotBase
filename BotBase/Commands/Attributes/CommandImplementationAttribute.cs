using System;

namespace BotBase.Commands.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class CommandImplementationAttribute : Attribute
    {
        public string Command { get; }

        public CommandImplementationAttribute(string command)
        {
            Command = command;
        }
    }
}
