using BotBase.Exceptions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace BotBase.Utils
{
    public static class ReflectionTool
    {
        public static object ExecuteMethod(object target, MethodInfo commandBody, IList<string> args)
        {
            ParameterInfo[] methodParameters = commandBody.GetParameters();
            object[] parameters = null;
            if (methodParameters.Length > 0)
            {
                parameters = new object[methodParameters.Length];
                int argsIndex = 0;

                Type lastType = null;
                string lastValue = null;

                for (int j = 0; j < parameters.Length; j++)
                {
                    ParameterInfo parameter = methodParameters[j];
                    try
                    {
                        Type parameterType = parameter.ParameterType;

                        if (parameter.IsParamArray())
                        {
                            Type elementType = lastType = parameter.ParameterType.GetElementType();
                            Array paramsValue = Array.CreateInstance(elementType, args.Count - argsIndex);
                            for (int i = 0; i < paramsValue.Length; i++)
                            {
                                object value = ConvertValue(lastValue = args[argsIndex + i], elementType);
                                paramsValue.SetValue(value, i);
                            }
                            parameters[j] = paramsValue;
                        }
                        else
                        {
                            string value = lastValue = args[argsIndex++];
                            lastType = parameterType;
                            parameters[j] = ConvertValue(value, parameterType);
                        }
                    }
                    catch
                    {
                        throw new InvalidParameterException(parameter.Name, lastValue, lastType);
                    }
                }
            }
            return commandBody.Invoke(target, parameters);
        }

        public static object ConvertValue(string value, Type resultType)
        {
            if (resultType == typeof(string))
            {
                return value;
            }
            else if (resultType.IsEnum)
            {
                return Enum.Parse(resultType, value, true);
            }
            else if (resultType.IsPrimitive)
            {
                return Convert.ChangeType(value, resultType);
            }
            else
            {
                MethodInfo parseMethod = resultType.GetMethod("Parse", new Type[] { typeof(string), typeof(IFormatProvider) });
                if (parseMethod != null)
                {
                    return parseMethod.Invoke(null, new object[] { value, CultureInfo.InvariantCulture });
                }
                else
                {
                    parseMethod = resultType.GetMethod("Parse", new Type[] { typeof(string) });
                    if (parseMethod != null)
                    {
                        return parseMethod.Invoke(null, new object[] { value });
                    }
                    else
                    {
                        return JsonConvert.DeserializeObject(value, resultType);
                    }
                }
            }
        }

        public static bool IsParamArray(this ParameterInfo parameterInfo) => parameterInfo.IsDefined(typeof(ParamArrayAttribute));

        public static bool HasParamsArray(this ParameterInfo[] pa) => pa.Length > 0 && pa[^1].IsParamArray();
    }
}
