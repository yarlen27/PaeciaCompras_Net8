using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cobalto.Mongo.Core.BLL;
using Core.BLL;
using Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class encuestaProveedorController : BLLController<EncuestaProveedor>
    {
        private EncuestaProveedorBLL _bll;

        public encuestaProveedorController(EncuestaProveedorBLL bll) : base(bll)
        {

            this._bll = bll;
        }


        [HttpPost]
        public override Task<EncuestaProveedor> PostAsync([FromBody] EncuestaProveedor entity)
        {
            return base.PostAsync(entity);
        }



        [HttpPost]
        [Route("reporte")]

        public async Task<List<EncuestaProveedor>> Reporte([FromBody] FiltroEncuesta param)
        {

            List<EncuestaProveedor> result = await this._bll.Reporte(param);

            return result;
        }


        [HttpPost]
        [Route("reporteCsv")]
        public async Task<IActionResult> ReporteCsv([FromBody] FiltroEncuesta param)
        {

          
            string reporte = await this._bll.ReporteCsv(param);


            var result = new FileContentResult(System.Text.Encoding.UTF8.GetBytes(reporte), "application/octet");
            result.FileDownloadName = "Reporte Encuesta Proveedores.csv";
            HttpContext.Response.Headers.Add("filename", result.FileDownloadName);
            HttpContext.Response.Headers.Add("access-control-expose-headers", "filename");

            return result;
        }

    }
}