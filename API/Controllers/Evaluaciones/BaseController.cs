using Cobalto.SQL.Core.BLL;
using Cobalto.SQL.Core.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API.Controllers.Evaluaciones
{
    public class BaseController<T, U> : ControllerBase where T : BaseBLL<U> where U : BaseTable
    {

        protected T bll;

        public BaseController(T bll)
        {
            this.bll = bll;
        }


        // GET: api/<Solicitudes>
        [HttpGet]
        public IEnumerable<U> Get()
        {
            return this.bll.Todos();
        }



        // GET api/<Solicitudes>/5
        [HttpGet("{id}")]
        public U Get(int id)
        {
            return this.bll.PorId(id);
        }

        // POST api/<Solicitudes>
        [HttpPost]
        public virtual int? Post([FromBody] U value)
        {
            return this.bll.Insertar(value);
        }


        // PUT api/<Solicitudes>/5
        [HttpPut]
        public void Put([FromBody] U value)
        {

            this.bll.Actualizar(value);
        }


        // DELETE api/<Solicitudes>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            this.bll.Borrar(id);

        }



    }
}
