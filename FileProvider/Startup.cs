using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Diflexmo.AspNetCore.AzureBlobFileProvider;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;

namespace FileProvider
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //var blobOptions = new AzureBlobOptions
            //{
            //    ConnectionString = @"UseDevelopmentStorage=true",
            //    DocumentContainer = "kevin"
            //};

            //var azureBlobFileProvider = new AzureBlobFileProvider(blobOptions);
            //services.AddSingleton<IFileProvider>(azureBlobFileProvider);

            services.Configure<RouteOptions>(options => options.ConstraintMap.Add("allowedgods", typeof(OnlyGodsConstraint)));

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseMvc();

            //var blobFileProvider = app.ApplicationServices.GetRequiredService<IFileProvider>();
            //app.UseStaticFiles(new StaticFileOptions()
            //{
            //    FileProvider = blobFileProvider,
            //    RequestPath = "/files"
            //});

            //app.UseDirectoryBrowser(new DirectoryBrowserOptions
            //{
            //    FileProvider = blobFileProvider,
            //    RequestPath = "/files",
            //    Formatter = new JsonDirectoryFormatter()
            //});
        }
    }

    public class OnlyGodsConstraint : IRouteConstraint
    {
        public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            var value = values[routeKey] as string;
            return Path.HasExtension(value);
        }
    }

    public class JsonDirectoryFormatter : IDirectoryFormatter
    {
        public async Task GenerateContentAsync(HttpContext context, IEnumerable<IFileInfo> contents)
        {
             var result = contents.Select(x => new
            {
                Name = x.Name,
                PhysicalPath = $"{context.Request.Path}{x.Name}",
                x.IsDirectory,
                x.LastModified
            });

            var jsonString = JsonConvert.SerializeObject(result);
            context.Response.ContentType = new MediaTypeHeaderValue("application/json").ToString();
            await context.Response.WriteAsync(jsonString, Encoding.UTF8);
        }
    }
}
