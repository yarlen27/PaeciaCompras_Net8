using System.Collections.Generic;

namespace Core.Models
{
    public interface IPedidoServicio
    {
        string archivoCotizacion { get; set; }
        string archivoContrato { get; set; }
        string archivoContratoFirmado { get; set; }
        //List<string> otrosArchivos { get; set; }

        string alcance { get; set; }
        string plazo { get; set; }
        double montoTotal { get; set; }

    }
}