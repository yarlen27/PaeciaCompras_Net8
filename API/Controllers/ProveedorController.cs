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
            try
            {
                Console.WriteLine("DEBUG: Iniciando PostAsync para Proveedor");
                
                // Validación inicial del parámetro entity
                if (entity == null)
                {
                    Console.WriteLine("ERROR: El parámetro entity es null - verificar el JSON del request");
                    throw new ArgumentNullException(nameof(entity), "El objeto Proveedor no puede ser null. Verificar Content-Type y JSON del request.");
                }

                // Sección 1: Extracción de claims
                var claim = this.Request.HttpContext.User.Identities.First().Claims.ToArray().First(x => x.Type == "id");
                try
                {
                    Console.WriteLine($"DEBUG: Claim ID extraído exitosamente: {claim.Value}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: Error extrayendo claims: {ex.Message}");
                    throw new Exception($"Error extrayendo claims de usuario: {ex.Message}");
                }

                // Sección 2: Búsqueda de usuario
                MongoIdentityUser user;
                try
                {
                    user = (await this.userManager.FindByIdAsync(claim.Value));
                    if (user == null)
                    {
                        Console.WriteLine($"ERROR: Usuario no encontrado para ID: {claim.Value}");
                        throw new Exception($"Usuario no encontrado para ID: {claim.Value}");
                    }
                    Console.WriteLine($"DEBUG: Usuario encontrado: {user.UserName}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: Error buscando usuario: {ex.Message}");
                    throw new Exception($"Error buscando usuario: {ex.Message}");
                }

                // Sección 3: Extracción de roles
                var roles = from c in user.Client select c.rol;
                try
                {
                    if (user.Client == null)
                    {
                        Console.WriteLine("ERROR: user.Client es null");
                        throw new Exception("La propiedad Client del usuario es null");
                    }
                    
                    Console.WriteLine($"DEBUG: Roles extraídos exitosamente. Cantidad: {roles.Count()}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: Error extrayendo roles: {ex.Message}");
                    throw new Exception($"Error extrayendo roles: {ex.Message}");
                }

                //
                //if (roles.Contains(5) || roles.Contains(8) || roles.Contains(11))
                //{

                //    throw new Exception();
                //}

                // Sección 4: Asignación de propiedades de usuario a entidad
                try
                {
                    Console.WriteLine($"DEBUG: Entrando a sección 4 - Asignación de propiedades");
                    Console.WriteLine($"DEBUG: entity es null? {(entity == null)}");
                    Console.WriteLine($"DEBUG: user es null? {(user == null)}");
                    
                    if (entity == null)
                    {
                        throw new Exception("entity es null en sección 4");
                    }
                    
                    if (user == null)
                    {
                        throw new Exception("user es null en sección 4");
                    }
                    
                    Console.WriteLine($"DEBUG: user.Id = {(user.Id ?? "NULL")}");
                    Console.WriteLine($"DEBUG: user.UserName = {(user.UserName ?? "NULL")}");
                    
                    if (user.Id == null)
                    {
                        throw new Exception("user.Id es null");
                    }
                    
                    if (user.UserName == null)
                    {
                        throw new Exception("user.UserName es null");
                    }
                    
                    Console.WriteLine($"DEBUG: Asignando entity.userid");
                    entity.userid = user.Id;
                    Console.WriteLine($"DEBUG: Asignando entity.UserName");
                    entity.UserName = user.UserName;
                    Console.WriteLine($"DEBUG: Propiedades de usuario asignadas a entidad");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: Error asignando propiedades de usuario: {ex.Message}");
                    throw new Exception($"Error asignando propiedades de usuario: {ex.Message}");
                }

                // Sección 5: Llamada al método base
                Proveedor result;
                try
                {
                    result = await base.PostAsync(entity);
                    Console.WriteLine($"DEBUG: Proveedor creado exitosamente con ID: {result.id}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: Error en base.PostAsync: {ex.Message}");
                    throw new Exception($"Error creando proveedor: {ex.Message}");
                }

                // Sección 6: Sincronización SQL
                try
                {
                    proveedorSQLBLL.SincronizarProveedor(result.id);
                    Console.WriteLine($"DEBUG: Sincronización SQL completada para proveedor ID: {result.id}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: Error en sincronización SQL: {ex.Message}");
                    // No lanzamos excepción aquí para no afectar el resultado principal
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR GENERAL: Error en PostAsync: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
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
