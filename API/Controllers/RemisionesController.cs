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
    public class RemisionesController : BLLController<Remision>
    {

        public RemisionBLL _bll { get; private set; }

        public RemisionesController(RemisionBLL bll) : base(bll)
        {

            _bll = bll;
        }


        [HttpGet("porOrden/{id}")]
        public async Task<List<Remision>> GetByOrdenId([FromRoute] Guid id)
        {
            return await this._bll.GetByProterty("idOrdenCompra", id);
        }

        [HttpGet("porFactura/{id}")]
        public async Task<List<Remision>> GetByFacturaId([FromRoute] Guid id)
        {
            return await this._bll.GetByProterty("idFacturaAsociada", id);
        }

        [HttpGet("facturaRemision/{idFactura}/{idRemision}")]
        public async Task<bool> RelacionarFacturaRemision([FromRoute] Guid idFactura, [FromRoute] Guid idRemision)
        {
            var entity = await this._bll.GetById(idRemision);
            entity.idFacturaAsociada = idFactura;
            await this._bll.Update(entity);
            return true;
        }

        [HttpGet("desLigarFactura/{idFactura}/{idRemision}")]
        public async Task<bool> DesligarFactura([FromRoute] Guid idFactura, [FromRoute] Guid idRemision)
        {
            var entity = await this._bll.GetById(idRemision);
            entity.idFacturaAsociada = Guid.Empty;
            await this._bll.Update(entity);
            return true;
        }
    }
}
