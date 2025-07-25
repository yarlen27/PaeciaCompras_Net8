using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCore.Identity.MongoDB.Models
{
    public class ClientRol
    {
        public Guid client { get; set; }
        public int rol { get; set; }
        public List<string> proyecto { get; set; }
    }
}
