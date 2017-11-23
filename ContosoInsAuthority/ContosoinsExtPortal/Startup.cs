using ContosoinsExtPortal.Models;
using ContosoinsExtPortal.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ContosoinsExtPortal
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
            services.AddAuthentication(sharedOptions =>
            {
                sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                sharedOptions.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddAzureAdB2C(options => Configuration.Bind("AzureAdB2C", options))
            .AddCookie();

            services.AddMvc();

            var clientId = Configuration["AzureKeyVault:ClientIdWeb"];
            var cerificateThumbprint = Configuration["AzureKeyVault:AuthCertThumbprint"];


            AKVServiceClient servClient = new AKVServiceClient(Configuration["AzureKeyVault:ClientIdWeb"],
                Configuration["AzureKeyVault:AuthCertThumbprint"], Configuration["AzureKeyVault:VaultName"],
                Configuration["AzureKeyVault:KeyName"]);


            services.AddSingleton<AKVServiceClient>(servClient);
            string connection = servClient.GetDbConnectionString();
            services.AddDbContext<ContosoinsauthdbContext>(options => options.UseSqlServer(connection));

            services.AddOptions();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseAuthentication();

            //app.UseMvc(routes =>
            //{
            //    routes.MapRoute(
            //        name: "default",
            //        template: "{controller=Home}/{action=Index}/{id?}");
            //});

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=VehiclePolicies}/{action=Index}/{id?}");
            });
        }
    }
}
