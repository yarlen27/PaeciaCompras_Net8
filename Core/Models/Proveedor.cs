using Cobalto.Mongo.Core.Attributes;
using Cobalto.Mongo.Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
    [CollectionName("proveedor")]

    public class Proveedor : CollectionDTO
    {
        public string nombre { get; set; }
        public string nit { get; set; }
        public string direccion { get; set; }
        public string telefono { get; set; }
        public string email { get; set; }
        public string categoria { get; set; }
        public string contacto { get; set; }
        public double? cupoCreditoGeneral { get; set; }
        public List<Adjunto> adjuntos { get; set; }
        public List<CupoProyecto> cupoProyecto { get; set; }
        public List<Categoria> categorias { get; set; }
        public string fechaDatosContables { get; set; }
        public int consecutivoDatosCotables { get; set; }
        public bool evaluacionSeleccion { get; set; }

        public string userid { get; set; }

        public string UserName { get; set; }

        //ACTAS
        public string representanteLegal { get; set; }
        public string cedulaRepresentanteLegal { get; set; }
        public string numeroCuenta { get; set; }
        public string tipoCuenta { get; set; }
        public string banco { get; set; }



        //Sagrilaf
        public string ClasificacionSagrilaf { get; set; }
        public bool? SagrilaftEnPausa { get; set; }


        //Lista documentos sagrilaf 
        //public List<DocumentoSagrilaf> DocumentosSagrilaf { get; set; }


    }



    public class CupoProyecto
    { 
        public double cupo { get; set; }
        public Guid proyecto { get; set; }
    }
}
