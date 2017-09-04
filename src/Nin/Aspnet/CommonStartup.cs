using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Cors.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Nin.Configuration;
using Nin.Diagnostic;
using Swashbuckle.AspNetCore.Swagger;

namespace Nin.Aspnet
{
    /// <summary>
    ///     Common configuration for web services
    /// </summary>
    public abstract class CommonStartup
    {
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            ConfigureNin(app);
            //var defaultHandler = (IRouter) app.ApplicationServices.GetRequiredService<MvcRouteHandler>();
            //app.UseRouter(new WebApiRouter(defaultHandler));
            app.UseMvcWithDefaultRoute();
            ConfigureDevelopment(app, env);

            app.UseDirectoryBrowser();
        }

        private static void ConfigureDevelopment(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (!env.IsDevelopment())
                return;

            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "NiN API V1");
            });
            string frontendPath = FileLocator.FindDirectoryInTree(Config.Settings.FrontEndSubdirectory);
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(frontendPath),
                RequestPath = new PathString("")
            });

            app.UseDirectoryBrowser(new DirectoryBrowserOptions
            {
                FileProvider = new PhysicalFileProvider(frontendPath),
                RequestPath = new PathString("")
            });
        }

        protected virtual void ConfigureNin(IApplicationBuilder app)
        {
        }

        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureNinServices(services);
            services.AddMvc(options =>
            {
                options.Filters.Add(typeof(ExceptionFilter));
                options.Filters.Add(new CorsAuthorizationFilterFactory("AllowFromAll"));
            });
            var mvcCore = services.AddMvcCore(options => { options.Filters.Add(typeof(ExceptionFilter)); });
            //            mvcCore.AddJsonFormatters(options => options.ContractResolver = new CamelCasePropertyNamesContractResolver());
            mvcCore.AddJsonFormatters();

            services.AddCors(options =>
            {
                options.AddPolicy("AllowFromAll", builder => builder
                    .WithMethods("GET", "POST")
                    .AllowAnyOrigin()
                    .AllowAnyHeader());
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info {Title = "NiN", Version = "v1"});
            });
        }

        protected virtual void ConfigureNinServices(IServiceCollection services)
        {
        }

        protected CommonStartup(IHostingEnvironment env)
        {
            Log.i("WWW", "Starting " + env.ApplicationName);
            var builder = new ConfigurationBuilder();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }
    }
}