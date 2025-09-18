using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCore.Identity.MongoDB;
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
    public class PedidoMaterialController : BLLController<PedidoMaterial>
    {
        private OrdenCompraBLL _ordenCompraBll;
        private EmailBLL _emailBLL;
        private PedidoMaterialBLL bll;
        private UserManager<MongoIdentityUser> _userManager;

        public PedidoMaterialController(PedidoMaterialBLL bll, EmailBLL emailBLL, OrdenCompraBLL ordenCompraBLL, UserManager<MongoIdentityUser> userManager) : base(bll)
        {
            this._ordenCompraBll = ordenCompraBLL;
            this._emailBLL = emailBLL;
            this.bll = bll;
            _userManager = userManager;
        }



        [HttpPost]
        public override async Task<PedidoMaterial> PostAsync([FromBody] PedidoMaterial entity)
        {



            PedidoMaterial result = null;
            if (entity.id != Guid.Empty)
            {
                result = await base.GetById(entity.id);
            }
            else
            {
              result =   await base.PostAsync(entity);

            }

            if (this._emailBLL != null && entity.temporal == false)
            {
                Guid IdTenant = new Guid(this.BLL.clientId);
                var listaUsuarios = _userManager.Users;
                listaUsuarios = listaUsuarios.Where(x => x.Client.Any(z => z.client == IdTenant) && x.DeletedOn == null);
                await _emailBLL.EmailNuevoPedidoMaterial(result.id, listaUsuarios.ToList());
            }


            return result;
        }

        [HttpPut("EditPrePedido")]
        public async Task<bool> EditPrePedido([FromBody] PedidoMaterial entity)
        {
            await base.Put(entity);
            if (this._emailBLL != null && entity.temporal == false)
            {
                Guid IdTenant = new Guid(this.BLL.clientId);
                var listaUsuarios = _userManager.Users;
                listaUsuarios = listaUsuarios.Where(x => x.Client.Any(z => z.client == IdTenant) && x.DeletedOn == null);
                await _emailBLL.EmailNuevoPedidoMaterial(entity.id, listaUsuarios.ToList());
            }
            return true;
        }

        [HttpGet]
        [Route("temporales/{id}")]
        public async Task<List<PedidoMaterial>> GetTemporales([FromRoute] Guid id)
        {


            var result = await this.bll.TemporalesXProyecto(id);

            return result;
        }

        [HttpGet]
        [Route("PedidosPendientes")]
        public async Task<List<PedidoMaterial>> PedidosPendientes()
        {


            var result = await this.bll.PedidosPendientes();

            return result;
        }


        [HttpGet]
        [Route("filtrado/{inicio}/{fin}/{proyectos}/{proveedor}")]
        public async Task<List<PedidoMaterial>> FiltrarMateriales([FromRoute] DateTime inicio, [FromRoute] DateTime fin, [FromRoute] string proyectos, [FromRoute] string proveedor)
        {
            var items = await this.Get();

            var proyectosArr = proyectos.Split(',').ToList();
            var proveedorArr = proveedor.Split(',').ToList();

            items = items.Where(x => x.fechaSolicitado >= inicio && x.fechaSolicitado <= fin).ToList();


            if (proyectos != "null" && proveedor != "null")
            {
                List<OrdenCompra> ordenesXProveedorXProyecto = await this._ordenCompraBll.GetAll();

                ordenesXProveedorXProyecto = ordenesXProveedorXProyecto.Where(x => proyectosArr.Exists(z => z == x.proyecto.ToString()) && proveedorArr.Exists(z => z == x.proveedor.ToString())).ToList();
                items= items.Where(x => ordenesXProveedorXProyecto.Exists(z => z.proyecto == x.proyecto)).ToList();
            }
            else if (proyectos != "null" || proveedor != "null")
            {
                List<OrdenCompra> ordenesXProveedorXProyecto = await this._ordenCompraBll.GetAll();

                ordenesXProveedorXProyecto = ordenesXProveedorXProyecto.Where(x => proyectosArr.Exists(z => z == x.proyecto.ToString()) || proveedorArr.Exists(z => z == x.proveedor.ToString())).ToList();
                items= items.Where(x => ordenesXProveedorXProyecto.Exists(z => z.proyecto == x.proyecto)).ToList();
            }

            return items;
        }

        [HttpPost]
        [Route("filtradoReporte")]
        public async Task<List<PedidoMaterialReporte>> FiltrarMaterialesReporte([FromBody] FiltroReporte filtro )
        {
            return await this.bll.FiltrarReporte(filtro);
        }



        [HttpPut("rechazarMaterial")]
        public virtual async Task<bool> RechazarMaterial([FromBody] PedidoMaterial entity)
        {
            var logPath = "C:\\tmp\\rechazar_material_debug.log";
            try
            {
                await System.IO.File.AppendAllTextAsync(logPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] INICIO RechazarMaterial - PedidoId: {entity.id}\n");

                var rechazados = entity.detalle.Where(x => x.rechazado == true);
                await System.IO.File.AppendAllTextAsync(logPath, $"DEBUG: Elementos a rechazar: {rechazados.Count()}\n");

                var entidadActual = (await this.GetById(entity.id));
                await System.IO.File.AppendAllTextAsync(logPath, $"DEBUG: Entidad actual obtenida - Detalle count: {entidadActual.detalle?.Count() ?? 0}\n");

                var actuales = (await this.GetById(entity.id)).detalle.Where(x => x.rechazado == false);
                await System.IO.File.AppendAllTextAsync(logPath, $"DEBUG: Elementos no rechazados actualmente: {actuales.Count()}\n");

                foreach (var rechazado in rechazados)
                {
                    await System.IO.File.AppendAllTextAsync(logPath, $"DEBUG: Procesando rechazo - Descripción: '{rechazado.descripcion}', Referencia: '{rechazado.referencia}', Cantidad: {rechazado.cantidad}\n");

                    IEnumerable<PedidoMaterialDetalle> valorActual = new List<PedidoMaterialDetalle>();

                    valorActual = ( entidadActual.detalle.Where(x => x.descripcion == rechazado.descripcion && x.referencia == rechazado.referencia && x.cantidad == rechazado.cantidad && x.nombre == rechazado.nombre && x.rechazado == false)).ToList();

                    await System.IO.File.AppendAllTextAsync(logPath, $"DEBUG: Elementos encontrados para rechazar: {valorActual.Count()}\n");

                    if (valorActual.Count() > 0)
                    {
                        await System.IO.File.AppendAllTextAsync(logPath, $"DEBUG: Marcando como rechazado: {rechazado.descripcion}\n");
                        valorActual.First().rechazado = true;
                        valorActual.First().observaciones = rechazado.observaciones;
                        await System.IO.File.AppendAllTextAsync(logPath, $"DEBUG: Elemento marcado - rechazado: {valorActual.First().rechazado}, observaciones: '{valorActual.First().observaciones}'\n");
                    }
                    else
                    {
                        await System.IO.File.AppendAllTextAsync(logPath, $"WARNING: No se encontró elemento para rechazar: {rechazado.descripcion}\n");
                    }
                }

                await System.IO.File.AppendAllTextAsync(logPath, $"DEBUG: Iniciando actualización en base de datos\n");
                try
                {
                    await this.bll.Update(entidadActual);
                    await System.IO.File.AppendAllTextAsync(logPath, $"SUCCESS: Actualización completada exitosamente\n");
                }
                catch (Exception updateEx)
                {
                    await System.IO.File.AppendAllTextAsync(logPath, $"ERROR: Error en actualización - {updateEx.Message}\n");
                    await System.IO.File.AppendAllTextAsync(logPath, $"StackTrace: {updateEx.StackTrace}\n");
                }

                await System.IO.File.AppendAllTextAsync(logPath, $"DEBUG: Iniciando envío de emails de rechazo\n");
                foreach (var item in rechazados)
                {
                    var valorActual = actuales.Where(x => x.descripcion == item.descripcion && x.referencia == item.referencia && x.cantidad == item.cantidad);
                    await System.IO.File.AppendAllTextAsync(logPath, $"DEBUG: Enviando email para: {item.descripcion}, encontrados: {valorActual.Count()}\n");

                    if (valorActual.Count() > 0)
                    {
                        try
                        {
                            await this._emailBLL.EmailMaterialRechazado(entity, item);
                            await System.IO.File.AppendAllTextAsync(logPath, $"SUCCESS: Email enviado para: {item.descripcion}\n");
                        }
                        catch (Exception emailEx)
                        {
                            await System.IO.File.AppendAllTextAsync(logPath, $"ERROR: Error enviando email para {item.descripcion} - {emailEx.Message}\n");
                        }
                    }
                }

                await System.IO.File.AppendAllTextAsync(logPath, $"SUCCESS: RechazarMaterial completado exitosamente\n");
                return true;
            }
            catch (Exception ex)
            {
                await System.IO.File.AppendAllTextAsync(logPath, $"ERROR: Error general en RechazarMaterial - {ex.Message}\n");
                await System.IO.File.AppendAllTextAsync(logPath, $"StackTrace: {ex.StackTrace}\n");
                throw;
            }


           
        }


    }
}
