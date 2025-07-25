using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
    public class Adjunto
    {


        public Guid Id { get; set; }

        public string NombreDelArchivo { get; set; }

        public string tipo { get; set; }

        public string fechaExpedicion { get; set; }
    }
}
