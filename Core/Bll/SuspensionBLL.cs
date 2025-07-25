using AspNetCore.Identity.MongoDB;
using Cobalto.Mongo.Core.BLL;
using Core.BLL;
using Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Bll
{
    public class SuspensionBLL : BaseBLL<Suspension>
    {

        public SuspensionBLL(IConfiguration configuration, IHttpContextAccessor httpContext, ProyectoBLL proyectoBLL, UserManager<MongoIdentityUser> userManager) : base(configuration, httpContext)
        {
            this._proyectoBLL = proyectoBLL;
            this._userManager = userManager;
        }

        Dictionary<Guid, string> proyectos = new Dictionary<Guid, string>();
        Dictionary<string, string> usuarios = new Dictionary<string, string>();
        private ProyectoBLL _proyectoBLL;
        private UserManager<MongoIdentityUser> _userManager;

        internal async Task<bool> SinCompletar(Guid id)
        {
            var entidades = await this.GetByProterty("idOrden", id);

            return entidades.Count() > 0 && entidades.Where(x => string.IsNullOrEmpty(x.archivoContratoFirmado)).Count() != 0;
        }
    }
}
