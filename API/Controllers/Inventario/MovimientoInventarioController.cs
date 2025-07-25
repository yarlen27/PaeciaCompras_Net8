using Cobalto.SQL.Core.BLL;
using Cobalto.SQL.Core.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Controllers.Inventario
{
    [Route("api/[controller]")]
    [ApiController]
    public class MovimientoInventarioController : BaseController<MovimientoInventarioBLL, MovimientoInventario>
    {
        public MovimientoInventarioController(MovimientoInventarioBLL bll) : base(bll)
        {
        }

        // POST api/<Solicitudes>
        [HttpPost("registrarOC")]
        public async Task<OrdenCompraSQLDTO> Post([FromBody] RegistroOC oc)
        {
            return await this.bll.RegistrarOC(oc.IdOC, oc.Usuario);
        }


        // POST api/<Solicitudes>
        [HttpPost("IngresarDesdeOC")]
        public IEnumerable<MovimientoInventario> IngresarDesdeOC([FromBody] IngresoDesdeOC ingreso)
        {
            return this.bll.IngresarDesdeOC(ingreso);
        }


        // POST api/<Solicitudes>
        [HttpPost("SalidaDesdeFrente")]
        public IEnumerable<MovimientoInventario> SalidaDesdeFrente([FromBody] SalidaDesdeFrente salida)
        {
            return this.bll.SalidaDesdeFrente(salida);
        }



        // POST api/<Solicitudes>
        [HttpPost("Salida")]
        public MovimientoInventario Salida([FromBody] SalidaSinFrente salida)
        {
            return this.bll.Salida(salida);
        }




        [HttpGet("UltimasCantidadesOC/{idOrden}")]
        public IEnumerable<UltimaCatindadItem> UltimasCantidadesOC([FromRoute] int idOrden)
        {
            return this.bll.UltimasCantidadesOC(idOrden);
        }


        [HttpGet("CantidadesPorProyecto/{idProyecto}")]
        public IEnumerable<CantidadActual> CantidadesPorProyecto([FromRoute] Guid idProyecto)
        {
            return this.bll.CantidadesPorProyecto(idProyecto);
        }



        // POST api/<Solicitudes>
        [HttpPost("IngresarSinOC")]
        public async Task< MovimientoInventario> IngresarSinOC([FromBody] IngresoItemSinOC ingreso)
        {
            return await this.bll.IngresarSinOC(ingreso);
        }


        // POST api/<Solicitudes>
        [HttpPost("ReingresoDevolucion")]
        public async Task<bool> ReingresoDevolucion([FromBody] ReingresoDevolucion reingreso)
        {
            return await this.bll.ReingresoDevolucion(reingreso);
        }



        // POST api/<Solicitudes>
        [HttpGet("SinAprobar/{idProyecto}")]
        public IEnumerable<MovimientoInventario> SinAprobar([FromRoute] Guid idProyecto)
        {
            return this.bll.SinAprobar(idProyecto);
        }



        [HttpPost("Aprobar")]
        public MovimientoInventario Aprobar([FromBody] AprobacionMovimiento aprobacion)
        {
            return this.bll.Aprobar(aprobacion);
        }



        [HttpGet("CantidadesPorProyectoYFrente/{idProyecto}/{idFrente}")]
        public IEnumerable<CantidadActual> CantidadesPorProyectoYFrente([FromRoute] Guid idProyecto, [FromRoute] int idFrente)
        {
            return this.bll.CantidadesPorProyectoYFrente(idProyecto, idFrente);
        }

        [HttpGet("Inventario/{idProyecto}/{idFrente}")]
        public async Task< IEnumerable<CantidadActual>> CantidadesPorProyectoYFrente([FromRoute] Guid idProyecto, [FromRoute] int? idFrente=null)
        {
            if (idFrente == 0)
            {
                idFrente = null;
            }

            return await this.bll.Inventario(idProyecto, idFrente);
        }
        

        [HttpPost("kardex")]
        public async Task<IEnumerable< MovimientoInventario>> Kardex([FromBody] FiltroKardex filtro)
        {
            return await this.bll.Kardex(filtro);
        }


        [HttpPost("ReporteEntradas")]
        public async Task<IEnumerable<MovimientoInventario>> ReporteEntradas([FromBody] FiltroKardex filtro)
        {
            return await this.bll.ReporteEntradas(filtro);
        }

        [HttpPost("ReporteSalidas")]
        public async Task<IEnumerable<MovimientoInventario>> ReporteSalidas([FromBody] FiltroKardex filtro)
        {
            return await this.bll.ReporteSalidas(filtro);
        }


        [HttpPost("PorFrente")]
        public async Task<IEnumerable<CantidadActualFrente>> PorFrente([FromBody] FiltroKardex filtro)
        {
            return await this.bll.PorFrente(filtro);
        }



    }

}



