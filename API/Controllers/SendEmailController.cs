using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCore.Identity.MongoDB;
using AspNetCore.Identity.MongoDB.Secure;
using AspNetCore.Identity.MongoDB.Secure.Auth;
using Core.BLL;
using Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API.Controllers
{
    [Route("api/[controller]")]
    public class SendEmailController : BLLController<EmailModel>
    {

        private readonly UserManager<MongoIdentityUser> _userManager;
        private readonly SignInManager<MongoIdentityUser> _signInManager;


        private readonly IJwtFactory _jwtFactory;
        private readonly JwtIssuerOptions _jwtOptions;
        private readonly OrdenCompraBLL _ordenCompraBLL;

        public SendEmailController(EmailBLL bll,

             OrdenCompraBLL ordenCompraBLL,

             UserManager<MongoIdentityUser> userManager,
    SignInManager<MongoIdentityUser> signInManager,
     IJwtFactory jwtFactory,
     IOptions<JwtIssuerOptions> jwtOptions


            ) : base(bll)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtFactory = jwtFactory;
            _jwtOptions = jwtOptions.Value;
            this._ordenCompraBLL = ordenCompraBLL;
        }





        // GET: api/<controller>
        [HttpGet]
        [Route("nuevopedidomateriales/{entityId}")]

        public async Task GetPedidoMateriales([FromRoute] Guid entityId)
        {

            Guid IdTenant = new Guid(this.BLL.clientId);

            //var listaUsuarios = new List<MongoIdentityUser>();
            var listaUsuarios = _userManager.Users;

            listaUsuarios = listaUsuarios.Where(x => x.Client.Any(z => z.client == IdTenant) && x.DeletedOn == null);

            await (this.BLL as EmailBLL).EmailNuevoPedidoMaterial(entityId, listaUsuarios.ToList());
        }




        // GET: api/<controller>
        [HttpGet]
        [Route("nuevopedidoservicios/{entityId}")]

        public async Task GetPedidoServicios([FromRoute] Guid entityId)
        {

            Guid IdTenant = new Guid(this.BLL.clientId);

            //var listaUsuarios = new List<MongoIdentityUser>();
            var listaUsuarios = _userManager.Users;

            listaUsuarios = listaUsuarios.Where(x => x.Client.Any(z => z.client == IdTenant) && x.DeletedOn == null);

            await (this.BLL as EmailBLL).EmailNuevoPedidoServicios(entityId, listaUsuarios.ToList());
        }



        // GET: api/<controller>
        [HttpGet]
        [Route("nuevaorden/{entityId}")]

        public async Task NuevaOrdenDeCompra([FromRoute] Guid entityId)
        {

            await (this.BLL as EmailBLL).EmailNuevaOrden(entityId);
        }


    }
}
