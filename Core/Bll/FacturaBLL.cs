using Cobalto.Mongo.Core.BLL;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DigitalOceanUploader.Shared;
using AspNetCore.Identity.MongoDB;
using Microsoft.AspNetCore.Identity;
using iText.Kernel.Pdf;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Extgstate;
using iText.Kernel.Pdf.Layer;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.IO.Image;
using iText.IO.Font.Constants;
using System.IO;
using static DigitalOceanUploader.Shared.DigitalOceanUploadManager;
using MongoDB.Driver;
using System.Linq;
using System.Data;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Core.Bll;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using MongoDB.Bson;
using System.IO.Compression;
using System.Xml.Linq;
using System.Xml;
using RestSharp;
using Newtonsoft.Json;

namespace Core.BLL
{
    public class FacturaBLL : BaseBLL<Factura>
    {

        public FacturaBLL(IConfiguration configuration,
            IHttpContextAccessor httpContext,
            EmailBLL emailBLL,
            OrdenCompraBLL ordenCompraBLL,
            ProyectoBLL proyectoBLL,
            ProveedorBLL proveedorBll,
            DigitalOceanUploadManager uploadManager,
            UserManager<MongoIdentityUser> userManager,
            ConsecutivoSoporteBLL consecutivoSoporteBLL,
            ConsecutivoCausacionBLL consecutivoCausacionBLL
            ) : base(configuration, httpContext)
        {
            _emailBLL = emailBLL;
            _proyectoBll = proyectoBLL;
            this.uploadManager = uploadManager;
            this._userManager = userManager;
            this.consecutivoCausacionBLL = consecutivoCausacionBLL;
            _proveedorBll = proveedorBll;
            this._ordenCompraBLL = ordenCompraBLL;
            this.consecutivoSoporteBLL = consecutivoSoporteBLL;
        }

        public EmailBLL _emailBLL { get; private set; }

        private ProyectoBLL _proyectoBll;
        private DigitalOceanUploadManager uploadManager;
        private UserManager<MongoIdentityUser> _userManager;
        private readonly ConsecutivoCausacionBLL consecutivoCausacionBLL;
        private ProveedorBLL _proveedorBll;
        private OrdenCompraBLL _ordenCompraBLL;
        private ConsecutivoSoporteBLL consecutivoSoporteBLL;

        public async Task<double> TotalProyecto(Guid id)
        {
            double result = 0.0;

            var facturas = await this.GetByProterty("idProyecto", id);

            result = facturas.Where(x => x.esAnticipo.HasValue == false || x.esAnticipo.Value == false).Sum(x => x.monto);


            return result;
        }

        public async Task<List<Factura>> FacturasFiltradasOtSinAprobacion(Filtro filtro)
        {
            var facturasSinAprobacion = await this.facturasFiltradasOT(filtro);

            return facturasSinAprobacion.Where(x => !x.erased
                                                && ((!x.aprobadaCoordinadorMantenimiento && !x.aprobadaAdministradorMantenimiento)
                                                || x.rechazadaAdmintradorMantenimiento)
                                                && !x.rechazada).ToList();

        }


        public async Task<List<Factura>> FacturasFiltradasOtPrimerAprobacion(Filtro filtro)
        {
            var facturasPrimerAprobacion = await this.facturasFiltradasOTAprobadasCoordinadorMantenimiento(filtro);
            return facturasPrimerAprobacion;
        }

        public async Task NotificarFacturaOT(Factura entity)
        {

            await this._emailBLL.NotificarFacturaOT(entity);
        }
        public async Task EmailNotificacionAuxiliar(Factura item)
        {
            await this._emailBLL.NotificarEdicionAuxiliarContable(item);
        }

        private async Task<List<Factura>> facturasFiltradasOT(Filtro filtro)
        {
            var filtroFechaInicial = Builders<Factura>.Filter.Gte("fecha", filtro.inicio.Value);

            var filtroFechaFinal = Builders<Factura>.Filter.Lte("fecha", filtro.fin.Value);

            var filtroOT = Builders<Factura>.Filter.Eq("isOT", true);

            return await this.GetByProterty(filtroFechaInicial & filtroFechaFinal & filtroOT);
        }

        private async Task<List<Factura>> facturasFiltradasOTAprobadasCoordinadorMantenimiento(Filtro filtro)
        {
            var filtroFechaInicial = Builders<Factura>.Filter.Gte("fecha", filtro.inicio.Value);

            var filtroFechaFinal = Builders<Factura>.Filter.Lte("fecha", filtro.fin.Value);
            var filtroOT = Builders<Factura>.Filter.Eq("isOT", true);

            var filtroBorradas = Builders<Factura>.Filter.Eq("erased", false);

            var filtroAprobada = Builders<Factura>.Filter.Eq("aprobadaCoordinadorMantenimiento", true);
            var filtroAprobadaAdmin = Builders<Factura>.Filter.Eq("aprobadaAdministradorMantenimiento", false);
            var filtroTieneOTAsociada = Builders<Factura>.Filter.Ne("idOT", BsonNull.Value);



            var filtroDB = filtroFechaInicial & filtroFechaFinal & filtroOT & filtroBorradas & filtroAprobadaAdmin & filtroAprobada;
            // var filtroDBMtto = filtroFechaInicial & filtroFechaFinal & filtroOT & filtroBorradas & filtroAprobadaAdmin & filtroTieneOTAsociada;



            if (filtro.proveedor != null && filtro.proveedor.Count > 0)
            {

                var filtro0 = Builders<Factura>.Filter.Eq("idProveedor", filtro.proveedor[0]);
                for (int i = 1; i < filtro.proveedor.Count(); i++)
                {
                    var filter = Builders<Factura>.Filter.Eq("idProveedor", filtro.proveedor[i]);
                    filtro0 = (filtro0 | filter);
                }

                filtroDB = filtroDB & filtro0;

            }



            if (filtro.proyectos != null && filtro.proyectos.Count > 0)
            {

                var filtro0 = Builders<Factura>.Filter.Eq("idProyecto", filtro.proyectos[0]);
                for (int i = 1; i < filtro.proyectos.Count(); i++)
                {
                    var filter = Builders<Factura>.Filter.Eq("idProyecto", filtro.proyectos[i]);
                    filtro0 = (filtro0 | filter);

                }



                filtroDB = filtroDB & filtro0;

            }



            var result = await this.GetByProterty(filtroDB);

            if (filtro.paecia.HasValue && filtro.paecia.Value)
            {
                result = result.Where(x => x.otPaecia == true).ToList();
            }
            else
            {
                //retorna todas las facturas que no son de paecia o q tenga el campo nulo
                result = result.Where(x => x.otPaecia == null || x.otPaecia == false).ToList();

            }


            return result;
        }

        public async Task AsigarOrdenCeroFactura(Factura factura)
        {
            if (factura.idProveedor.HasValue && factura.idProyecto.HasValue && !factura.idOrdenCompra.HasValue)
            {

                var filter = Builders<OrdenCompra>.Filter.Eq("proyecto", factura.idProyecto.Value);
                var filter2 = Builders<OrdenCompra>.Filter.Eq("proveedor", factura.idProveedor.Value);
                var filter3 = Builders<OrdenCompra>.Filter.Eq("consecutivo", 0);

                var ordenesXProveedorXProyecto = (await this._ordenCompraBLL.GetByProterty((filter & filter2 & filter3))).FirstOrDefault();


                OrdenCompra order;

                if (ordenesXProveedorXProyecto != null)
                {
                    ordenesXProveedorXProyecto.fechaGenerado = factura.fecha;
                    await this._ordenCompraBLL.Update(ordenesXProveedorXProyecto);
                    order = ordenesXProveedorXProyecto;
                }
                else
                {
                    var nuevaOrden = new OrdenCompra();
                    nuevaOrden.fechaGenerado = DateTime.Now;
                    nuevaOrden.proveedor = factura.idProveedor.Value;
                    nuevaOrden.proyecto = factura.idProyecto.Value;
                    order = await _ordenCompraBLL.Insert(nuevaOrden, true);

                }


                factura.idOrdenCompra = order.id;

                await Update(factura);
            }
            else
            {
                await Update(factura);

            }
        }

        public async Task AsigarOrdenCero()
        {


            var facturas = await this.GetByProterty("isOT", true);

            foreach (var entity in facturas)
            {

                if (entity.idProveedor.HasValue && entity.idProyecto.HasValue && entity.idOrdenCompra.HasValue == false)
                {


                    var filter = Builders<OrdenCompra>.Filter.Eq("proyecto", entity.idProyecto.Value);
                    var filter2 = Builders<OrdenCompra>.Filter.Eq("proveedor", entity.idProveedor.Value);
                    var filter3 = Builders<OrdenCompra>.Filter.Eq("consecutivo", 0);

                    var ordenesXProveedorXProyecto = (await this._ordenCompraBLL.GetByProterty((filter & filter2 & filter3))).FirstOrDefault();


                    OrdenCompra order;

                    if (ordenesXProveedorXProyecto != null)
                    {
                        ordenesXProveedorXProyecto.fechaGenerado = entity.fecha;
                        await this._ordenCompraBLL.Update(ordenesXProveedorXProyecto);
                        order = ordenesXProveedorXProyecto;
                    }
                    else
                    {
                        var nuevaOrden = new OrdenCompra();
                        nuevaOrden.fechaGenerado = DateTime.Now;
                        nuevaOrden.proveedor = entity.idProveedor.Value;
                        nuevaOrden.proyecto = entity.idProyecto.Value;
                        order = await _ordenCompraBLL.Insert(nuevaOrden, true);

                    }


                    entity.idOrdenCompra = order.id;

                    await Update(entity);
                }


            }





        }

        //public Task AsigarOrdenCero()
        //{
        //    var filter = Builders<OrdenCompra>.Filter.Ne("proyecto", null);
        //    var filter2 = Builders<OrdenCompra>.Filter.Eq("proveedor", entity.idProveedor.Value);
        //    var filter3 = Builders<OrdenCompra>.Filter.Eq("consecutivo", 0);

        //    var ordenesXProveedorXProyecto = (await this._ordenCompraBLL.GetByProterty((filter & filter2 & filter3))).FirstOrDefault();
        //}

        public async Task<Factura> CrearConOrdenZero(Factura entity)
        {

            var filter = Builders<OrdenCompra>.Filter.Eq("proyecto", entity.idProyecto.Value);
            var filter2 = Builders<OrdenCompra>.Filter.Eq("proveedor", entity.idProveedor.Value);
            var filter3 = Builders<OrdenCompra>.Filter.Eq("consecutivo", 0);

            var ordenesXProveedorXProyecto = (await this._ordenCompraBLL.GetByProterty((filter & filter2 & filter3))).FirstOrDefault();

            OrdenCompra order;

            if (ordenesXProveedorXProyecto != null)
            {
                ordenesXProveedorXProyecto.fechaGenerado = entity.fecha;
                await this._ordenCompraBLL.Update(ordenesXProveedorXProyecto);
                order = ordenesXProveedorXProyecto;
            }
            else
            {
                var nuevaOrden = new OrdenCompra();
                nuevaOrden.fechaGenerado = DateTime.Now;
                nuevaOrden.proveedor = entity.idProveedor.Value;
                nuevaOrden.proyecto = entity.idProyecto.Value;
                order = await _ordenCompraBLL.Insert(nuevaOrden, true);

            }


            entity.idOrdenCompra = order.id;

            if (entity.idOrdenCompra == null)
            {


                throw new Exception("Error creando orden cero");
            }

            return await base.Insert(entity);
        }

        public async Task<List<ObservacionesFactura>> ObservacionesFactura(Guid idFactura, List<OrdenTrabajo> OTs)
        {

            var result = new List<ObservacionesFactura>();
            foreach (var item in OTs)
            {

                var user = _userManager.FindByIdAsync(item.idUsuario).Result;
                result.Add(new Models.ObservacionesFactura()
                {
                    fecha = item.fechaCreacion,
                    observaciones = item.observacion,
                    usuario = user.UserName
                });
            }
            return result.OrderBy(x => x.fecha).ToList();
        }

        public async Task<bool> DatosContables(Guid idFactura, List<DatosContables> datosContableses, string usuario, InformacionContable informacionContable)
        {

            var facturaActual = await this.GetById(idFactura);


            Proyecto proyecto = null;


            if (facturaActual.idProyecto.HasValue == false)
            {
                proyecto = new Proyecto();
                proyecto.nit = facturaActual.nit;

            }
            else
            {

                proyecto = await this._proyectoBll.GetById(facturaActual.idProyecto.Value);
            }



            string fechaDatosContables = facturaActual.fecha.ToString("yyyyMM");


            var filter = Builders<ConsecutivoCausacion>.Filter.Eq("nit", proyecto.nit);
            var filter2 = Builders<ConsecutivoCausacion>.Filter.Eq("fechaDatosContables", fechaDatosContables);





            var consecutivo = (await this.consecutivoCausacionBLL.GetByProterty(filter & filter2)).FirstOrDefault();



            if (informacionContable.EsDocumentoSoporte)
            {

            }
            else
            {
                if (consecutivo == null)
                {
                    consecutivo = new ConsecutivoCausacion
                    {
                        fechaDatosContables = fechaDatosContables,
                        nit = proyecto.nit,
                        consecutivoDatosCotables = 1
                    };

                    await this.consecutivoCausacionBLL.Insert(consecutivo);
                }
                else
                {
                    consecutivo.consecutivoDatosCotables = consecutivo.consecutivoDatosCotables + 1;

                }

            }





            var user = _userManager.FindByIdAsync(usuario).Result;

            string consecutivoSoporte = "";

            if (informacionContable.EsDocumentoSoporte)
            {
                var r = this.consecutivoSoporteBLL.Insertar(new ConsecutivoDocumentoSoporte
                {

                    IdDocumento = idFactura,
                    Nit = proyecto.nit
                });//await this.CrearConsecutivo($"{siglaArea}-{siglaTipoDocumento}", codigoDocumentoCero);

                consecutivoSoporte = await this.consecutivoSoporteBLL.ObtenerConsecutivo(idFactura);


                datosContableses.Insert(0, new DatosContables() { codigo = "Consecutivo", valor = consecutivoSoporte });

            }
            else
            {
                datosContableses.Insert(0, new DatosContables() { codigo = "Consecutivo", valor = consecutivo.consecutivoDatosCotables.ToString() });

            }


            datosContableses.Add(new DatosContables() { codigo = "EsDocumentoSoporte", valor = informacionContable.EsDocumentoSoporte.ToString() });



            datosContableses.Add(new DatosContables() { codigo = "Fecha contabilización", valor = DateTime.Now.ToString("yyyy/MM/dd") });
            datosContableses.Add(new DatosContables() { codigo = "Usuario", valor = user.Nombre });
            if (informacionContable.esAnticipo.HasValue)
            {
                datosContableses.Add(new DatosContables() { codigo = "EsAnticipo", valor = informacionContable.esAnticipo.Value.ToString() });
            }
            else
            {
                datosContableses.Add(new DatosContables() { codigo = "EsAnticipo", valor = "False" });

            }

            facturaActual.datosContables = datosContableses.ToArray();
            facturaActual.conDatosContables = true;
            facturaActual.esAnticipo = informacionContable.esAnticipo;

            await base.Update(facturaActual);

            facturaActual = await this.GetById(idFactura);



            datosContableses = informacionContable.ToDatosContablesFormat();



            datosContableses.Add(new DatosContables() { codigo = "Fecha contabilización", valor = DateTime.Now.ToString("yyyy/MM/dd") });
            datosContableses.Add(new DatosContables() { codigo = "Usuario", valor = user.Nombre });


            if (informacionContable.EsDocumentoSoporte)
            {


                datosContableses.Insert(0, new DatosContables() { codigo = "Consecutivo", valor = consecutivoSoporte });

            }
            else
            {

                datosContableses.Insert(0, new DatosContables() { codigo = "Consecutivo", valor = consecutivo.consecutivoDatosCotables.ToString() });

            }

            var idArchivo = await ImprimirDatosContables(datosContableses.ToArray(), facturaActual.archivo, idFactura);
            facturaActual.fechaDatosContables = DateTime.Now;
            facturaActual.archivoOriginal = facturaActual.archivo;
            facturaActual.archivo = idArchivo;
            if (facturaActual.archivoFirmado != Guid.Empty)
            {
                var idArchivoFirmado = await ImprimirDatosContables(datosContableses.ToArray(), facturaActual.archivoFirmado, idFactura);
                facturaActual.archivoFirmado = idArchivoFirmado;
            }


            if (informacionContable.EsDocumentoSoporte)
            {

            }
            else
            {
                await this.consecutivoCausacionBLL.Update(consecutivo);

            }
            await base.Update(facturaActual);




            return true;
        }

        public async Task<Factura> RechazoDirectorMantenimiento(RechazoCoordinadorMantenimiento rechazoDirectorMantenimiento)
        {
            var factura = await this.GetById(rechazoDirectorMantenimiento.IdFactura);
            var usuario = await _userManager.FindByIdAsync(rechazoDirectorMantenimiento.IdUsuario);
            rechazoDirectorMantenimiento.usuario = $"{usuario.Nombre} {usuario.Apellido}";
            rechazoDirectorMantenimiento.Fecha = DateTime.Now;

            if (factura.rechazosMantenimiento == null)
            {
                factura.rechazosMantenimiento = new List<RechazoCoordinadorMantenimiento> { rechazoDirectorMantenimiento };
            }
            else
            {
                factura.rechazosMantenimiento.Add(rechazoDirectorMantenimiento);
            }

            factura.rechazada = true;
            factura.aprobadaCoordinadorMantenimiento = false;
            factura.aprobadaAdministradorMantenimiento = false;
            factura.aprobadaDirectorProyecto = false;

            await base.Update(factura);

            if (rechazoDirectorMantenimiento.Razon.Equals("Error en recepción"))
            {
                await this._emailBLL.NotificarRechazoFacturaDirectorRecepcion(factura);
            }
            else
            {
                await this._emailBLL.NotificarRechazoFacturaDirectorProveedor(factura);

            }

            return factura;
        }

        public async Task<bool> ActualizarDatosContables(Guid idFactura,
            List<DatosContables> datosContableses,
            string usuario,
            InformacionContable informacionContable)
        {

            var facturaActual = await this.GetById(idFactura);


            var proyecto = await this._proyectoBll.GetById(facturaActual.idProyecto.Value);

            string fechaDatosContables = facturaActual.fecha.ToString("yyyyMM");


            var filter = Builders<ConsecutivoCausacion>.Filter.Eq("nit", proyecto.nit);
            var filter2 = Builders<ConsecutivoCausacion>.Filter.Eq("fechaDatosContables", fechaDatosContables);


            var user = _userManager.FindByIdAsync(usuario).Result;


            datosContableses.Insert(0, facturaActual.datosContables.FirstOrDefault(x => x.codigo == "Consecutivo"));

            datosContableses.Add(facturaActual.datosContables.FirstOrDefault(x => x.codigo == "Fecha contabilización"));
            datosContableses.Add(facturaActual.datosContables.FirstOrDefault(x => x.codigo == "Usuario"));


            facturaActual.datosContables = datosContableses.ToArray();
            facturaActual.conDatosContables = true;

            await base.Update(facturaActual);



            facturaActual = await this.GetById(idFactura);



            datosContableses = informacionContable.ToDatosContablesFormat();



            datosContableses.Add(facturaActual.datosContables.FirstOrDefault(x => x.codigo == "Fecha contabilización"));
            datosContableses.Add(facturaActual.datosContables.FirstOrDefault(x => x.codigo == "Usuario"));

            datosContableses.Insert(0, facturaActual.datosContables.FirstOrDefault(x => x.codigo == "Consecutivo"));


            var idArchivo = await ImprimirDatosContables(datosContableses.ToArray(), facturaActual.archivo, idFactura);
            if (!facturaActual.fechaDatosContables.HasValue)
            {
                facturaActual.fechaDatosContables = DateTime.Now;
            }


            //facturaActual.archivoOriginal = facturaActual.archivo;
            facturaActual.archivo = idArchivo;
            if (facturaActual.archivoFirmado != Guid.Empty)
            {
                var idArchivoFirmado = await ImprimirDatosContables(datosContableses.ToArray(), facturaActual.archivoFirmado, idFactura);
                facturaActual.archivoFirmado = idArchivoFirmado;

            }

            facturaActual.esAnticipo = informacionContable.esAnticipo;

            await base.Update(facturaActual);



            return true;
        }


        public async Task ImprimirSoloDatosContables(Guid idFactura)
        {

            var facturaActual = await this.GetById(idFactura);


            facturaActual = await this.GetById(idFactura);




            var idArchivoFirmado = await ImprimirDatosContables(facturaActual.datosContables, facturaActual.archivo, idFactura);
            facturaActual.archivoFirmado = Guid.Empty;
            facturaActual.archivo = idArchivoFirmado;
            await base.Update(facturaActual);

        }

        public async Task<Factura> EmailAprobacionCoordinador(Guid idFactura)
        {

            var factura = await this.GetById(idFactura);
            await this._emailBLL.NotificarPrimerAprobacionFacturaOT(factura);

            return factura;
        }

        public async Task<Factura> RechazoAdministradorMantenimiento(RechazoAdministradorMantenimiento rechazoAdministradorMantenimiento)
        {
            var factura = await this.GetById(rechazoAdministradorMantenimiento.IdFactura);
            var usuario = await _userManager.FindByIdAsync(rechazoAdministradorMantenimiento.IdUsuario);
            rechazoAdministradorMantenimiento.usuario = $"{usuario.Nombre} {usuario.Apellido}";

            rechazoAdministradorMantenimiento.Fecha = DateTime.Now;
            if (factura.AprobacionesMantenimiento == null)
            {
                factura.AprobacionesMantenimiento = new List<AprobacionMantenimiento> { rechazoAdministradorMantenimiento };

            }
            else
            {

                factura.AprobacionesMantenimiento.Add(rechazoAdministradorMantenimiento);
            }

            factura.aprobadaCoordinadorMantenimiento = false;
            factura.aprobadaAdministradorMantenimiento = false;
            factura.rechazadaAdmintradorMantenimiento = true;
            await base.Update(factura);

            //TODO: Notificar a Coordinador de mantenimiento que se rechazó la factura
            await this._emailBLL.NotificarRechazoFacturaOT(factura);
            return factura;


        }

        public async Task<Factura> RechazoCoordinadorMantenimiento(RechazoCoordinadorMantenimiento rechazoCoordinadorMantenimiento)
        {
            var factura = await this.GetById(rechazoCoordinadorMantenimiento.IdFactura);
            var usuario = await _userManager.FindByIdAsync(rechazoCoordinadorMantenimiento.IdUsuario);
            rechazoCoordinadorMantenimiento.usuario = $"{usuario.Nombre} {usuario.Apellido}";
            rechazoCoordinadorMantenimiento.Fecha = DateTime.Now;

            if (factura.rechazosMantenimiento == null)
            {
                factura.rechazosMantenimiento = new List<RechazoCoordinadorMantenimiento> { rechazoCoordinadorMantenimiento };
            }
            else
            {
                factura.rechazosMantenimiento.Add(rechazoCoordinadorMantenimiento);
            }

            factura.rechazada = true;
            factura.aprobadaCoordinadorMantenimiento = false;
            factura.aprobadaAdministradorMantenimiento = false;
            await base.Update(factura);

            if (rechazoCoordinadorMantenimiento.Razon.Equals("Error en recepción"))
            {
                await this._emailBLL.NotificarRechazoFacturaCoordinadorRecepcion(factura);
            }
            else
            {
                await this._emailBLL.NotificarRechazoFacturaCoordinadorProveedor(factura);

            }

            ///await this._emailBLL.NotificarRechazoFacturaContabilidad(factura);

            return factura;

        }

        public async Task<Factura> AprobacionAdministradorMantenimiento(AprobacionMantenimiento aprobacionMantenimiento, List<OrdenTrabajo> ots)
        {
            var factura = await this.GetById(aprobacionMantenimiento.IdFactura);
            var usuario = await _userManager.FindByIdAsync(aprobacionMantenimiento.IdUsuario);
            aprobacionMantenimiento.usuario = $"{usuario.Nombre} {usuario.Apellido}";

            aprobacionMantenimiento.Fecha = DateTime.Now;
            if (factura.AprobacionesMantenimiento == null)
            {
                factura.AprobacionesMantenimiento = new List<AprobacionMantenimiento> { aprobacionMantenimiento };

            }
            else
            {

                factura.AprobacionesMantenimiento.Add(aprobacionMantenimiento);
            }



            factura.aprobadaCoordinadorMantenimiento = true;
            factura.aprobadaAdministradorMantenimiento = true;
            factura.rechazadaAdmintradorMantenimiento = false;


            var idArchivo = await ImprimirAprobacionFactura(aprobacionMantenimiento, factura.AprobacionesMantenimiento, factura.archivo, ots, factura);

            factura.archivo = idArchivo;




            var filter = Builders<OrdenCompra>.Filter.Eq("proyecto", factura.idProyecto.Value);
            var filter2 = Builders<OrdenCompra>.Filter.Eq("proveedor", factura.idProveedor.Value);
            var filter3 = Builders<OrdenCompra>.Filter.Eq("consecutivo", 0);

            var ordenesXProveedorXProyecto = (await this._ordenCompraBLL.GetByProterty((filter & filter2 & filter3))).FirstOrDefault();


            OrdenCompra order;

            if (ordenesXProveedorXProyecto != null)
            {
                ordenesXProveedorXProyecto.fechaGenerado = factura.fecha;
                await this._ordenCompraBLL.Update(ordenesXProveedorXProyecto);
                order = ordenesXProveedorXProyecto;
            }
            else
            {
                var nuevaOrden = new OrdenCompra();
                nuevaOrden.fechaGenerado = DateTime.Now;
                nuevaOrden.proveedor = factura.idProveedor.Value;
                nuevaOrden.proyecto = factura.idProyecto.Value;
                order = await _ordenCompraBLL.Insert(nuevaOrden, true);

            }


            factura.idOrdenCompra = order.id;



            await base.Update(factura);

            //TODO: Notificar a auxiliar(es) contable que tenga asignado este proyecto

            await this._emailBLL.EmailFactura(factura);


            var antes = DateTime.Now;

            await this.AsociarFacturaOTMtto(factura);

            //VALIDAR SI LAC FACTURA ES DE BUSETICAS, SE DEBE LLAMAR A APROBAR

            var total = (DateTime.Now - antes).TotalSeconds;

            if (factura.idProyecto == new Guid("34bf39d1-7ff2-4c79-9a95-e45633dea28c"))
            {
                factura.aprobada = true;
                factura.observacionAprobar = "APROBADA";
                factura.aprobador = new List<string>() { usuario.Id };

                await this.Aprobar(factura);
            }

            return factura;


        }

        private async Task AsociarFacturaOTMtto(Factura factura)
        {
            if (factura.idOT.HasValue)
            {
                var urlMtto = this.configuration.GetValue(typeof(string), "urlMtto");


                var client = new RestClient($"{urlMtto}");
                var request = new RestRequest($"api/OrdenesTrabajo/AsociarFactura", Method.Post);

                var obj = new { idFactura = factura.id, fecha = factura.fecha, monto = factura.monto, numeroFactura = factura.numeroFactura, id = factura.idOT.Value };

                var objSerializado = JsonConvert.SerializeObject(obj);


                request.AddJsonBody(obj);

                var response = await client.ExecuteAsync<int>(request);
            }

        }

        public async Task<bool> ValidarEscrituraPDF(Guid uploadId)
        {

            var archivoFactura = await DescargarFactura(uploadId);

            try
            {
                using (var inputStream = new MemoryStream(archivoFactura.file))
                using (var reader = new PdfReader(inputStream))
                using (var fs = new MemoryStream())
                using (var writer = new PdfWriter(fs))
                using (var pdfDoc = new PdfDocument(reader, writer))
                {
                    // Si llegamos aquí sin excepción, el PDF se puede escribir
                    return true;
                }

            }
            catch (Exception ex)
            {
                return false;

            }

        }

        private async Task<Guid> ImprimirAprobacionFactura(AprobacionMantenimiento aprobacionMantenimiento, List<AprobacionMantenimiento> aprobacionesMantenimiento, Guid item, List<OrdenTrabajo> ots, Factura factura)
        {
            var archivoFactura = await DescargarFactura(item);

            using var inputStream = new MemoryStream(archivoFactura.file);
            using var _reader = new PdfReader(inputStream);



            var facttemp = await GetById(item);
            var bytes = await AgregarPaginaAprobacion(aprobacionMantenimiento, aprobacionesMantenimiento, archivoFactura.file, factura, ots);


            var mstream = new MemoryStream(bytes.ToArray());

            var uploadResult = await this.uploadManager.UploadFile(mstream, archivoFactura.filename);

            return uploadResult;




        }



        internal async Task ActualizarOT(AsociacionFacturaOT asociacion)
        {
            var factura = await this.GetById(asociacion.IdFactura);

            factura.OrdenesTrabajo = asociacion.IdOT;

            await base.Update(factura);
        }

        public async Task<bool> ExistFacturaProveedor(string numeroFactura, Guid proveedor)
        {
            var filter = Builders<Factura>.Filter.Eq("numeroFactura", numeroFactura);
            var filter2 = Builders<Factura>.Filter.Eq("idProveedor", proveedor);
            var filter3 = Builders<Factura>.Filter.Eq("rechazada", false);

            var filtroBorradas = Builders<Factura>.Filter.Eq("erased", false);

            var filtros = (filter & filter2 & filter3 & filtroBorradas);
            var FacturaExist = await base.GetByProterty(filtros);
            if (FacturaExist.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }

        }


        public async Task<long> ContarSinAprobarporOrden(Guid idOrden)
        {
            var filter = Builders<Factura>.Filter.Eq("idOrdenCompra", idOrden);
            var filter2 = Builders<Factura>.Filter.Eq("aprobada", false);
            var filter3 = Builders<Factura>.Filter.Eq("rechazada", false);


            var filter4 = Builders<Factura>.Filter.Eq("rechazada", false);


            var filtro5 = Builders<Factura>.Filter.Gte("fecha", new DateTime(2020, 6, 30));
            //
            var filtroBorradas = Builders<Factura>.Filter.Eq("erased", false);


            var f = await base.GetByProterty((filter & filter2 & filter3 & filtroBorradas & filtro5));
            long result = await collection.CountDocumentsAsync((filter & filter2 & filter3 & filtroBorradas & filtro5));
            return result;
        }


        public async Task<List<EjecutadoProyecto>> TotalProyectoProveedores(Guid id)
        {

            List<EjecutadoProyecto> result = new List<EjecutadoProyecto>();

            var filtroProyecto = Builders<Factura>.Filter.Eq("idProyecto", id);
            var filtroBorradas = Builders<Factura>.Filter.Eq("erased", false);


            var facturasproyecto = await this.GetByProterty(filtroProyecto & filtroBorradas);

            if (facturasproyecto != null && facturasproyecto.Count() > 0)
            {


                var grouped = facturasproyecto.GroupBy(x => x.idProveedor);

                foreach (var item in grouped)
                {
                    var ejecutadoProveedor = new EjecutadoProyecto();


                    ejecutadoProveedor.idProveedor = item.Key.ToString();
                    ejecutadoProveedor.totalEjecutado = item.Where(x => x.esAnticipo.HasValue == false || x.esAnticipo == false).Sum(x => x.monto);

                    result.Add(ejecutadoProveedor);
                }

            }

            return result;

        }


        //
        public async Task<List<EjecutadoProyecto>> TotalProyectoProveedoresMes(Guid id)
        {
            List<EjecutadoProyecto> result = new List<EjecutadoProyecto>();


            var filtroProyecto = Builders<Factura>.Filter.Eq("idProyecto", id);
            var filtroBorradas = Builders<Factura>.Filter.Eq("erased", false);

            var facturasproyecto = await this.GetByProterty(filtroProyecto & filtroBorradas);

            if (facturasproyecto != null && facturasproyecto.Count() > 0)
            {


                var grouped = facturasproyecto.GroupBy(x => new { idProveedor = x.idProveedor, fecha = x.fecha.ToString("yyyy/MM") });

                foreach (var item in grouped)
                {
                    var ejecutadoProveedor = new EjecutadoProyecto();


                    ejecutadoProveedor.idProveedor = item.Key.idProveedor.ToString();
                    ejecutadoProveedor.Mes = item.Key.fecha;
                    ejecutadoProveedor.totalEjecutado = item.Where(x => x.esAnticipo.HasValue == false || x.esAnticipo == false).Sum(x => x.monto);

                    result.Add(ejecutadoProveedor);
                }

            }

            return result;
        }


        public async Task<string> FacturasFiltradasCsv(FiltroFacturas param)
        {
            var listaFacturas = (await FacturasFiltradas(param));
            listaFacturas = CambiarHoraListaFacturas(listaFacturas);

            return await GenerarCSV(listaFacturas.OrderBy(x => x.fecha).ToList());

        }


        public async Task<List<ReporteFacturasJson>> FacturasFiltradasJson(FiltroFacturas param)
        {
            var listaFacturas = (await FacturasFiltradas(param));
            listaFacturas = CambiarHoraListaFacturas(listaFacturas);

            return await GenerarJson(listaFacturas);

        }


        private async Task<List<ReporteFacturasJson>> GenerarJson(List<Factura> listaFacturas)
        {
            List<ReporteFacturasJson> listaReporteFacturasJson = new List<ReporteFacturasJson>();

            foreach (var item in listaFacturas)
            {
                var aprobador = await this.ListToString(item.aprobador);
                var proyecto = await this.NombreProyecto(item.idProyecto);
                var proveedor = await Nombreproveedor(item.idProveedor);
                var ordenDeCompra = await OrdenDeCompra(item.idOrdenCompra);
                var nitProveedor = await NitProveedor(item.idProveedor);
                var nuevaFactura = new ReporteFacturasJson
                {
                    numeroFactura = item.numeroFactura,
                    fecha = item.fecha,
                    fechaCreado = item.fechaCreado,
                    fechaVencimiento = item.fechaVencimiento,
                    valor = item.monto,
                    aprobada = item.aprobada,
                    aprobador = aprobador,
                    rechazada = item.rechazada,
                    observacionRechazo = item.observacionRechazo,
                    Proyecto = proyecto,
                    Nit = item.nit,
                    Proveedor = proveedor,
                    NitProveedor = nitProveedor,
                    ordenDeCompra = ordenDeCompra,
                    fechaAprobacion = item.fechaAprobacion,
                    ObservacionesAprobacion = item.observacionAprobar,
                    ObservacionesRechazo = item.observacionRechazo,
                    FechaPagado = item.fechaPagado,
                    FechaDatosContables = item.fechaDatosContables,
                    FechaImpresoContabilidad = item.fechaImpresoContabilidad,
                    FechaImpresoTesoreria = item.fechaImpresoTesoreria,
                    FechaSegundaAprobacion = item.fechaAprobacion2
                };

                if (item.AprobacionesMantenimiento != null)
                {
                    if (item.AprobacionesMantenimiento.Count > 0)
                    {
                        nuevaFactura.AprobadorMantenimiento = item.AprobacionesMantenimiento[0].usuario;
                        nuevaFactura.FechaAprobacionMantenimiento = item.AprobacionesMantenimiento[0].Fecha;
                    }
                }

                listaReporteFacturasJson.Add(nuevaFactura);

            }


            return listaReporteFacturasJson.OrderBy(f => f.fecha).ToList();


        }



        private async Task<string> GenerarCSV(List<Factura> listaFacturas)
        {

            var sb = new StringBuilder("numeroFactura|fecha|fechaCreado|fechaVencimiento|valor|aprobada|rechazada|aprobador|observacionRechazo|Proyecto|Proveedor|ordenDeCompra|fechaAprobacion|ObservacionesAprobacion|ObservacionesRechazo|FechaPagado|FechaDatosContables|" +
                "FechaImpresoContabilidad|FechaImpresoTesoreria|FechaSegundaAprobacion|AprobadorMantenimiento|FechaAprobacionMantenimiento");
            sb.AppendLine();

            foreach (var item in listaFacturas)
            {
                sb.AppendLine(await LineaReporte(item));
            }

            return sb.ToString();


        }



        private async Task<string> LineaReporte(Factura item)
        {
            string linea;
            try
            {

                linea = $"{item.numeroFactura}|{item.fecha.ToString("dd/MM/yyyy hh:mm:ss tt")}|{item.fechaCreado.ToString("dd/MM/yyyy hh:mm:ss tt")}|{item.fechaVencimiento.ToString("dd/MM/yyyy hh:mm:ss tt")}|{item.monto}|" +
                       $"{item.aprobada}|{item.rechazada}|{await this.ListToString(item.aprobador)}|{item.observacionRechazo}|{await this.NombreProyecto(item.idProyecto)}|{await Nombreproveedor(item.idProveedor)}|{await OrdenDeCompra(item.idOrdenCompra)}|{(item.fechaAprobacion.HasValue ? item.fechaAprobacion.Value.ToString("dd/MM/yyyy hh:mm:ss tt") : string.Empty)}|" +
                       $"{item.observacionAprobar}|{item.observacionRechazo}|{(item.fechaPagado.HasValue ? item.fechaPagado.Value.ToString("dd/MM/yyyy hh:mm:ss tt") : string.Empty)}|{(item.fechaDatosContables.HasValue ? item.fechaDatosContables.Value.ToString("dd/MM/yyyy hh:mm:ss tt") : string.Empty)}|" +
                       $"{(item.fechaImpresoContabilidad.HasValue ? item.fechaImpresoContabilidad.Value.ToString("dd/MM/yyyy hh:mm:ss tt") : string.Empty)}|{(item.fechaImpresoTesoreria.HasValue ? item.fechaImpresoTesoreria.Value.ToString("dd/MM/yyyy hh:mm:ss tt") : string.Empty)}|{(item.fechaAprobacion2.HasValue ? item.fechaAprobacion2.Value.ToString("dd/MM/yyyy hh:mm:ss tt") : string.Empty)}";
                if (item.AprobacionesMantenimiento != null)
                {
                    if (item.AprobacionesMantenimiento.Count > 0)
                    {
                        var aprobador = $"{item.AprobacionesMantenimiento[0].usuario}";
                        var fechaAprobacionMantenimiento = $"{item.AprobacionesMantenimiento[0].Fecha.Value.ToString("dd/MM/yyyy")}";
                        linea = linea += $"|{aprobador}|{fechaAprobacionMantenimiento}";
                    }
                }


            }
            catch (Exception ex)
            {

                throw;
            }
            return linea;
        }

        private async Task<int> OrdenDeCompra(Guid? idOrdenCompra)
        {

            int result = 0;

            if (idOrdenCompra.HasValue)
            {
                var orden = await this._ordenCompraBLL.GetById(idOrdenCompra.Value);
                if (orden != null)
                {

                    return orden.consecutivo;
                }
            }

            return result;
        }

        private async Task<string> NombreProyecto(Guid? idProyecto)
        {
            string result = string.Empty;

            if (idProyecto.HasValue)
            {
                var proyecto = await this._proyectoBll.GetById(idProyecto.Value);

                return proyecto?.nombre ?? "No encontrado";
            }

            return result;
        }

        private async Task<string> Nombreproveedor(Guid? idProveedor)
        {
            string result = string.Empty;

            if (idProveedor.HasValue)
            {
                var proveedor = await this._proveedorBll.GetById(idProveedor.Value);

                return proveedor?.nombre ?? "No encontrado";
            }

            return result;
        }

        private async Task<string> NitProveedor(Guid? idProveedor)
        {
            string result = string.Empty;

            if (idProveedor.HasValue)
            {
                var proveedor = await this._proveedorBll.GetById(idProveedor.Value);

                return proveedor?.nit ?? "No encontrado";
            }

            return result;
        }




        private async Task<string> ListToString(List<string> aprobador)
        {
            var result = string.Empty;

            if (aprobador != null)
            {
                foreach (var item in aprobador)
                {
                    var usuario = await _userManager.FindByIdAsync(item);

                    if (usuario != null)
                    {
                        result += ($"{usuario.Nombre} {usuario.Apellido} - ");
                    }
                }
            }

            return result;
        }

        public async Task<List<Factura>> FacturasFiltradas(FiltroFacturas param)
        {

            var result = new List<Factura>();

            if (param.proyectos != null && param.proyectos.Count() > 0)
            {
                if (param.proveedor == null || param.proveedor.Count() == 0)
                {

                    result = await this.GetAprobadasXProyecto(param);
                }
                else
                {
                    result = await this.GetAprobadasXProyectoXProveedor(param);
                }
            }
            else
            {

                if (param.proveedor != null && param.proveedor.Count() > 0)
                {
                    result = await this.GetAprobadasXProveedor(param);
                }
                else
                {
                    result = await this.GetAprobadasFecha(param);


                }
            }

            List<MongoIdentityUser> listaUsuarios = new List<MongoIdentityUser>();

            listaUsuarios = _userManager.Users.ToList();


            foreach (var item in result)
            {
                await this.obtenerNombreUsuario(item, listaUsuarios);
            }


            foreach (var item in result)
            {
                if (item.datosContables != null)
                {
                    var esAnticipo = item.datosContables.Where(x => x.codigo == "EsAnticipo").FirstOrDefault();
                    if (esAnticipo != null)
                    {
                        if (item.esAnticipo.HasValue)
                        {
                            esAnticipo.valor = item.esAnticipo.ToString();
                        }
                        else
                        {
                            esAnticipo.valor = false.ToString();
                        }
                    }
                    else
                    {

                        var dc = item.datosContables.ToList();
                        esAnticipo = new DatosContables();
                        dc.Add(esAnticipo);
                        esAnticipo.codigo = "EsAnticipo";
                        esAnticipo.valor = false.ToString();

                        if (item.esAnticipo.HasValue)
                        {
                            esAnticipo.valor = item.esAnticipo.ToString();
                        }
                        else
                        {
                            esAnticipo.valor = false.ToString();
                        }

                        item.datosContables = dc.ToArray();
                    }
                }
            }


            return result;
        }

        private async Task obtenerNombreUsuario(Factura item, List<MongoIdentityUser> listaUsuarios)
        {

            if (item.aprobador != null)
            {
                item.nombreAprobador = new List<string>();
                foreach (var usr in item.aprobador)
                {
                    var usuario = listaUsuarios.Where(x => x.Id == usr).FirstOrDefault();

                    try
                    {
                        item.nombreAprobador.Add(usuario.UserName);
                    }
                    catch (Exception ex)
                    {

                        throw;
                    }
                }
            }

        }

        private async Task<List<Factura>> GetAprobadasFecha(FiltroFacturas param)
        {
            var filter3 = Builders<Factura>.Filter.Gte("fecha", param.inicio);
            var filter4 = Builders<Factura>.Filter.Lte("fecha", param.fin);
            var filtroBorradas = Builders<Factura>.Filter.Eq("erased", false);

            if (param.estado.Equals("Aprobada"))
            {
                var filtroAprobadas = Builders<Factura>.Filter.Eq("aprobada", true);
                var ordenesXFecha = await this.GetByProterty(filter3 & filter4 & filtroBorradas & filtroAprobadas);
                return ordenesXFecha;
            }
            else if (param.estado.Equals("Rechazada"))
            {
                var filtroRechazadas = Builders<Factura>.Filter.Eq("rechazada", true);

                var ordenesXFecha = await this.GetByProterty(filter3 & filter4 & filtroBorradas & filtroRechazadas);
                return ordenesXFecha;
            }

            else if (param.estado.Equals("Pagada"))
            {
                var filtroRechazadas = Builders<Factura>.Filter.Eq("pagada", true);

                var ordenesXFecha = await this.GetByProterty(filter3 & filter4 & filtroBorradas & filtroRechazadas);
                return ordenesXFecha;
            }
            else if (param.estado.Equals("Pendiente"))
            {
                var filtroAprobadas = Builders<Factura>.Filter.Eq("aprobada", false);
                var filtroRechazadas = Builders<Factura>.Filter.Eq("rechazada", false);

                var ordenesXFecha = await this.GetByProterty(filter3 & filter4 & filtroBorradas
                                                                & filtroAprobadas & filtroRechazadas);
                return ordenesXFecha;
            }
            else
            {
                var ordenesXFecha = await this.GetByProterty(filter3 & filter4 & filtroBorradas);
                return ordenesXFecha;
            }

            //var FacturasXFecha = await this.GetByProterty(filter3 & filter4 & filtroBorradas & filtroEstado);

            //return FacturasXFecha;
        }

        private async Task<List<Factura>> GetAprobadasXProveedor(FiltroFacturas param)
        {
            var filter0 = Builders<Factura>.Filter.Eq("idProveedor", param.proveedor[0]);

            if (param.proveedor[0] == Guid.Empty)
            {
                filter0 = Builders<Factura>.Filter.Eq("idProveedor", BsonNull.Value);
            }

            var filter3 = Builders<Factura>.Filter.Gte("fecha", param.inicio);
            var filter4 = Builders<Factura>.Filter.Lte("fecha", param.fin);
            var filtroBorradas = Builders<Factura>.Filter.Eq("erased", false);


            for (int i = 1; i < param.proveedor.Count(); i++)
            {
                var filter = Builders<Factura>.Filter.Eq("idProveedor", param.proveedor[i]);

                if (param.proveedor[i] == Guid.Empty)
                {
                    filter0 = Builders<Factura>.Filter.Eq("idProveedor", BsonNull.Value);
                }
                filter0 = (filter0 | filter);

            }

            if (param.estado.Equals("Aprobada"))
            {
                var filtroAprobadas = Builders<Factura>.Filter.Eq("aprobada", true);
                var ordenesXProveedor = await this.GetByProterty(filter0 & filter3 & filter4 & filtroBorradas & filtroAprobadas);

                return ordenesXProveedor;
            }
            else if (param.estado.Equals("Rechazada"))
            {
                var filtroRechazadas = Builders<Factura>.Filter.Eq("rechazada", true);
                var ordenesXProveedor = await this.GetByProterty(filter0 & filter3 & filter4 & filtroBorradas & filtroRechazadas);

                return ordenesXProveedor;
            }
            else if (param.estado.Equals("Pendiente"))
            {
                var filtroAprobadas = Builders<Factura>.Filter.Eq("aprobada", false);
                var filtroRechazadas = Builders<Factura>.Filter.Eq("rechazada", false);

                var ordenesXProveedor = await this.GetByProterty(filter0 & filter3 & filter4
                                                                    & filtroBorradas & filtroAprobadas & filtroRechazadas);

                return ordenesXProveedor;
            }

            else
            {
                var ordenesXProveedor = await this.GetByProterty(filter0 & filter3 & filter4 & filtroBorradas);

                return ordenesXProveedor;
            }

            //var ordenesXProveedor = await this.GetByProterty(filter0 & (filter3 & filter4) & filtroBorradas );

            //return ordenesXProveedor;
        }

        private async Task<List<Factura>> GetAprobadasXProyectoXProveedor(FiltroFacturas param)
        {

            var result = new List<Factura>();
            foreach (var proyecto in param.proyectos)
            {
                foreach (var proveedor in param.proveedor)
                {
                    var filter = Builders<Factura>.Filter.Eq("idProyecto", proyecto);

                    if (proyecto == Guid.Empty)
                    {
                        filter = Builders<Factura>.Filter.Eq("idProyecto", BsonNull.Value);
                    }

                    var filter2 = Builders<Factura>.Filter.Eq("idProveedor", proveedor);
                    var filter3 = Builders<Factura>.Filter.Gte("fecha", param.inicio);
                    var filter4 = Builders<Factura>.Filter.Lte("fecha", param.fin);
                    var filtroBorradas = Builders<Factura>.Filter.Eq("erased", false);

                    List<Factura> ordenesXProyectoXProveedor;

                    if (param.estado.Equals("Aprobada"))
                    {
                        var filtroAprobadas = Builders<Factura>.Filter.Eq("aprobada", true);
                        ordenesXProyectoXProveedor = await this.GetByProterty(filter & filter2 & filter3 & filter4
                                                                                & filtroBorradas & filtroAprobadas);
                    }
                    else if (param.estado.Equals("Rechazada"))
                    {
                        var filtroRechazadas = Builders<Factura>.Filter.Eq("rechazada", true);
                        ordenesXProyectoXProveedor = await this.GetByProterty(filter & filter2 & filter3 & filter4
                                                                                & filtroBorradas & filtroRechazadas);
                    }
                    else if (param.estado.Equals("Pendiente"))
                    {
                        var filtroAprobadas = Builders<Factura>.Filter.Eq("aprobada", false);
                        var filtroRechazadas = Builders<Factura>.Filter.Eq("rechazada", false);
                        ordenesXProyectoXProveedor = await this.GetByProterty(filter & filter2 & filter3 & filter4
                                                                            & filtroBorradas & filtroAprobadas & filtroRechazadas);
                    }
                    else
                    {
                        ordenesXProyectoXProveedor = await this.GetByProterty(filter & filter2 & filter3 & filter4 & filtroBorradas);
                    }
                    //var ordenesXProveedorXProyecto = await this.GetByProterty((filter & filter2 & filtroBorradas & filter3 & filter4));

                    result.AddRange(ordenesXProyectoXProveedor);

                }
            }

            return result;
        }

        private async Task<List<Factura>> GetAprobadasXProyecto(FiltroFacturas param)
        {
            var filter0 = Builders<Factura>.Filter.Eq("idProyecto", param.proyectos[0]);

            if (param.proyectos[0] == Guid.Empty)
            {
                filter0 = Builders<Factura>.Filter.Eq("idProyecto", BsonNull.Value);
            }


            var filter3 = Builders<Factura>.Filter.Gte("fecha", param.inicio);
            var filter4 = Builders<Factura>.Filter.Lte("fecha", param.fin);
            var filtroBorradas = Builders<Factura>.Filter.Eq("erased", false);


            for (int i = 1; i < param.proyectos.Count(); i++)
            {

                var filter = Builders<Factura>.Filter.Eq("idProyecto", param.proyectos[i]);

                if (param.proyectos[i] == Guid.Empty)
                {
                    filter = Builders<Factura>.Filter.Eq("idProyecto", BsonNull.Value);
                }


                filter0 = filter0 | filter;

            }

            if (param.estado.Equals("Aprobada"))
            {
                var filtroAprobadas = Builders<Factura>.Filter.Eq("aprobada", true);
                var ordenesXProyecto = await this.GetByProterty(filter0 & filter3 & filter4 & filtroBorradas & filtroAprobadas);
                return ordenesXProyecto;
            }
            else if (param.estado.Equals("Rechazada"))
            {
                var filtroRechazadas = Builders<Factura>.Filter.Eq("rechazada", true);

                var ordenesXProyecto = await this.GetByProterty(filter0 & filter3 & filter4 & filtroBorradas & filtroRechazadas);
                return ordenesXProyecto;
            }
            else if (param.estado.Equals("Pagada"))
            {
                var filtroRechazadas = Builders<Factura>.Filter.Eq("pagada", true);

                var ordenesXProyecto = await this.GetByProterty(filter0 & filter3 & filter4 & filtroBorradas & filtroRechazadas);
                return ordenesXProyecto;
            }

            else if (param.estado.Equals("Pendiente"))
            {
                var filtroAprobadas = Builders<Factura>.Filter.Eq("aprobada", false);
                var filtroRechazadas = Builders<Factura>.Filter.Eq("rechazada", false);
                var ordenesXProyecto = await this.GetByProterty(filter0 & filter3 & filter4
                                                                & filtroBorradas & filtroAprobadas & filtroRechazadas);

                return ordenesXProyecto;
            }
            else
            {
                var ordenesXProyecto = await this.GetByProterty(filter0 & filter3 & filter4 & filtroBorradas);
                return ordenesXProyecto;
            }

        }








        public override async Task Update(Factura item)
        {
            var old = await this.GetById(item.id);

            if (item.idOrdenCompra == null)
            {
                item.idOrdenCompra = old.idOrdenCompra;
            }


            if (item.aprobador != null && item.aprobador.Count == 1 && old.aprobador == null)
            {
                item.fechaAprobacion = DateTime.Now;
            }

            if (item.rechazada == true && old.rechazada == false)
            {
                await _emailBLL.EmailFacturaRechazada(item);
            }



            await base.Update(item);
        }


        public async Task<Factura> Aprobar(Factura item)
        {

            if (item.fechaAprobacion.HasValue)
            {
                item.fechaAprobacion2 = DateTime.Now;
            }
            else
            {
                item.fechaAprobacion = DateTime.Now;

            }

            try
            {

                var result = await this.FirmarFactura(item);
                var itemActual = base.GetById(result);
                item.archivoFirmado = result;



                await base.Update(item);
                await _emailBLL.NotificarAlertaPagoTesoreria(item);
                // enviar correo
                return item;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<bool> Impreso(Guid Id)
        {
            var exist = await this.GetById(Id);

            exist.impresoContabilidad = true;
            exist.fechaImpresoContabilidad = DateTime.Now;

            await base.Update(exist);

            return true;
        }
        public async Task<bool> ImpresoTesoreria(Guid Id)
        {
            var exist = await this.GetById(Id);

            exist.impresoTesoreria = true;
            exist.fechaImpresoTesoreria = DateTime.Now;

            await base.Update(exist);

            return true;
        }
        public async Task<bool> Pagado(Guid Id, string idUsuario)
        {
            var exist = await this.GetById(Id);

            exist.pagada = true;
            exist.usuarioTesoreria = idUsuario;
            exist.fechaPagado = DateTime.Now;

            await base.Update(exist);

            return true;
        }

        private async Task<Guid> FirmarFactura(Factura item)
        {
            var usuariosAprobadores = item.aprobador;

            var firmas = await DescargarFirmas(usuariosAprobadores);

            var archivoFactura = await DescargarFactura(item.archivo);

            var facttemp = await GetById(item.id);
            var bytes = AgregarFirmas(firmas, archivoFactura.file, item.observacionAprobar, facttemp);
            //var bytes = fileStream.ToArray();


            var mstream = new MemoryStream(bytes.ToArray());

            var uploadResult = await this.uploadManager.UploadFile(mstream, archivoFactura.filename);

            return uploadResult;
            //{bac2edf9-ca1d-420b-b4c1-7eb944c254c7}


        }


        private async Task<Guid> ImprimirDatosContables(DatosContables[] datosContables, Guid item, Guid idFactura)
        {


            var archivoFactura = await DescargarFactura(item);

            var bytesArchivoPdf = ExtraerPdfConClave(archivoFactura.file);


            using var inputStream = new MemoryStream(bytesArchivoPdf);
            using var _reader = new PdfReader(inputStream);





            var facttemp = await GetById(idFactura);
            var bytes = AgregarDatosContables(datosContables, bytesArchivoPdf, facttemp);



            var mstream = new MemoryStream(bytes.ToArray());

            var uploadResult = await this.uploadManager.UploadFile(mstream, archivoFactura.filename);

            return uploadResult;

        }



        private static MemoryStream AgregarFirmas(List<KeyValuePair<string, byte[]>> firmas, byte[] pdfBytes, string observaciones, Factura item)
        {
            using (var inputStream = new MemoryStream(pdfBytes))
            using (var reader = new PdfReader(inputStream))
            using (var fs = new MemoryStream())
            using (var writer = new PdfWriter(fs))
            using (var pdfDoc = new PdfDocument(reader, writer))
            {
                var pageCount = pdfDoc.GetNumberOfPages();
                var layer = new PdfLayer("WatermarkLayer", pdfDoc);
                var paginaInicial = 1;

                if (item.isOT == true)
                {
                    if (pageCount > 1)
                    {
                        paginaInicial = 2;
                    }
                }

                for (var i = paginaInicial; i <= pageCount; i++)
                {
                    var page = pdfDoc.GetPage(i);
                    var rect = page.GetPageSize();
                    var canvas = new PdfCanvas(page);
                    canvas.BeginLayer(layer);

                    for (int j = 0; j < firmas.Count; j++)
                    {
                        if (j >= 2)
                        {
                            break;
                        }

                        float h = 100;
                        var image = firmas[j];
                        var imageData = ImageDataFactory.Create(image.Value);
                        var pic = new Image(imageData);

                        // Scale image if too tall
                        if (pic.GetImageHeight() > h)
                        {
                            float percentage = h / pic.GetImageHeight();
                            pic.Scale(percentage, percentage);
                        }

                        // Set position
                        float xPos = (200 * j) + 20;
                        float yPos = 0;

                        pic.SetFixedPosition(xPos, yPos);
                        
                        // Use Canvas to add the scaled image
                        var layoutCanvas = new iText.Layout.Canvas(canvas, page.GetPageSize());
                        layoutCanvas.Add(pic);
                        layoutCanvas.Close();


                        // Add signature text
                        var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                        var gState = new PdfExtGState().SetFillOpacity(1.0f);
                        canvas.SetExtGState(gState);
                        canvas.SetFillColor(ColorConstants.BLACK);

                        canvas.BeginText()
                            .SetFontAndSize(font, 12)
                            .MoveText(xPos, 20)
                            .ShowText(image.Key)
                            .EndText();

                        var fechaFirma = DateTime.Now.AddHours(-0);

                        if (j == 0 && item.fechaAprobacion.HasValue)
                        {
                            fechaFirma = item.fechaAprobacion.Value.AddHours(-0);
                        }

                        if (j == 1 && item.fechaAprobacion2.HasValue)
                        {
                            fechaFirma = item.fechaAprobacion2.Value;
                        }

                        canvas.BeginText()
                            .SetFontAndSize(font, 12)
                            .MoveText(xPos, 10)
                            .ShowText(fechaFirma.ToString("dd/MMM/yyyy - hh:mm:ss"))
                            .EndText();
                    }

                    if (!string.IsNullOrEmpty(observaciones))
                    {
                        var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                        canvas.BeginText()
                            .SetFontAndSize(font, 12)
                            .MoveText(20, 0)
                            .ShowText("Observaciones: " + observaciones)
                            .EndText();
                    }

                    canvas.EndLayer();
                }

                pdfDoc.Close();
                return fs;
            }

        }


        private async Task<DataTable> GetDataTable(List<AprobacionMantenimiento> aprobaciones, Factura factura, List<OrdenTrabajo> OTs)
        {
            DataTable table = new DataTable();




            //    public DateTime? Fecha { get; set; }
            //public string IdUsuario { get; set; }
            //public Guid IdFactura { get; set; }
            //public string Comentarios { get; set; }
            //public string usuario { get; set; }

            table.Columns.Add("Fecha");
            table.Columns.Add("Usuario");
            table.Columns.Add("Comentarios");

            foreach (var aprobacion in aprobaciones)
            {
                var row = table.NewRow();
                row["Fecha"] = aprobacion.Fecha;
                row["Usuario"] = aprobacion.usuario;
                row["Comentarios"] = aprobacion.Comentarios;
                table.Rows.Add(row);
            }


            if (factura.isOT == true)
            {



                foreach (var item in OTs)
                {
                    var row = table.NewRow();
                    row["Fecha"] = item.fechaCreacion;
                    row["Usuario"] = "";
                    row["Comentarios"] = $"OT: {item.numeroOrden} {item.observacion}";
                    table.Rows.Add(row);
                }


            }


            return table;
        }

        private async Task<MemoryStream> AgregarPaginaAprobacion(AprobacionMantenimiento aprobacionMantenimiento, List<AprobacionMantenimiento> aprobacionesMantenimiento, byte[] bytesArchivoPdf, Factura facttemp, List<OrdenTrabajo> ots)
        {
            using (var inputStream = new MemoryStream(bytesArchivoPdf))
            using (var reader = new PdfReader(inputStream))
            using (var fs = new MemoryStream())
            using (var writer = new PdfWriter(fs))
            using (var pdfDoc = new PdfDocument(reader, writer))
            {
                var document = new Document(pdfDoc);
                var rectangle = pdfDoc.GetDefaultPageSize();

                // Insert a new page at the beginning
                pdfDoc.AddNewPage(1, rectangle);
                var pageCount = 1;

                // Create fonts
                var normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

                // Create table
                Table table = null;
                DataTable dt = await GetDataTable(aprobacionesMantenimiento, facttemp, ots);

                if (dt != null)
                {
                    table = new Table(dt.Columns.Count);
                    table.SetWidth(UnitValue.CreatePercentValue(100));

                    // Add header cells
                    var headerCell1 = new Cell().Add(new Paragraph("Fecha").SetFont(normalFont).SetFontSize(7));
                    headerCell1.SetBackgroundColor(ColorConstants.WHITE);
                    table.AddCell(headerCell1);

                    var headerCell2 = new Cell().Add(new Paragraph("Usuario").SetFont(normalFont).SetFontSize(7));
                    headerCell2.SetBackgroundColor(ColorConstants.WHITE);
                    table.AddCell(headerCell2);

                    var headerCell3 = new Cell().Add(new Paragraph("Comentarios").SetFont(normalFont).SetFontSize(7));
                    headerCell3.SetBackgroundColor(ColorConstants.WHITE);
                    table.AddCell(headerCell3);

                    // Add data rows
                    for (int rows = 0; rows < dt.Rows.Count; rows++)
                    {
                        for (int column = 0; column < dt.Columns.Count; column++)
                        {
                            var cellText = dt.Rows[rows][column].ToString();
                            var cell = new Cell().Add(new Paragraph(cellText).SetFont(normalFont).SetFontSize(7));
                            cell.SetBackgroundColor(ColorConstants.WHITE);
                            table.AddCell(cell);
                        }
                    }

                    // Position table on the page
                    table.SetFixedPosition(0, 700, rectangle.GetWidth());
                }

                // Add signatures
                var firmas = await DescargarFirmas(new List<string>() { aprobacionMantenimiento.IdUsuario.ToString() });
                var page = pdfDoc.GetPage(pageCount);
                var canvas = new PdfCanvas(page);

                var h = 100f;
                for (int j = 0; j < firmas.Count && j < 2; j++)
                {
                    var image = firmas[j];
                    var imageData = ImageDataFactory.Create(image.Value);

                    // Scale image if needed
                    if (imageData.GetHeight() > h)
                    {
                        var percentage = h / imageData.GetHeight();
                        imageData.SetWidth(imageData.GetWidth() * percentage);
                        imageData.SetHeight(h);
                    }

                    // Position and add image
                    var x = (200 * j) + 20;
                    var y = 0f;
                    canvas.AddImageFittedIntoRectangle(imageData, new Rectangle(x, y, imageData.GetWidth(), imageData.GetHeight()), false);

                    // Add border around image
                    canvas.SetStrokeColor(ColorConstants.BLACK)
                          .SetLineWidth(3)
                          .Rectangle(x, y, imageData.GetWidth(), imageData.GetHeight())
                          .Stroke();

                    // Add text below image
                    canvas.BeginText()
                          .SetFontAndSize(PdfFontFactory.CreateFont(StandardFonts.HELVETICA), 12)
                          .SetColor(ColorConstants.BLACK, true)
                          .MoveText(x, 20)
                          .ShowText(image.Key)
                          .EndText();

                    var fechaFirma = DateTime.Now.AddHours(-0);
                    canvas.BeginText()
                          .SetFontAndSize(PdfFontFactory.CreateFont(StandardFonts.HELVETICA), 12)
                          .SetColor(ColorConstants.BLACK, true)
                          .MoveText(x, 10)
                          .ShowText(fechaFirma.ToString("dd/MMM/yyyy - hh:mm:ss"))
                          .EndText();
                }

                document.Close();
                return fs;
            }
        }

        private static MemoryStream AgregarDatosContables(DatosContables[] datosContables, byte[] bytesArchivoPdf, Factura item)
        {
            using (var inputStream = new MemoryStream(bytesArchivoPdf))
            using (var reader = new PdfReader(inputStream))
            using (var fs = new MemoryStream())
            using (var writer = new PdfWriter(fs))
            using (var pdfDoc = new PdfDocument(reader, writer))
            {
                var numPages = pdfDoc.GetNumberOfPages();

                // Create fonts
                var normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

                // Create table
                Table table = null;
                DataTable dt = GetDataTable(datosContables);

                if (dt != null)
                {
                    table = new Table(dt.Columns.Count);
                    table.SetWidth(UnitValue.CreatePercentValue(30)); // Reducir ancho para evitar conflicto con firmas

                    // Add header cells
                    var headerCell1 = new Cell().Add(new Paragraph("Concepto").SetFont(boldFont).SetFontSize(8));
                    headerCell1.SetBackgroundColor(ColorConstants.WHITE);
                    table.AddCell(headerCell1);

                    var headerCell2 = new Cell().Add(new Paragraph("Valor").SetFont(boldFont).SetFontSize(8));
                    headerCell2.SetBackgroundColor(ColorConstants.WHITE);
                    table.AddCell(headerCell2);

                    // Add data rows
                    for (int rows = 0; rows < dt.Rows.Count; rows++)
                    {
                        for (int column = 0; column < dt.Columns.Count; column++)
                        {
                            var cellText = dt.Rows[rows][column].ToString();
                            var cell = new Cell();

                            if (rows == 14 && item.noteCredito == false)
                            {
                                cell.Add(new Paragraph(cellText).SetFont(boldFont).SetFontSize(9));
                            }
                            else if (rows == 14 && item.noteCredito == true)
                            {
                                cell.Add(new Paragraph("- " + cellText).SetFont(boldFont).SetFontSize(9).SetFontColor(ColorConstants.RED));
                            }
                            else
                            {
                                cell.Add(new Paragraph(cellText).SetFont(normalFont).SetFontSize(7));
                            }

                            cell.SetBackgroundColor(ColorConstants.WHITE);
                            table.AddCell(cell);
                        }
                    }

                    // Add table to first page using Canvas (like Program.cs example)
                    var pdfPage = pdfDoc.GetPage(1);
                    var pageSize = pdfPage.GetPageSize();
                    var canvas = new iText.Layout.Canvas(new PdfCanvas(pdfPage), pageSize);
                    
                    // Position table higher to avoid conflict with signatures
                    float tableX = 50; // 50 units from left edge
                    float tableY = 150; // 150 units from bottom (higher than signatures)
                    
                    table.SetFixedPosition(tableX, tableY, 180); // width 180 units (smaller)
                    canvas.Add(table);
                    
                    canvas.Close();
                }

                pdfDoc.Close();
                return fs;
            }
        }

        private static object DesbloquearFactura()
        {
            return null;
        }

        private static DataTable GetDataTable(DatosContables[] datosContables)
        {
            DataTable table = new DataTable();

            table.Columns.Add("Item");
            table.Columns.Add("Valor");


            foreach (var item in datosContables)
            {
                var row = table.NewRow();
                row["Item"] = item.codigo;
                row["Valor"] = item.valor;

                table.Rows.Add(row);
            }

            return table;
        }


        private async Task<DataTable> GetDataTable(AprobacionMantenimiento aprobacion)
        {
            DataTable table = new DataTable();


            //    public DateTime? Fecha { get; set; }
            //public string IdUsuario { get; set; }
            //public Guid IdFactura { get; set; }
            //public string Comentarios { get; set; }
            //public string usuario { get; set; }

            table.Columns.Add("Item");
            table.Columns.Add("Valor");

            var row = table.NewRow();
            row["Item"] = "Fecha Aprobación";
            row["Valor"] = aprobacion.Fecha;
            table.Rows.Add(row);


            var usuario = await _userManager.FindByIdAsync(aprobacion.IdUsuario);
            var row2 = table.NewRow();
            row2["Item"] = "Aprobado por: ";
            row2["Valor"] = usuario.UserName;
            table.Rows.Add(row2);



            var row3 = table.NewRow();
            row3["Item"] = "Comentarios";
            row3["Valor"] = aprobacion.Comentarios;
            table.Rows.Add(row3);



            return table;
        }

        private async Task<FileDonwnload> DescargarFactura(Guid archivo)
        {

            var file = await this.uploadManager.DownloadFile(archivo.ToString());

            return file;
        }

        private async Task<List<KeyValuePair<string, byte[]>>> DescargarFirmas(List<string> usuariosAprobadores)
        {

            var result = new List<KeyValuePair<string, byte[]>>();

            foreach (var id in usuariosAprobadores)
            {
                var user = _userManager.FindByIdAsync(id).Result;

                var file = await this.uploadManager.DownloadFile(user.imageId.ToString());
                result.Add(new KeyValuePair<string, byte[]>(user.Nombre, file.file));
            }

            return result;
        }

        public async Task<Factura> AprobacionCoordinador(AprobacionMantenimiento aprobacionMantenimiento)
        {

            var factura = await this.GetById(aprobacionMantenimiento.IdFactura);
            var usuario = await _userManager.FindByIdAsync(aprobacionMantenimiento.IdUsuario);

            aprobacionMantenimiento.usuario = $"{usuario.Nombre} {usuario.Apellido}";

            aprobacionMantenimiento.Fecha = DateTime.Now;
            if (factura.AprobacionesMantenimiento == null)
            {

                factura.AprobacionesMantenimiento = new List<AprobacionMantenimiento> { aprobacionMantenimiento };

            }
            else
            {

                factura.AprobacionesMantenimiento.Add(aprobacionMantenimiento);
            }

            factura.aprobadaCoordinadorMantenimiento = true;
            factura.aprobadaAdministradorMantenimiento = false;
            factura.rechazadaAdmintradorMantenimiento = false;
            await base.Update(factura);
            await this._emailBLL.NotificarPrimerAprobacionFacturaOT(factura);
            return factura;
        }
        public async Task<Factura> RechazoCoordinador(AprobacionMantenimiento aprobacionMantenimiento)
        {
            var factura = await this.GetById(aprobacionMantenimiento.IdFactura);
            var usuario = await _userManager.FindByIdAsync(aprobacionMantenimiento.IdUsuario);

            aprobacionMantenimiento.usuario = $"{usuario.Nombre} {usuario.Apellido}";

            aprobacionMantenimiento.Fecha = DateTime.Now;
            if (factura.AprobacionesMantenimiento == null)
            {

                factura.AprobacionesMantenimiento = new List<AprobacionMantenimiento> { aprobacionMantenimiento };

            }
            else
            {

                factura.AprobacionesMantenimiento.Add(aprobacionMantenimiento);
            }

            factura.aprobadaCoordinadorMantenimiento = true;
            factura.aprobadaAdministradorMantenimiento = false;
            factura.rechazadaAdmintradorMantenimiento = false;
            await base.Update(factura);
            await this._emailBLL.NotificarPrimerAprobacionFacturaOT(factura);
            return factura;
        }
        public async Task<bool> BorrarFacturaBaseDatosXId(Guid idFactura)
        {
            await base.DeleteByIdFromDataBase(idFactura);
            return true;
        }
        public List<Factura> CambiarHoraListaFacturas(List<Factura> Facturas)
        {

            Facturas.ForEach(factura =>
            {
                factura.fecha = CambiarZonaHoraria(factura.fecha);
                factura.fechaCreado = CambiarZonaHoraria(factura.fechaCreado);
                factura.fechaVencimiento = CambiarZonaHoraria(factura.fechaVencimiento);

                factura.fechaAprobacion = CambiarZonaHorariaValidarFecha(factura.fechaAprobacion);
                factura.fechaAprobacion2 = CambiarZonaHorariaValidarFecha(factura.fechaAprobacion2);
                factura.fechaDatosContables = CambiarZonaHorariaValidarFecha(factura.fechaDatosContables);
                factura.fechaImpresoContabilidad = CambiarZonaHorariaValidarFecha(factura.fechaImpresoContabilidad);
                factura.fechaImpresoTesoreria = CambiarZonaHorariaValidarFecha(factura.fechaImpresoTesoreria);
                factura.fechaPagado = CambiarZonaHorariaValidarFecha(factura.fechaPagado);

            });


            return Facturas;
        }
        public DateTime CambiarZonaHoraria(DateTime fecha)
        {
            TimeZoneInfo zonaHoraria = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
            DateTime fechaColombia = TimeZoneInfo.ConvertTimeFromUtc(fecha, zonaHoraria);

            return fechaColombia;
        }
        public DateTime? CambiarZonaHorariaValidarFecha(DateTime? fecha)
        {
            if (fecha.HasValue)
            {
                TimeZoneInfo zonaHoraria = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
                DateTime fechaColombia = TimeZoneInfo.ConvertTimeFromUtc(Convert.ToDateTime(fecha), zonaHoraria);

                return fechaColombia;
            }
            else
            {
                return fecha;
            }
        }

        public Factura CambiarHoraFactura(Factura result)
        {
            result.fecha = result.fecha.Subtract(TimeSpan.FromHours(5));
            result.fechaCreado = result.fechaCreado.Subtract(TimeSpan.FromHours(5));
            result.fechaVencimiento = result.fechaVencimiento.Subtract(TimeSpan.FromHours(5));
            result.fechaAprobacion = validarFecha(result.fechaAprobacion);
            result.fechaAprobacion2 = validarFecha(result.fechaAprobacion2);
            result.fechaDatosContables = validarFecha(result.fechaDatosContables);
            result.fechaImpresoContabilidad = validarFecha(result.fechaImpresoContabilidad);
            result.fechaImpresoTesoreria = validarFecha(result.fechaImpresoTesoreria);
            result.fechaPagado = validarFecha(result.fechaPagado);

            return result;
        }
        private DateTime? validarFecha(DateTime? fechaFactura)
        {
            if (fechaFactura.HasValue)
            {
                fechaFactura = fechaFactura.Value.Subtract(TimeSpan.FromHours(5));
            }
            return fechaFactura;
        }

        internal byte[] ExtraerPdfConClave(byte[] bytesArchivoPdf, bool reintento = false)
        {


            try
            {
                var urlUnlockPdf = this.configuration.GetSection("UnlockPdf")["Uri"];

                var client = new RestClient($"{urlUnlockPdf}");
                var request = new RestRequest($"", Method.Post);

                var archivoPdfBase64 = Convert.ToBase64String(bytesArchivoPdf);

                string jsonToSend = "{\"Archivo\": \"" + archivoPdfBase64 + "\"}";

                request.AddParameter("application/json; charset=utf-8", jsonToSend, ParameterType.RequestBody);
                request.AddHeader("Content-Type", "application/json");
                request.RequestFormat = DataFormat.Json;

                var response = client.Execute(request);

                var archivoPdfProcesado = JsonConvert.DeserializeObject<PdfModel>(response.Content);

                var bytesArchivoPdfProcesado = Convert.FromBase64String(archivoPdfProcesado.Archivo);

                return bytesArchivoPdfProcesado;
            }
            catch (Exception ex)
            {

                if (reintento == false)
                {
                    return ExtraerPdfConClave(bytesArchivoPdf, true);
                }
                else
                {
                    return bytesArchivoPdf;
                }
            }


        }


        public async Task<bool> AnticiposPendientes(Guid idProveedor, Guid idProyecto)
        {

            Factura factura = new Factura();

            var facturasPorProveedor = await this.GetByProterty(Builders<Factura>.Filter.Eq("idProveedor", idProveedor));

            var facturasPorProyecto = facturasPorProveedor.Where(f => f.idProyecto == idProyecto).ToList();


            var totalAnticipos = facturasPorProyecto.Where(x => x.esAnticipo.HasValue && x.esAnticipo == true).Sum(x => x.monto);

            var totalDescuentosAnticipos = from f in facturasPorProyecto.Where(x => x.datosContables != null)
                                           from dc in f.datosContables
                                           where dc.codigo == "Anticipo "
                                           select double.Parse(dc.valor);
            var sumAnticipos = totalDescuentosAnticipos.Sum();

            var totalAnticiposPendientes = totalAnticipos - sumAnticipos;

            return totalAnticipos > sumAnticipos;

        }


    }
}