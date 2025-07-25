using System;
using System.Collections.Generic;
using System.IO;
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
    public class ReferenciaController : BLLController<Referencia>
    {
        AutoGeneradoBLL autobll;

        public ReferenciaController(ReferenciaBLL bll, AutoGeneradoBLL autoBll) : base(bll)
        {

            this.autobll = autoBll;
        }


        [HttpGet("codigo")]
        public async Task<autogenerado> codigo([FromRoute] Guid id)
        {
            var insert = await this.autobll.Insert(new AutoGenerado());
            long codigo = await this.autobll.CountAll();

            return new autogenerado() { id = codigo };
        }

        [HttpGet("PorCategoria/{idCategoria}")]
        public async Task<IEnumerable<Referencia>> PorCategoria([FromRoute] Guid idCategoria)
        {
            return await (this.BLL as ReferenciaBLL).PorCategoria(idCategoria);
        }


        public class autogenerado
        {
            public long id { get; set; }
        }

        [HttpPost("cargaMasiva")]
        public async Task<List<string>> CargaMasiva()
        {

            var file = Request.Form.Files[0];
            List<string> respuesta = new List<string>();
            if (file.Length > 0)
            {
                var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                respuesta = await (this.BLL as ReferenciaBLL).CrearItems(ms);

            }


            return respuesta;


        }







    }
}