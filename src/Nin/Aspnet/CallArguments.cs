using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Nin.Aspnet
{
    public class CallArguments
    {
        private readonly string[] arguments = new string[0];

        public CallArguments(string requestPath)
        {
            requestPath = requestPath.Trim('/');
            if (requestPath.Length > 0)
                arguments = requestPath.Split('/');
        }

        public object[] MapArgs(MethodInfo targetMethod, HttpRequest request)
        {
            var parms = targetMethod.GetParameters();
            var args = MapArguments(parms, arguments);

            if (request.Method == "POST" && parms.Length > 0)
                args[0] = JsonSerializer.Create().Deserialize(
                    new StreamReader(request.Body), parms[0].ParameterType);
            return args;
        }

        private static object[] MapArguments(ParameterInfo[] parms, string[] parts)
        {
            object[] p = new object[parms.Length];
            for (int i = 0; i < parms.Length; i++)
            {
                object r;
                if (i < parts.Length)
                    r = Convert(parts[i], parms[i].ParameterType);
                else
                    r = CreateDefaultParameter(parms[i]);
                p[i] = r;
            }
            return p;
        }

        private static object CreateDefaultParameter(ParameterInfo parameterInfo)
        {
            if (parameterInfo.IsOptional)
                return null;
            if (parameterInfo.ParameterType == typeof(string)) return null;
            return Activator.CreateInstance(parameterInfo.ParameterType);
        }

        private static object Convert(string value, Type targetType)
        {
            switch (targetType.Name)
            {
                case "String":
                    return value;
                case "Int32":
                    return int.Parse(value, CultureInfo.InvariantCulture);
                default:
                    throw new Exception("Unkown parameter type '" + targetType.Name + "'.");
            }
        }
    }
}