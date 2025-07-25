using Cobalto.Mongo.Core.BLL;
using Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using AspNetCore.Identity.MongoDB;
using Newtonsoft.Json;

namespace Core.BLL
{
    public class PedidoServicioBLL: BaseBLL<PedidoServicio>
    {
        public PedidoServicioBLL(IConfiguration configuration, IHttpContextAccessor httpContext, ProyectoBLL proyectoBLL, OrdenCompraBLL ordenCompraBLL, UserManager<MongoIdentityUser> userManager) : base(configuration, httpContext)
        {
            this._proyectoBLL = proyectoBLL;
            this._userManager = userManager;
            this.ordenCompraBLL = ordenCompraBLL;
        }

        Dictionary<Guid, string> proyectos = new Dictionary<Guid, string>();
        Dictionary<string, string> usuarios = new Dictionary<string, string>();
        private ProyectoBLL _proyectoBLL;
        private UserManager<MongoIdentityUser> _userManager;
        private OrdenCompraBLL ordenCompraBLL;

        public async Task<List<PedidoReporte>> Reporte(FiltroPedidos filtro)
        {



            var filter = Builders<PedidoServicio>.Filter.Gte("fechaSolicitado", filtro.inicio);
            var filter2 = Builders<PedidoServicio>.Filter.Lte("fechaSolicitado", filtro.fin);

            var filterFecha = filter & filter2;



            FilterDefinition<PedidoServicio> filtroProyecto = null;

            if (filtro.proyectos != null && filtro.proyectos.Count > 0)
            {
                filtroProyecto = Builders<PedidoServicio>.Filter.Eq("proyecto", filtro.proyectos[0]);

                for (int i = 1; i < filtro.proyectos.Count(); i++)
                {
                    var tempFilter = Builders<PedidoServicio>.Filter.Eq("proyecto", filtro.proyectos[i]);
                    filtroProyecto = filtroProyecto | tempFilter;

                }
            }



            FilterDefinition<PedidoServicio> filtroUsuarios = null;

            if (filtro.usuarios != null && filtro.usuarios.Count > 0)
            {
                filtroUsuarios = Builders<PedidoServicio>.Filter.Eq("solicitante", filtro.usuarios[0]);

                for (int i = 1; i < filtro.usuarios.Count(); i++)
                {
                    var tempFilter = Builders<PedidoServicio>.Filter.Eq("solicitante", filtro.usuarios[i]);
                    filtroUsuarios = filtroUsuarios | tempFilter;

                }
            }


            var pedidos = new List<PedidoServicio>();

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


        public List<PedidoServicioReporte> Convertir(object M)
        {
            // Serialize the original object to json
            // Desarialize the json object to the new type 
            var obj = JsonConvert.DeserializeObject<List<PedidoServicioReporte>>(JsonConvert.SerializeObject(M));
            return obj;
        }


        public async Task<List<PedidoServicioReporte>> FiltrarReporte(FiltroReporte filtro)
        {
            // var items = await this.Get();

            var result = new List<PedidoServicioReporte>();

            //var proyectosArr = proyectos.Split(',').ToList();
            //var proveedorArr = proveedor.Split(',').ToList();


            var filter = Builders<PedidoServicio>.Filter.Gte("fechaSolicitado", filtro.inicio);
            var filter2 = Builders<PedidoServicio>.Filter.Lte("fechaSolicitado", filtro.fin);

            var filterFecha = filter & filter2;

           


            var items = await this.GetByProterty(filterFecha);

            if (filtro.proyectos != null && filtro.proyectos.Count > 0)
            {
                items = items.Where(x => filtro.proyectos.Contains(x.proyecto)).ToList();
            }

            if (items == null || items.Count() == 0)
            {
                return new List<PedidoServicioReporte>();
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

                var filtradosPorProveedor = new List<PedidoServicioReporte>();
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
    }
}
