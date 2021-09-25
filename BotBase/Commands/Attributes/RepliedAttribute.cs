using System;

namespace BotBase.Commands.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class RepliedAttribute : Attribute
    { }
}
