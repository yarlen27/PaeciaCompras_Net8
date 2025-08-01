﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.BLL;
using Core.Models;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API.Controllers
{
    [Route("api/[controller]")]
    public class ReporteController : Controller
    {
        private OrdenCompraBLL _ordenCompraBLL;

        public ReporteController(OrdenCompraBLL ordenCompraBll)
        {
            this._ordenCompraBLL = ordenCompraBll;
        }

        // GET: api/<controller>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        [HttpPost("totalCategorias")]
        public async Task<List<ResultadoReporteReferencia>>  Post([FromBody]FiltroReporteReferencias filtro)
        {
            return await this._ordenCompraBLL.ReporteReferencias(filtro);

        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
