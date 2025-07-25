using AspNetCore.Identity.MongoDB;
using Cobalto.Mongo.Core.BLL;
using Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.BLL
{
    public class PedidoMaterialBLL : BaseBLL<PedidoMaterial>
    {

        public PedidoMaterialBLL(IConfiguration configuration, IHttpContextAccessor httpContext, OrdenCompraBLL ordenCompraBLL,
            ProyectoBLL proyectoBLL, UserManager<MongoIdentityUser> userManager) : base(configuration, httpContext)
        {
            this.ordenCompraBLL = ordenCompraBLL;
            this._proyectoBLL = proyectoBLL;
            this._userManager = userManager;
        }

        public async Task<List<PedidoReporte>> Reporte(FiltroPedidos filtro)
        {



            var filter = Builders<PedidoMaterial>.Filter.Gte("fechaSolicitado", filtro.inicio);
            var filter2 = Builders<PedidoMaterial>.Filter.Lte("fechaSolicitado", filtro.fin);

            var filterFecha = filter & filter2;



            FilterDefinition<PedidoMaterial> filtroProyecto = null;

            if (filtro.proyectos != null && filtro.proyectos.Count > 0)
            {
                filtroProyecto = Builders<PedidoMaterial>.Filter.Eq("proyecto", filtro.proyectos[0]);

                for (int i = 1; i < filtro.proyectos.Count(); i++)
                {
                    var tempFilter = Builders<PedidoMaterial>.Filter.Eq("proyecto", filtro.proyectos[i]);
                    filtroProyecto = filtroProyecto | tempFilter;

                }
            }



            FilterDefinition<PedidoMaterial> filtroUsuarios = null;

            if (filtro.usuarios != null && filtro.usuarios.Count > 0)
            {
                filtroUsuarios = Builders<PedidoMaterial>.Filter.Eq("solicitante", filtro.usuarios[0]);

                for (int i = 1; i < filtro.usuarios.Count(); i++)
                {
                    var tempFilter = Builders<PedidoMaterial>.Filter.Eq("solicitante", filtro.usuarios[i]);
                    filtroUsuarios = filtroUsuarios | tempFilter;

                }
            }


            var pedidos = new List<PedidoMaterial>();

            if (filtroProyecto != null)
            {

                if (filtroUsuarios != null)
                {
                    pedidos = await this.GetByProterty(filterFecha & filtroProyecto & filtroUsuarios);

                }
                else
                {
                    pedidos = await this.GetByProterty(filterFecha & filtroProyecto);


                }

            }
            else if (filtroUsuarios != null)
            {
                pedidos = await this.GetByProterty(filterFecha & filtroUsuarios);

            }
            else
            {

                pedidos = await this.GetByProterty(filterFecha);

            }





            var result = new List<PedidoReporte>();

            foreach (var item in pedidos)
            {

                var pedidoReporte = new PedidoReporte(item);
                if (!this.proyectos.ContainsKey(pedidoReporte.proyectoId))
                {
                    var proyecto = await this._proyectoBLL.GetById(pedidoReporte.proyectoId);
                    this.proyectos.Add(proyecto.id, proyecto.nombre);

                }

                pedidoReporte.proyecto = this.proyectos[pedidoReporte.proyectoId];


                if (!this.usuarios.ContainsKey(pedidoReporte.solicitante))
                {
                    var usuario = await this._userManager.FindByIdAsync(pedidoReporte.solicitante);
                    if (usuario != null)
                    {
                        this.usuarios.Add(usuario.Id, usuario.Nombre + " " + usuario.Apellido);
                    }
                    else
                    {

                        this.usuarios.Add(pedidoReporte.solicitante, "N/A");

                    }

                }
                pedidoReporte.solicitante = this.usuarios[pedidoReporte.solicitante];

                result.Add(pedidoReporte);
            }

            return result;

        }

        public async Task<List<PedidoMaterial>> TemporalesXProyecto(Guid proyectoId)
        {

            var filter = Builders<PedidoMaterial>.Filter.Eq("proyecto", proyectoId);
            var filter2 = Builders<PedidoMaterial>.Filter.Eq("temporal", true);

            var filtroTemporales = filter & filter2;

            return await this.GetByProterty(filtroTemporales);

        }

        public async Task<List<PedidoMaterialReporte>> FiltrarReporte(FiltroReporte filtro)
        {
            // var items = await this.Get();

            var result = new List<PedidoMaterialReporte>();

            //var proyectosArr = proyectos.Split(',').ToList();
            //var proveedorArr = proveedor.Split(',').ToList();


            var filter = Builders<PedidoMaterial>.Filter.Gte("fechaSolicitado", filtro.inicio);
            var filter2 = Builders<PedidoMaterial>.Filter.Lte("fechaSolicitado", filtro.fin);

            var filterFecha = filter & filter2;


            var items = await this.GetByProterty(filterFecha);

            if (filtro.proyectos != null && filtro.proyectos.Count > 0)
            {
                items = items.Where(x => filtro.proyectos.Contains(x.proyecto)).ToList();
            }

            if (items  == null || items.Count() == 0)
            {
                return new List<PedidoMaterialReporte>();
            }

            var idsPedidos = from p in items
                             select p.id;

            var ordenes = await this.ordenCompraBLL.PorIdsPedidos(idsPedidos.ToList());


            result = Convertir(items);

            foreach (var pedido in result)
            {
                pedido.OrdenesCompra = ordenes.Where(x => x.idPedido == pedido.id).ToList();
            }

            if (filtro.proveedores != null && filtro.proveedores.Count() > 0)
            {

                var filtradosPorProveedor = new List<PedidoMaterialReporte>();
                foreach (var item in result)
                {
                    foreach (var orden in item.OrdenesCompra)
                    {
                        if (filtro.proveedores.Contains(orden.proveedor))
                        {
                            filtradosPorProveedor.Add(item);
                            break;
                        }
                    }

                }

                result = filtradosPorProveedor;
            }


            return result;


        }

        public List<PedidoMaterialReporte> Convertir(object M)
        {
            // Serialize the original object to json
            // Desarialize the json object to the new type 
            var obj = JsonConvert.DeserializeObject<List<PedidoMaterialReporte>>(JsonConvert.SerializeObject(M));
            return obj;
        }


        Dictionary<Guid, string> proyectos = new Dictionary<Guid, string>();
        Dictionary<string, string> usuarios = new Dictionary<string, string>();
        private OrdenCompraBLL ordenCompraBLL;
        private ProyectoBLL _proyectoBLL;
        private UserManager<MongoIdentityUser> _userManager;





        public string ReporteCsv(List<PedidoReporte> pedidos)
        {


            var sb = new StringBuilder("proyecto;fechaSolicitado;solicitante;urgente; TipoPedido");
            sb.AppendLine();
            foreach (var item in pedidos)
            {

                sb.AppendLine(LineaReporte(item));
            }

            return sb.ToString();


        }

        public string LineaReporte(PedidoReporte item)
        {
            var linea = $"{item.proyecto};{item.fechaSolicitado};{item.solicitante};{item.urgente};{item.TipoPedido}";
            return linea;
        }


        public async Task<List<PedidoMaterial>> PedidosPendientes()
        {

            var filter = Builders<PedidoMaterial>.Filter.Eq("ordenCompra", false);
            var filter2 = Builders<PedidoMaterial>.Filter.Eq("temporal", false);

            var filtroPedidos = filter & filter2;

            var pedidos =  await this.GetByProterty(filtroPedidos);


            List<PedidoMaterial> result = new List<PedidoMaterial>();
            foreach (var item in pedidos)
            {
                if (item.detalle.Any(x=> x.rechazado == false && x.tieneOrden == false))
                {
                    result.Add(item);
                }
            }

            return result;

        }
       

       

        /*********/


    }
}
