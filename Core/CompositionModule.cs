//using Creimed.Core.BLL;

using Core.Bll;
using Core.BLL;
using Core.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Core
{
    public static class CompositionModule
    {
        public static void AddBLL(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddScoped<ClientBLL>();
            services.AddScoped<Core.BLL.ProveedorBLL>();
            services.AddScoped<ProyectoBLL>();
            services.AddScoped<DepartamentoBLL>();
            services.AddScoped<MunicipioBLL>();
            services.AddScoped<PedidoMaterialBLL>();
            services.AddScoped<PedidoServicioBLL>();
            services.AddScoped<OrdenCompraBLL>();
            services.AddScoped<EmailBLL>();
            services.AddScoped<FacturaBLL>();
            services.AddScoped<FacturaBLL>();
            services.AddScoped<TipoContratoBLL>();
            services.AddScoped<RemisionBLL>();
            services.AddScoped<EncuestaProveedorBLL>();
            services.AddScoped<FacturaElectronicaBLL>();


            services.AddScoped<ReferenciaBLL>();
            services.AddScoped<CategoriaBLL>();
            services.AddScoped<AutoGeneradoBLL>();
            services.AddScoped<ReferenciaProyectoBLL>();
            services.AddScoped<AdicionPedidoServicioBLL>();
            services.AddScoped<SuspensionBLL>();
            services.AddScoped<ReanudacionBLL>();

            services.AddScoped<AdicionesFacade>();
            services.AddScoped<ConsecutivoCausacionBLL>();

            services.AddScoped<VehiculoBLL>();
            services.AddScoped<OrdenTrabajoBLL>();
            services.AddScoped<FacturaOTBLL>();



            services.AddSingleton<IFileProvider>(new EmbeddedFileProvider(Assembly.GetEntryAssembly()));
            services.AddScoped<DigitalOceanUploader.Shared.DigitalOceanUploadManager>();
            services.AddScoped<UnidadBLL>();
            services.AddScoped<ConsecutivoSoporteBLL>();




          
            

        }

    }
}
