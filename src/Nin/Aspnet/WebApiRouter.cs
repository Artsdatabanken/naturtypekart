using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Nin.Aspnet
{
    public class WebApiRouter : IRouter
    {
        readonly Dictionary<string, MethodInfo> routes = new Dictionary<string, MethodInfo>();
        private readonly IRouter defaultRouter;

        public WebApiRouter(IRouter defaultRouter)
        {
            this.defaultRouter = defaultRouter;
            ScanForEndpoints();
        }

        public Task RouteAsync(RouteContext context)
        {
            var request = context.HttpContext.Request;

            string targetKey = FindEndpointFor(request.Path.Value);
            if (targetKey == null)
            {
                defaultRouter.RouteAsync(context);
                return Task.FromResult(0);
            }

            context.Handler = CreateHandler(targetKey, request);
            return Task.FromResult(0);
        }

        private void ScanForEndpoints()
        {
            foreach (var type in Assembly.GetEntryAssembly().GetExportedTypes())
            {
                const string controllerSuffix = "Controller";
                if (!type.Name.EndsWith(controllerSuffix))
                    continue;
                string controllerName = type.Name.Substring(0, type.Name.Length - controllerSuffix.Length);
                const BindingFlags bindingFlags =
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
                foreach (var mi in type.GetMethods(bindingFlags))
                    routes.Add("/" + controllerName + "/" + mi.Name, mi);
            }
        }

        private RequestDelegate CreateHandler(string targetKey, HttpRequest request)
        {
            var targetMethod = routes[targetKey];
            CallArguments ca = new CallArguments(request.Path.Value.Substring(targetKey.Length));

            var controller = Activator.CreateInstance(targetMethod.DeclaringType);
            return new RequestHandler(controller, targetMethod, ca.MapArgs(targetMethod, request)).RequestDelegate;
        }

        private string FindEndpointFor(string requestPath)
        {
            foreach (var key in routes.Keys)
                if (requestPath.StartsWith(key, StringComparison.OrdinalIgnoreCase))
                    return key;
            return null;
        }

        public VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            throw new NotImplementedException();
        }
    }
}