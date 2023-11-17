using Core.LanguageIdentifier;
using Microsoft.AspNetCore.Http.Connections;

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


            services.AddMvc();

            services.AddCors(options => options.AddPolicy("CorsPolicy",
               builder =>
               {
                   builder.AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowAnyOrigin();
               }));

            //var li = LanguageIdentifier.New(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Data", "langprofiles-char-1_5-nfc-all.bin.gz"), "Vector", -1);
            //services.AddSingleton<LanguageIdentifier>(li);
            //commenting aboce line, and mocking below (18-11-23 - moving to new cshtml)
            services.AddSingleton<LanguageIdentifier>(new LanguageIdentifierVector());

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
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseRouting();
            //app.UseRouter(routes =>
            //{
            //    routes.MapRoute(
            //       name: "default",
            //       template: "{controller=Home}/{action=Index}/{id?}");
            //});
            app.UseEndpoints((configure) =>
            {
                var desiredTransports =
                    HttpTransportType.WebSockets |
                    HttpTransportType.LongPolling;

                configure.MapHub<Hubs.ProgressHub>("/progress", (options) =>
                {
                    options.Transports = desiredTransports;
                });
                configure.MapControllerRoute(
               name: "default",
               pattern: "{controller=Home}/{action=Index}/{id?}");
            });
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseCors("CorsPolicy");

            
        }
    }
}
