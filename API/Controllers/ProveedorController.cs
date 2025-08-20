using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cobalto.SQL.Core.BLL;
using Core.BLL;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using AspNetCore.Identity.MongoDB;
using Cobalto.SQL.Core.Models;
using System.IO;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API.Controllers
{
    [Route("api/[controller]")]
    [Authorize]

    public class ProveedorController : BLLController<Proveedor>
    {
        ProveedorBLL _bll;
        ProveedorSQLBLL proveedorSQLBLL;
        private readonly DocumentoSagrilafBLL documentoSagrilafBLL;
        private readonly UserManager<MongoIdentityUser> userManager;

        public ProveedorController(ProveedorBLL bll, ProveedorSQLBLL proveedorSQLBLL, DocumentoSagrilafBLL documentoSagrilafBLL, UserManager<MongoIdentityUser> userManager) : base(bll)
        {
            this._bll = bll;
            this.proveedorSQLBLL = proveedorSQLBLL;
            this.documentoSagrilafBLL = documentoSagrilafBLL;
            this.userManager = userManager;
        }


        [HttpPost]
        public override async Task<Proveedor> PostAsync([FromBody] Proveedor entity)
        {


            var claim = this.Request.HttpContext.User.Identities.First().Claims.ToArray().First(x => x.Type == "id");

            var user = (await this.userManager.FindByIdAsync(claim.Value));
            
            var roles = from c in user.Client
                        select c.rol;
            //
            //if (roles.Contains(5) || roles.Contains(8) || roles.Contains(11))
            //{

            //    throw new Exception();
            //}

            entity.userid = user.Id;
            entity.UserName = user.UserName;

            var result = await base.PostAsync(entity);



            proveedorSQLBLL.SincronizarProveedor(result.id);

            return result;
        }


        [HttpPost("ArchivosSagrilaf")]
        public async Task<bool> ArchivosSagrilaf([FromBody] List<DocumentoSagrilaf> entity)
        {

            this.documentoSagrilafBLL.InsertarArchivos(entity);

            return true;

        }


        [HttpGet("erased/{erased}")]
        public async Task<List<Proveedor>> GetEreased([FromRoute] Boolean erased)
        {
            return await this.BLL.GetAllWithEreased();
        }


        [HttpGet]
        public override async Task<List<Proveedor>> Get()
        {
            var result = await base.Get();

            return result.OrderBy(x => x.nombre.Trim()).ToList();
        }



        [HttpGet("ArchivosSagrilaf/{id}")]
        public IEnumerable<DocumentoSagrilaf> ArchivosSagrilafProveedor([FromRoute] Guid id)
        {
            return this.documentoSagrilafBLL.PorProveedor(id);
        }


        [HttpGet("Sagrilaf")]
        public async Task<List<DocumentoSagrilafBLL.SagrilafProveedor>> Sagrilaf()
        {
            return await this.documentoSagrilafBLL.Sagrilaf();
        }


        [HttpGet("SagrilafValido/{id}")]
        public async Task<string> SagrilafValido([FromRoute] Guid id)
        {
            return await this.documentoSagrilafBLL.SagrilafValido(id);
        }

        [HttpGet("PausarSagrilaft/{id}")]
        public async Task<bool> PausarSagrilaft([FromRoute] Guid id)
        {
            var proveedor = await this._bll.GetById(id);
            if (proveedor != null)
            {
                proveedor.SagrilaftEnPausa = true;
                await this._bll.Update(proveedor);
                return true;
            }
            return false;
        }

        [HttpGet("ReanudarSagrilaft/{id}")]
        public async Task<bool> ReanudarSagrilaft([FromRoute] Guid id)
        {
            var proveedor = await this._bll.GetById(id);
            if (proveedor != null)
            {
                proveedor.SagrilaftEnPausa = false;
                await this._bll.Update(proveedor);
                return true;
            }
            return false;
        }

        



        [HttpGet("ProximosVencimientos")]
        public async Task<List<Cobalto.SQL.Core.BLL.DocumentoSagrilafBLL.NotificacionSagrilaf>> ProximosVencimientos()
        {
            return await this.documentoSagrilafBLL.ProximosVencimientos();
        }


      


        [HttpPost("SagrilafActualizacion")]
        public async Task<List<string>> SagrilafActualizacion()
        {
            var file = Request.Form.Files[0];
            if (file.Length > 0)
            {
                var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                return await this.documentoSagrilafBLL.SagrilafActualizacion(ms);
            }

            return null;

        }



        [HttpGet("pornit/{nit}")]
        public async Task<Proveedor> GetXNit([FromRoute] string nit)
        {
            return await this._bll.PorNIT(nit);
        }





        [HttpGet("nit/{nit}")]
        public async Task<bool> nit([FromRoute] string nit)
        {
            return await this._bll.NIT(nit);
        }


        [HttpPost("crearDesdeMtto/{id}")]
        public async Task<Proveedor> CrearDesdeMtto([FromRoute] int id)
        {
            return await this._bll.CrearDesdeMtto(id);
        }






    }
}
