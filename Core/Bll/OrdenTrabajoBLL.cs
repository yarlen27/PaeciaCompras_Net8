using Cobalto.Mongo.Core.BLL;
using Core.BLL;
using Core.Models;
// using iTextSharp.text; // TODO: Migrate to iText7
using iText.Kernel.Pdf;
using iText.Layout;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Bll
{
    public class OrdenTrabajoBLL : BaseBLL<OrdenTrabajo>
    {

        public OrdenTrabajoBLL(IConfiguration configuration,
            IHttpContextAccessor httpContext,
            FacturaBLL facturaBLL) : base(configuration, httpContext)
        {
            this._facturaBLL = facturaBLL;
        }
        private readonly FacturaBLL _facturaBLL;

        public override Task<OrdenTrabajo> Insert(OrdenTrabajo item)
        {
            item.fechaCreacion = DateTime.Now;
            return base.Insert(item);
        }

        public async Task<List<OrdenTrabajo>> OrdenesTrabajoFiltradas(Filtro filtro)
        {


            if (filtro.proyectos != null && filtro.proyectos.Count() > 0)
            {
                if (filtro.proveedor != null)
                {
                    return await this.GetOrdenesXProyectoXProveedorXFechaCreacion(filtro);

                }
                else
                {
                    return await this.GetOrdenTrabajoXProyectoXFechaCreacion(filtro);
                }
            }
            else
            {

                if (filtro.proveedor != null && filtro.proveedor.Count() > 0)
                {

                    return await this.GetOrdenesTrabajoXProveedorXFechaCreacion(filtro);
                }
                else
                {
                    return await this.OrdenesTrabajoFiltradasXFechaCreacion(filtro);
                }

            }
        }

        private async Task<List<OrdenTrabajo>> OrdenesTrabajoFiltradasXFechaCreacion(Filtro filtro)
        {
            var filtro0 = Builders<OrdenTrabajo>.Filter.Gte("fechaCreacion", filtro.inicio.Value);
            var filtro1 = Builders<OrdenTrabajo>.Filter.Lte("fechaCreacion", filtro.fin.Value);
            var filtro2 = Builders<OrdenTrabajo>.Filter.Eq("erased", false);

            var resultado = await this.GetByProterty(filtro0 & filtro1 & filtro2);

            return resultado;
        }
        private async Task<List<OrdenTrabajo>> GetOrdenesTrabajoXProveedorXFechaCreacion(Filtro filtro)
        {
            var filtro0 = Builders<OrdenTrabajo>.Filter.Eq("idProveedor", filtro.proveedor[0]);
            var filtro1 = Builders<OrdenTrabajo>.Filter.Gte("fechaCreacion", filtro.inicio);
            var filtro2 = Builders<OrdenTrabajo>.Filter.Lte("fechaCreacion", filtro.fin);
            var filtro3 = Builders<OrdenTrabajo>.Filter.Eq("erased", false);


            for (int i = 1; i < filtro.proveedor.Count(); i++)
            {
                var filter = Builders<OrdenTrabajo>.Filter.Eq("idProveedor", filtro.proveedor[i]);
                filtro0 = (filtro0 | filter);

            }

            var ordenesXProveedor = await this.GetByProterty(filtro0 & filtro1 & filtro2 & filtro3);

            return ordenesXProveedor;
        }

        private async Task<List<OrdenTrabajo>> GetOrdenesXProyectoXProveedorXFechaCreacion(Filtro filtro)
        {

            var resultado = new List<OrdenTrabajo>();
            foreach (var proyecto in filtro.proyectos)
            {
                foreach (var proveedor in filtro.proveedor)
                {


                    var filtro0 = Builders<OrdenTrabajo>.Filter.Eq("idProyecto", proyecto);
                    var filtro2 = Builders<OrdenTrabajo>.Filter.Eq("idProveedor", proveedor);
                    var filtro3 = Builders<OrdenTrabajo>.Filter.Gte("fechaCreacion", filtro.inicio);
                    var filtro4 = Builders<OrdenTrabajo>.Filter.Lte("fechaCreacion", filtro.fin);
                    var filtro5 = Builders<OrdenTrabajo>.Filter.Eq("erased", false);


                    var ordenesXProveedorXProyecto = await this.GetByProterty(filtro0 & filtro2 & filtro3 & filtro4 & filtro5);

                    resultado.AddRange(ordenesXProveedorXProyecto);

                }
            }

            return resultado;
        }

        private async Task<List<OrdenTrabajo>> GetOrdenTrabajoXProyectoXFechaCreacion(Filtro filtro)
        {


            var filtro0 = Builders<OrdenTrabajo>.Filter.Eq("idProyecto", filtro.proyectos[0]);
            var filtro2 = Builders<OrdenTrabajo>.Filter.Gte("fechaCreacion", filtro.inicio);
            var filtro1 = Builders<OrdenTrabajo>.Filter.Lte("fechaCreacion", filtro.fin);
            var filtro3 = Builders<OrdenTrabajo>.Filter.Eq("erased", false);


            for (int i = 1; i < filtro.proyectos.Count(); i++)
            {
                var filter = Builders<OrdenTrabajo>.Filter.Eq("idProyecto", filtro.proyectos[i]);
                filtro0 = filtro0 | filter;

            }

            var ordenesXProveedor = await this.GetByProterty(filtro0 & filtro1 & filtro2 & filtro3);

            return ordenesXProveedor;

        }

        public async Task<List<OrdenTrabajo>> UpdateIdFacturaOT(AsociacionFacturaOT asociacion)
        {
            List <OrdenTrabajo> lista = new List<OrdenTrabajo>();



            var otsActuales = await this.GetByProterty("idFactura", asociacion.IdFactura);

            foreach (var item in otsActuales)
            {
                if (asociacion.IdOT.Where(x=> x == item.id).Count() == 0)
                {
                    item.idFactura = null;
                    await this.Update(item);

                }
            }

            foreach (var item in asociacion.IdOT)
            {
                var consultaOT = await this.GetById(item);

                if (consultaOT.idFactura != asociacion.IdFactura || consultaOT.idFactura == null)
                {
                    consultaOT.idFactura = asociacion.IdFactura;
                    await this.Update(consultaOT);
                }
                lista.Add(consultaOT);
            }

            

            await this._facturaBLL.ActualizarOT(asociacion);

            return lista;
        }

        public async Task<List<OrdenTrabajo>> GetOTSinFacturaAsociada()
        {

            var filtro1 = Builders<OrdenTrabajo>.Filter.Eq("erased", false);
            var resultado = await this.GetByProterty(filtro1);

            return resultado;
        }
    }
}
