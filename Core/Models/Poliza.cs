using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
    public class Poliza
    {
        public string garantia { get; set; }
        public string garantiaOtro { get; set; }
        public string cuantia { get; set; }
        public string vigencia { get; set; }

        public Guid id { get; set; }

        public string archivo { get; set; }

    }
}
