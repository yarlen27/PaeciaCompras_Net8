
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
    public class ProyectoController : BLLController<Proyecto>
    {
        private ProyectoBLL _bll;

        public ProyectoController(ProyectoBLL bll) : base(bll)
        {

            this._bll = bll;
        }

        [HttpGet]
        [Route("esdiapedido/{id}")]

        public async  Task<bool> EsDiaPedido([FromRoute] Guid id) {
           return  await this._bll.EsDiaPedido(id);
        }


        [HttpGet]
        [Route("nits")]

        public async Task<List<Empresa>> Empresas()
        {

            return await this._bll.Empresas();
            
        }

        [HttpGet("ProximosVencimientos")]
        public async Task<List<Notificacion>> ProximosVencimientos()
        {
            return await this._bll.ProximosVencimientos();
        }

    }
}
