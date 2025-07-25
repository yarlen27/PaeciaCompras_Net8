using Cobalto.Mongo.Core.Attributes;
using Cobalto.Mongo.Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
    public class PedidoReporte
    {

        public PedidoReporte()
        {

        }
        public PedidoReporte(PedidoMaterial item)
        {

            this.proyectoId = item.proyecto;
            this.fechaSolicitado = item.fechaSolicitado;
            this.solicitante = item.solicitante;
            this.urgente = this.urgente;
            this.TipoPedido = "Materiales";

        }

        public PedidoReporte(PedidoServicio item)
        {

            this.proyectoId = item.proyecto;
            this.fechaSolicitado = item.fechaSolicitado;
            this.solicitante = item.solicitante;
            this.urgente = this.urgente;
            this.TipoPedido = "Servicio";
        }

        public Guid proyectoId { get; set; }
        public string proyecto { get; set; }
        public DateTime fechaSolicitado { get; set; }
        public string solicitante { get; set; }
        public bool urgente { get; set; }

        public string TipoPedido { get; set; }
    }
}
