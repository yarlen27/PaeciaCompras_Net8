using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using AspNetCore.Identity.MongoDB.Secure.Auth;

namespace AspNetCore.Identity.MongoDB.Secure
{
    public class Tokens
    {
        public static async Task<string> GenerateJwt(ClaimsIdentity identity, IJwtFactory jwtFactory, string userName, JwtIssuerOptions jwtOptions, MongoIdentityUser usuario)
        {
            var response = new
            {
                id = identity.Claims.Single(c => c.Type == "id").Value,
                client = usuario.Client,
                super = usuario.Super,
                auth_token = await jwtFactory.GenerateEncodedToken(userName, identity),
                expires_in = (int)jwtOptions.ValidFor.TotalSeconds,
                usuario = usuario.Nombre,
                userName = usuario.UserName
            };

            return JsonSerializer.Serialize(response);
        }

    }
}
