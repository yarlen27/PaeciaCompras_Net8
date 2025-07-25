using Cobalto.SQL.Core.BLL;
using Cobalto.SQL.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Inventario
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrenteController : BaseController<FrenteBLL, Frente>
    {
        public FrenteController(FrenteBLL bll) : base(bll)
        {
        }
    }
}
