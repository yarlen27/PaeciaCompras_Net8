using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCore.Identity.MongoDB;
using Core.Bll;
using Core.BLL;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API.Controllers
{
    [Route("api/[controller]")]
    [Authorize]

    public class PedidoServicioController : BLLController<PedidoServicio>
    {
        private SuspensionBLL _suspensionBLL;
        private OrdenCompraBLL _ordenCompraBll;
        private EmailBLL _emailBLL;
        private PedidoServicioBLL bll;
        private AdicionPedidoServicioBLL _adicionPedidoServicioBLL;
        private readonly ReanudacionBLL _reanudacionBLL;
        private UserManager<MongoIdentityUser> _userManager;

        public PedidoServicioController(PedidoServicioBLL bll,  EmailBLL emailBLL, OrdenCompraBLL ordenCompraBLL, UserManager<MongoIdentityUser> userManager) : base(bll)
        {
            this._ordenCompraBll = ordenCompraBLL;
            this._emailBLL = emailBLL;
            this.bll = bll;
            _userManager = userManager;
        }

        [HttpGet]
        public override async Task<List<PedidoServicio>> Get()
        {
            var result = await base.Get();
            return result.Where(x => !x.ordenCompra).ToList();
        }

        [HttpGet]
        [Route("All")]
        public async Task<List<PedidoServicio>> GetAll()
        {
            var result = await base.Get();
            return result;
        }


        public override async Task<PedidoServicio> PostAsync([FromBody] PedidoServicio entity)
        {
            var result = base.PostAsync(entity);

            if (this._emailBLL != null)
            {
                Guid IdTenant = new Guid(this.BLL.clientId);
                var listaUsuarios = _userManager.Users;
                listaUsuarios = listaUsuarios.Where(x => x.Client.Any(z => z.client == IdTenant) && x.DeletedOn == null);
                await _emailBLL.EmailNuevoPedidoServicios(result.Result.id, listaUsuarios.ToList());
            }


            return result.Result;
        }





        [HttpGet]
        [Route("filtrado/{inicio}/{fin}/{proyectos}/{proveedor}")]
        public async Task<List<PedidoServicio>> FiltrarMateriales([FromRoute] DateTime inicio, [FromRoute] DateTime fin, [FromRoute] string proyectos, [FromRoute] string proveedor)
        {
            var items = await this.Get();

            var proyectosArr = proyectos.Split(',').ToList();
            var proveedorArr = proveedor.Split(',').ToList();

            items = items.Where(x => x.fechaSolicitado >= inicio && x.fechaSolicitado <= fin).ToList();


            if (proyectos != "null" && proveedor != "null")
            {
                List<OrdenCompra> ordenesXProveedorXProyecto = await this._ordenCompraBll.GetAll();

                ordenesXProveedorXProyecto = ordenesXProveedorXProyecto.Where(x => proyectosArr.Exists(z => z == x.proyecto.ToString()) && proveedorArr.Exists(z => z == x.proveedor.ToString())).ToList();
                items = items.Where(x => ordenesXProveedorXProyecto.Exists(z => z.proyecto == x.proyecto) && ordenesXProveedorXProyecto.Exists(z => z.proveedor.ToString() == x.detalle.proveedor)).ToList();
            }
            else if (proyectos != "null" || proveedor != "null")
            {
                List<OrdenCompra> ordenesXProveedorXProyecto = await this._ordenCompraBll.GetAll();

                ordenesXProveedorXProyecto = ordenesXProveedorXProyecto.Where(x => proyectosArr.Exists(z => z == x.proyecto.ToString()) || proveedorArr.Exists(z => z == x.proveedor.ToString())).ToList();
                items = items.Where(x => ordenesXProveedorXProyecto.Exists(z => z.proyecto == x.proyecto) &&  ordenesXProveedorXProyecto.Exists(z => z.proveedor.ToString() == x.detalle.proveedor)).ToList();
            }

           return items;
        }



        [HttpPost]
        [Route("filtradoReporte")]
        public async Task<List<PedidoServicioReporte>> FiltrarMaterialesReporte([FromBody] FiltroReporte filtro)
        {
            return await this.bll.FiltrarReporte(filtro);
        }





    }
}
