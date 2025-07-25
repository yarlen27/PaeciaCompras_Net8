using Cobalto.SQL.Core.BLL;
using Cobalto.SQL.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Inventario
{
    [Route("api/[controller]")]
    [ApiController]
    public class CausaRetiroController : BaseController<CausaRetiroBLL, CausaRetiro>
    {
        public CausaRetiroController(CausaRetiroBLL bll) : base(bll)
        {
        }
    }
}
