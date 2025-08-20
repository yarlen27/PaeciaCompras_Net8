using Cobalto.Mongo.Core.BLL;
using Core.BLL;
using Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using SendGrid;
using SendGrid.Helpers.Mail;
using System.Text.RegularExpressions;
using AspNetCore.Identity.MongoDB;
using System.Linq;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using System.Reflection;
using Microsoft.AspNetCore.Identity;
using DigitalOceanUploader.Shared;
using jsreport.Binary;
using jsreport.Types;
using jsreport.Client;
// using Microsoft.AspNetCore.Mvc.Formatters.Internal; // Removed in .NET 8
using Newtonsoft.Json;
using System.Globalization;

namespace Core.BLL
{
    public class EmailBLL : BaseBLL<EmailModel>
    {

        PedidoMaterialBLL pedidoMaterialBLL;

        private IFileProvider _fileProvider;

        private ProyectoBLL proyectoBLL;


        private PedidoServicioBLL pedidoServicioBLL;
        private OrdenCompraBLL _ordenCompraBLL;
        private ProveedorBLL _proveedorBLL;
        private IHostingEnvironment _env;

        public UserManager<MongoIdentityUser> _userManager { get; private set; }
        public DigitalOceanUploadManager uploadManager { get; private set; }



        public EmailBLL(IConfiguration configuration,
            IHttpContextAccessor httpContext,
            PedidoMaterialBLL pedidoMaterialBLL,
            PedidoServicioBLL pedidoServicioBLL,
              UserManager<MongoIdentityUser> userManager,
            ProyectoBLL proyectoBLL,
            OrdenCompraBLL ordenCompraBLL,
             DigitalOceanUploadManager uploadManager,
            ProveedorBLL proveedorBLL,
             IHostingEnvironment env,
             IFileProvider fileProvider
            ) : base(configuration, httpContext)
        {
            this.pedidoMaterialBLL = pedidoMaterialBLL;
            this.proyectoBLL = proyectoBLL;

            this.pedidoServicioBLL = pedidoServicioBLL;

            this._ordenCompraBLL = ordenCompraBLL;
            this._proveedorBLL = proveedorBLL;
            this._fileProvider = fileProvider;
            this._env = env;
            this._userManager = userManager;

            this.uploadManager = uploadManager;
        }


        public async Task EmailMaterialRechazado(PedidoMaterial entity, PedidoMaterialDetalle item)
        {
            var template = await ObtenerPlantilla("Core.EmailTemplates.materialRechazado.html");

            //#PROYECTO
            //#MATERIAL
            //#MOTIVODELRECHAZO
            //#FECHA

            var pedido = await this.pedidoMaterialBLL.GetById(entity.id);

            var proyecto = await this.proyectoBLL.GetById(pedido.proyecto);

            template = template.Replace("#PROYECTO", proyecto.nombre);
            template = template.Replace("#MATERIAL", $"REF: {item.referencia} - DESC: {item.descripcion} - CANT: {item.cantidad} - UNID: {item.unidad} ");
            template = template.Replace("#MOTIVODELRECHAZO", item.observaciones);
            template = template.Replace("#FECHA", DateTime.Now.ToString("dd/MMM/yyyy"));


            var direcciones = new List<string>();

            var director = await this._userManager.FindByIdAsync(proyecto.directorProyecto);



            SendGridClient client = BuildSendGridClient();
            var from = new EmailAddress("compras@paecia.com", "Notificaciones");



            direcciones.Add(director.Email.Value);

            foreach (var direccion in direcciones)
            {
                try
                {

                    var to = new EmailAddress(direccion);


                    var emailText = template;

                    var plainTextContent = Regex.Replace(emailText, "<[^>]*>", "");
                    var msg = MailHelper.CreateSingleEmail(from, to, "NOTIFICACIÓN - MATERIAL RECHAZADO", plainTextContent, emailText);


                    var response = await client.SendEmailAsync(msg);
                }
                catch (Exception ex)
                {

                }
            }


        }

        internal async Task NotificarEdicionAuxiliarContable(Factura item)
        {
            try
            {
                var usuarios = this._userManager.Users.ToList();

                var auxiliaresContables = from u in usuarios
                                          from r in u.Client
                                          where r.rol == 8 && r.client == new Guid(this.clientId)
                                          select u;

                SendGridClient client = BuildSendGridClient();
                var from = new EmailAddress("compras@paecia.com", "Notificaciones");

                string nombreProyecto = item.idProyecto.HasValue ? (await this.proyectoBLL.GetById(item.idProyecto.Value)).nombre : item.nit;

                var proveedor = (await this._proveedorBLL.GetById(item.idProveedor.Value)).nombre;

                foreach (var auxiliarContable in auxiliaresContables)
                {

                    var to = new EmailAddress(auxiliarContable.Email.Value);
                    var emailText = await ObtenerPlantilla("Core.EmailTemplates.NotificacionFacturaEditadaAuxiliarContable.html");
                    var rechazo = item.rechazosMantenimiento.OrderByDescending(r => r.Fecha).FirstOrDefault();
                    var comentarios = rechazo.Comentarios;
                    var razon = string.Empty;
                    if (rechazo != null && rechazo.Razon != null)
                    {
                        razon = rechazo.Razon;
                    }

                    emailText = emailText.Replace("#MOTIVODELRECHAZO", razon);
                    emailText = emailText.Replace("#NUMEROFACTURA", item.numeroFactura);
                    emailText = emailText.Replace("#PROYECTO", nombreProyecto);
                    emailText = emailText.Replace("#PROVEEDOR", proveedor);
                    emailText = emailText.Replace("#MONTO", item.monto.ToString());
                    emailText = emailText.Replace("#FECHACREADO", item.fechaCreado.ToString("dd/MMM/yyyy"));
                    emailText = emailText.Replace("#FECHAVENCIMIENTO", item.fechaVencimiento.ToString("dd/MMM/yyyy"));
                    emailText = emailText.Replace("#COMENTARIOS", comentarios);
                    emailText = emailText.Replace("#FECHA", item.fecha.ToString("dd/MMM/yyyy"));

                    var plainTextContent = Regex.Replace(emailText, "<[^>]*>", "");
                    var msg = MailHelper.CreateSingleEmail(from, to, "NOTIFICACIÓN - FACTURA DE MANTENIMIENTO RECHAZADA", plainTextContent, emailText);

                    var response = await client.SendEmailAsync(msg);
                }
            }
            catch (Exception)
            {

                throw;
            }


        }

        internal async Task NotificarFacturaOT(Factura entity)
        {
            try
            {
                var usuarios = this._userManager.Users.ToList();

                var coordinadorMantenimiento = from u in usuarios
                                               from r in u.Client
                                               where r.rol == 9 && r.client == new Guid(this.clientId)
                                               select u;


                SendGridClient client = BuildSendGridClient();
                var from = new EmailAddress("compras@paecia.com", "Notificaciones");



                foreach (var usaurios in coordinadorMantenimiento)
                {

                    var to = new EmailAddress(usaurios.Email.Value);



                    var emailText = await ObtenerPlantilla("Core.EmailTemplates.FacturaMantenimiento.html");


                    emailText = emailText.Replace("#NUMEROFACTURA", entity.numeroFactura);
                    emailText = emailText.Replace("#MONTO", entity.monto.ToString());
                    emailText = emailText.Replace("#FECHACREADO", entity.fechaCreado.ToString("dd/MMM/yyyy"));
                    emailText = emailText.Replace("#FECHAVENCIMIENTO", entity.fechaVencimiento.ToString("dd/MMM/yyyy"));
                    emailText = emailText.Replace("#FECHA", entity.fecha.ToString("dd/MMM/yyyy"));
                    //


                    var plainTextContent = Regex.Replace(emailText, "<[^>]*>", "");
                    var msg = MailHelper.CreateSingleEmail(from, to, "NOTIFICACIÓN - NUEVA FACTURA DE MANTENIMIENTO", plainTextContent, emailText);
                    var response = await client.SendEmailAsync(msg);

                }


            }
            catch (Exception ex)
            {

                throw;
            }


        }

        public async Task EmailNuevoPedidoMaterial(Guid idPedido, List<MongoIdentityUser> usuarios)
        {


            var pedido = await this.pedidoMaterialBLL.GetById(idPedido);

            var proyecto = await this.proyectoBLL.GetById(pedido.proyecto);

            var contacts = await this.GetAll();



            SendGridClient client = BuildSendGridClient();
            var from = new EmailAddress("compras@paecia.com", "Notificaciones");


            var analistaCompras = usuarios.Where(x => x.Client.Any(z => z.client == new Guid(this.clientId) && z.rol == (int)RolesEnum.Analista_de_compra) && x.DeletedOn == null).ToList();

            var solicitante = usuarios.Where(x => x.Id == pedido.solicitante).FirstOrDefault();
          
            var listaDeCorreo = new List<string>();


            foreach (var item in analistaCompras)
            {
                var rolEnCliente = item.Client.First(x => x.rol == 3 && x.client == new Guid(this.clientId));

                if (rolEnCliente.proyecto.Count > 0)
                {
                    if (rolEnCliente.proyecto.Count(x => new Guid(x) == proyecto.id) == 0)
                    {
                        continue; 
                    }
                }

                listaDeCorreo.Add(item.Email.Value);
            }


            foreach (var item in listaDeCorreo)
            {
                try
                {

                    var to = new EmailAddress(item);


                    var emailText = await ObtenerPlantilla("Core.EmailTemplates.NuevoPedidoMateriales.html");


                    emailText = emailText.Replace("#NOMBREPROYECTO", proyecto.nombre);
                    emailText = emailText.Replace("#FECHA", pedido.fechaSolicitado.ToString("dd/MMM/yyyy"));
                    emailText = emailText.Replace("#SOLICITANTE", $"{solicitante.Nombre} {solicitante.Apellido}");
                    emailText = emailText.Replace("#SITEURL", this.configuration.GetValue("siteURL", ""));
                    //


                    var plainTextContent = Regex.Replace(emailText, "<[^>]*>", "");
                    var msg = MailHelper.CreateSingleEmail(from, to, "NOTIFICACIÓN - NUEVO PEDIDO", plainTextContent, emailText);
                    var response = await client.SendEmailAsync(msg);
                }
                catch (Exception ex)
                {

                }
            }



            return;
        }

        public async Task EmailNuevaOrden(Guid entityId)
        {
            var ordenDeCompra = this._ordenCompraBLL.GetById(entityId).Result;

            if (ordenDeCompra.pedidoMaterial)
            {
                var detalle = ordenDeCompra.detalle;

                foreach (var item in detalle)
                {


                    await this.EnviarOrdenProveedor(item, ordenDeCompra);

                }
            }
            else
            {

                var detalle = ordenDeCompra.servicio;

                // await this.EnviarOrdenServicioProveedor(detalle, ordenDeCompra);


            }

        }



        public async Task<byte[]> PDFOrden(Guid entityId)
        {
            var ordenDeCompra = this._ordenCompraBLL.GetById(entityId).Result;

            if (ordenDeCompra.pedidoMaterial)
            {
                var detalle = ordenDeCompra.detalle;

                foreach (var item in detalle)
                {


                    return await this.PdfOrdenProveedor(item, ordenDeCompra);

                }
            }

            return null;
        }



        public async Task EmailRemision(Remision remision)
        {
            var template = await ObtenerPlantilla("Core.EmailTemplates.nuevaRemision.html");

            var orden = await this._ordenCompraBLL.GetById(remision.idOrdenCompra);

            var proyecto = await this.proyectoBLL.GetById(orden.proyecto);

            template = template.Replace("#PROYECTO", proyecto.nombre);
            template = template.Replace("#FECHA", DateTime.Now.ToString("dd/MMM/yyyy"));


            var direcciones = new List<string>();

            var director = await this._userManager.FindByIdAsync(proyecto.directorProyecto);



            SendGridClient client = BuildSendGridClient();
            var from = new EmailAddress("compras@paecia.com", "Notificaciones");



            direcciones.Add(director.Email.Value);

            foreach (var direccion in direcciones)
            {
                try
                {

                    var to = new EmailAddress(direccion);


                    var emailText = template;

                    var plainTextContent = Regex.Replace(emailText, "<[^>]*>", "");
                    var msg = MailHelper.CreateSingleEmail(from, to, "NOTIFICACIÓN - NUEVA REMISIÓN", plainTextContent, emailText);

                    await AddAttachment(msg, remision);

                    var response = await client.SendEmailAsync(msg);
                }
                catch (Exception ex)
                {

                }
            }

        }

        internal async Task NotificarRechazoFacturaOT(Factura factura)
        {
            try
            {
                var usuarios = this._userManager.Users.ToList();

                var coordinadorMantenimiento = from u in usuarios
                                               from r in u.Client
                                               where r.rol == 9 && r.client == new Guid(this.clientId)
                                               select u;


                SendGridClient client = BuildSendGridClient();
                var from = new EmailAddress("compras@paecia.com", "Notificaciones");


                foreach (var usaurios in coordinadorMantenimiento)
                {

                    var to = new EmailAddress(usaurios.Email.Value);


                    var emailText = await ObtenerPlantilla("Core.EmailTemplates.FacturaMantenimientoRechazada.html");



                    var proyecto = (await this.proyectoBLL.GetById(factura.idProyecto.Value)).nombre;
                    var proveedor = (await this._proveedorBLL.GetById(factura.idProveedor.Value)).nombre;

                    emailText = emailText.Replace("#NUMEROFACTURA", factura.numeroFactura);
                    emailText = emailText.Replace("#PROYECTO", proyecto);
                    emailText = emailText.Replace("#PROVEEDOR", proveedor);
                    emailText = emailText.Replace("#MONTO", factura.monto.ToString());
                    emailText = emailText.Replace("#FECHACREADO", factura.fechaCreado.ToString("dd/MMM/yyyy"));
                    emailText = emailText.Replace("#FECHAVENCIMIENTO", factura.fechaVencimiento.ToString("dd/MMM/yyyy"));
                    emailText = emailText.Replace("#FECHA", factura.fecha.ToString("dd/MMM/yyyy"));
                    //


                    var plainTextContent = Regex.Replace(emailText, "<[^>]*>", "");
                    var msg = MailHelper.CreateSingleEmail(from, to, "NOTIFICACIÓN - FACTURA DE MANTENIMIENTO RECHAZADA", plainTextContent, emailText);
                    var response = await client.SendEmailAsync(msg);

                }


            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task EmailFactura(Factura factura)
        {
            var template = await ObtenerPlantilla("Core.EmailTemplates.nuevaFactura.html");

            Proyecto proyecto = null;
            Proveedor proveedor = null;


            if (factura.idOrdenCompra.HasValue)
            {
                var orden = await this._ordenCompraBLL.GetById(factura.idOrdenCompra.Value);

                proyecto = await this.proyectoBLL.GetById(orden.proyecto);
                proveedor = await this._proveedorBLL.GetById(orden.proveedor);
            }
            else
            {
                proyecto = await this.proyectoBLL.GetById(factura.idProyecto.Value);
                proveedor = await this._proveedorBLL.GetById(factura.idProveedor.Value);
            }




            template = template.Replace("#PROYECTO", proyecto.nombre);
            template = template.Replace("#FECHA#", factura.fecha.ToString("dd/MMM/yyyy"));
            template = template.Replace("#FECHAVENCIMIENTO#", factura.fechaVencimiento.ToString("dd/MMM/yyyy"));
            template = template.Replace("#PROVEEDOR", proveedor.nombre);
            template = template.Replace("#VALORTOTAL", factura.monto.ToString("0.0"));


            var direcciones = new List<string>();

            var director = await this._userManager.FindByIdAsync(proyecto.directorProyecto);

          




            var listaUsuarios = _userManager.Users.ToList();

            var contables = from u in listaUsuarios
                            from c in u.Client
                            from p in c.proyecto
                            where c.rol == 8 && p.Contains(factura.idProyecto.ToString())
                            select u.Email.Value;


            if (director == null)
            {
                listaUsuarios = _userManager.Users.ToList();

                director = listaUsuarios.FirstOrDefault(x  => x.Id == proyecto.directorProyecto);
            }


            var instanciasDeAprobacin = int.Parse(proyecto.instanciasAprobacion);

            if (instanciasDeAprobacin == 2 && !string.IsNullOrEmpty(proyecto.segundoAprobador))
            {

                var segundaInstancia = await this._userManager.FindByIdAsync(proyecto.segundoAprobador);

                direcciones.Add(segundaInstancia.Email.Value);

            }



            SendGridClient client = BuildSendGridClient();
            var from = new EmailAddress("compras@paecia.com", "Notificaciones");





            direcciones.Add(director.Email.Value);

            direcciones.AddRange(contables);

            foreach (var direccion in direcciones)
            {
                try
                {

                    var to = new EmailAddress(direccion);


                    var emailText = template;

                    var plainTextContent = Regex.Replace(emailText, "<[^>]*>", "");
                    var msg = MailHelper.CreateSingleEmail(from, to, "NOTIFICACIÓN - NUEVA FACTURA", plainTextContent, emailText);

                    // await AddAttachment(msg, factura);

                    var response = await client.SendEmailAsync(msg);
                }
                catch (Exception ex)
                {

                }
            }

        }

        internal async Task NotificarRechazoFacturaDirectorProveedor(Factura factura)
        {
            try
            {
                SendGridClient client = BuildSendGridClient();
                var proyecto = await this.proyectoBLL.GetById(factura.idProyecto.Value);
                var usuarios = this._userManager.Users.ToList();

                var directoresProyectos = from u in usuarios
                                          from r in u.Client
                                          where r.rol == 2 && r.client == new Guid(this.clientId)
                                          select u;

                var contabilidad = from u in usuarios
                                   from r in u.Client
                                   where (r.rol == 5 || r.rol == 8) && r.client == new Guid(this.clientId)
                                   select u;


                var directorProyecto = directoresProyectos.FirstOrDefault(dp => dp.Id.Equals(proyecto.directorProyecto));

                directorProyecto = directorProyecto ?? usuarios.FirstOrDefault(u => u.Id.Equals(proyecto.directorProyecto));

                var from = new EmailAddress("compras@paecia.com");

                var proveedor = await this._proveedorBLL.GetById(factura.idProveedor.Value);

                var emailsContabilidad = contabilidad.Select(uc => uc.Email.ToString()).ToList();
                var emailsDestino = GenerarCorreosDestinoProveedorYContabilidad(proveedor.email, emailsContabilidad);

                foreach (var email in emailsDestino)
                {
                    var to = new EmailAddress(email);
                    var emailText = await ObtenerPlantilla("Core.EmailTemplates.facturaRechazadaDirectorProyectoProveedor.html");

                    var rechazo = factura.rechazosMantenimiento.OrderByDescending(r => r.Fecha).First();
                    var comentarios = rechazo.Comentarios;
                    var razon = rechazo.Razon;

                    emailText = emailText.Replace("#MOTIVODELRECHAZO", razon);
                    emailText = emailText.Replace("#NUMEROFACTURA", factura.numeroFactura);
                    emailText = emailText.Replace("#PROYECTO", proyecto.nombre);
                    emailText = emailText.Replace("#PROVEEDOR", proveedor.nombre);
                    emailText = emailText.Replace("#MONTO", factura.monto.ToString());
                    emailText = emailText.Replace("#FECHACREADO", factura.fechaCreado.ToString("dd/MMM/yyyy"));
                    emailText = emailText.Replace("#FECHAVENCIMIENTO", factura.fechaVencimiento.ToString("dd/MMM/yyyy"));
                    emailText = emailText.Replace("#COMENTARIOS", comentarios);
                    emailText = emailText.Replace("#FECHA", factura.fecha.ToString("dd/MMM/yyyy"));


                    var plainTextContent = Regex.Replace(emailText, "<[^>]*>", "");
                    var msg = MailHelper.CreateSingleEmail(from, to, "NOTIFICACIÓN - FACTURA DE MANTENIMIENTO RECHAZADA", plainTextContent, emailText);
                    var response = await client.SendEmailAsync(msg);
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private List<string> GenerarCorreosDestinoProveedorYContabilidad(string emailProveedor, List<string> emailsContabilidad)
        {
            List<string> emailsDestino = new List<string>();
            if (emailProveedor.Contains(";"))
            {
                emailsDestino = (emailProveedor.Split(";")).OfType<string>().ToList();
                emailsDestino.AddRange(emailsContabilidad);
                return emailsDestino;
            }
            else
            {
                emailsDestino.Add(emailProveedor);
                emailsDestino.AddRange(emailsContabilidad);
                return emailsDestino;

            }
        }

        internal async Task NotificarRechazoFacturaDirectorRecepcion(Factura factura)
        {
            try
            {
                var usuarios = this._userManager.Users.ToList();

                var recepcion = from u in usuarios
                                from r in u.Client
                                where r.rol == 4 && r.client == new Guid(this.clientId)
                                select u;

                string nombreProyecto = factura.idProyecto.HasValue ?
                                        (await this.proyectoBLL.GetById(factura.idProyecto.Value)).nombre
                                        : factura.nit;

                SendGridClient client = BuildSendGridClient();

                var from = new EmailAddress("compras@paecia.com", "Notificaciones");


                foreach (var usaurios in recepcion)
                {

                    var to = new EmailAddress(usaurios.Email.Value);


                    var emailText = await ObtenerPlantilla("Core.EmailTemplates.facturaRechazadaDirectorProyectoRecepcion.html");

                    var proveedor = (await this._proveedorBLL.GetById(factura.idProveedor.Value)).nombre;

                    var rechazo = factura.rechazosMantenimiento.OrderByDescending(r => r.Fecha).First();
                    var comentarios = rechazo.Comentarios;
                    var razon = rechazo.Razon;


                    emailText = emailText.Replace("#MOTIVODELRECHAZO", razon);
                    emailText = emailText.Replace("#NUMEROFACTURA", factura.numeroFactura);
                    emailText = emailText.Replace("#PROYECTO", nombreProyecto);
                    emailText = emailText.Replace("#PROVEEDOR", proveedor);
                    emailText = emailText.Replace("#MONTO", factura.monto.ToString());
                    emailText = emailText.Replace("#FECHACREADO", factura.fechaCreado.ToString("dd/MMM/yyyy"));
                    emailText = emailText.Replace("#FECHAVENCIMIENTO", factura.fechaVencimiento.ToString("dd/MMM/yyyy"));
                    emailText = emailText.Replace("#COMENTARIOS", comentarios);
                    emailText = emailText.Replace("#FECHA", factura.fecha.ToString("dd/MMM/yyyy"));
                    //


                    var plainTextContent = Regex.Replace(emailText, "<[^>]*>", "");
                    var msg = MailHelper.CreateSingleEmail(from, to, "NOTIFICACIÓN - FACTURA DE MANTENIMIENTO RECHAZADA", plainTextContent, emailText);
                    var response = await client.SendEmailAsync(msg);

                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task EmailFacturaRechazada(Factura factura)
        {
            var template = await ObtenerPlantilla("Core.EmailTemplates.facturaRechazada.html");

            var orden = await this._ordenCompraBLL.GetById(factura.idOrdenCompra.Value);

            var proyecto = await this.proyectoBLL.GetById(orden.proyecto);

            template = template.Replace("#PROYECTO", proyecto.nombre);
            template = template.Replace("#FECHA", factura.fecha.ToString("dd/MMM/yyyy"));
            template = template.Replace("#FECHAVENCIMIENTO", factura.fechaVencimiento.ToString("dd/MMM/yyyy"));
            template = template.Replace("#MONTO", factura.monto.ToString("0.0"));
            template = template.Replace("#NUMEROFACTURA", factura.numeroFactura);

            var proveedor = await this._proveedorBLL.GetById(orden.proveedor);


            template = template.Replace("#PROVEEDOR", $"{proveedor.nombre}  - {proveedor.nit}");
            template = template.Replace("#MOTIVODELRECHAZO", factura.observacionRechazo);




            var direcciones = new List<string>();


            var cliente = new Guid(this._proveedorBLL.clientId);

            var listaUsuarios = _userManager.Users;
            listaUsuarios = listaUsuarios.Where(x => x.Client.Any(z => z.client == cliente && z.rol == (int)RolesEnum.Recepcionista) && x.DeletedOn == null);


            foreach (var item in listaUsuarios)
            {
                direcciones.Add(item.Email.Value);
            }


            SendGridClient client = BuildSendGridClient();
            var from = new EmailAddress("compras@paecia.com", "Notificaciones");




            foreach (var direccion in direcciones)
            {
                try
                {

                    var to = new EmailAddress(direccion);


                    var emailText = template;

                    var plainTextContent = Regex.Replace(emailText, "<[^>]*>", "");
                    var msg = MailHelper.CreateSingleEmail(from, to, "NOTIFICACIÓN - FACTURA RECHAZADA", plainTextContent, emailText);

                    await AddAttachment(msg, factura);

                    var response = await client.SendEmailAsync(msg);
                }
                catch (Exception ex)
                {

                }
            }

        }

        internal async Task NotificarRechazoFacturaCoordinadorProveedor(Factura factura)
        {
            try
            {
                SendGridClient client = BuildSendGridClient();
                var proyecto = await this.proyectoBLL.GetById(factura.idProyecto.Value);
                var usuarios = this._userManager.Users.ToList();

                var directoresProyectos = from u in usuarios
                                          from r in u.Client
                                          where r.rol == 2 && r.client == new Guid(this.clientId)
                                          select u;

                var contabilidad = from u in usuarios
                                   from r in u.Client
                                   where (r.rol == 5 || r.rol == 8) && r.client == new Guid(this.clientId)
                                   select u;

                var director = proyecto.directorProyecto;
                var directorProyecto = directoresProyectos.FirstOrDefault(dp => dp.Id.Equals(director));

                var from = new EmailAddress(directorProyecto.Email.Value, "Notificaciones");

                var proveedor = await this._proveedorBLL.GetById(factura.idProveedor.Value);

                var emailsContabilidad = contabilidad.Select(uc => uc.Email.ToString()).ToList();
                var emailsDestino = GenerarCorreosDestinoProveedorYContabilidad(proveedor.email, emailsContabilidad);

                foreach (var email in emailsDestino)
                {
                    var to = new EmailAddress(email);
                    var emailText = await ObtenerPlantilla("Core.EmailTemplates.facturaRechazadaCoordinadorProveedor.html");

                    var rechazo = factura.rechazosMantenimiento.OrderByDescending(r => r.Fecha).First();
                    var comentarios = rechazo.Comentarios;
                    var razon = rechazo.Razon;

                    emailText = emailText.Replace("#MOTIVODELRECHAZO", razon);
                    emailText = emailText.Replace("#NUMEROFACTURA", factura.numeroFactura);
                    emailText = emailText.Replace("#PROYECTO", proyecto.nombre);
                    emailText = emailText.Replace("#PROVEEDOR", proveedor.nombre);
                    emailText = emailText.Replace("#MONTO", factura.monto.ToString());
                    emailText = emailText.Replace("#FECHACREADO", factura.fechaCreado.ToString("dd/MMM/yyyy"));
                    emailText = emailText.Replace("#FECHAVENCIMIENTO", factura.fechaVencimiento.ToString("dd/MMM/yyyy"));
                    emailText = emailText.Replace("#COMENTARIOS", comentarios);
                    emailText = emailText.Replace("#FECHA", factura.fecha.ToString("dd/MMM/yyyy"));


                    var plainTextContent = Regex.Replace(emailText, "<[^>]*>", "");
                    var msg = MailHelper.CreateSingleEmail(from, to, "NOTIFICACIÓN - FACTURA DE MANTENIMIENTO RECHAZADA", plainTextContent, emailText);
                    var response = await client.SendEmailAsync(msg);
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }



        internal async Task NotificarRechazoFacturaCoordinadorRecepcion(Factura factura)
        {
            try
            {
                var usuarios = this._userManager.Users.ToList();

                var recepcion = from u in usuarios
                                from r in u.Client
                                where r.rol == 4 && r.client == new Guid(this.clientId)
                                select u;

                string nombreProyecto = factura.idProyecto.HasValue ?
                                        (await this.proyectoBLL.GetById(factura.idProyecto.Value)).nombre
                                        : factura.nit;

                SendGridClient client = BuildSendGridClient();

                var from = new EmailAddress("compras@paecia.com", "Notificaciones");


                foreach (var usaurios in recepcion)
                {

                    var to = new EmailAddress(usaurios.Email.Value);


                    var emailText = await ObtenerPlantilla("Core.EmailTemplates.facturaRechazadaCoordinadorRecepcion.html");
                    var proveedor = (await this._proveedorBLL.GetById(factura.idProveedor.Value)).nombre;

                    var rechazo = factura.rechazosMantenimiento.OrderByDescending(r => r.Fecha).First();
                    var comentarios = rechazo.Comentarios;
                    var razon = rechazo.Razon;


                    emailText = emailText.Replace("#MOTIVODELRECHAZO", razon);
                    emailText = emailText.Replace("#NUMEROFACTURA", factura.numeroFactura);
                    emailText = emailText.Replace("#PROYECTO", nombreProyecto);
                    emailText = emailText.Replace("#PROVEEDOR", proveedor);
                    emailText = emailText.Replace("#MONTO", factura.monto.ToString());
                    emailText = emailText.Replace("#FECHACREADO", factura.fechaCreado.ToString("dd/MMM/yyyy"));
                    emailText = emailText.Replace("#FECHAVENCIMIENTO", factura.fechaVencimiento.ToString("dd/MMM/yyyy"));
                    emailText = emailText.Replace("#COMENTARIOS", comentarios);
                    emailText = emailText.Replace("#FECHA", factura.fecha.ToString("dd/MMM/yyyy"));
                    //


                    var plainTextContent = Regex.Replace(emailText, "<[^>]*>", "");
                    var msg = MailHelper.CreateSingleEmail(from, to, "NOTIFICACIÓN - FACTURA DE MANTENIMIENTO RECHAZADA", plainTextContent, emailText);
                    var response = await client.SendEmailAsync(msg);

                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task AddAttachment(SendGridMessage msg, Factura factura)
        {

            var facturaFile = await this.uploadManager.DownloadFile(factura.archivo.ToString());

            string file = Convert.ToBase64String(facturaFile.file);
            msg.AddAttachment(facturaFile.filename, file);


        }

        private async Task AddAttachmentContratoFirmado(SendGridMessage msg, OrdenCompra ordenCompra)
        {

            var contratoFile = await this.uploadManager.DownloadFile(ordenCompra.servicio.archivoContrato.ToString());

            string file = Convert.ToBase64String(contratoFile.file);
            msg.AddAttachment(contratoFile.filename, file);


        }

        private async Task AddAttachmentContrato(SendGridMessage msg, OrdenCompra ordenCompra)
        {

            var contratoFile = await this.uploadManager.DownloadFile(ordenCompra.servicio.archivoContrato.ToString());

            string file = Convert.ToBase64String(contratoFile.file);
            msg.AddAttachment(contratoFile.filename, file);


        }

        private async Task AddAttachment(SendGridMessage msg, Remision remision)
        {

            var facturaFile = await this.uploadManager.DownloadFile(remision.archivo.ToString());

            string file = Convert.ToBase64String(facturaFile.file);
            msg.AddAttachment(facturaFile.filename, file);


        }

        public async Task EnviarOrdenServicioProveedor(PedidoServicioDetalle detalle, OrdenCompra ordenDeCompra)
        {

            var proveedor = await this._proveedorBLL.GetById(new Guid(detalle.proveedor));

            var direcciones = proveedor.email.Split(';').ToList();

            string template = await this.OrdenServicioTemplate(detalle, ordenDeCompra);
            SendGridClient client = BuildSendGridClient();
            //var from = new EmailAddress("notificaciones.gestioncompras@paecia.com", "Notificaciones");
            var from = new EmailAddress("compras@paecia.com", "Notificaciones");




            foreach (var direccion in direcciones)
            {
                try
                {

                    var to = new EmailAddress(direccion);


                    var emailText = template;

                    var plainTextContent = Regex.Replace(emailText, "<[^>]*>", "");
                    var msg = MailHelper.CreateSingleEmail(from, to, "NOTIFICACIÓN - CONTRATO SERVICIO", plainTextContent, emailText);
                    await AddAttachmentContrato(msg, ordenDeCompra);
                    var response = await client.SendEmailAsync(msg);
                }
                catch (Exception ex)
                {

                }
            }



        }

        public async Task EnviarOrdenServicioProveedorContratoFirmado(OrdenCompra ordenDeCompra)
        {

            var proveedor = await this._proveedorBLL.GetById(ordenDeCompra.proveedor);

            var direcciones = proveedor.email.Split(';').ToList();

            string template = await this.OrdenServicioTemplate(ordenDeCompra.servicio, ordenDeCompra);
            SendGridClient client = BuildSendGridClient();
            //var from = new EmailAddress("notificaciones.gestioncompras@paecia.com", "Notificaciones");
            var from = new EmailAddress("compras@paecia.com", "Notificaciones");

            foreach (var direccion in direcciones)
            {
                try
                {

                    var to = new EmailAddress(direccion);


                    var emailText = template;

                    var plainTextContent = Regex.Replace(emailText, "<[^>]*>", "");
                    var msg = MailHelper.CreateSingleEmail(from, to, "NOTIFICACIÓN - CONTRATO SERVICIO FIRMADO", plainTextContent, emailText);
                    await AddAttachmentContratoFirmado(msg, ordenDeCompra);
                    var response = await client.SendEmailAsync(msg);
                }
                catch (Exception ex)
                {

                }
            }



        }

        private async Task<string> OrdenServicioTemplate(PedidoServicioDetalle detalle, OrdenCompra ordenCompra)
        {
            //var htmlTemplate = File.ReadAllText(@"\EmailTemplates\nuevaOrden");
            var cultureInfo = new CultureInfo("es-CO");
            var plantillaCuerpo = await ObtenerPlantilla("Core.EmailTemplates.nuevaOrdenServicio.html");
            var plantillaDetalle = await ObtenerPlantilla("Core.EmailTemplates.NuevaOrdenServicioItem.html");





            foreach (var item in detalle.servicio)
            {
                //#Referencia
                //#Descripción
                //#Cantidad
                //#Unidad
                //#Observaciones

                plantillaDetalle = ReemplazarToken("#Actividad", item.actividades, plantillaDetalle);
                plantillaDetalle = ReemplazarToken("#Unidad", item.unidad, plantillaDetalle);
                plantillaDetalle = ReemplazarToken("#Cantidad", item.cantidad, plantillaDetalle);
                plantillaDetalle = ReemplazarToken("#ValorUnidad", "$" + Convert.ToDecimal(item.valorUnidad).ToString("#,##0.00", cultureInfo), plantillaDetalle);
                plantillaDetalle = ReemplazarToken("#ValorTotal", "$" + Convert.ToDecimal(item.valorTotal).ToString("#,##0.00", cultureInfo), plantillaDetalle);
            }

            var proyecto = this.proyectoBLL.GetById(ordenCompra.proyecto).Result.nombre;

            plantillaCuerpo = plantillaCuerpo.Replace("#TABLEITEMS", plantillaDetalle);

            plantillaCuerpo = plantillaCuerpo.Replace("#PROYECTO", proyecto);
            plantillaCuerpo = plantillaCuerpo.Replace("#FECHA", ordenCompra.fechaGenerado.ToString("dd/MMM/yyyy"));
            plantillaCuerpo = plantillaCuerpo.Replace("#OBJETO", ordenCompra.servicio.objeto);
            plantillaCuerpo = plantillaCuerpo.Replace("#ALCANCE", ordenCompra.servicio.alcance);
            plantillaCuerpo = plantillaCuerpo.Replace("#PLAZO", ordenCompra.servicio.plazo);
            plantillaCuerpo = plantillaCuerpo.Replace("#MONTOTOTAL", "$" + ordenCompra.servicio.montoTotal.ToString("#,##0.00", cultureInfo));
            plantillaCuerpo = plantillaCuerpo.Replace("#OBSERVACIONES", ordenCompra.servicio.observaciones);

            return plantillaCuerpo;
        }

        private async Task EnviarOrdenProveedor(OrdenCompraMaterialDetalle item, OrdenCompra ordenCompra)
        {
            var proveedor = await this._proveedorBLL.GetById(item.proveedor);
            var proyecto = await this.proyectoBLL.GetById(ordenCompra.proyecto);
            var direcciones = proveedor.email.Split(';').ToList();
            if (proyecto.emailOrdenes != string.Empty && proyecto.emailOrdenes != null)
            {
                direcciones.AddRange(proyecto.emailOrdenes.Split(';').ToList());
            }

            direcciones.Add("compras@paecia.com");



            //# if DEBUG

            //            direcciones.Clear();
            //#endif

            string template = await this.OrdenMaterialesTemplate(item, ordenCompra);

            double total = 0.0;

            foreach (var material in item.detalleMaterial)
            {
                total += material.cantidad * material.valorUnitario;
            }

            SendGridClient client = BuildSendGridClient();
            //var from = new EmailAddress("notificaciones.gestioncompras@paecia.com", "Notificaciones");
            var from = new EmailAddress("compras@paecia.com", "Compras");
            direcciones = direcciones.Distinct().Where(x => x != string.Empty).ToList();
            var tos = new List<EmailAddress>();


            direcciones = (from a in direcciones
                           select a.ToLower()).Distinct().ToList();

            foreach (var direccion in direcciones)
            {
                tos.Add(new EmailAddress(direccion));

            }



            try
            {



                var emailText = template;
                var cultureInfo = new CultureInfo("es-CO");
                emailText = emailText.Replace("#TOTAL#", "$" + total.ToString("#,##0.00", cultureInfo));

                var plainTextContent = Regex.Replace(emailText, "<[^>]*>", "");
                var msg = MailHelper.CreateSingleEmailToMultipleRecipients(from, tos, $"NOTIFICACIÓN - NUEVA ORDEN DE COMPRA No:{ordenCompra.consecutivo}", plainTextContent, emailText, true);



                
                var rs = new ReportingService("http://localhost:39280/");


                var chromeTemplate = new Template()
                {
                    Recipe = Recipe.ChromePdf,
                    Engine = Engine.None,
                    Content = emailText,


                };

                chromeTemplate.Chrome = new Chrome();

                chromeTemplate.Chrome.Landscape = false;
                chromeTemplate.Chrome.Format = "A4";

                chromeTemplate.Chrome.Scale = 0.5m;

                var report = await rs.RenderAsync(new RenderRequest()
                {
                    Template = chromeTemplate
                });




                msg.AddAttachment($"OrdenDeCompra - No {ordenCompra.consecutivo}.pdf", this.ConvertToBase64(report.Content));

                var response = await client.SendEmailAsync(msg);
            }
            catch (Exception ex)
            {
                throw;
            }

        }


        private async Task<byte[]> PdfOrdenProveedor(OrdenCompraMaterialDetalle item, OrdenCompra ordenCompra)
        {
            var cultureInfo = new CultureInfo("es-CO");
            var proveedor = await this._proveedorBLL.GetById(item.proveedor);
            var proyecto = await this.proyectoBLL.GetById(ordenCompra.proyecto);




            //# if DEBUG

            //            direcciones.Clear();
            //#endif

            string template = await this.OrdenMaterialesTemplate(item, ordenCompra);

            double total = 0.0;

            foreach (var material in item.detalleMaterial)
            {
                total += material.cantidad * material.valorUnitario;
            }

            try
            {



                var emailText = template;
                emailText = emailText.Replace("#TOTAL#", "$" + total.ToString("#,##0.00", cultureInfo));

                var plainTextContent = Regex.Replace(emailText, "<[^>]*>", "");



               
                var rs = new ReportingService("http://localhost:39280/");


                var chromeTemplate = new Template()
                {
                    Recipe = Recipe.ChromePdf,
                    Engine = Engine.None,
                    Content = emailText,


                };

                chromeTemplate.Chrome = new Chrome();

                chromeTemplate.Chrome.Landscape = false;
                chromeTemplate.Chrome.Format = "A4";

                chromeTemplate.Chrome.Scale = 0.5m;

                var report = await rs.RenderAsync(new RenderRequest()
                {
                    Template = chromeTemplate
                });


                MemoryStream ms = new MemoryStream();
                report.Content.CopyTo(ms);
                return ms.ToArray();


            }
            catch (Exception ex)
            {
                throw;
            }

        }


        internal async Task NotificarAlertaPagoTesoreria(Factura factura)
        {
            try
            {
                var usuarios = this._userManager.Users.ToList();

                var usuariosTesoreria = from u in usuarios
                                        from r in u.Client
                                        where r.rol == 11 && r.client == new Guid(this.clientId)
                                        select u;


                SendGridClient client = BuildSendGridClient();
                var from = new EmailAddress("compras@paecia.com", "Notificaciones");


                foreach (var usuario in usuariosTesoreria)
                {

                    var to = new EmailAddress(usuario.Email.Value);


                    var emailText = await ObtenerPlantilla("Core.EmailTemplates.FacturaAlertaTesoreria.html");



                    var proyecto = (await this.proyectoBLL.GetById(factura.idProyecto.Value)).nombre;
                    var proveedor = (await this._proveedorBLL.GetById(factura.idProveedor.Value)).nombre;

                    emailText = emailText.Replace("#NUMEROFACTURA", factura.numeroFactura);
                    emailText = emailText.Replace("#PROYECTO", proyecto);
                    emailText = emailText.Replace("#PROVEEDOR", proveedor);
                    emailText = emailText.Replace("#FECHA", factura.fecha.ToString("dd/MMM/yyyy"));
                    //


                    var plainTextContent = Regex.Replace(emailText, "<[^>]*>", "");
                    var msg = MailHelper.CreateSingleEmail(from, to, "ALERTA - PAGO TESORERIA", plainTextContent, emailText);
                    var response = await client.SendEmailAsync(msg);

                }


            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public string ConvertToBase64(Stream stream)
        {
            byte[] bytes;
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                bytes = memoryStream.ToArray();
            }

            string base64 = Convert.ToBase64String(bytes);

            return base64;
        }

        private async Task<string> OrdenMaterialesTemplate(OrdenCompraMaterialDetalle detalle, OrdenCompra ordenCompra)
        {
            //var htmlTemplate = File.ReadAllText(@"\EmailTemplates\nuevaOrden");
            var cultureInfo = new CultureInfo("es-CO");
            var plantillaCuerpo = await ObtenerPlantilla("Core.EmailTemplates.nuevaOrden.html");
            var plantillaDetalle = string.Empty;

            foreach (var item in detalle.detalleMaterial)
            {
                //#Referencia
                //#Descripción
                //#Cantidad
                //#UnidadC:\GestionCompras\API\API\Controllers\SendEmailController.cs
                //#Observaciones
                var itemPlantillaDetalle = await ObtenerPlantilla("Core.EmailTemplates.NuevaOrdenItem.html");



                itemPlantillaDetalle = ReemplazarToken("#Referencia", item.referencia, itemPlantillaDetalle);
                itemPlantillaDetalle = ReemplazarToken("#Nombre", item.nombre, itemPlantillaDetalle);
                itemPlantillaDetalle = ReemplazarToken("#Descripción", item.descripcion, itemPlantillaDetalle);
                itemPlantillaDetalle = ReemplazarToken("#Cantidad", item.cantidad.ToString("0.0"), itemPlantillaDetalle);
                itemPlantillaDetalle = ReemplazarToken("#Unidad", item.unidad, itemPlantillaDetalle);


                try
                {
                    var itemJson = JsonConvert.DeserializeObject<PedidoMaterialDetalle>(item.observaciones);
                    itemPlantillaDetalle = ReemplazarToken("#Observaciones", itemJson.observaciones, itemPlantillaDetalle);

                }
                catch (Exception ex)
                {

                    itemPlantillaDetalle = ReemplazarToken("#Observaciones", item.observaciones, itemPlantillaDetalle);

                }


                itemPlantillaDetalle = ReemplazarToken("#ValorUnitario", "$" + item.valorUnitario.ToString("#,##0.00", cultureInfo), itemPlantillaDetalle);
                itemPlantillaDetalle = ReemplazarToken("#ValorTotal", "$" + (item.valorUnitario * item.cantidad).ToString("#,##0.00", cultureInfo), itemPlantillaDetalle);
                plantillaDetalle += itemPlantillaDetalle;
            }

            var proyecto = this.proyectoBLL.GetById(ordenCompra.proyecto).Result;

            //#
            //#
            //#
            plantillaCuerpo = plantillaCuerpo.Replace("#PROYECTO", proyecto.nombre);
            plantillaCuerpo = plantillaCuerpo.Replace("#ORDENCOMPRA", ordenCompra.consecutivo.ToString());

            var proveedor = await this._proveedorBLL.GetById(ordenCompra.proveedor);


            plantillaCuerpo = plantillaCuerpo.Replace("#PROVEEDOR#", $"{proveedor.nombre}  - {proveedor.nit}");
            plantillaCuerpo = plantillaCuerpo.Replace("#TELPROVEEDOR#", $" {proveedor.telefono}");




            plantillaCuerpo = plantillaCuerpo.Replace("#CONTACTO", ordenCompra.contacto);
            plantillaCuerpo = plantillaCuerpo.Replace("#TABLEITEMS", plantillaDetalle);
            plantillaCuerpo = plantillaCuerpo.Replace("#FECHA", ordenCompra.fechaGenerado.ToString("dd/MMM/yyyy"));

            plantillaCuerpo = plantillaCuerpo.Replace("#DIRECCIONFACTURACION", ordenCompra.direccionFacturacion);
            plantillaCuerpo = plantillaCuerpo.Replace("#DIRECCIONPROYECTO", ordenCompra.direccionEntrega);
            plantillaCuerpo = plantillaCuerpo.Replace("#EMPRESA", proyecto.empresa);
            plantillaCuerpo = plantillaCuerpo.Replace("#NITEMPRESA", proyecto.nit);



            return plantillaCuerpo;
        }

        private string ReemplazarToken(string token, string valor, string plantilla)
        {
            if (valor == null)
            {
                valor = String.Empty;
            }
            plantilla = plantilla.Replace(token, valor);

            return plantilla;
        }

        private static async Task<string> ObtenerPlantilla(string archivo)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceStream = assembly.GetManifestResourceStream(archivo);


            var plantilla = "";
            //NuevaOrdenItem
            using (var reader = new StreamReader(resourceStream, Encoding.UTF8))
            {
                plantilla = await reader.ReadToEndAsync();
            }


            return plantilla;
        }

        public async Task EmailNuevoPedidoServicios(Guid idPedido, List<MongoIdentityUser> usuarios)
        {


            var pedido = await this.pedidoServicioBLL.GetById(idPedido);

            var proyecto = await this.proyectoBLL.GetById(pedido.proyecto);

            var contacts = await this.GetAll();

            SendGridClient client = BuildSendGridClient();
            var from = new EmailAddress("compras@paecia.com", "Notificaciones");


            var analistaCompras = usuarios.Where(x => x.Client.Any(z => z.client == new Guid(this.clientId) && z.rol == (int)RolesEnum.Analista_de_compra) && x.DeletedOn == null).ToList();



            var solicitante = usuarios.Where(x => x.Id == pedido.solicitante).FirstOrDefault();

            var listaDeCorreo = new List<string>();


            foreach (var item in analistaCompras)
            {

                var proyectos = from c in item.Client
                                from p in c.proyecto
                                select p;

                var proyectoAnalista = from c in item.Client
                                       from p in c.proyecto
                                       where p == proyecto.id.ToString()
                                       select p;
                //falso
                if (proyectos.Count() == 0 || proyectoAnalista.Count() > 0)
                {
                    listaDeCorreo.Add(item.Email.Value);

                }

              
            }
            listaDeCorreo.Add("gestionproyectos@paecia.com");


            foreach (var item in listaDeCorreo)
            {
                try
                {

                    var to = new EmailAddress(item);


                    var emailText = EmailTemplates.TemplateNuevoPedidoServicios.template;

                    emailText = emailText.Replace("#NOMBREPROYECTO", proyecto.nombre);
                    emailText = emailText.Replace("#FECHA", pedido.fechaSolicitado.ToString("dd/MMM/yyyy"));
                    emailText = emailText.Replace("#SOLICITANTE", $"{solicitante.Nombre} {solicitante.Apellido}");
                    emailText = emailText.Replace("#SITEURL", this.configuration.GetValue("siteURL", ""));
                    //


                    var plainTextContent = Regex.Replace(emailText, "<[^>]*>", "");
                    var msg = MailHelper.CreateSingleEmail(from, to, "NOTIFICACIÓN - NUEVO PEDIDO", plainTextContent, emailText);
                    var response = await client.SendEmailAsync(msg);
                }
                catch (Exception ex)
                {

                }
            }



            return;
        }

        

        internal async Task NotificarPrimerAprobacionFacturaOT(Factura factura)
        {
            try
            {
                var usuarios = this._userManager.Users.ToList();

                var coordinadorMantenimiento = from u in usuarios
                                               from r in u.Client
                                               where r.rol == 10 && r.client == new Guid(this.clientId)
                                               select u;


                SendGridClient client = BuildSendGridClient();
                var from = new EmailAddress("compras@paecia.com", "Notificaciones");


                foreach (var usaurios in coordinadorMantenimiento)
                {

                    var to = new EmailAddress(usaurios.Email.Value);


                    var emailText = await ObtenerPlantilla("Core.EmailTemplates.FacturaMantenimientoPrimerAprobacion.html");



                    var proyecto = (await this.proyectoBLL.GetById(factura.idProyecto.Value)).nombre;
                    var proveedor = (await this._proveedorBLL.GetById(factura.idProveedor.Value)).nombre;

                    emailText = emailText.Replace("#NUMEROFACTURA", factura.numeroFactura);
                    emailText = emailText.Replace("#PROYECTO", proyecto);
                    emailText = emailText.Replace("#PROVEEDOR", proveedor);
                    emailText = emailText.Replace("#MONTO", factura.monto.ToString());
                    emailText = emailText.Replace("#FECHACREADO", factura.fechaCreado.ToString("dd/MMM/yyyy"));
                    emailText = emailText.Replace("#FECHAVENCIMIENTO", factura.fechaVencimiento.ToString("dd/MMM/yyyy"));
                    emailText = emailText.Replace("#FECHA", factura.fecha.ToString("dd/MMM/yyyy"));
                    //


                    var plainTextContent = Regex.Replace(emailText, "<[^>]*>", "");
                    var msg = MailHelper.CreateSingleEmail(from, to, "NOTIFICACIÓN - FACTURA DE MANTENIMIENTO APROBADA POR COORDINADOR", plainTextContent, emailText);
                    var response = await client.SendEmailAsync(msg);

                }


            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private static SendGridClient BuildSendGridClient()
        {
            var apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY") ?? "YOUR_SENDGRID_API_KEY_HERE";
            var client = new SendGridClient(apiKey);
            return client;
        }
    }
}
