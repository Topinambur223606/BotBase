using BotBase.Commands.Abstract;
using BotBase.Commands.Attributes;
using BotBase.Exceptions;
using BotBase.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BotBase.Commands
{
    public abstract class CommandList<T> : ICommandList where T : CommandList<T>
    {
        public abstract string CommandPrefix { get; }

        public string ConstructHelp()
        {
            IEnumerable<FieldInfo> commandEntries =
                typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static).Where(c => c.IsDefined(typeof(CommandEntryAttribute)));

            StringBuilder sb = new();
            foreach (FieldInfo command in commandEntries.Select(e => (e, a: e.GetCustomAttribute<CommandEntryAttribute>()))
                                                        .OrderBy(t => t.a.Group).ThenBy(t => t.a.Number).Select(t => t.e))
            {
                ConstructHelp(sb, command);
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public string ConstructHelp(string command)
        {
            FieldInfo commandEntry =
                typeof(T)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(c => c.IsDefined(typeof(CommandEntryAttribute)) &&
                                     command.Equals(c.GetValue(null).ToString(), StringComparison.InvariantCultureIgnoreCase));

            if (commandEntry == null)
            {
                throw new CommandNotSupportedException();
            }

            StringBuilder sb = new();
            ConstructHelp(sb, commandEntry);
            return sb.ToString();
        }

        private static void ConstructHelp(StringBuilder sb, FieldInfo command)
        {
            string commandSpelling = command.GetValue(null).ToString().MdEscape();
            CommandEntryAttribute cmdEntry = command.GetCustomAttribute<CommandEntryAttribute>();
            sb.AppendLine($"`{commandSpelling}`");
            sb.AppendLine(cmdEntry.Description.MdEscape());
            if (cmdEntry.Parameters != null)
            {
                sb.AppendLine($"*Parameters:* {cmdEntry.Parameters.MdEscape()}");
            }
        }

        public bool IsSupported(string command)
        {
            return typeof(T)
                    .GetFields(BindingFlags.Public | BindingFlags.Static)
                    .Where(c => c.GetCustomAttribute<CommandEntryAttribute>()?.IsNotCommand != true)
                    .Any(f => command.Equals(f.GetValue(null) as string, StringComparison.InvariantCultureIgnoreCase));
        }

        private static readonly Rune escapeRune = new('\\');
        private static readonly Rune quoteRune = new('"');

        public List<string> ParseArguments(string commandLine)
        {
            List<string> result = new();

            StringBuilder argumentBuilder = new();
            bool openQutes = false;
            bool escaped = false;
            bool constructing = false;

            void StartConstructing()
            {
                if (constructing)
                {
                    throw new InvalidCommandLineException();
                }
                argumentBuilder.Clear();
                constructing = true;
            }

            void FinishConstructing()
            {
                if (!constructing)
                {
                    return;
                }
                result.Add(argumentBuilder.ToString());
                constructing = false;
            }

            void Append(Rune r)
            {
                if (!constructing)
                {
                    StartConstructing();
                }
                argumentBuilder.Append(r);
            }

            foreach (Rune r in commandLine.EnumerateRunes())
            {
                if (r == escapeRune)
                {
                    if (escaped)
                    {
                        Append(r);
                    }
                    escaped = !escaped;
                    continue;
                }

                if (r == quoteRune)
                {
                    if (escaped)
                    {
                        Append(r);
                        escaped = false;
                    }
                    else
                    {
                        if (openQutes)
                        {
                            FinishConstructing();
                        }
                        else
                        {
                            StartConstructing();
                        }
                        openQutes = !openQutes;
                    }
                    continue;
                }

                if (escaped)
                {
                    throw new InvalidCommandLineException();
                }

                if (Rune.IsWhiteSpace(r))
                {
                    if (openQutes)
                    {
                        Append(r);
                    }
                    else
                    {
                        FinishConstructing();
                    }
                    continue;
                }

                Append(r);
            }

            if (openQutes)
            {
                throw new InvalidCommandLineException();
            }

            FinishConstructing();

            return result;
        }
    }
}
