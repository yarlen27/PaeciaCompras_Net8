using Core.BLL;
using Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProveedorAuthController : BLLController<Proveedor>
    {


        ProveedorBLL _bll;
        FacturaBLL facturabll;
        public ProveedorAuthController(ProveedorBLL bll, FacturaBLL facturabll ) : base(bll)
        {
            this._bll = bll;
            this.facturabll = facturabll;
        }


        [HttpGet("erased/{erased}")]
        public async Task<List<Proveedor>> GetEreased([FromRoute] Boolean erased)
        {
            return await this.BLL.GetAllWithEreased();
        }

    

       
        [HttpGet("pornit/{nit}/{otp}")]
        public async Task<List<Proveedor>> GetNits([FromRoute] string nit, [FromRoute] string otp = null)
        {

            return await this._bll.PorNITs(nit);

        }





        [HttpPost]
        [Route("facturas")]
        public async Task<List<Factura>> GetFacturasFiltered([FromBody] FiltroFacturas param)
        {
            var result = await this.facturabll.FacturasFiltradas(param);
            return result.OrderBy(x => x.fecha).ToList();
        }


        [HttpGet("otp/{nit}")]
        public async Task<string> GenerarOTP([FromRoute] string nit)
        {

            return await this._bll.GenerarOTP(nit);
        }



    }
}
