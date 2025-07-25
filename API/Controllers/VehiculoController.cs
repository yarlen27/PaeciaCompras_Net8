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
    public class VehiculoController : BLLController<Vehiculo>
    {
        public VehiculoController(VehiculoBLL bll) : base(bll)
        {

        }




        [HttpPost("cargamasiva")]
        public async Task CargaMasiva([FromBody] List<Vehiculo>vehiculos)
        {
            foreach (var item in vehiculos)
            {
                var r =  await this.BLL.Insert(item);
            }

        }
    }
}
