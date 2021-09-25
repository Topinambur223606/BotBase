using BotBase.Commands.Abstract;
using BotBase.Commands.Attributes;
using BotBase.Exceptions;
using BotBase.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotBase.Commands
{
    public abstract class CommandImplementations<T> : ICommandImplementations where T : CommandImplementations<T>
    {
        public MethodInfo GetCommandBody(string command, int argsCount, bool hasRepliedMessage, ChatType chatType, bool isAdmin)
        {
            bool NeedsReply(ParameterInfo[] parameters) => parameters.Any(p => p.ParameterType == typeof(Message) && p.GetCustomAttribute<RepliedAttribute>() != null);
            bool HasParamsArg(ParameterInfo[] parameters) => parameters.LastOrDefault()?.IsParamArray() ?? false;

            IOrderedEnumerable<MethodInfo> commandOverloads = typeof(T)
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => m.GetCustomAttribute<CommandImplementationAttribute>()?
                                .Command.Equals(command, StringComparison.InvariantCultureIgnoreCase)
                            ?? false)
                .OrderBy(c => c.GetParameters().HasParamsArray()) //params-containing are last checked
                .ThenByDescending(c => NeedsReply(c.GetParameters())); //replied are preferred
            bool needsRepliedMessage = false;
            bool notSupported = true;
            bool notAdmin = false;
            bool wrongChatType = false;

            foreach (MethodInfo overload in commandOverloads)
            {
                notSupported = false;

                ParameterInfo[] parameters = overload.GetParameters();
                bool needsReply = NeedsReply(parameters);
                bool isParams = HasParamsArg(parameters);
                int neededArgsCount = parameters.Length - parameters.Count(p => p.ParameterType == typeof(Message));

                if (needsReply && !hasRepliedMessage)
                {
                    if (argsCount == neededArgsCount)
                    {
                        needsRepliedMessage = true;
                    }
                    continue;
                }

                if (argsCount == neededArgsCount || isParams && argsCount >= neededArgsCount - 1)
                {
                    if (!(isAdmin || overload.GetCustomAttribute<AdminAttribute>() == null))
                    {
                        notAdmin = true;
                        continue;
                    }

                    ChatType? chatTypeRestriction = overload.GetCustomAttribute<ChatTypeAttribute>()?.ChatType;

                    if (chatTypeRestriction.HasValue && chatType != chatTypeRestriction)
                    {
                        wrongChatType = true;
                        continue;
                    }

                    return overload;
                }
            }

            if (wrongChatType)
            {
                throw new WrongChatTypeException();
            }

            if (notAdmin)
            {
                throw new NotAdminException();
            }

            if (needsRepliedMessage)
            {
                throw new ReplyOnlyCommandException();
            }

            if (notSupported)
            {
                throw new CommandNotSupportedException();
            }

            throw new InvalidParameterCountException();
        }

        public object ExecuteCommandBody(MethodInfo commandBody, List<string> args, Message message)
        {
            ParameterInfo[] methodParameters = commandBody.GetParameters();
            object[] parameters = null;
            if (methodParameters.Length > 0)
            {
                parameters = new object[methodParameters.Length];
                int argsIndex = 0;
                for (int j = 0; j < parameters.Length; j++)
                {
                    ParameterInfo parameter = methodParameters[j];
                    Type parameterType = parameter.ParameterType;

                    if (parameterType == typeof(Message))
                    {
                        if (parameter.GetCustomAttribute<RepliedAttribute>() != null)
                        {
                            parameters[j] = message.ReplyToMessage;
                        }
                        else
                        {
                            parameters[j] = message;
                        }
                    }
                    else if (parameter.IsParamArray())
                    {
                        Type elementType = parameter.ParameterType.GetElementType();
                        Array paramsValue = Array.CreateInstance(elementType, args.Count - argsIndex);
                        for (int i = 0; i < paramsValue.Length; i++)
                        {
                            object value = ReflectionTool.ConvertValue(args[argsIndex + i], elementType);
                            paramsValue.SetValue(value, i);
                        }
                        parameters[j] = paramsValue;
                    }
                    else
                    {
                        string value = args[argsIndex++];
                        try
                        {
                            parameters[j] = ReflectionTool.ConvertValue(value, parameterType);
                        }
                        catch
                        {
                            throw new InvalidParameterException(parameter.Name, value, parameterType);
                        }
                    }
                }
            }
            return commandBody.Invoke(this, parameters);
        }



        public async Task<string> GetStringFromResult(object commandResult)
        {
            if (commandResult is Task task)
            {
                Type taskType = commandResult.GetType();
                await task;
                PropertyInfo resultProperty = taskType.GetProperty("Result");
                if (resultProperty.PropertyType.Name == "VoidTaskResult")
                {
                    commandResult = null;
                }
                else
                {
                    commandResult = resultProperty.GetValue(task);
                }
            }

            if (!(commandResult == null || commandResult is string || commandResult.GetType().IsPrimitive))
            {
                commandResult = JsonConvert.SerializeObject(commandResult, Formatting.Indented);
            }

            return commandResult?.ToString();
        }

        public abstract Task HandleButtonCallback(CallbackQuery callbackQuery);
        public abstract Task HandleInlineQuery(InlineQuery inlineQuery);
    }
}
