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
    public class ProveedorEvaluacionController : BaseController<Cobalto.SQL.Core.BLL.ProveedorSQLBLL, Cobalto.SQL.Core.Models.ProveedorSQL>
    {
        public ProveedorEvaluacionController(ProveedorSQLBLL bll) : base(bll)
        {
        }

        [HttpPost("sync")]
        public async Task<int> SincronizarMongoSQLAsync(List<Guid> ids)
        {
            var response = this.bll.Sincronizar(ids);
            return default;
        }


        // GET api/<Solicitudes>/5
        [HttpGet("PorGuid/{id}")]
        public async Task<ProveedorSQL> GetPorGuid(Guid id)
        {
            return await this.bll.PorId(id);
        }


        [HttpGet("porProyecto/{idProyecto}")]
        public async Task<List<ProveedorSQL>> PorProyecto([FromRoute] Guid idProyecto)
        {
            return await this.bll.PorProyecto(idProyecto);
        }

    }
}
