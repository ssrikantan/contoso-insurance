using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ContosoInsAuthorityAdminPortal.Models;
using Microsoft.EntityFrameworkCore;
using ContosoInsAuthorityAdminPortal.Services;

namespace ContosoInsAuthorityAdminPortal
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
            //Adding the Azure AD integration for User authentication
            services.AddAuthentication(sharedOptions =>
            {
                sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                sharedOptions.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddAzureAd(options => Configuration.Bind("AzureAd", options))
            .AddCookie();
           
            services.AddMvc();

            //Add the Key Vault Service Client Connection to the Context Object 
            AKVServiceClient servClient = new AKVServiceClient(Configuration["AzureKeyVault:ClientIdWeb"],
                Configuration["AzureKeyVault:AuthCertThumbprint"], Configuration["AzureKeyVault:VaultName"],
                Configuration["AzureKeyVault:KeyName"]);
            services.AddSingleton<AKVServiceClient>(servClient);
            
            //Get the Connection string to Azure SQL Database from Azure Key Vault Secret
            string connection = servClient.GetDbConnectionString();
            //Add the Azure SQL Database connection to the Context Object
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
