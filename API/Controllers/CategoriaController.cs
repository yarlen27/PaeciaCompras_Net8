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
    public class CategoriaController : BLLController<Categoria>
    {


        public CategoriaController(CategoriaBLL bll) : base(bll)
        {
        }
    }
}