using Cobalto.SQL.Core.BLL;
using Cobalto.SQL.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Controllers.Inventario
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResponsableController : BaseController<ResponsableBLL, Responsable>
    {
        public ResponsableController(ResponsableBLL bll) : base(bll)
        {
        }
    }
}



