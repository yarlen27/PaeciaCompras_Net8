using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Cobalto.SQL.Core.BLL;
using Cobalto.SQL.Core.Models;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API.Controllers.Evaluaciones
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemEvaluarLikerController : BaseController<ItemEvaluarLikerBLL, ItemEvaluarLiker>
    {
        public ItemEvaluarLikerController(ItemEvaluarLikerBLL bll) : base(bll)
        {
        }

        [HttpPost("likerCriterios")]
        public int? InsertarItemCriterios([FromBody] ItemsLiker value)
        {
            return this.bll.InsertarItemCriterios(value);
        }
    }
}
