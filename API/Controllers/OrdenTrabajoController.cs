using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cobalto.Mongo.Core.BLL;
using Core.Bll;
using Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdenTrabajoController : BLLController<OrdenTrabajo>
    {
        public OrdenTrabajoBLL _bll { get; private set; }
        public OrdenTrabajoController(OrdenTrabajoBLL bll) : base(bll)
        {
            _bll = bll;
        }

        [HttpGet("porFactura/{id}")]
        public async Task<List<OrdenTrabajo>> GetByFacturaId([FromRoute] Guid id)
        {
            return await this._bll.GetByProterty("idFactura", id);
        }

        [HttpPost]
        [Route("filtrado")]
        public async Task<List<OrdenTrabajo>> RecibirFiltroOrdenTrabajo([FromBody] Filtro filtro)
        {
            var result = await this._bll.OrdenesTrabajoFiltradas(filtro);

            return result.OrderBy(x => x.fechaCreacion).ToList();
        }

        [HttpPut("AsociarFacturaOT")]
        public async Task<List<OrdenTrabajo>> AsociarFacturaOT([FromBody] AsociacionFacturaOT asociacion)
        {
            return await this._bll.UpdateIdFacturaOT(asociacion);
        }


        [HttpGet("OtSinFacturaAsociada")]
        public async Task<List<OrdenTrabajo>> ConseguirOTsSinFacturaAsociada()
        {
            var resultado = await this._bll.GetOTSinFacturaAsociada();
            //comenta
            return resultado.Where(ordenTrabajo => ordenTrabajo.idFactura == null).ToList();
        }

    }
}
