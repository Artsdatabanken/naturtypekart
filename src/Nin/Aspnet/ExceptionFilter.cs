using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Nin.Diagnostic;

namespace Nin.Aspnet
{
    public class ExceptionFilter : ActionFilterAttribute, IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            Log.e(GetControllerAndActionString(context.RouteData), context.Exception);

            ObjectResult result;
            //if (context.Exception is ItemNotFoundException)
            {
                result = new BadRequestObjectResult(context.Exception.Message);
            }
            //else
            //    result = new serverBadRequestObjectResult(400);
            //result.Value = context.Exception.Message;
            context.Result = result;
        }

        private static string GetControllerAndActionString(RouteData routeData)
        {
            string r = GetValue(routeData, "controller") + "." + GetValue(routeData, "action");
            return r;
        }

        private static string GetValue(RouteData routeData, string key)
        {
            foreach (KeyValuePair<string, object> keyValuePair in routeData.Values)
                if (keyValuePair.Key == key)
                    return keyValuePair.Value.ToString();
            return "?";
        }
    }
}