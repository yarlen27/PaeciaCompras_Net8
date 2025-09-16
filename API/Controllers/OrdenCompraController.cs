

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.BLL;
using Core.Models;
using Microsoft.AspNetCore.Mvc;

using System.Linq;
using Microsoft.AspNetCore.Identity;
using AspNetCore.Identity.MongoDB;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API.Controllers
{
    [Route("api/[controller]")]
    [Authorize]

    public class OrdenCompraController : BLLController<OrdenCompra>
    {
        private EmailBLL _emailBLL;
        private UserManager<MongoIdentityUser> _userManager;

        public OrdenCompraBLL bll { get; private set; }
        public FacturaBLL _facturasBll { get; private set; }


        public OrdenCompraController(OrdenCompraBLL bll, EmailBLL emailBLL, FacturaBLL facturasBLL, UserManager<MongoIdentityUser> userManager) : base(bll)
        {
            this._facturasBll = facturasBLL;
            this._emailBLL = emailBLL;
            this.bll = bll;
        }

        [HttpGet("proyecto/{idProyecto}")]
        public async Task<List<OrdenCompra>> PorProyecto([FromRoute] Guid idProyecto)
        {
            var list = await this.bll.OrdenesValidas(idProyecto);
            return list;
        }




        [HttpPost]
        public override async Task<OrdenCompra> PostAsync([FromBody] OrdenCompra entity)
        {
            //var result = base.PostAsync(entity);
            var result = this.bll.Insert(entity);

            if (this._emailBLL != null)
            {
                await this._emailBLL.EmailNuevaOrden(result.Result.id);
            }

            return result.Result;
        }


        [HttpGet("email/{ordenId}")]
        public async Task Email([FromRoute] Guid ordenId)
        {
            var logPath = "C:\\tmp\\email_orden_debug.log";
            try
            {
                await System.IO.File.AppendAllTextAsync(logPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] INICIO Email - OrdenId: {ordenId}\n");
                
                if (this._emailBLL == null)
                {
                    await System.IO.File.AppendAllTextAsync(logPath, $"ERROR: _emailBLL es null\n");
                    throw new Exception("EmailBLL no está inicializado");
                }
                
                await System.IO.File.AppendAllTextAsync(logPath, $"DEBUG: Llamando a EmailNuevaOrden para orden: {ordenId}\n");
                await this._emailBLL.EmailNuevaOrden(ordenId);
                await System.IO.File.AppendAllTextAsync(logPath, $"DEBUG: EmailNuevaOrden completado exitosamente para orden: {ordenId}\n");
            }
            catch (Exception ex)
            {
                await System.IO.File.AppendAllTextAsync(logPath, $"ERROR: Error en Email endpoint - OrdenId: {ordenId}\n");
                await System.IO.File.AppendAllTextAsync(logPath, $"ERROR: {ex.Message}\n");
                await System.IO.File.AppendAllTextAsync(logPath, $"StackTrace: {ex.StackTrace}\n");
                throw;
            }

            //return result.Result;
        }


        [HttpGet("pendientes")]
        public async Task<List<OrdenCompra>> OrdenesPendientes()
        {
            var ordenesPendientes = await this.bll.OrdenesPendientes();
            return ordenesPendientes;
            //return result.Result;
        }




        [HttpPost]
        [Route("saveBulk")]
        public async Task<List<OrdenCompra>> PostOrdersAsync([FromBody] List<OrdenCompra> entities)
        {
            //var result = base.PostAsync(entity);

            var result = new List<OrdenCompra>();
            foreach (var entity in entities)
            {
                var resultOrden = await this.bll.Insert(entity);

                result.Add(resultOrden);
                if (this._emailBLL != null)
                {
                    await this._emailBLL.EmailNuevaOrden(resultOrden.id);
                }

            }

            return result;
        }



        [HttpGet]
        [Route("filtrado/{inicio}/{fin}/{proyectos}/{proveedor}/{orden}/{factura}")]
        public async Task<List<OrdenCompra>> GetFiltered([FromRoute] DateTime inicio, [FromRoute] DateTime fin, [FromRoute] string proyectos, [FromRoute] string proveedor, [FromRoute] string orden, [FromRoute] string factura)
        {
            var proyectosArr = proyectos.Split(',').ToList();
            var proveedorArr = proveedor.Split(',').ToList();


            if (factura != "null")
            {
                var response = (await this._facturasBll.GetByProterty("numeroFactura", factura)).Select(f => f.idOrdenCompra).ToList();

                List<OrdenCompra> respuesta = new List<OrdenCompra>();

                //Nullable<Guid> idOt;

                if (response.Any() && response.Count>1)
                {
                    
                    

                    foreach (var item in response)
                    {
                        if (item != null)
                        {
                            var ot = await this.bll.GetById(item.Value);
                            respuesta.Add(ot);
                        }
                        
                    }

                    if (proyectos != "null" && proveedor != "null")
                    {
                        respuesta = respuesta.Where(x => proyectosArr.Exists(z => z == x.proyecto.ToString()) && proveedorArr.Exists(z => z == x.proveedor.ToString())).ToList();



                    }
                    else if (proyectos != "null" || proveedor != "null")
                    {
                        respuesta = respuesta.Where(x => proyectosArr.Exists(z => z == x.proyecto.ToString()) || proveedorArr.Exists(z => z == x.proveedor.ToString())).ToList();

                    }
                    return respuesta;
                }
                else
                {
                    var idOt = response.FirstOrDefault();
                    if (idOt != null)
                    {
                        var ot = await this.bll.GetById(idOt.Value);
                        return new List<OrdenCompra>() { ot };
                    }
                    else
                    {
                        return new List<OrdenCompra>();
                    }

                }

                


               



                //items.Where(x => x.estadoFacturas == orden).ToList();
            }
            else
            {
                var items = await this.Get();

                //var proyectosArr = proyectos.Split(',').ToList();
                //var proveedorArr = proveedor.Split(',').ToList();


                items = items.Where(x => x.fechaGenerado >= inicio && x.fechaGenerado <= fin).ToList();




                if (proyectos == "null" && proveedor == "null" && orden == "null" && factura == "null")
                {
                    return items;
                }



                if (proyectos != "null" && proveedor != "null")
                {
                    items = items.Where(x => proyectosArr.Exists(z => z == x.proyecto.ToString()) && proveedorArr.Exists(z => z == x.proveedor.ToString())).ToList();



                }
                else if (proyectos != "null" || proveedor != "null")
                {
                    items = items.Where(x => proyectosArr.Exists(z => z == x.proyecto.ToString()) || proveedorArr.Exists(z => z == x.proveedor.ToString())).ToList();



                }



                if (orden != "null")
                {
                    var ordenInt = Convert.ToInt32(orden.Trim());
                    return items.Where(x => x.consecutivo == ordenInt).ToList();
                }



                else
                {
                    return items;
                }
            }




        }

        [HttpGet]
        [Route("filtradoCsvMateriales/{inicio}/{fin}/{proyectos}/{proveedor}")]
        public async Task<IActionResult> GetFilteredCsv([FromRoute] DateTime inicio, [FromRoute] DateTime fin, [FromRoute] string proyectos, [FromRoute] string proveedor)
        {
            var items = await this.Get();

            var proyectosArr = proyectos.Split(',').ToList();
            var proveedorArr = proveedor.Split(',').ToList();
            items = items.Where(x => x.fechaGenerado >= inicio && x.fechaGenerado <= fin).ToList();


            var result = items;

            if (proyectos == "null" && proveedor == "null")
            {

            }
            else if (proyectos != "null" && proveedor != "null")
            {
                result = items.Where(x => proyectosArr.Exists(z => z == x.proyecto.ToString()) && proveedorArr.Exists(z => z == x.proveedor.ToString())).ToList();
            }
            else
            {
                result = items.Where(x => proyectosArr.Exists(z => z == x.proyecto.ToString()) || proveedorArr.Exists(z => z == x.proveedor.ToString())).ToList();

            }


            var reporte = await this.bll.ReporteCsv(result.Where(x => x.pedidoMaterial == true).ToList());


            var csv = new FileContentResult(System.Text.Encoding.UTF8.GetBytes(reporte), "application/octet");
            csv.FileDownloadName = "Reporte Ordenes Material.csv";
            HttpContext.Response.Headers.Add("filename", csv.FileDownloadName);
            HttpContext.Response.Headers.Add("access-control-expose-headers", "filename");

            return csv;


        }

        [HttpGet]
        [Route("filtradoJsonMateriales/{inicio}/{fin}/{proyectos}/{proveedor}")]
        public async Task<List<ReporteOrdenesCompraJson>> GetFilteredJson([FromRoute] DateTime inicio, [FromRoute] DateTime fin, [FromRoute] string proyectos, [FromRoute] string proveedor)
        {
            var items = await this.Get();

            var proyectosArr = proyectos.Split(',').ToList();
            var proveedorArr = proveedor.Split(',').ToList();
            items = items.Where(x => x.fechaGenerado >= inicio && x.fechaGenerado <= fin).ToList();


            var result = items;

            if (proyectos == "null" && proveedor == "null")
            {

            }
            else if (proyectos != "null" && proveedor != "null")
            {
                result = items.Where(x => proyectosArr.Exists(z => z == x.proyecto.ToString()) && proveedorArr.Exists(z => z == x.proveedor.ToString())).ToList();
            }
            else
            {
                result = items.Where(x => proyectosArr.Exists(z => z == x.proyecto.ToString()) || proveedorArr.Exists(z => z == x.proveedor.ToString())).ToList();

            }


            var reporte = await this.bll.ReporteJson(result.Where(x => x.pedidoMaterial == true).ToList());

            return reporte;


        }




        [HttpGet]
        [Route("filtradoCsvServicio/{inicio}/{fin}/{proyectos}/{proveedor}")]
        public async Task<IActionResult> GetFilteredServicioCsv([FromRoute] DateTime inicio, [FromRoute] DateTime fin, [FromRoute] string proyectos, [FromRoute] string proveedor)
        {
            var items = await this.Get();

            var proyectosArr = proyectos.Split(',').ToList();
            var proveedorArr = proveedor.Split(',').ToList();
            items = items.Where(x => x.fechaGenerado >= inicio && x.fechaGenerado <= fin).ToList();


            var result = items;
            if (proyectos == "null" && proveedor == "null")
            {

            }
            else if (proyectos != "null" && proveedor != "null")
            {
                result = items.Where(x => proyectosArr.Exists(z => z == x.proyecto.ToString()) && proveedorArr.Exists(z => z == x.proveedor.ToString())).ToList();
            }
            else
            {
                result = items.Where(x => proyectosArr.Exists(z => z == x.proyecto.ToString()) || proveedorArr.Exists(z => z == x.proveedor.ToString())).ToList();

            }


            var reporte = await this.bll.ReporteCsvServicio(result.Where(x => x.pedidoMaterial == false).ToList());


            var csv = new FileContentResult(System.Text.Encoding.UTF8.GetBytes(reporte), "application/octet");
            csv.FileDownloadName = "Reporte Ordenes Servicio.csv";
            HttpContext.Response.Headers.Add("filename", csv.FileDownloadName);
            HttpContext.Response.Headers.Add("access-control-expose-headers", "filename");

            return csv;


        }


        [HttpGet]
        [Route("filtradoJsonServicio/{inicio}/{fin}/{proyectos}/{proveedor}")]
        public async Task<List<ReporteServicioJson>> GetFilteredServicioJson([FromRoute] DateTime inicio, [FromRoute] DateTime fin, [FromRoute] string proyectos, [FromRoute] string proveedor)
        {
            var items = await this.Get();

            var proyectosArr = proyectos.Split(',').ToList();
            var proveedorArr = proveedor.Split(',').ToList();
            items = items.Where(x => x.fechaGenerado >= inicio && x.fechaGenerado <= fin).ToList();


            var result = items;
            if (proyectos == "null" && proveedor == "null")
            {

            }
            else if (proyectos != "null" && proveedor != "null")
            {
                result = items.Where(x => proyectosArr.Exists(z => z == x.proyecto.ToString()) && proveedorArr.Exists(z => z == x.proveedor.ToString())).ToList();
            }
            else
            {
                result = items.Where(x => proyectosArr.Exists(z => z == x.proyecto.ToString()) || proveedorArr.Exists(z => z == x.proveedor.ToString())).ToList();

            }


            var reporte = await this.bll.ReporteJsonServicio(result.Where(x => x.pedidoMaterial == false).ToList());
            return reporte;


        }


        [HttpGet]
        [Route("contratoPendiente")]
        public async Task<List<OrdenCompra>> GetContratoPendiente()
        {
            var items = await this.Get();

            return items.Where(x => !x.pedidoMaterial && !x.contratoFirmado && x.consecutivo != 0).ToList();

        }

        [HttpGet]
        [Route("porId/{idOrder}")]
        public async Task<OrdenCompra> PorId([FromRoute] Guid idOrder)
        {
            var items = await this.GetById(idOrder);

            return items;

        }



        [HttpGet]
        [Route("calcularCupo/{idProyecto}/{idProveedor}")]
        public async Task<CupoDisponibleProyecto> CalcularCupo([FromRoute] Guid idProyecto, [FromRoute] Guid idProveedor)
        {

            return await this.bll.CalcularCupo(idProyecto, idProveedor);


        }


        [HttpGet]
        [Route("ordenPorConsecutivo/{consecutivo}/{idProyecto}/{idProveedor}")]
        public async Task<OrdenCompra> OrdenPorConsecutivo([FromRoute] int consecutivo, [FromRoute] Guid idProyecto, [FromRoute] Guid idProveedor)
        {

            return await this.bll.ObtenerPorConsecutivo(consecutivo, idProyecto, idProveedor);


        }


        [HttpGet]
        [Route("ordenPorConsecutivo/{consecutivo}/{idProyecto}")]
        public async Task<OrdenCompra> OrdenXConsecutivo([FromRoute] int consecutivo, [FromRoute] Guid idProyecto, [FromRoute] Guid idProveedor)
        {

            return await this.bll.ObtenerPorConsecutivo(consecutivo, idProyecto);


        }



        [HttpGet]
        [Route("liberarCupo/{idOrden}")]
        public async Task<OrdenCompra> liberarCupo([FromRoute] Guid idOrden)
        {

            var item = await this.bll.GetById(idOrden);
            item.pagada = true;
            await this.bll.Update(item);
            return item;
        }



        [HttpPost]
        [Route("reasignar/{idOrden}")]
        public async Task<result> Reasignar([FromBody] List<OrdenCompra> entities, [FromRoute] Guid idOrden)
        {
            //var result = base.PostAsync(entity);

            //var result = new List<OrdenCompra>();


            var ordenActual = await this.bll.GetById(idOrden);

            foreach (var nuevaOrden in entities)
            {

                try
                {
                    var material = nuevaOrden.detalle.FirstOrDefault();

                    if (material != null)
                    {
                        foreach (var item in material.detalleMaterial)
                        {

                            buscarMaterialEnOrdebActual(item, ordenActual);
                        }

                    }

                    await this.bll.Update(ordenActual);

                    var result = this.bll.Insert(nuevaOrden);

                    if (this._emailBLL != null)
                    {
                        await this._emailBLL.EmailNuevaOrden(result.Result.id);
                    }


                }
                catch (Exception ex)
                {

                    return new result { msg = "error" };
                }

            }

            //return result;

            return null;
        }

        private void buscarMaterialEnOrdebActual(PedidoMaterialDetalle item, OrdenCompra ordenActual)
        {
            var detalleOrdenActual = ordenActual.detalle.First();

            var itemAnterior = JsonConvert.DeserializeObject<PedidoMaterialDetalle>(item.observaciones);

            var materialActual = detalleOrdenActual.detalleMaterial.Where(x => x.descripcion == itemAnterior.descripcion && x.cantidad == itemAnterior.cantidad).FirstOrDefault();

            if (materialActual != null)
            {
                detalleOrdenActual.detalleMaterial.Remove(materialActual);

            }

            item.observaciones = itemAnterior.observaciones;


        }

        public class result
        {

            public string msg { get; set; }
        }


    }
}
