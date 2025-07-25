using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNetCore.Identity.MongoDB;
using AspNetCore.Identity.MongoDB.Secure;
using AspNetCore.Identity.MongoDB.Secure.Auth;
using API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;
using System.Net;
using  Microsoft.AspNetCore.Mvc;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API.Controllers
{
    [Route("api/[controller]")]
    public class AccountController : Controller
    {

        private readonly UserManager<MongoIdentityUser> _userManager;
        private readonly SignInManager<MongoIdentityUser> _signInManager;


        private readonly IJwtFactory _jwtFactory;
        private readonly JwtIssuerOptions _jwtOptions;


        public AccountController(
            UserManager<MongoIdentityUser> userManager,
            SignInManager<MongoIdentityUser> signInManager,
             IJwtFactory jwtFactory,
             IOptions<JwtIssuerOptions> jwtOptions)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtFactory = jwtFactory;
            _jwtOptions = jwtOptions.Value;
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Post([FromBody] LoginModel credentials)
        {

            try
            {

                var identity = await GetClaimsIdentity(credentials.UserName, credentials.Password);
                if (identity == null)
                {
                    throw new Exception("login_failure");// Errors.AddErrorToModelState("login_failure", "Invalid username or password.", ModelState));
                }

                var usuario = await _userManager.FindByNameAsync(credentials.UserName);
                var jwt = await Tokens.GenerateJwt(identity, _jwtFactory, credentials.UserName, _jwtOptions, usuario);
                return new OkObjectResult(jwt);
            }
            catch (Exception ex)
            {

                throw;
            }
        }



        [HttpGet]
        [Authorize]
        [Route("Usuarios/{IdTenant}")]
        public List<MongoIdentityUser> Get([FromRoute] Guid IdTenant)
        {

            try
            {
                var listaUsuarios = new List<MongoIdentityUser>();
                listaUsuarios = _userManager.Users.ToList();
                if (IdTenant == Guid.Empty)
                {
                    //es consulta de todos los usuarios
                    return listaUsuarios;
                }

                listaUsuarios = listaUsuarios.Where(x => x.Client != null).ToList();
                listaUsuarios = listaUsuarios.Where(x => x.Client.Any(z => z.client == IdTenant)).ToList();


                return listaUsuarios;
            }
            catch (Exception ex)
            {

                throw;
            }

        }




        [HttpGet]
        [Authorize]
        [Route("UsuariosNoBorrados/{IdTenant}")]
        public List<MongoIdentityUser> GetNoBorrados([FromRoute] Guid IdTenant)
        {

            try
            {
                var listaUsuarios = new List<MongoIdentityUser>();
                listaUsuarios = _userManager.Users.ToList();
                if (IdTenant == Guid.Empty)
                {
                    //es consulta de todos los usuarios
                    return listaUsuarios;
                }
                listaUsuarios = listaUsuarios.Where(x => x.Client != null).ToList();

                listaUsuarios = listaUsuarios.Where(x => x.Client.Any(z => z.client == IdTenant) && x.DeletedOn == null).ToList();
                
                var listaUsuariosInnecesarios = (from u in listaUsuarios
                                                 from r in u.Client
                                                 where r.rol == 15
                                                 || r.rol == 17
                                                 || r.rol == 18
                                                 select u).ToList();

                foreach (var item in listaUsuariosInnecesarios)
                {
                    listaUsuarios.Remove(item);
                }

                return listaUsuarios;
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        [HttpPost]
        [Route("ResetPass")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordViewModel model)
        {

            try
            {
                var user = await _userManager.FindByNameAsync(model.userName);
                var result = await _userManager.RemovePasswordAsync(user);
                result = await _userManager.AddPasswordAsync(user, model.password);
                if (result.Succeeded)
                {
                    return new OkResult();
                }
                else
                {

                    throw new Exception("Error");
                }
            }
            catch (Exception ex)
            {

                throw;
            }


        }


        //Post agregar usuario
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] RegisterModel model)
        {



            var user = new MongoIdentityUser(model.UserName, model.Email, model.Nombre, model.Apellido, model.Identificacion, model.Client);
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return StatusCode(500, result.Errors.First().Description);
            }
            else
            {
                return Ok(model);
            }
        }


        //Post Editar
        [HttpPost]
        [Route("Editar")]
        public async Task<RegisterModel> Edit([FromBody] RegisterModel model)
        {

            var user = _userManager.FindByNameAsync(model.UserName).Result;
            user.Identificacion = model.Identificacion;
            user.Nombre = model.Nombre;
            user.Apellido = model.Apellido;
            user.Client = model.Client;
            user.SetEmail(model.Email);
            var result = await _userManager.UpdateAsync(user);
            return model;
        }

        [HttpDelete]
        [Route("{userName}")]
        public async Task<MongoIdentityUser> Remove([FromRoute] string userName)
        {

            try
            {
                var user = await _userManager.FindByIdAsync(userName);

                var result = await _userManager.DeleteAsync(user);
                return user;
            }
            catch (Exception ex)
            {

                throw;
            }
        }


        private async Task<ClaimsIdentity> GetClaimsIdentity(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                return await Task.FromResult<ClaimsIdentity>(null);

            // get the user to verifty
            var userToVerify = await _userManager.FindByNameAsync(userName);

            if (userToVerify == null) return await Task.FromResult<ClaimsIdentity>(null);

            // check the credentials
            if (await _userManager.CheckPasswordAsync(userToVerify, password))
            {
                return await Task.FromResult(_jwtFactory.GenerateClaimsIdentity(userName, userToVerify.Id));
            }

            // Credentials are invalid, or account doesn't exist
            return await Task.FromResult<ClaimsIdentity>(null);
        }




        [HttpGet]
        [Authorize]
        [Route("UsuariosEncargados/{IdTenant}")]
        public IEnumerable<MongoIdentityUser> GetUsuariosEncargados([FromRoute] Guid IdTenant)
        {

            try
            {

                var usuarios = this._userManager.Users.Where(u => u.DeletedOn == null).ToList();

                var listaUsuarios = from u in usuarios
                                    from r in u.Client
                                    where r.rol == 15
                                    || r.rol == 17
                                    || r.rol == 18
                                    && r.client == IdTenant
                                    select u;

                return listaUsuarios.Distinct().ToList();
            }
            catch (Exception ex)
            {

                throw;
            }

        }


    }
}
