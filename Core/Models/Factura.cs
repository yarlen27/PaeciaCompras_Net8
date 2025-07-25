using Cobalto.Mongo.Core.Attributes;
using Cobalto.Mongo.Core.DTO;
using MongoDB.Bson.Serialization.Attributes;
using Org.BouncyCastle.Bcpg;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{

    [CollectionName("factura")]
    public class Factura : CollectionDTO
    {



        public double monto { get; set; }
        public string numeroFactura { get; set; }
        public Guid archivo { get; set; }
        public Guid archivoOriginal { get; set; }
        public Guid archivoFirmado { get; set; }
        public bool ereased { get; set; }
        public bool aprobada { get; set; }
        public bool impresoContabilidad { get; set; }
        public bool rechazada { get; set; }
        public List<string> aprobador { get; set; }

        [BsonIgnore]
        public List<string> nombreAprobador { get; set; }
        public string observacionRecepcion { get; set; }
        public string observacionRechazo { get; set; }
        public string observacionAprobar { get; set; }
        public List<AprobacionMantenimiento> AprobacionesMantenimiento { get; set; }
        public List<RechazoCoordinadorMantenimiento> rechazosMantenimiento { get; set; }
        public bool pagada { get; set; }
        public string usuarioTesoreria { get; set; }
        public bool impresoTesoreria { get; set; }

        public List<ArchivoAdicional> otrosArchivos { get; set; }
        public DatosContables[] datosContables { get; set; }

        public Guid? idProyecto { get; set; }
        public Guid? idProveedor { get; set; }


        [BsonIgnoreIfDefault]
        public bool? otPaecia { get; set; } = false;

        public Guid? idOrdenCompra { get; set; }
        public bool conDatosContables { get; set; }

        public bool? isOT { get; set; }
        public List<Guid> OrdenesTrabajo { get; set; }

        public bool aprobadaCoordinadorMantenimiento { get; set; }
        public bool aprobadaAdministradorMantenimiento { get; set; }
        public bool aprobadaDirectorProyecto { get; set; }
        public string nit { get; set; }
        public bool rechazadaAdmintradorMantenimiento { get; set; }

        public bool noteCredito { get; set; }


        public DateTime? fechaDatosContables { get; set; }
        public DateTime? fechaImpresoContabilidad { get; set; }
        internal DateTime? fechaImpresoTesoreria { get; set; }

        public DateTime fecha { get; set; }
        public DateTime fechaCreado { get; set; }
        public DateTime fechaVencimiento { get; set; }
        public DateTime? fechaPagado { get; set; }

        public DateTime? fechaAprobacion { get; set; }
        public DateTime? fechaAprobacion2 { get; set; }

        public int? idOT { get; set; }

        public bool? esAnticipo { get; set; }
    }

 
}
