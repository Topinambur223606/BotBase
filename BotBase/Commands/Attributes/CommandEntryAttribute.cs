using System;

namespace BotBase.Commands.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class CommandEntryAttribute : Attribute
    {
        public string Description { get; }


        public int Group { get; }

        public int Number { get; }

        public bool IsNotCommand { get; set; }

        public string Parameters { get; set; }

        public CommandEntryAttribute(int group, int number, string description)
        {
            Group = group;
            Number = number;
            Description = description;
        }
    }
}
