using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AspNetCore.Identity.MongoDB;
using Cobalto.SQL.Core;
using Core;
using Core.Models;
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
            // Configurar cultura para que funcione igual que en IIS
            var culture = new CultureInfo("es-CO"); // O la cultura que usa tu servidor IIS
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

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


            services.AddMvc(options => options.EnableEndpointRouting = false)
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddNewtonsoftJson(options =>
                {
                    // Configuración flexible como en .NET 2.2 - ¡No más converters personalizados!
                    options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Include;
                    options.SerializerSettings.DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Include;
                    options.SerializerSettings.DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat;
                    options.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;
                    
                    // Usar cultura invariante para números en JSON (punto decimal, no coma)
                    options.SerializerSettings.Culture = CultureInfo.InvariantCulture;
                    
                    // Mantener compatibilidad con el frontend existente
                    options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.None;
                });



            var allProviderTypes = Assembly.Load(System.Reflection.Assembly.GetExecutingAssembly().GetReferencedAssemblies().FirstOrDefault(x => x.Name == "Cobalto.SQL.Core"))
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

    // Converter personalizado para fechas flexibles
    public class FlexibleDateTimeConverter : JsonConverter<DateTime>
    {
        private readonly string[] _formats = new[]
        {
            "yyyy-MM-dd HH:mm",           // Formato original: "2025-07-01 00:00"
            "yyyy-MM-ddTHH:mm:ss",        // Formato ISO: "2025-07-01T00:00:00"
            "yyyy-MM-ddTHH:mm:ss.fff",    // Con milisegundos
            "yyyy-MM-ddTHH:mm:ss.fffZ",   // Con UTC
            "yyyy-MM-dd"                  // Solo fecha
        };

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dateString = reader.GetString();

            foreach (var format in _formats)
            {
                if (DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                {
                    return date;
                }
            }

            // Fallback a parsing normal
            if (DateTime.TryParse(dateString, out var fallbackDate))
            {
                return fallbackDate;
            }

            throw new JsonException($"No se pudo convertir '{dateString}' a DateTime");
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ss"));
        }
    }
    
    public class FlexibleDoubleConverter : JsonConverter<double>
    {


        public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var str = reader.GetString()?.Replace(",", "."); // cambiar coma por punto
                if (double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out var val))
                    return val;
            }
            else if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetDouble();
            }

            return 0.0;
        }

        public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }

    public class FlexibleInformacionContableConverter : JsonConverter<InformacionContable>
    {
        public override InformacionContable Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var informacion = new InformacionContable();

            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("Se esperaba un objeto JSON");
            }

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return informacion;
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException("Se esperaba un nombre de propiedad");
                }

                var propertyName = reader.GetString();
                reader.Read();

                switch (propertyName?.ToLower())
                {
                    case "base":
                        informacion.Base = ReadFlexibleDouble(ref reader);
                        break;
                    case "iva":
                        informacion.Iva = ReadFlexibleDouble(ref reader);
                        break;
                    case "tarifa":
                        informacion.Tarifa = ReadFlexibleDouble(ref reader);
                        break;
                    case "retfte":
                        informacion.RetFte = ReadFlexibleDouble(ref reader);
                        break;
                    case "base2":
                        informacion.Base2 = ReadFlexibleDouble(ref reader);
                        break;
                    case "tarifa2":
                        informacion.Tarifa2 = ReadFlexibleDouble(ref reader);
                        break;
                    case "retfte2":
                        informacion.RetFte2 = ReadFlexibleDouble(ref reader);
                        break;
                    case "retgarantia":
                        informacion.RetGarantia = ReadFlexibleDouble(ref reader);
                        break;
                    case "retgtia":
                        informacion.RetGtia = ReadFlexibleDouble(ref reader);
                        break;
                    case "porcentajeica":
                        informacion.PorcentajeICA = ReadFlexibleDouble(ref reader);
                        break;
                    case "valorica":
                        informacion.ValorICA = ReadFlexibleDouble(ref reader);
                        break;
                    case "anticipo":
                        informacion.Anticipo = ReadFlexibleDouble(ref reader);
                        break;
                    case "otrosdescuentos":
                        informacion.OtrosDescuentos = ReadFlexibleDouble(ref reader);
                        break;
                    case "apagar":
                        informacion.APagar = ReadFlexibleDouble(ref reader);
                        break;
                    case "usuario":
                        informacion.usuario = reader.GetString();
                        break;
                    case "esdocumentosoporte":
                        informacion.EsDocumentoSoporte = reader.GetBoolean();
                        break;
                    case "esanticipo":
                        informacion.esAnticipo = reader.TokenType == JsonTokenType.Null ? (bool?)null : reader.GetBoolean();
                        break;
                    default:
                        // Ignorar propiedades desconocidas
                        reader.Skip();
                        break;
                }
            }

            return informacion;
        }

        private double ReadFlexibleDouble(ref Utf8JsonReader reader)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var str = reader.GetString()?.Replace(",", "."); // cambiar coma por punto
                if (double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out var val))
                    return val;
            }
            else if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetDouble();
            }

            return 0.0;
        }

        public override void Write(Utf8JsonWriter writer, InformacionContable value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteNumber("APagar", value.APagar);
            writer.WriteNumber("Anticipo", value.Anticipo);
            
            if (value.esAnticipo.HasValue)
                writer.WriteBoolean("esAnticipo", value.esAnticipo.Value);
            else
                writer.WriteNull("esAnticipo");

            writer.WriteNumber("Base", value.Base);
            writer.WriteNumber("Iva", value.Iva);
            writer.WriteNumber("OtrosDescuentos", value.OtrosDescuentos);
            writer.WriteNumber("RetFte", value.RetFte);
            writer.WriteNumber("RetGarantia", value.RetGarantia);
            writer.WriteNumber("RetGtia", value.RetGtia);
            writer.WriteNumber("Tarifa", value.Tarifa);
            writer.WriteNumber("PorcentajeICA", value.PorcentajeICA);
            writer.WriteNumber("ValorICA", value.ValorICA);
            writer.WriteNumber("Base2", value.Base2);
            writer.WriteNumber("RetFte2", value.RetFte2);
            writer.WriteNumber("Tarifa2", value.Tarifa2);
            writer.WriteString("usuario", value.usuario);
            writer.WriteBoolean("EsDocumentoSoporte", value.EsDocumentoSoporte);

            writer.WriteEndObject();
        }
    }

}
