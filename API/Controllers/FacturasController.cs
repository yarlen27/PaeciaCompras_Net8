using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Core.Bll;
using Core.BLL;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API.Controllers
{
    [Route("api/[controller]")]
    //[Authorize]

    public class FacturasController : BLLController<Factura>
    {
        private FacturaOTBLL otbll;

        public FacturaBLL _bll { get; private set; }
        private FacturaElectronicaBLL _facturaXMLBLL;
        private OrdenCompraBLL _ordenBll;
        private ConsecutivoCausacionBLL consecutivoBll;
        public FacturasController(FacturaBLL bll, FacturaElectronicaBLL facturaXMLBLL, OrdenCompraBLL ordenBll, ConsecutivoCausacionBLL consecutivoBll, FacturaOTBLL otbll) : base(bll)
        {
            this.otbll = otbll;
            _bll = bll;
            _facturaXMLBLL = facturaXMLBLL;
            _ordenBll = ordenBll;
            this.consecutivoBll = consecutivoBll;
        }

        [HttpPut]
        public override async Task<bool> Put(Factura item)
        {

            //var respuesta = await base.Put(item);
             await this._bll.Update(item);

            return true;

            //if (item.isOT.HasValue && item.isOT.Value)
            //{
            //    await _bll.EmailNotificacionAuxiliar(item);
            //}
            //await base 

        }

        [HttpPut("edicionRechazo")]
        public async Task<bool> EdicionRechazo([FromBody] Factura item)
        {
            var respuesta = await base.Put(item);
            if (item.isOT.HasValue && item.isOT.Value)
            {
                await _bll.EmailNotificacionAuxiliar(item);
            }
            return respuesta;

        }

        [HttpGet("buscadorOrdenCompra/{idOrdenCompra}/{proveedor}/{proyecto}")]
        public async Task<Factura> BuscadorOrdenesCompra([FromRoute] Guid idOrdenCompra, [FromRoute] Guid proveedor, [FromRoute] Guid proyecto)
        {
            var respuesta = await this._bll.GetByProterty("idOrdenCompra", idOrdenCompra);
            return respuesta.FirstOrDefault(x => x.idProveedor == proveedor && x.idProyecto == proyecto);
        }


        [HttpGet("AnticiposPendientes/{idProveedor}/{idProyecto}")]
        public async Task<bool> AnticiposPendientes(
          [FromRoute] Guid idProveedor, [FromRoute] Guid idProyecto)
        {
            return await this._bll.AnticiposPendientes(idProveedor, idProyecto);
        }

        [HttpGet("porOrden/{id}")]
        public async Task<List<Factura>> GetByOrdenId([FromRoute] Guid id)
        {
            var facturas = await this._bll.GetByProterty("idOrdenCompra", id);

            facturas = facturas.OrderBy(x => x.aprobada).ThenByDescending(x => x.fecha).ToList();


            return facturas;
        }


        [HttpGet("porOrdenTrabajo/{id}")]
        public async Task<List<string>> porOrdenTrabajo([FromRoute] int id)
        {
            var facturas = await this._bll.GetByProterty("idOT", id);

            var resultado = new List<string>();

            foreach (Factura factura in facturas)
            {
                if (factura.otPaecia.HasValue && factura.otPaecia.Value)
                {
                    resultado.Add($"MP-{factura.numeroFactura}");
                }
                else
                {
                    resultado.Add(factura.numeroFactura);

                }
            }

            return resultado;
        }



        [HttpGet("sinAprobarPorOrden/{id}")]
        public async Task<string> GetsinAprobarPorOrden([FromRoute] Guid id)
        {
            //var facturas = await this._bll.GetByProterty("idOrdenCompra", id);
            var facturas = await this._bll.ContarSinAprobarporOrden(id);
            return facturas.ToString();

            //return $"{facturas.Where(x => !x.aprobada).Count()}";
        }


        [HttpPost("actualizarconsecutivo")]
        public async Task<ConsecutivoCausacion> ActualizarConsecutivo([FromBody] ConsecutivoCausacion entity)
        {
            return await this.consecutivoBll.Insert(entity);
        }

        [HttpGet("existeFactura/{id}/{proveedor}")]
        public async Task<bool> ExistFacturaIdProveedor([FromRoute] string id, [FromRoute] Guid proveedor)
        {
            return await this._bll.ExistFacturaProveedor(id, proveedor);
        }

        // POST api/<controller>
        [HttpPost]
        public override async Task<Factura> PostAsync([FromBody] Factura entity)
        {
            entity.fechaCreado = DateTime.Now;

            if (entity.noteCredito)
            {
                entity.monto = (-1 * entity.monto);
            }

            if (entity.idOrdenCompra.HasValue)
            {
                var order = await this._ordenBll.GetById(entity.idOrdenCompra.Value);

                entity.idProveedor = order.proveedor;
                entity.idProyecto = order.proyecto;
                var facturaExistente = await this._bll.ExistFacturaProveedor(entity.numeroFactura, entity.idProveedor.Value);



                if (facturaExistente == false)
                {

                    return await base.PostAsync(entity);
                }

                return entity;
            }
            else
            {
                if (entity.isOT == true)
                {


                    if (entity.idOT.HasValue && entity.idOT.Value > 0)
                    {
                        entity.aprobadaCoordinadorMantenimiento = true;
                    }

                    var resultado = await base.PostAsync(entity);
                    await this._bll.NotificarFacturaOT(entity);
                    return resultado;

                }
                return await this._bll.CrearConOrdenZero(entity);
            }
        }

        [HttpPut("aprobar")]
        public async Task<Factura> Aprobar([FromBody] Factura entity)
        {
            return await this._bll.Aprobar(entity);
        }

        [HttpPut("editarSinOrdenCompra")]
        public async Task EditarSinOrdenCompra([FromBody] Factura entity)
        {
            await this._bll.AsigarOrdenCeroFactura(entity);
        }


        [HttpGet("AsigarOrdenCero/{id}")]
        public async Task AsigarOrdenCero([FromRoute] Guid id)
        {
            await this._bll.AsigarOrdenCero();
        }



        [HttpGet("impreso/{id}")]
        public async Task<bool> Impreso([FromRoute] Guid id)
        {
            return await this._bll.Impreso(id);
        }

        [HttpGet("impresoContabilidad/{id}")]
        public async Task<bool> ImpresoContabilidad([FromRoute] Guid id)
        {
            return await this._bll.ImpresoTesoreria(id);
        }

        [HttpGet("pagado/{id}/{idUsuario}")]
        public async Task<bool> Pagado([FromRoute] Guid id, [FromRoute] string idUsuario)
        {
            return await this._bll.Pagado(id, idUsuario);
        }

        [HttpPost]
        [Route("datosContables/{idFactura}")]
        public async Task<bool> GetFacturasFiltered([FromRoute] Guid idFactura, [FromBody] InformacionContable informacionContable)
        {
            DatosContables[] datosContables = informacionContable.ToDatosContables();
            var result = await this._bll.DatosContables(idFactura, datosContables.ToList(), informacionContable.usuario, informacionContable);

            return result;
        }


        [HttpPost]
        [Route("ActualizarDatosContables/{idFactura}")]
        public async Task<bool> ActualizarDatosContables([FromRoute] Guid idFactura, [FromBody] InformacionContable informacionContable)
        {
            DatosContables[] datosContables = informacionContable.ToDatosContables();
            var result = await this._bll.ActualizarDatosContables(idFactura, datosContables.ToList(), informacionContable.usuario, informacionContable);

            return result;
        }



        [HttpGet]
        [Route("ImprimirSoloDatosContables/{idFactura}")]
        public async Task ImprimirSoloDatosContables([FromRoute] Guid idFactura)
        {
            await this._bll.ImprimirSoloDatosContables(idFactura);

        }

        [HttpPost]
        [Route("filtrado")]
        public async Task<List<Factura>> GetFacturasFiltered([FromBody] FiltroFacturas param)
        {
            var result = await this._bll.FacturasFiltradas(param);
            return result.OrderBy(x => x.fecha).ToList();
        }


        [HttpPost]
        [Route("filtradoCsv")]
        public async Task<IActionResult> GetFacturasFilteredCsv([FromBody] FiltroFacturas param)
        {
            var reporte = await this._bll.FacturasFiltradasCsv(param);
            var result = new FileContentResult(System.Text.Encoding.UTF8.GetBytes(reporte), "application/octet");
            result.FileDownloadName = "Reporte Facturas.csv";
            HttpContext.Response.Headers.Add("filename", result.FileDownloadName);
            HttpContext.Response.Headers.Add("access-control-expose-headers", "filename");

            return result;
        }


        [HttpPost]
        [Route("filtradoJson")]
        public async Task<List<ReporteFacturasJson>> GetFacturasFilteredJson([FromBody] FiltroFacturas param)
        {
            var reporte = await this._bll.FacturasFiltradasJson(param);


            return reporte;
        }


        [HttpPost("FacturasOT")]
        public async Task<List<Factura>> RecibirFiltroFacturaOtSinAprobacion([FromBody] Filtro filtro)
        {
            return await _bll.FacturasFiltradasOtSinAprobacion(filtro);
        }



        [HttpPost("FacturasOtPrimerAprobacion")]
        public async Task<List<Factura>> RecibirFiltroFacturaOtPrimerAprobacion([FromBody] Filtro filtro)
        {
            return await _bll.FacturasFiltradasOtPrimerAprobacion(filtro);
        }

        [HttpPost]
        [Route("AprobacionMantenimiento")]
        public async Task<Factura> AprobacionMatenimientoCoordinador([FromBody] AprobacionMantenimiento aprobacionMantenimiento)
        {
            return await this._bll.AprobacionCoordinador(aprobacionMantenimiento);
        }

        [HttpGet]
        [Route("ReenviarCorreoAprobacionCoordinadorOT/{idFactura}")]
        public async Task<Factura> ReenviarCorreoAprobacionCoordinadorOT([FromRoute] Guid idFactura)
        {
            return await this._bll.EmailAprobacionCoordinador(idFactura);
        }

        [HttpGet]
        [Route("ObservacionesFactura/{idFactura}")]
        public async Task<List<ObservacionesFactura>> ObservacionesFactura([FromRoute] Guid idFactura)
        {
            return await this._bll.ObservacionesFactura(idFactura, await otbll.OrdenesPorFactura(idFactura));
        }


        [HttpPost]
        [Route("AprobacionAdministradorMantenimiento")]
        public async Task<Factura> AprobacionAdministradorMantenimiento([FromBody] AprobacionMantenimiento aprobacionMantenimiento)
        {

            return await this._bll.AprobacionAdministradorMantenimiento(aprobacionMantenimiento, await otbll.OrdenesPorFactura(aprobacionMantenimiento.IdFactura));

        }

        [HttpPost]
        [Route("RechazoAdministradorMantenimiento")]
        public async Task<Factura> RechazoAdministradorMantenimiento([FromBody] RechazoAdministradorMantenimiento rechazoCoordinadorMantenimiento)
        {
            return await this._bll.RechazoAdministradorMantenimiento(rechazoCoordinadorMantenimiento);
        }

        [HttpPost]
        [Route("RechazoCoordinadorMantenimiento")]
        public async Task<Factura> RechazoCoordinadorMantenimiento([FromBody] RechazoCoordinadorMantenimiento rechazoCoordinadorMantenimiento)
        {
            return await this._bll.RechazoCoordinadorMantenimiento(rechazoCoordinadorMantenimiento);
        }

        [HttpPost]
        [Route("RechazoDirectorMantenimiento")]
        public async Task<Factura> RechazoDirectorMantenimiento([FromBody] RechazoCoordinadorMantenimiento rechazoDirectorMantenimiento)
        {
            return await this._bll.RechazoDirectorMantenimiento(rechazoDirectorMantenimiento);
        }

        [HttpDelete("borrarFactura/{idFactura}")]
        public async Task<bool> BorrarFactura([FromRoute] Guid idFactura)
        {

            return await this._bll.BorrarFacturaBaseDatosXId(idFactura);
        }

        //
        [HttpPost("obtenerFacturasElectronicas")]
        public async Task<List<FacturaElectronica>> FacturasPowerAutomate([FromBody] FiltroFacturas filtro)
        {

            return await _facturaXMLBLL.ObtenerFacturasSinModificar(filtro);
        }

        [HttpPut("facturasElectronicaProcesada/{idFactura}")]
        public async Task<bool> CambiarEstadoFacturaElectronica([FromRoute] Guid idFactura)
        {
            return await _facturaXMLBLL.CambiarEstadoFacturaElectronica(idFactura);

        }
        [HttpDelete("borrarFacturasElectronicas/{idFactura}")]
        public async Task<bool> BorrarFacturasElectronicas([FromRoute] Guid idFactura)
        {
            return await _facturaXMLBLL.BorrarFacturasElectronicas(idFactura);

        }
    }


}
