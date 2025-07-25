using Cobalto.Mongo.Core.BLL;
using Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using AspNetCore.Identity.MongoDB;

namespace Core.BLL
{
    public class EncuestaProveedorBLL : BaseBLL<EncuestaProveedor>
    {
        public EncuestaProveedorBLL(IConfiguration configuration, IHttpContextAccessor httpContext, ProveedorBLL proveedorBLL, UserManager<MongoIdentityUser> userManager) : base(configuration, httpContext)
        {
            this._proveedorBLL = proveedorBLL;
            this._userManager = userManager;
        }

        Dictionary<Guid, string> proveedores = new Dictionary<Guid, string>();
        Dictionary<string, string> usuarios = new Dictionary<string, string>();
        private ProveedorBLL _proveedorBLL;
        private UserManager<MongoIdentityUser> _userManager;




        public async Task<List<EncuestaProveedor>> Reporte(FiltroEncuesta filtro)
        {



            var filter = Builders<EncuestaProveedor>.Filter.Gte("fecha", filtro.inicio);
            var filter2 = Builders<EncuestaProveedor>.Filter.Lte("fecha", filtro.fin);

            var filterFecha = filter & filter2;



            FilterDefinition<EncuestaProveedor> filtroProveedor = null;

            if (filtro.proveedor != null && filtro.proveedor.Count > 0)
            {
                filtroProveedor = Builders<EncuestaProveedor>.Filter.Eq("proveedor", filtro.proveedor[0]);

                for (int i = 1; i < filtro.proveedor.Count(); i++)
                {
                    var tempFilter = Builders<EncuestaProveedor>.Filter.Eq("proveedor", filtro.proveedor[i]);
                    filtroProveedor = filtroProveedor | tempFilter;

                }
            }



            FilterDefinition<EncuestaProveedor> filtroUsuarios = null;

            if (filtro.usuarios != null && filtro.usuarios.Count > 0)
            {
                filtroUsuarios = Builders<EncuestaProveedor>.Filter.Eq("usuario", filtro.usuarios[0]);

                for (int i = 1; i < filtro.usuarios.Count(); i++)
                {
                    var tempFilter = Builders<EncuestaProveedor>.Filter.Eq("usuario", filtro.usuarios[i]);
                    filtroUsuarios = filtroUsuarios | tempFilter;

                }
            }


            var encuestas = new List<EncuestaProveedor>();

            if (filtroProveedor != null)
            {

                if (filtroUsuarios != null)
                {
                    encuestas = await this.GetByProterty(filterFecha & filtroProveedor & filtroUsuarios);

                }
                else
                {
                    encuestas = await this.GetByProterty(filterFecha & filtroProveedor);


                }

            }
            else if (filtroUsuarios != null)
            {
                encuestas = await this.GetByProterty(filterFecha & filtroUsuarios);

            }
            else
            {

                encuestas = await this.GetByProterty(filterFecha);

            }






            foreach (var item in encuestas)
            {


                if (!this.proveedores.ContainsKey(item.proveedor))
                {
                    var proveedor = await this._proveedorBLL.GetById(item.proveedor);
                    this.proveedores.Add(proveedor.id, proveedor.nombre);

                }

                item.nombreProveedor = this.proveedores[item.proveedor];


                if (!this.usuarios.ContainsKey(item.usuario))
                {
                    var usuario = await this._userManager.FindByIdAsync(item.usuario);
                    if (usuario != null)
                    {
                        this.usuarios.Add(usuario.Id, usuario.Nombre + " " + usuario.Apellido);
                    }
                    else
                    {

                        this.usuarios.Add(item.usuario, "N/A");

                    }

                }
                item.usuario = this.usuarios[item.usuario];

            }

            return encuestas;

        }


        public async Task<string> ReporteCsv(FiltroEncuesta param)
        {



            //public string usuario { get; set; }

            //public Guid proveedor { get; set; }
            //public string nombreProveedor { get; set; }

            //public DateTime fecha { get; set; }

            //List<PreguntaProveedor> preguntas { get; set; }


            var encuestas = await this.Reporte(param);

            var sb = new StringBuilder("usuario;nombreProveedor;fecha;");


            sb.Append("Cumplimiento de las especificaciones;");
            sb.Append("Cumplimiento del tiempo;");
            sb.Append("Cumplimiento con las cantidades solicitadas (aplica solo para materiales);");
            sb.Append("Disponibilidad de recursos físicos, materiales, maquinaria, talento humano.;");
            sb.Append("Cumple con las normas de seguridad (usa elementos de protección personal, evidencia certificados de pago a seguridad social, respeta las normas dentro de nuestras instalaciones).;");
            sb.Append("Respuesta oportuna a las solicitudes, quejas o reclamos.;");
            sb.Append("Amabilidad del personal.;");


            sb.AppendLine();
            foreach (var item in encuestas)
            {

                sb.AppendLine(LineaReporte(item));
            }

            return sb.ToString();

        }

        private string LineaReporte(EncuestaProveedor encuesta)
        {
            var sb = new StringBuilder();


            sb.Append($"{encuesta.usuario};{encuesta.nombreProveedor};{encuesta.fecha.ToString("dd/MMM/yyyy")};");

            foreach (var item in encuesta.preguntas.OrderBy(x=>x.id) )
            {
                sb.Append($"{item.valor};");

            }

            return sb.ToString();
        }
    }
}
