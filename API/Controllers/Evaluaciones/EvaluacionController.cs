using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Cobalto.SQL.Core.BLL;
using Cobalto.SQL.Core.Models;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API.Controllers.Evaluaciones
{
    [Route("api/[controller]")]
    [ApiController]
    public class EvaluacionController : BaseController<EvaluacionBLL, Evaluacion>
    {
        public EvaluacionController(EvaluacionBLL bll) : base(bll)
        {
        }

        [HttpGet("porProveedor/{id}")]
        public EvaluacionVacia ObtenerEvaluacionCompletaXProveedor([FromRoute] int id)
        {
            return this.bll.ObtenerEvaluacionXProveedor(id);
        }



        [HttpGet("SeleccionPorProveedor/{id}")]
        public EvaluacionVacia ObtenerEvaluacionCompletaXProveedor([FromRoute] Guid id)
        {
            return this.bll.SeleccionPorProveedor(id);
        }


        [HttpGet("porId/{id}")]
        public EvaluacionVacia ObtenerEvaluacionCompletaXId([FromRoute] int id)
        {
            return this.bll.ObtenerEvaluacionCompletaXId(id);
        }


        [HttpGet("porProveedorGuid/{idMongo}")]
        public EvaluacionVacia ObtenerEvaluacionCompletaXProveedorGuid([FromRoute] Guid idMongo)
        {
            return this.bll.ObtenerEvaluacionXProveedor(idMongo);
        }

        [HttpPost("porProveedor")]
        public int ObtenerEvaluacionCompletaXProveedor([FromBody] EvaluacionRealizada evaluacionRealizada)
        {
            return this.bll.GuardarEvaluacionXProveedor(evaluacionRealizada);
        }

        [HttpPost("Reporte")]
        public async Task<List<RegistroReporteEvaluacion>> ObtenerReporte([FromBody] FiltroEvaluacion filtro)
        {
            return await this.bll.ObtenerReporte(filtro);
        }


        [HttpPost("ReporteConsolidado")]
        public async Task<List<RegistroReporteConsolidado>> ReporteConsolidado([FromBody] FiltroEvaluacion filtro)
        {
            return await this.bll.ObtenerReporteConsolidado(filtro);
        }


    }
}
