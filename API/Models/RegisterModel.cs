using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace API.Models
{
    public class RegisterModel
    {
       
        public string Email { get; set; }
        public string UserName { get; set; }

        public string Nombre { get; set; }
        public string Apellido { get; set; }


        public string Identificacion { get; set; }

        public string Password { get; set; }
       
        public string ConfirmPassword { get; set; }

        public List<AspNetCore.Identity.MongoDB.Models.ClientRol> Client { get; set; }

    }
}
