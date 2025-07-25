using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
    public class PedidoMaterialDetalleReporte
    {
        private PedidoMaterialDetalle detalleMaterial;

        public PedidoMaterialDetalleReporte()
        {

        }

        public PedidoMaterialDetalleReporte(PedidoMaterialDetalle detalleMaterial)
        {

            this.referencia = detalleMaterial.referencia;
            this.nombre = detalleMaterial.nombre;
            this.descripcion = detalleMaterial.descripcion;
            this.unidad = detalleMaterial.unidad;
            this.cantidad = detalleMaterial.cantidad;
            this.observaciones = detalleMaterial.observaciones;
            this.rechazado = detalleMaterial.rechazado;
            this.valorUnitario = detalleMaterial.valorUnitario;

        }

        public string referencia { get; set; }
        public string descripcion { get; set; }
        public string unidad { get; set; }
        public double cantidad { get; set; }
        public string observaciones { get; set; }
        public bool rechazado { get; set; }
        public double valorUnitario { get; set; }
        public string nombre { get; set; }

        public DateTime? fecha { get; set; }
        public string proveedor { get; set; }
        public string proyecto { get; set; }
    }
}
