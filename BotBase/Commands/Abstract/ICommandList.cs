using System.Collections.Generic;

namespace BotBase.Commands.Abstract
{
    public interface ICommandList
    {
        string ConstructHelp();

        string ConstructHelp(string command);

        bool IsSupported(string command);

        List<string> ParseArguments(string commandLine);

        string CommandPrefix { get; }
    }
}
