using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AspNetCore.Identity.MongoDB;
using AspNetCore.Identity.MongoDB.Secure;
using AspNetCore.Identity.MongoDB.Secure.Auth;
using Core.Bll;
using Core.BLL;
using Core.Models;
using Core.Utils;
using DigitalOceanUploader.Shared;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Creimed.Api.Controllers
{



    [Produces("application/json")]
    [Route("api/[controller]")]
    public class UploadController : Controller
    {
        DigitalOceanUploadManager uploadManager;
        private IWebHostEnvironment _hostingEnvironment;
        private ProveedorBLL bll;
        private OrdenTrabajoBLL ordenTrabajoBll;
        private readonly AdicionesFacade _adicionesFacade;
        private readonly OrdenTrabajoBLL ordenTrabajoBLL;
        private readonly UserManager<MongoIdentityUser> _userManager;
        private readonly SignInManager<MongoIdentityUser> _signInManager;


        private readonly IJwtFactory _jwtFactory;
        private readonly JwtIssuerOptions _jwtOptions;

        public PedidoServicioBLL _pedidoServicioBLL { get; set; }

        private OrdenCompraBLL _ordenCompraBLL;

        public FacturaBLL _facturaBLL { get; set; }
        public EmailBLL _emailBLL { get; private set; }
        public RemisionBLL _remisionBLL { get; private set; }

        private readonly ILogger _logger;

        public FacturaElectronicaBLL _facturaElectronicaBLL { get; set; }
        public UploadController(IWebHostEnvironment hostingEnvironment,
            DigitalOceanUploadManager uploadManager,
            ProveedorBLL bll,
            AdicionesFacade adicionesFacade,
            PedidoServicioBLL pedidoServicioBLL,
            OrdenCompraBLL ordenCompraBLL,
            FacturaBLL facturaBLL,
            FacturaElectronicaBLL facturaXMLBLL,
            OrdenTrabajoBLL ordenTrabajoBLL,
              UserManager<MongoIdentityUser> userManager,
            SignInManager<MongoIdentityUser> signInManager,
             IJwtFactory jwtFactory,
              EmailBLL emailBLL,
              RemisionBLL remisionBLL,
             IOptions<JwtIssuerOptions> jwtOptions,
             ILogger logger
            )
        {
            _hostingEnvironment = hostingEnvironment;
            this.bll = bll;
            _adicionesFacade = adicionesFacade;
            this.uploadManager = uploadManager;

            _userManager = userManager;
            _signInManager = signInManager;
            _jwtFactory = jwtFactory;
            _jwtOptions = jwtOptions.Value;
            _pedidoServicioBLL = pedidoServicioBLL;
            _ordenCompraBLL = ordenCompraBLL;
            _facturaBLL = facturaBLL;
            _facturaElectronicaBLL = facturaXMLBLL;
            this.ordenTrabajoBLL = ordenTrabajoBLL;
            _emailBLL = emailBLL;
            _remisionBLL = remisionBLL;

            _logger = logger;
        }
        [HttpPost, DisableRequestSizeLimit]
        public async Task<ActionResult> UploadFile()
        {
            try
            {


                var file = Request.Form.Files[0];
                string webRootPath = _hostingEnvironment.WebRootPath;

                string proveedorId = Request.Form["proveedorId"];
                string tipo = Request.Form["tipo"];
                string fechaExpedicion = Request.Form["fechaExpedicion"];


                if (file.Length > 0)
                {
                    string fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');


                    using (var stream = new MemoryStream())
                    {
                        file.CopyTo(stream);
                        stream.Position = 0; // Reset stream position to beginning
                        var uploadId = await this.uploadManager.UploadFile(stream, fileName);

                        var adjunto = new Adjunto();

                        adjunto.Id = uploadId;
                        adjunto.tipo = tipo;
                        adjunto.fechaExpedicion = fechaExpedicion;
                        adjunto.NombreDelArchivo = file.FileName;
                        await this.bll.UploadFile(new Guid(proveedorId), adjunto);



                        //adjunto.Id = uploadId;
                    }
                }
                return Json("Upload Successful.");
            }
            catch (System.Exception ex)
            {

                this._logger.LogError(ex, "Error Subiendo factura");
                return Json("Upload Failed: " + ex.Message);
            }
        }



        [HttpPost, DisableRequestSizeLimit]
        [Route("signature")]
        public async Task<ActionResult> UploadSignature()
        {
            try
            {


                var file = Request.Form.Files[0];
                string webRootPath = _hostingEnvironment.WebRootPath;

                string userId = Request.Form["userId"];


                if (file.Length > 0)
                {
                    string fileName = userId;

                    using (var stream = new MemoryStream())
                    {
                        file.CopyTo(stream);
                        var uploadId = await this.uploadManager.UploadFile(stream, fileName);

                        var user = _userManager.FindByIdAsync(userId).Result;
                        user.imageId = uploadId;
                        var result = await _userManager.UpdateAsync(user);



                    }
                }
                return Json("Upload Successful.");
            }
            catch (System.Exception ex)
            {
                return Json("Upload Failed: " + ex.Message);
            }
        }

        //        objectResponse.Metadata["filename"]
        //"Copia de CONTENEDORES.xlsx"


        [HttpPost, DisableRequestSizeLimit]
        [Route("cotizacion")]
        public async Task<ActionResult> UploadCotizacion()
        {
            try
            {

                Guid id = new Guid(Request.Form["id"]);

                var pedidoservicio = await _pedidoServicioBLL.GetById(id);
                pedidoservicio.detalle.archivosCotizacion = new List<DocumentoCotizacion>();

                for (int i = 0; i < Request.Form.Files.Count; i++)
                {

                    var file = Request.Form.Files[i];
                    string webRootPath = _hostingEnvironment.WebRootPath;



                    if (file.Length > 0)
                    {
                        // string fileName = id.ToString();

                        string fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');

                        using (var stream = new MemoryStream())
                        {
                            file.CopyTo(stream);
                            var uploadId = await this.uploadManager.UploadFile(stream, fileName);


                            if (i == 0 )
                            {
                                pedidoservicio.detalle.archivoCotizacion = uploadId.ToString(); 
                            }

                            pedidoservicio.detalle.archivosCotizacion.Add(new DocumentoCotizacion()
                            {
                                id = uploadId,
                                nombreDelArchivo = fileName
                            });


                            await this._pedidoServicioBLL.Update(pedidoservicio);



                        }
                    }
                }

              
                return Json("Upload Successful.");
            }
            catch (System.Exception ex)
            {
                return Json("Upload Failed: " + ex.Message);
            }
        }



        //[HttpPost, DisableRequestSizeLimit]
        //[Route("cotizacion")]
        //public async Task<ActionResult> UploadCotizacion()
        //{
        //    try
        //    {

        //        Guid id = new Guid(Request.Form["id"]);

        //        var pedidoservicio = await _pedidoServicioBLL.GetById(id);
        //        pedidoservicio.detalle.archivosCotizacion = new List<DocumentoCotizacion>();

        //        for (int i = 0; i < Request.Form.Files.Count; i++)
        //        {

        //            var file = Request.Form.Files[i];
        //            string webRootPath = _hostingEnvironment.WebRootPath;



        //            if (file.Length > 0)
        //            {
        //                // string fileName = id.ToString();

        //                string fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');

        //                using (var stream = new MemoryStream())
        //                {
        //                    file.CopyTo(stream);
        //                    var uploadId = await this.uploadManager.UploadFile(stream, fileName);


        //                    if (i == 0)
        //                    {
        //                        pedidoservicio.detalle.archivoCotizacion = uploadId.ToString();
        //                    }

        //                    pedidoservicio.detalle.archivosCotizacion.Add(new DocumentoCotizacion()
        //                    {
        //                        id = uploadId,
        //                        nombreDelArchivo = fileName
        //                    });


        //                    await this._pedidoServicioBLL.Update(pedidoservicio);



        //                }
        //            }
        //        }


        //        return Json("Upload Successful.");
        //    }
        //    catch (System.Exception ex)
        //    {
        //        return Json("Upload Failed: " + ex.Message);
        //    }
        //}




        [HttpPost, DisableRequestSizeLimit]
        [Route("cotizacionreferencia")]
        public async Task<UploadId> UploadCotizacionreferencia()
        {

            var file = Request.Form.Files[0];
            string webRootPath = _hostingEnvironment.WebRootPath;


            var id = string.Empty;
            if (file.Length > 0)
            {
                string fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');

                using (var stream = new MemoryStream())
                {
                    file.CopyTo(stream);
                    var uploadId = await this.uploadManager.UploadFile(stream, fileName);
                    id = uploadId.ToString();
                }
            }
            return new UploadId() { id = id };

        }


        public class UploadId
        {
            public string id { get; set; }
        }

        [HttpPost, DisableRequestSizeLimit]
        [Route("contrato")]
        public async Task<ActionResult> UploadContrato()
        {
            try
            {


                var file = Request.Form.Files[0];
                string webRootPath = _hostingEnvironment.WebRootPath;

                Guid id = new Guid(Request.Form["id"]);


                if (file.Length > 0)
                {
                    // string fileName = id.ToString();

                    string fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');

                    using (var stream = new MemoryStream())
                    {
                        file.CopyTo(stream);
                        var uploadId = await this.uploadManager.UploadFile(stream, fileName);

                        var ordenCompra = await _ordenCompraBLL.GetById(id);

                        ordenCompra.servicio.archivoContrato = uploadId.ToString();
                        var detalle = ordenCompra.servicio;


                        await this._ordenCompraBLL.Update(ordenCompra);
                        await this._emailBLL.EnviarOrdenServicioProveedor(detalle, ordenCompra);



                    }
                }
                return Json("Upload Successful.");
            }
            catch (System.Exception ex)
            {
                return Json("Upload Failed: " + ex.Message);
            }
        }



        [HttpPost, DisableRequestSizeLimit]
        [Route("otrosArchivosContrato")]
        public async Task<DocumentoAdicionalContrato> UploadOtrosContrato()
        {

            DocumentoAdicionalContrato result = null;
            try
            {


                var file = Request.Form.Files[0];
                string webRootPath = _hostingEnvironment.WebRootPath;

                Guid id = new Guid(Request.Form["id"]);


                if (file.Length > 0)
                {
                    // string fileName = id.ToString();

                    string fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');

                    using (var stream = new MemoryStream())
                    {
                        file.CopyTo(stream);
                        var uploadId = await this.uploadManager.UploadFile(stream, fileName);

                        var ordenCompra = await _ordenCompraBLL.GetById(id);


                        if (ordenCompra.servicio.otrosArchivos == null)
                        {
                            ordenCompra.servicio.otrosArchivos = new List<DocumentoAdicionalContrato>();
                        }

                        result = new DocumentoAdicionalContrato() { id = uploadId, nombreDelArchivo = fileName };

                        ordenCompra.servicio.otrosArchivos.Add(result);
                        var detalle = ordenCompra.servicio;


                        await this._ordenCompraBLL.Update(ordenCompra);
                    }
                }

                return result;
            }
            catch (System.Exception ex)
            {
                return null;
            }


        }


        [HttpPost, DisableRequestSizeLimit]
        [Route("otrosArchivosFacturaElectronica")]
        public async Task<FacturaElectronica> UploadOtrosFacturaElectronica([FromBody] ArchivoZip file)
        {

            #region formFile
            try
            {


                //string webRootPath = _hostingEnvironment.WebRootPath;

                FacturaElectronica facturaXML = new FacturaElectronica();

                //if (file.Length > 0)
                //{
                var file2 = file;
                facturaXML = await this._facturaElectronicaBLL.ObtenerDatosFacturaXML(file);
                var archivoPDF = Convert.FromBase64String(facturaXML.pdf);

                using (var stream = new MemoryStream(archivoPDF))
                {
                    var uploadId = await this.uploadManager.UploadFile(stream, facturaXML.pdfFileName);

                    facturaXML.idPdfUploaded = uploadId;
                    facturaXML.procesado = false;
                    facturaXML.pdf = string.Empty;

                    await this._facturaElectronicaBLL.Insert(facturaXML);
                }
                //}

                return facturaXML;
            }
            catch (Exception ex)
            {
                throw;
            }
            #endregion
        }


        [HttpPost, DisableRequestSizeLimit]
        [Route("otrosArchivosFactura")]
        public async Task<ArchivoAdicional> UploadOtrosFactura()
        {

            ArchivoAdicional result = null;
            try
            {
                var file = Request.Form.Files[0];
                string webRootPath = _hostingEnvironment.WebRootPath;

                Guid id = new Guid(Request.Form["id"]);


                if (file.Length > 0)
                {
                    // string fileName = id.ToString();

                    string fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');

                    using (var stream = new MemoryStream())
                    {
                        file.CopyTo(stream);

                        stream.Position = 0; // Reset stream position to beginning

                        var uploadId = await this.uploadManager.UploadFile(stream, fileName);

                        var factura = await this._facturaBLL.GetById(id);


                        if (factura.otrosArchivos == null)
                        {
                            factura.otrosArchivos = new List<ArchivoAdicional>();
                        }

                        result = new ArchivoAdicional() { id = uploadId, nombreDelArchivo = fileName };

                        factura.otrosArchivos.Add(result);
                        //var detalle = ordenCompra.servicio;


                        await this._facturaBLL.Update(factura);
                    }
                }

                return result;
            }
            catch (System.Exception ex)
            {
                return null;
            }


        }



        [HttpPost, DisableRequestSizeLimit]
        [Route("otrosArchivosFacturaPorLista")]
        public async Task<FacturaElectronica> OtrosArchivosFacturaPorLista([FromBody] ArchivoZipSeparado files)
        {

            #region formFile
            try
            {
                FacturaElectronica facturaXML = new FacturaElectronica();

                facturaXML = await this._facturaElectronicaBLL.ObtenerDatosFacturaXMLArchivosSeparados(files);
                var archivoPDF = Convert.FromBase64String(facturaXML.pdf);
                var FiltroNumeroFactura = await this._facturaElectronicaBLL.ExisteFacturaElectronicaAsync(facturaXML);

                if (FiltroNumeroFactura == null)
                {
                    using (var stream = new MemoryStream(archivoPDF))
                    {
                        var uploadId = await this.uploadManager.UploadFile(stream, facturaXML.pdfFileName);

                        facturaXML.idPdfUploaded = uploadId;
                        facturaXML.procesado = false;
                        facturaXML.pdf = string.Empty;
                        facturaXML.fechaAgregado = DateTime.Now;
                        await this._facturaElectronicaBLL.Insert(facturaXML);
                    }
                }
                else
                {
                    facturaXML.idPdfUploaded = FiltroNumeroFactura.idPdfUploaded;
                    facturaXML.procesado = FiltroNumeroFactura.procesado;
                    facturaXML.pdf = FiltroNumeroFactura.pdf;
                    facturaXML.id = FiltroNumeroFactura.id;
                    facturaXML.erased = FiltroNumeroFactura.erased;
                }

                return facturaXML;
            }
            catch (Exception ex)
            {
                return null;
            }
            #endregion
        }









        [HttpPost, DisableRequestSizeLimit]
        [Route("contratoFirmado")]
        public async Task<ActionResult> UploadContratoFirmado()
        {
            try
            {

                var file = Request.Form.Files[0];
                string webRootPath = _hostingEnvironment.WebRootPath;

                Guid id = new Guid(Request.Form["id"]);


                if (file.Length > 0)
                {
                    // string fileName = id.ToString();

                    string fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');

                    using (var stream = new MemoryStream())
                    {
                        file.CopyTo(stream);

                        var uploadId = await this.uploadManager.UploadFile(stream, fileName);

                        var ordenCompra = await _ordenCompraBLL.GetById(id);

                        ordenCompra.servicio.archivoContratoFirmado = uploadId.ToString();
                        ordenCompra.contratoFirmado = true;

                        await this._ordenCompraBLL.Update(ordenCompra);
                        await _emailBLL.EnviarOrdenServicioProveedorContratoFirmado(ordenCompra);

                    }
                }
                return Json("Upload Successful.");
            }
            catch (System.Exception ex)
            {
                return Json("Upload Failed: " + ex.Message);
            }
        }


        [HttpPost, DisableRequestSizeLimit]
        [Route("factura")]
        public async Task<ActionResult> UploadFactura()
        {
            try
            {


                var file = Request.Form.Files[0];
                string webRootPath = _hostingEnvironment.WebRootPath;

                //Guid id = new Guid(Request.Form["id"]);
                Guid facturaId = new Guid(Request.Form["facturaId"]);

                if (file.Length > 0)
                {
                    // string fileName = id.ToString();

                    string fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');

                    using (var stream = new MemoryStream())
                    {
                        file.CopyTo(stream);
                        stream.Position = 0; // Reset stream position to beginning

                        //var ordenCompra = await _ordenCompraBLL.GetById(id);
                        var factura = await _facturaBLL.GetById(facturaId);

                        var ext = Path.GetExtension(fileName);

                        fileName = "Factura" + factura.numeroFactura.Replace(" ", string.Empty) + ext;

                        var uploadId = await this.uploadManager.UploadFile(stream, fileName);


                        if (factura != null)
                        {

                            bool archivoValido = await _facturaBLL.ValidarEscrituraPDF(uploadId);
                            if (!archivoValido)
                            {
                                return Json("Upload Failed: Archivo protegido");
                            }


                            factura.archivo = uploadId;
                            factura.archivoOriginal = uploadId;

                            await _facturaBLL.Update(factura);

                            if (factura.isOT == true)
                            {
                            }
                            else
                            {

                                await this._emailBLL.EmailFactura(factura);

                            }
                        }
                        else
                        {
                            return Json("Upload Failed: Factura no encontrada");
                        }



                    }
                }
                return Json("Upload Successful.");
            }
            catch (System.Exception ex)
            {

                this._logger.LogError(ex, "Error Subiendo factura");
                return Json("Upload Failed: " + ex.Message);
            }
        }


        [HttpPost, DisableRequestSizeLimit]
        [Route("remision")]
        public async Task<ActionResult> UploadRemision()
        {
            try
            {


                var file = Request.Form.Files[0];
                string webRootPath = _hostingEnvironment.WebRootPath;

                Guid remisionId = new Guid(Request.Form["remisionId"]);

                if (file.Length > 0)
                {

                    string fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');

                    using (var stream = new MemoryStream())
                    {
                        var remision = await _remisionBLL.GetById(remisionId);

                        if (remision != null)
                        {

                            file.CopyTo(stream);
                            var uploadId = await this.uploadManager.UploadFile(stream, fileName);

                            remision.archivo = uploadId;

                            await this._remisionBLL.Update(remision);

                            await this._emailBLL.EmailRemision(remision);

                        }
                        else
                        {
                            return Json("Upload Failed: Remision no encontrada");
                        }

                        // ordenCompra.facturas.Add(uploadId);


                    }
                }
                return Json("Upload Successful.");
            }
            catch (System.Exception ex)
            {
                return Json("Upload Failed: " + ex.Message);
            }
        }


        [HttpPost, DisableRequestSizeLimit]
        [Route("poliza")]
        public async Task<string> UploadPoliza()
        {
            try
            {
                var file = Request.Form.Files[0];
                Guid id = new Guid(Request.Form["id"]);
                Guid polizaId = new Guid(Request.Form["polizaId"]);

                if (file.Length > 0)
                {
                    //string fileName = polizaId.ToString();

                    string fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');

                    using (var stream = new MemoryStream())
                    {
                        file.CopyTo(stream);
                        stream.Position = 0; // Reset stream position to beginning

                        var uploadId = await this.uploadManager.UploadFile(stream, fileName);

                        var ordencompra = await _ordenCompraBLL.GetById(id);

                        var poliza = ordencompra.servicio.poliza.Where(x => x.id == polizaId).FirstOrDefault();

                        if (poliza != null)
                        {
                            poliza.archivo = uploadId.ToString();
                        }
                        else
                        {

                            return "Upload Failed: Poliza no encontrada";
                        }

                        await this._ordenCompraBLL.Update(ordencompra);
                        return uploadId.ToString();
                    }
                }
                return "Upload Successful.";
                // return uploadId.ToString();
            }
            catch (System.Exception ex)
            {
                return "Upload Failed: " + ex.Message;
            }
        }


        [HttpPost, DisableRequestSizeLimit]
        [Route("modificacion/poliza")]
        public async Task<string> PolizaModificacion()
        {
            try
            {
                var file = Request.Form.Files[0];
                Guid id = new Guid(Request.Form["id"]);
                Guid polizaId = new Guid(Request.Form["polizaId"]);
                string tipoModificacion = Request.Form["tipo"];

                if (file.Length > 0)
                {
                    //string fileName = polizaId.ToString();

                    string fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');

                    using (var stream = new MemoryStream())
                    {
                        file.CopyTo(stream);
                        var uploadId = await this.uploadManager.UploadFile(stream, fileName);
                        return await this._adicionesFacade.UpdatePolizas(id, polizaId, tipoModificacion, uploadId.ToString());
                    }
                }
                return "Upload Successful.";
            }
            catch (System.Exception ex)
            {
                return "Upload Failed: " + ex.Message;
            }
        }




        [HttpPost, DisableRequestSizeLimit]
        [Route("modificacion/contrato")]
        public async Task<string> ContratoModificacion()
        {
            try
            {
                var file = Request.Form.Files[0];
                Guid id = new Guid(Request.Form["id"]);
                string tipoModificacion = Request.Form["tipo"];

                if (file.Length > 0)
                {
                    //string fileName = polizaId.ToString();

                    string fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');

                    using (var stream = new MemoryStream())
                    {
                        file.CopyTo(stream);
                        var uploadId = await this.uploadManager.UploadFile(stream, fileName);
                        return await this._adicionesFacade.UpdateContrato(id, tipoModificacion, uploadId.ToString());
                    }
                }
                return "Upload Successful.";
            }
            catch (System.Exception ex)
            {
                return "Upload Failed: " + ex.Message;
            }
        }


        [HttpPost, DisableRequestSizeLimit]
        [Route("modificacion/contratofirmado")]
        public async Task<string> ContratoFirmadoModificacion()
        {
            try
            {
                var file = Request.Form.Files[0];
                Guid id = new Guid(Request.Form["id"]);
                string tipoModificacion = Request.Form["tipo"];

                if (file.Length > 0)
                {
                    //string fileName = polizaId.ToString();

                    string fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');

                    using (var stream = new MemoryStream())
                    {
                        file.CopyTo(stream);
                        var uploadId = await this.uploadManager.UploadFile(stream, fileName);
                        return await this._adicionesFacade.UpdateContratoFirmado(id, tipoModificacion, uploadId.ToString());
                    }
                }
                return "Upload Successful.";
            }
            catch (System.Exception ex)
            {
                return "Upload Failed: " + ex.Message;
            }
        }



        [HttpGet("{id}")]
        public async Task<IActionResult> DownloadFile([FromRoute] string id)
        {

            try
            {
                var bytes = await this.uploadManager.DownloadFile(id);
                var result = new FileContentResult(bytes.file, "application/octet");
                result.FileDownloadName = bytes.filename;
                HttpContext.Response.Headers.Add("filename", result.FileDownloadName);
                HttpContext.Response.Headers.Add("access-control-expose-headers", "filename");

                return result;
            }
            catch (Exception ex)
            {

                throw;
            }

        }


        [HttpGet("DescargarOC/{id}")]
        public async Task<IActionResult> DescargarOC([FromRoute] Guid id)
        {

            try
            {
                byte[] bytes = await this._emailBLL.PDFOrden(id);
                if (bytes != null)
                {
                    var result = new FileContentResult(bytes, "application/octet");
                    result.FileDownloadName = "OC" + id + ".pdf";
                    HttpContext.Response.Headers.Add("filename", result.FileDownloadName);
                    HttpContext.Response.Headers.Add("access-control-expose-headers", "filename");

                    return result; 
                }

                return null;
            }
            catch (Exception ex)
            {

                throw;
            }

        }




        [HttpGet("signature/{id}")]

        public async Task<IActionResult> DownloaSignaturedFile([FromRoute] string id)
        {

            var user = _userManager.FindByIdAsync(id).Result;

            var file = await this.uploadManager.DownloadFile(user.imageId.ToString());
            var result = new FileContentResult(file.file, "application/octet");
            return result;

        }


        [HttpGet("signatureBase64/{id}")]

        public async Task<string> DownloaSignaturedFileBase64([FromRoute] string id)
        {

            var user = _userManager.FindByIdAsync(id).Result;

            var file = await this.uploadManager.DownloadFile(user.imageId.ToString());


            return Convert.ToBase64String(file.file);

        }


        [HttpDelete("{id}/{id2}")]
        public async Task<bool> DeleteFile([FromRoute] string id, [FromRoute] Guid id2)
        {
            var item = await this.bll.GetById(id2);
            item.adjuntos.RemoveAll(x => x.Id.ToString() == id);
            await this.bll.Update(item);
            var delete = await this.uploadManager.DeleteFile(id);
            var result = delete;
            return result;

        }


        [HttpPost, DisableRequestSizeLimit]
        [Route("ot")]
        public async Task<ActionResult> UploadOrdenTrabajo()
        {
            try
            {


                var file = Request.Form.Files[0];
                string webRootPath = _hostingEnvironment.WebRootPath;

                Guid id = new Guid(Request.Form["id"]);


                if (file.Length > 0)
                {
                    // string fileName = id.ToString();

                    string fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');

                    using (var stream = new MemoryStream())
                    {
                        file.CopyTo(stream);
                        var uploadId = await this.uploadManager.UploadFile(stream, fileName);

                        var ordenTrabajo = await this.ordenTrabajoBLL.GetById(id);

                        ordenTrabajo.idArchivo = uploadId.ToString();

                        await this.ordenTrabajoBLL.Update(ordenTrabajo);



                    }
                }
                return Json("Upload Successful.");
            }
            catch (System.Exception ex)
            {
                return Json("Upload Failed: " + ex.Message);
            }
        }


    }
}
