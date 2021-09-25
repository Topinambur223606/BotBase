using System;

namespace BotBase.Commands.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class AdminAttribute : Attribute
    { }
}
