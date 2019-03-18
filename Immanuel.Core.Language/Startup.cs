using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.LanguageIdentifier;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Immanuel.Core.Language
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
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });


            services.AddMvc()
                .AddJsonOptions(opts =>
                {
                    opts.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddCors(options => options.AddPolicy("CorsPolicy",
               builder =>
               {
                   builder.AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowAnyOrigin();
               }));

            var li = LanguageIdentifier.New(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Data", "langprofiles-char-1_5-nfc-all.bin.gz"), "Vector", -1);
            services.AddSingleton<LanguageIdentifier>(li);

            services.AddCors(options => options.AddPolicy("CorsPolicy",
            builder =>
            {
                builder.AllowAnyMethod().AllowAnyHeader()
                .AllowAnyOrigin();
            }));

            services.AddSignalR(hubOptions =>
            {
                hubOptions.EnableDetailedErrors = true;
                hubOptions.KeepAliveInterval = TimeSpan.FromMinutes(1);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseCors("CorsPolicy");

            app.UseSignalR((configure) =>
            {
                var desiredTransports =
                    HttpTransportType.WebSockets |
                    HttpTransportType.LongPolling;

                configure.MapHub<Hubs.ProgressHub>("/progress", (options) =>
                {
                    options.Transports = desiredTransports;
                });
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
