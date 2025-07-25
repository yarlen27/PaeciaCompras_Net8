using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AspNetCore.Identity.MongoDB;
using Cobalto.SQL.Core;
using Core;
using Core.Utils;
using jsreport.AspNetCore;
using jsreport.Binary;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;

namespace API
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
            services.Configure<MongoDbSettings>(Configuration.GetSection("MongoDb"));
            services.AddBLL(Configuration);
            services.AddSQLBLL(Configuration);



            services.AddSingleton<IUserStore<MongoIdentityUser>>(provider =>
            {
                var options = provider.GetService<IOptions<MongoDbSettings>>();
                var client = new MongoClient(options.Value.ConnectionString);
                var database = client.GetDatabase(options.Value.DatabaseName);

                return new MongoUserStore<MongoIdentityUser>(database);
            });
            MongoIdentityServiceCollectionExtensions.Configuration = Configuration;

            services.AddIdentity<MongoIdentityUser>(options =>
            {
                options.User.RequireUniqueEmail = false;
            })
                .AddDefaultTokenProviders();


            var serviceProvider = services.BuildServiceProvider();
            var logger = serviceProvider.GetService<ILogger<ApplicationLogs>>();
            services.AddSingleton(typeof(ILogger), logger);

            services.AddCors();

            AddSwagger(services);


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);



            var allProviderTypes =  Assembly.Load(System.Reflection.Assembly.GetExecutingAssembly().GetReferencedAssemblies().FirstOrDefault(x => x.Name == "Cobalto.SQL.Core"))
                .GetTypes().Where(t => t.Namespace != null && t.Namespace.Contains("Cobalto.SQL.Core.BLL")); ;
                                       

            foreach (var intfc in allProviderTypes)
            {
                //var impl = allProviderTypes.FirstOrDefault(c => c.IsClass && !intfc.FullName.StartsWith("WebApi.BLL.BaseBL"));
                //if (impl != null)
                //{
                //    services.AddTransient(impl);
                //}

                services.AddTransient(intfc);

            }

        }


        private void AddSwagger(IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                var groupName = "v1";

                options.SwaggerDoc(groupName, new OpenApiInfo
                {
                    Title = $"GestionCompras",
                    Version = groupName,
                    Description = "GestionCompras",
                    Contact = new OpenApiContact
                    {
                        Name = "GestionCompras",
                        Email = string.Empty,
                        // Url = new Uri("https://foo.com/"),
                    }
                });
            });
        }



        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.Use(async (ctx, next) =>
            {
                await next();
                if (ctx.Response.StatusCode == 204)
                {
                    ctx.Response.ContentLength = 0;
                }
            });

           
            app.UseDeveloperExceptionPage();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Foo API V1");
            });


            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseCors(
               options => options.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod()
           );

            //   app.UseHttpsRedirection();
            app.UseMvc();

        }
    }
}
