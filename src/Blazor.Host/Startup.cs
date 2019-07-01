using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using NetPack;
using NetPack.Blazor;
using System;

namespace Blazor.Host
{
    public class Startup
    {
        public IFileProvider BlazorClientStaticFileProvider { get; private set; }

        public IFileProvider BlazorClientProjectFileProvider { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // Get hold of the FileProviders for the blazor client - webroot content, and project content.
            BlazorClientStaticFileProvider = BlazorClientAppFileProviderHelper.GetStaticFileProvider<Client.Startup>();
            BlazorClientProjectFileProvider = BlazorClientAppFileProviderHelper.GetContentFileProvider<Client.Startup>();

            services.AddNetPack((a) =>
            {
                a.AddPipeline((b) =>
                {
                    _ = b.WithFileProvider(BlazorClientProjectFileProvider)
                    .AddBlazorRecompilePipe<Client.Startup>()
                    .Watch();
                });
            });

            // SignalR is needed as a dependency for netpack's AddBrowserReload()..
            services.AddSignalR();
            services.AddBrowserReload((options) =>
            {
                // watch these files from the file provider, and trigger reload when changes detected.
                options.FileProvider(BlazorClientStaticFileProvider)
                         .Watch("/**/*.dll")
                         .Watch("/**/*.css")
                         .Watch("/**/*.html");
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseNetPack();
            app.UseStaticFiles();
            app.UseBrowserReload();

            app.UseClientSideBlazorFiles(BlazorClientStaticFileProvider, true);

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapFallbackToClientSideBlazor<Client.Startup>("index.html");
            });
        }
    }
}
