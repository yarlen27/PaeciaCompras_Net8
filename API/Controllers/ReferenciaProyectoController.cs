using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Core.Bll;
using Core.Models;
using DigitalOceanUploader.Shared;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API.Controllers
{
    [Route("api/[controller]")]
    public class ReferenciaProyectoController : BLLController<ReferenciaProyecto>
    {
        private CategoriaBLL categoriaBll;

        DigitalOceanUploadManager uploadManager;
        private IWebHostEnvironment _hostingEnvironment;
        ReferenciaProyectoBLL bll;
        private ReferenciaBLL referenciabll;

        public ReferenciaProyectoController(IWebHostEnvironment hostingEnvironment, DigitalOceanUploadManager uploadManager, ReferenciaProyectoBLL bll, CategoriaBLL categoriaBll, ReferenciaBLL referenciabll) : base(bll)
        {
            _hostingEnvironment = hostingEnvironment;
            this.uploadManager = uploadManager;
            this.categoriaBll = categoriaBll;
            this.bll = bll;
            this.referenciabll = referenciabll;
        }




        [HttpPost("bulk"), DisableRequestSizeLimit]
        public virtual async Task<List<ReferenciaProyecto>> PostBulkAsync([FromBody] List<ReferenciaProyecto> entity)
        {
            var insert = await this.bll.InsertBulk(entity, false);
            return insert;
        }


        //[HttpPost("aprobadaProyecto"), DisableRequestSizeLimit]

        [HttpGet("aprobadaProyecto/{proyecto}/{referencia}")]

        public virtual async Task<ReferenciaProyecto> AprobadaProyecto([FromRoute] Guid proyecto, [FromRoute] Guid referencia)
        {

            ReferenciaProyecto entity = new ReferenciaProyecto { proyecto = proyecto, referencia  = referencia };
            return await this.bll.ObtenerReferenciaAprobadaPorProyecto(entity);
            
        }



        [HttpPost("bulkaprobado"), DisableRequestSizeLimit]
        public virtual async Task<List<ReferenciaProyecto>> PostBulAprobadokAsync([FromBody] List<ReferenciaProyecto> entity)
        {
            var insert = await this.bll.InsertBulk(entity, true);
            return insert;
        }



        [HttpGet("proyecto/{id}")]
        public virtual async Task<List<grupoCategoria>> GetProProyectoAsync([FromRoute] Guid id)
        {
            var referencias = await this.BLL.GetByProterty("proyecto", id);

            var categoryDictionary = new List<grupoCategoria>();

            var grp = referencias.GroupBy(x=>x.categoria);


            foreach (var item in grp)
            {

                var categoriaID = item.Key;
                var categoria = await this.categoriaBll.GetById(categoriaID);
                if (categoria != null)
                {
                    var grupoCategoria = new grupoCategoria();
                    grupoCategoria.categoria = categoria;
                    grupoCategoria.items = item.ToList();
                    categoryDictionary.Add(grupoCategoria);

                    foreach (var referencia in grupoCategoria.items)
                    {
                        var objReferencia = await this.referenciabll.GetById(referencia.referencia);
                        referencia.objReferencia = objReferencia;
                    }
                }
            }



            return categoryDictionary;
        }


        [HttpGet("aprobadasProyecto/{id}")]
        public virtual async Task<List<grupoCategoria>> GetAprobadasPorProyectoAsync([FromRoute] Guid id)
        {

            var filter = Builders<ReferenciaProyecto>.Filter.Eq("proyecto", id) & Builders<ReferenciaProyecto>.Filter.Eq("aprobado", true);
            var referencias = await this.BLL.GetByProterty(filter);

            var categoryDictionary = new List<grupoCategoria>();

            var grp = referencias.GroupBy(x => x.categoria);


            foreach (var item in grp)
            {

                var categoriaID = item.Key;
                var categoria = await this.categoriaBll.GetById(categoriaID);
                if (categoria != null)
                {
                    var grupoCategoria = new grupoCategoria();
                    grupoCategoria.categoria = categoria;
                    grupoCategoria.items = item.ToList();
                    categoryDictionary.Add(grupoCategoria);

                    foreach (var referencia in grupoCategoria.items)
                    {
                        var objReferencia = await this.referenciabll.GetById(referencia.referencia);
                        referencia.objReferencia = objReferencia;
                    }
                }
            }



            return categoryDictionary;
        }


        public class grupoCategoria
        {
            public Categoria categoria { get; set; }

            public List<ReferenciaProyecto> items { get; set; }
        }
    }
}
