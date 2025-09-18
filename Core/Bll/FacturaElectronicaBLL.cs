using Cobalto.Mongo.Core.BLL;
using Core.BLL;
using Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Core.Bll
{
    public class FacturaElectronicaBLL : BaseBLL<FacturaElectronica>
    {
        private readonly ProyectoBLL _proyectoBLL;

        public FacturaElectronicaBLL(IConfiguration configuration, IHttpContextAccessor httpContext, ProyectoBLL proyectoBLL)
                                 : base(configuration, httpContext)
        {
            this._proyectoBLL = proyectoBLL;
        }
        public async Task<FacturaElectronica> ObtenerDatosFacturaXML(ArchivoZip archivoZip)
        {

            return ExtraerArchivos(archivoZip);

        }
        public async Task<FacturaElectronica> ObtenerDatosFacturaXMLArchivosSeparados(ArchivoZipSeparado archivoZip)
        {

            return ExtraerArchivosPorLista(archivoZip);

        }

        internal FacturaElectronica ExtraerArchivos(ArchivoZip archivoZip)
        {
            FacturaElectronica modeloDatos = new FacturaElectronica();
            string base64 = string.Empty;
            string nombreArchivo = string.Empty;

            var zip = Convert.FromBase64String(archivoZip.Base64Archivo);
            using (var memoryStream = new MemoryStream(zip))
            {
                // archivoZip.CopyTo(memoryStream);

                using (var archive = new ZipArchive(memoryStream))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        using (var entryStream = entry.Open())
                        {
                            if (entry.FullName.ToLower().EndsWith("pdf"))
                            {
                                using (var reader = new BinaryReader(entryStream))
                                {
                                    var bytesArchivo = reader.ReadBytes((int)entry.Length);
                                    base64 = Convert.ToBase64String(bytesArchivo);
                                    nombreArchivo = entry.Name;
                                }
                            }
                            else if (entry.FullName.ToLower().EndsWith("xml"))
                            {
                                using (var reader = new BinaryReader(entryStream))
                                {
                                    var bytesArchivo = reader.ReadBytes((int)entry.Length);

                                    var xmlMs = new MemoryStream(bytesArchivo);

                                    XDocument xmlFile = XDocument.Load(xmlMs);
                                    XNamespace ns = xmlFile.Root.Name.Namespace;
                                    var nodes = xmlFile.DescendantNodes().Where(el => el.NodeType == XmlNodeType.CDATA);

                                    modeloDatos = ObtenerDatosFacturaArchivoXml(nodes);
                                }
                            }
                        }
                    }
                }
            }
            modeloDatos.pdf = base64;
            modeloDatos.pdfFileName = nombreArchivo;
            return modeloDatos;
        }

        internal FacturaElectronica ExtraerArchivosPorLista(ArchivoZipSeparado archivoZip)
        {
            FacturaElectronica modeloDatos = new FacturaElectronica();

            string base64 = archivoZip.Base64StringPdf;
            string nombreArchivo = archivoZip.NombreArchivoPdf;

            var bytesXml = Convert.FromBase64String(archivoZip.Base64StringXml);
            var xmlMs = new MemoryStream(bytesXml);

            //convertimos xmlMs a string
            string xmlString = Encoding.UTF8.GetString(bytesXml);

            //eliminamos los caracteres del inicio hasta el primer <, para evitar problemas de parsing
            xmlString = xmlString.Substring(xmlString.IndexOf("<"));


            //XDocument xmlFile = XDocument.Load(xmlMs);
            //creamos XDocument a partir de string
            XDocument xmlFile = XDocument.Parse(xmlString);

            XNamespace ns = xmlFile.Root.Name.Namespace;
            var nodes = xmlFile.DescendantNodes().Where(el => el.NodeType == XmlNodeType.CDATA);

            modeloDatos = ObtenerDatosFacturaArchivoXml(nodes);



            modeloDatos.pdf = base64;
            modeloDatos.pdfFileName = nombreArchivo;
            return modeloDatos;
        }

        internal FacturaElectronica ObtenerDatosFacturaArchivoXml(IEnumerable<XNode> nodes)
        {
            string[] listaEtiquetas = new string[] { "TaxInclusiveAmount", "ID", "Name", "IssueDate", "PaymentDueDate" };
            XDocument nodoUtil = new XDocument();

            foreach (var node in nodes)
            {
                try
                {
                    var content = node.Parent.Value.Trim();
                    var xdoc = XDocument.Parse(content);

                    var etiqueta1 = listaEtiquetas.First();
                    var etiquetaReceptor = "AccountingCustomerParty";
                    var etiquetaEmisor = "AccountingSupplierParty";

                    if (xdoc.Root.Descendants().Any(element => element.Name.LocalName.Equals(etiqueta1))
                        && xdoc.Root.Descendants().Any(element => element.Name.LocalName.Equals(etiquetaReceptor))
                        && xdoc.Root.Descendants().Any(element => element.Name.LocalName.Equals(etiquetaEmisor)))
                    {
                        nodoUtil = xdoc;
                    }
                }
                catch (Exception ex)
                {
                    _ = ex.Message;
                }
            }

            var fechaElaboracion = ObtenerValorModelo(nodoUtil, "IssueDate");
            var fechaVencimiento = ObtenerValorModelo(nodoUtil, "PaymentDueDate");
            var nitProveedor = ObtenerValorModelo(nodoUtil, "CompanyID", "AccountingSupplierParty");
            var nitDestino = ObtenerValorModelo(nodoUtil, "CompanyID", "AccountingCustomerParty");
            var total = ObtenerValorModelo(nodoUtil, "TaxInclusiveAmount");
            var numeroFactura = ObtenerValorModelo(nodoUtil, "ID");

            total = total.Replace(".", ",", StringComparison.InvariantCultureIgnoreCase);

            var facturaXML = new FacturaElectronica()
            {
                fechaElaboracion = DateTime.TryParse(fechaElaboracion, out DateTime fechaE) ? fechaE : DateTime.Now,
                fechaVencimiento = DateTime.TryParse(fechaVencimiento, out DateTime fechaV) ? fechaV : DateTime.Now,
                nitProveedor = nitProveedor,
                nitProyecto = nitDestino,
                total = double.TryParse(total, out double vtotal) ? vtotal : 0,
                numeroFactura = numeroFactura
            };

            return facturaXML;
        }

        internal string ObtenerValorModelo(XDocument nodoUtil, string value, string partXml = null)
        {
            string response = string.Empty;


            try
            {
                if (partXml == null)
                {
                    response = nodoUtil.Root
                                .Descendants()
                                .Where(element => element.Name.LocalName.Equals(value))
                                .FirstOrDefault()?.Value;
                    return response?.Trim();
                }
                else
                {
                    var algo = nodoUtil.Root
                                .Descendants()
                                .Where(element => element.Name.LocalName.Equals(partXml))
                                .FirstOrDefault();

                    response = algo.Descendants()
                                .Where(element => element.Name.LocalName.Equals(value))
                                .FirstOrDefault()?.Value;

                    return response?.Trim();
                }


            }
            catch (Exception ex)
            {
                return string.Empty;
                throw;
            }

        }

        public async Task<List<FacturaElectronica>> ObtenerFacturasSinModificar(FiltroFacturas filtro)
        {
            var filtroFechaInicial = Builders<FacturaElectronica>.Filter.Gte("fechaElaboracion", filtro.inicio);

            var filtroFechaFinal = Builders<FacturaElectronica>.Filter.Lte("fechaElaboracion", filtro.fin);

            var filtroProcesado = Builders<FacturaElectronica>.Filter.Eq("procesado", false);

            var filtroBorrado = Builders<FacturaElectronica>.Filter.Eq("erased", false);

            var filtroAplicado = await this.GetByProterty(filtroFechaInicial & filtroFechaFinal & filtroProcesado & filtroBorrado);

            if (filtro.proyectos != null)
            {

                List<FacturaElectronica> facturasConProyectos = new List<FacturaElectronica>();

                foreach (var item in filtro.proyectos)
                {
                    var proyecto = await this._proyectoBLL.GetById(item);
                    filtroAplicado = filtroAplicado.Where(f => !string.IsNullOrEmpty(f.nitProyecto)).ToList();

                    var encontradas = filtroAplicado.Where(f => proyecto.nit.Contains(f.nitProyecto)).ToList();

                    facturasConProyectos.AddRange(encontradas);
                }
                filtroAplicado = facturasConProyectos;
            }

            return filtroAplicado;
        }

        public async Task<bool> CambiarEstadoFacturaElectronica(Guid idFactura)
        {
            var factura = await this.GetById(idFactura);
            factura.procesado = true;
            await this.Update(factura);
            return true;

        }

        public async Task<bool> BorrarFacturasElectronicas(Guid idFactura)
        {
            var factura = await this.GetById(idFactura);
            factura.erased = true;
            await this.Update(factura);
            return factura.erased;
        }

        public async Task<FacturaElectronica> ExisteFacturaElectronicaAsync(FacturaElectronica factura)
        {
            var filtroProyecto = Builders<FacturaElectronica>.Filter.Eq("numeroFactura", factura.numeroFactura);
            var filtroBorradas = Builders<FacturaElectronica>.Filter.Eq("nitProveedor", factura.nitProveedor);
            var facturaConsultar = (await this.GetByProterty(filtroProyecto & filtroBorradas)).FirstOrDefault();
            return facturaConsultar;


        }
    }
}
