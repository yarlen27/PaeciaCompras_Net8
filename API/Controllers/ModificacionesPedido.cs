using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Bll;
using Core.BLL;
using Core.Models;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API.Controllers
{
    [Route("api/[controller]")]
    public class ModificacionesPedido : Controller
    {


        private readonly SuspensionBLL _suspensionBll;
        private readonly AdicionPedidoServicioBLL _adicionPedidoServicioBll;
        private readonly ReanudacionBLL _reanudacionBll;
        private readonly OrdenCompraBLL _ordenCompraBll;

        public ModificacionesPedido(AdicionPedidoServicioBLL adicionPedidoServicioBll, ReanudacionBLL reanudacionBll, SuspensionBLL suspensionBll, OrdenCompraBLL oderCompraBll)
        {
            this._suspensionBll = suspensionBll ?? throw new ArgumentNullException(nameof(suspensionBll));
            this._adicionPedidoServicioBll = adicionPedidoServicioBll ?? throw new ArgumentNullException(nameof(adicionPedidoServicioBll));
            this._reanudacionBll = reanudacionBll ?? throw new ArgumentNullException(nameof(reanudacionBll));
            this._ordenCompraBll = oderCompraBll;
        }


        [HttpPost]
        [Route("adicion")]
        public async Task<AdicionPedidoServicio> PostAdicionAsync([FromBody] AdicionPedidoServicio entity)
        {

            var insert = await this._adicionPedidoServicioBll.Insert(entity);
            await this._ordenCompraBll.Pendiente(entity.idOrden);
            return insert;
        }



        [HttpPost]
        [Route("suspension")]
        public async Task<Suspension> PostSuspensionAsync([FromBody] Suspension entity)
        {

            var insert = await this._suspensionBll.Insert(entity);
            await this._ordenCompraBll.Pendiente(entity.idOrden);

            return insert;
        }


        [HttpPost]
        [Route("reanudacion")]
        public async Task<Reanudacion> PostReanudacionAsync([FromBody] Reanudacion entity)
        {

            var insert = await this._reanudacionBll.Insert(entity);
            await this._ordenCompraBll.Pendiente(entity.idOrden);

            return insert;
        }



        [HttpGet]
        [Route("adicion/pororden/{id}")]
        public async Task<List<AdicionPedidoServicio>> GetAdicionesPorOrdenAsync([FromRoute] Guid id)
        {

            var lista = await this._adicionPedidoServicioBll.GetByProterty("idOrden", id);

            return lista;
        }



        [HttpGet]
        [Route("suspension/pororden/{id}")]
        public async Task<List<Suspension>> GetSuspensionesPorOrdenAsync([FromRoute] Guid id)
        {

            var lista = await this._suspensionBll.GetByProterty("idOrden", id);

            return lista;
        }
        [HttpGet]
        [Route("reanudacion/pororden/{id}")]
        public async Task<List<Reanudacion>> GetReanudacionPorOrdenAsync([FromRoute] Guid id)
        {

            var lista = await this._reanudacionBll.GetByProterty("idOrden", id);

            return lista;
        }



        /**********/

        [HttpPut]
        [Route("adicion")]
        public async Task PutAdicionAsync([FromBody] AdicionPedidoServicio entity)
        {

            await this._adicionPedidoServicioBll.Update(entity);
            await this._ordenCompraBll.NoPendiente(entity.idOrden);
        }


        [HttpPut]
        [Route("suspension")]
        public async Task PutSuspensionAsync([FromBody] Suspension entity)
        {

            await this._suspensionBll.Update(entity);
            await this._ordenCompraBll.NoPendiente(entity.idOrden);

        }
        [HttpPut]
        [Route("reanudacion")]
        public async Task PutReanudacionAsync([FromBody] Reanudacion entity)
        {

            await this._reanudacionBll.Update(entity);
            await this._ordenCompraBll.NoPendiente(entity.idOrden);

        }
    }
}
