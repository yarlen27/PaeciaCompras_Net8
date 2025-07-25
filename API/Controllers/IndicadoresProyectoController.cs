using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.BLL;
using Core.Models;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API.Controllers
{
    [Route("api/[controller]")]
    public class IndicadoresProyectoController : BLLController<Proyecto>
    {
        private ProyectoBLL _bll;
        private OrdenCompraBLL _ordenesbll;
        private FacturaBLL _facturaBLL;

        PedidoMaterialBLL _pedidoMaterialBLL;
        PedidoServicioBLL _pedidoServicioBLL;

        public IndicadoresProyectoController(ProyectoBLL bll, OrdenCompraBLL ordenesbll, FacturaBLL facturaBLL, 
            PedidoMaterialBLL pedidoMaterialBLL, PedidoServicioBLL pedidoServicioBLL) : base(bll)
        {

            this._bll = bll;
            this._ordenesbll = ordenesbll;
            this._facturaBLL = facturaBLL;
            this._pedidoMaterialBLL = pedidoMaterialBLL;
            this._pedidoServicioBLL = pedidoServicioBLL;

        }

        [HttpGet]
        [Route("ejecutadoOrdenes/{id}")]

        public async Task<EjecutadoProyecto> EjecutadoProyecto([FromRoute] Guid id)
        {


            var result =  await this._bll.EjecutadoProyecto(id);

            double totalOrdenes = await this._ordenesbll.TotalProyecto(id);

            result.totalEjecutado = totalOrdenes;

            result.porcentaje = 100.0 * (result.totalEjecutado / result.presupuestoProyecto);

            return result;
        }


        [HttpGet]
        [Route("ejecutadoFacturas/{id}")]

        public async Task<EjecutadoProyecto> EjecutadoProyectoFacturas([FromRoute] Guid id)
        {


            var result = await this._bll.EjecutadoProyecto(id);

            double totalFacturas = await this._facturaBLL.TotalProyecto(id);

            result.totalEjecutado = totalFacturas;

            result.porcentaje = 100.0 * (result.totalEjecutado / result.presupuestoProyecto);

            return result;
        }



        [HttpGet]
        [Route("ejecutadoOrdenesProveedor/{id}")]

        public async Task<List<EjecutadoProyecto>> EjecutadoProyectoOrdenesProveedor([FromRoute] Guid id)
        {


            var result = await this._bll.EjecutadoProyecto(id);

            List<EjecutadoProyecto> totalOrdenesProveedor = await this._ordenesbll.TotalProyectoProveedores(id);

            foreach (var item in totalOrdenesProveedor)
            {
                item.presupuestoProyecto = result.presupuestoProyecto;
                item.porcentaje = 100.0 * (item.totalEjecutado / item.presupuestoProyecto);

            }

            return totalOrdenesProveedor;
        }



        [HttpGet]
        [Route("ejecutadoFacturasProveedor/{id}")]

        public async Task<List<EjecutadoProyecto>> EjecutadoProyectoFacturasProveedor([FromRoute] Guid id)
        {


            var result = await this._bll.EjecutadoProyecto(id);

            List<EjecutadoProyecto> totalFacturasProveedor = await this._facturaBLL.TotalProyectoProveedores(id);

            foreach (var item in totalFacturasProveedor)
            {
                item.presupuestoProyecto = result.presupuestoProyecto;
                item.porcentaje = 100.0 * (item.totalEjecutado / item.presupuestoProyecto);

            }

            return totalFacturasProveedor;
        }



        [HttpGet]
        [Route("ejecutadoOrdenesProveedorMes/{id}")]

        public async Task<List<EjecutadoProyecto>> EjecutadoProyectoOrdenesProveedorMes([FromRoute] Guid id)
        {


            var result = await this._bll.EjecutadoProyecto(id);

            List<EjecutadoProyecto> totalOrdenesProveedor = await this._ordenesbll.TotalProyectoProveedoresMes(id);

            foreach (var item in totalOrdenesProveedor)
            {
                item.presupuestoProyecto = result.presupuestoProyecto;
                item.porcentaje = 100.0 * (item.totalEjecutado / item.presupuestoProyecto);

            }

            return totalOrdenesProveedor;
        }


        [HttpGet]
        [Route("ejecutadoFacturasProveedorMes/{id}")]

        public async Task<List<EjecutadoProyecto>> EjecutadoProyectoFacturasProveedorMes([FromRoute] Guid id)
        {


            var result = await this._bll.EjecutadoProyecto(id);

            List<EjecutadoProyecto> totalFacturasProveedor = await this._facturaBLL.TotalProyectoProveedoresMes(id);

            foreach (var item in totalFacturasProveedor)
            {
                item.presupuestoProyecto = result.presupuestoProyecto;
                item.porcentaje = 100.0 * (item.totalEjecutado / item.presupuestoProyecto);

            }

            return totalFacturasProveedor;
        }



        [HttpPost]
        [Route("reporteMaterial")]

        public async Task<List<PedidoMaterialDetalleReporte>> OrdenesXMaterial([FromBody] FiltroFacturas param)
        {

            List<PedidoMaterialDetalleReporte> result = await this._ordenesbll.MaterialesXFechas(param);
            return result;
        }


        [HttpPost]
        [Route("reporteMaterialCsv")]
        public async Task<IActionResult> GetFacturasFilteredCsv([FromBody] FiltroFacturas param)
        {
            var reporte = await this._ordenesbll.MaterialesXFechasCsv(param);


            var result = new FileContentResult(System.Text.Encoding.UTF8.GetBytes(reporte), "application/octet");
            result.FileDownloadName = "Reporte Material.csv";
            HttpContext.Response.Headers.Add("filename", result.FileDownloadName);
            HttpContext.Response.Headers.Add("access-control-expose-headers", "filename");

            return result;
        }




        [HttpPost]
        [Route("reportePedidos")]

        public async Task<List<PedidoReporte>> Reporte([FromBody] FiltroPedidos param)
        {

            List<PedidoReporte> resultMateriales = await this._pedidoMaterialBLL.Reporte(param);
            List<PedidoReporte> resultServicios = await this._pedidoServicioBLL.Reporte(param);

            resultMateriales.AddRange(resultServicios);
            return resultMateriales;
        }


        [HttpPost]
        [Route("reportePedidosCsv")]
        public async Task<IActionResult> GetFacturasFilteredCsv([FromBody] FiltroPedidos param)
        {

            List<PedidoReporte> resultMateriales = await this._pedidoMaterialBLL.Reporte(param);
            List<PedidoReporte> resultServicios = await this._pedidoServicioBLL.Reporte(param);

            resultMateriales.AddRange(resultServicios);


            var reporte = this._pedidoMaterialBLL.ReporteCsv(resultMateriales);


            var result = new FileContentResult(System.Text.Encoding.UTF8.GetBytes(reporte), "application/octet");
            result.FileDownloadName = "Reporte Pedidos.csv";
            HttpContext.Response.Headers.Add("filename", result.FileDownloadName);
            HttpContext.Response.Headers.Add("access-control-expose-headers", "filename");

            return result;
        }




    }
}
