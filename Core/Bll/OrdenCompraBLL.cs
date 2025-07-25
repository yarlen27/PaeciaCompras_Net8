using AspNetCore.Identity.MongoDB;
using Cobalto.Mongo.Core.BLL;
using Core.Bll;
using Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// using iTextSharp.text.pdf.codec; // TODO: Migrate to iText7
using iText.Kernel.Pdf;
using iText.Layout;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Bson;

namespace Core.BLL
{
    public class OrdenCompraBLL : BaseBLL<OrdenCompra>
    {
        private ProyectoBLL _proyectoBll;
        private ProveedorBLL _proveedorBLL;
        private readonly AdicionPedidoServicioBLL _adicionPedidoServicioBLL;
        private readonly ReanudacionBLL _reanudacionBLL;
        private readonly SuspensionBLL _suspensionBLL;
        private CategoriaBLL _categoriasBLL;
        public OrdenCompraBLL(ProyectoBLL proyectoBll, CategoriaBLL categoriasBLL, AdicionPedidoServicioBLL adicionPedidoServicioBLL, ReanudacionBLL reanudacionBLL, SuspensionBLL suspensionBLL, ProveedorBLL proveedorBLL, IConfiguration configuration, IHttpContextAccessor httpContext, UserManager<MongoIdentityUser> userManager) : base(configuration, httpContext)
        {
            _proyectoBll = proyectoBll;
            _proveedorBLL = proveedorBLL;
            _adicionPedidoServicioBLL = adicionPedidoServicioBLL;
            _reanudacionBLL = reanudacionBLL;
            _suspensionBLL = suspensionBLL;
            _userManager = userManager;
            _categoriasBLL = categoriasBLL;
        }

        public async Task<List<Guid>> ProveedoresXProyecto(Guid item)
        {

            var filter = new BsonDocument("proyecto", item);
            var proveedores = await this.collection.Distinct<Guid>("proveedor", filter).ToListAsync();

            return proveedores;
        }

        private UserManager<MongoIdentityUser> _userManager;

        public async Task<OrdenCompra> Insert(OrdenCompra item, bool keepNumber = false)
        {
            if (keepNumber == false)
            {
                var proyecto = await this._proyectoBll.GetById(item.proyecto);

                if (proyecto != null)
                {
                    if (item.pedidoMaterial)
                    {
                        item.consecutivo = proyecto.consecutivo + 1;
                        proyecto.consecutivo = proyecto.consecutivo + 1;
                    }
                    else
                    {
                        item.consecutivo = proyecto.consecutivoServicio + 1;
                        proyecto.consecutivoServicio = proyecto.consecutivoServicio + 1;
                    }

                    await this._proyectoBll.Update(proyecto);
                }

                return await base.Insert(item);
            }
            else
            {
                item.consecutivo = 0;
                return await base.Insert(item);

            }
        }

        public async Task<List<OrdenCompra>> OrdenesValidas(Guid idProyecto)
        {
            var list = await this.GetByProterty("proyecto", idProyecto);

            var listContratoFirmado = list.Where(x => x.contratoFirmado == true && x.pedidoMaterial == false).ToList();


            var result = new List<OrdenCompra>();
            foreach (var orden in listContratoFirmado)
            {

                bool tieneAdicionesSinCompletar = await this._adicionPedidoServicioBLL.SinCompletar(orden.id);
                bool tieneSuspensionesSinCompletar = await this._suspensionBLL.SinCompletar(orden.id);
                bool tieneReanudacionesSinCompletar = await this._reanudacionBLL.SinCompletar(orden.id);

                if (!tieneAdicionesSinCompletar && !tieneSuspensionesSinCompletar & !tieneReanudacionesSinCompletar)
                {
                    result.Add(orden);
                }
            }

            return result;
        }

        public async Task<double> TotalProyecto(Guid id)
        {
            var ordenesPorProyecto = await this.GetByProterty("proyecto", id);
            double result = 0.0;

            if (ordenesPorProyecto != null && ordenesPorProyecto.Count() > 0)
            {

                double sumaDetalleMateriales = this.SumarMateriales(ordenesPorProyecto.Where(x => x.pedidoMaterial == true));
                double sumaDetalleServicios = this.SumarServicios(ordenesPorProyecto.Where(x => x.pedidoMaterial == false && x.servicio != null));

                result += sumaDetalleMateriales;
                result += sumaDetalleServicios;


            }

            return result;

        }

        public async Task<List<EjecutadoProyecto>> TotalProyectoProveedores(Guid id)
        {

            List<EjecutadoProyecto> result = new List<EjecutadoProyecto>();

            var ordenesPorProyecto = await this.GetByProterty("proyecto", id);

            if (ordenesPorProyecto != null && ordenesPorProyecto.Count() > 0)
            {


                var grouped = ordenesPorProyecto.GroupBy(x => x.proveedor);

                foreach (var item in grouped)
                {
                    var ejecutadoProveedor = new EjecutadoProyecto();
                    double sumaDetalleMateriales = this.SumarMateriales(item.Where(x => x.pedidoMaterial == true));
                    double sumaDetalleServicios = this.SumarServicios(item.Where(x => x.pedidoMaterial == false && x.servicio != null));

                    ejecutadoProveedor.idProveedor = item.Key.ToString();
                    ejecutadoProveedor.totalEjecutado = (sumaDetalleMateriales + sumaDetalleServicios);

                    result.Add(ejecutadoProveedor);
                }

            }

            return result;

        }

        public async Task<string> ReporteCsv(List<OrdenCompra> ordenes)
        {
            var sb = new StringBuilder("consecutivo|proyecto|proveedor|fechaGenerado|analistaCompras|urgente|direccionFacturacion|referencia|descripcion|unidad|cantidad|observaciones|rechazado|valorUnitario|");
            sb.AppendLine();
            foreach (var item in ordenes)
            {

                var proyecto = await nombreProyecto(item);
                var proveedor = await nombreProveedor(item);
                var analista = await nombreAnalista(item.analistaCompras);

                var lineasMaterial = LineaReporte(item.detalle);

                foreach (var material in lineasMaterial)
                {
                    var linea = $"{item.consecutivo}|{proyecto}|{proveedor}|{item.fechaGenerado.ToString("dd-MMM-yyyy")}|{analista}" +
                        $"|{item.urgente}|{item.direccionFacturacion}|{material}";

                    sb.AppendLine(linea);
                }
            }

            return sb.ToString();

        }

        internal async Task<List<OrdenCompra>> PorIdsPedidos(List<Guid> guids)
        {

            List<OrdenCompra> result = new List<OrdenCompra>();
            foreach (var item in guids)
            {

                var filter = Builders<OrdenCompra>.Filter.Eq("idPedido", item);

                var ocs = await this.GetByProterty(filter);

                result.AddRange(ocs);
            }
            return result;
        }

        //

        public async Task<List<ReporteOrdenesCompraJson>> ReporteJson(List<OrdenCompra> ordenes)
        {
            List<ReporteOrdenesCompraJson> listaReporteOrdenesCompraJson = new List<ReporteOrdenesCompraJson>();

            foreach (var item in ordenes)
            {

                var proyecto = await nombreProyecto(item);
                var proveedor = await nombreProveedor(item);
                var analista = await nombreAnalista(item.analistaCompras);

                var lineasMaterial = LineaReporteObjeto(item.detalle);

                foreach (var material in lineasMaterial)
                {

                    var nuevaOrdenCompra = new ReporteOrdenesCompraJson
                    {
                        consecutivo = item.consecutivo,
                        proyecto = proyecto,
                        proveedor = proveedor,
                        fechaGenerado = item.fechaGenerado,
                        analistaCompras = analista,
                        urgente = item.urgente,
                        direccionFacturacion = item.direccionFacturacion,
                        referencia = material.referencia,
                        descripcion = material.descripcion,
                        unidad = material.unidad,
                        cantidad = material.cantidad,
                        observaciones = material.observaciones,
                        rechazado = material.rechazado,
                        valorUnitario = material.valorUnitario
                    };

                    listaReporteOrdenesCompraJson.Add(nuevaOrdenCompra);

                }
            }

            return listaReporteOrdenesCompraJson;

        }
        //


        public async Task<string> ReporteCsvServicio(List<OrdenCompra> ordenes)
        {
            var sb = new StringBuilder("consecutivo|proyecto|proveedor|fechaGenerado|analistaCompras|urgente|direccionFacturacion|objeto|alcance|plazo|montoTotal|anticipo|montoAnticipo|porcentajeAnticipo|fechaAnticipo|fechaInicio|utilidad|unidad|tipoContrato|formaPago|observaciones|actividades|unidad|cantidad|valorUnidad|valorTotal|");
            sb.AppendLine();
            foreach (var item in ordenes)
            {

                if (item.servicio == null)
                {
                    continue;
                }

                var proyecto = await nombreProyecto(item);
                var proveedor = await nombreProveedor(item);
                var analista = await nombreAnalista(item.analistaCompras);

                var lineasServicio = LineaReporte(item.servicio);

                foreach (var servicio in lineasServicio)
                {
                    var linea = $"{item.consecutivo}|{proyecto}|{proveedor}|{item.fechaGenerado.ToString("dd-MMM-yyyy")}|{analista}|" +
                        $"{item.urgente}|{item.direccionFacturacion}|{servicio}";

                    sb.AppendLine(linea);
                }
            }

            return sb.ToString();

        }

        public async Task<List<ReporteServicioJson>> ReporteJsonServicio(List<OrdenCompra> ordenes)
        {
            List<ReporteServicioJson> listaReporteServicioJson = new List<ReporteServicioJson>();


            foreach (var item in ordenes)
            {

                if (item.servicio == null)
                {
                    continue;
                }

                var proyecto = await nombreProyecto(item);
                var proveedor = await nombreProveedor(item);
                var analista = await nombreAnalista(item.analistaCompras);

                var lineasServicio = LineaReporteServicioObjeto(item.servicio);

                foreach (var servicio in lineasServicio)
                {
                    var nuevoServicio = new ReporteServicioJson
                    {
                        consecutivo = item.consecutivo,
                        proyecto = proyecto,
                        proveedor = proveedor,
                        fechaGenerado = item.fechaGenerado,
                        analistaCompras = analista,
                        urgente = item.urgente,
                        direccionFacturacion = item.direccionFacturacion,
                        objeto = item.servicio.objeto,
                        alcance = item.servicio.alcance,
                        plazo = item.servicio.plazo,
                        montoTotal = item.servicio.montoTotal,
                        anticipo = item.servicio.anticipo,
                        montoAnticipo = item.servicio.montoAnticipo,
                        porcentajeAnticipo = item.servicio.porcentajeAnticipo,
                        fechaAnticipo = item.servicio.fechaAnticipo,
                        fechaInicio = item.servicio.fechaInicio,
                        utilidad = item.servicio.utilidad,
                        unidad = item.servicio.unidad,
                        tipoContrato = item.servicio.tipoContrato,
                        formaPago = item.servicio.formaPago,
                        observaciones = item.servicio.observaciones,
                        actividades = servicio.actividades,
                        unidad2 = servicio.unidad,
                        cantidad = servicio.cantidad,
                        valorUnidad = servicio.valorUnidad,
                        valorTotal = servicio.valorTotal
                    };

                    listaReporteServicioJson.Add(nuevoServicio);
                }
            }

            return listaReporteServicioJson;
        }

        Dictionary<string, string> usuarios = new Dictionary<string, string>();


        private async Task<string> nombreAnalista(string analistaCompras)
        {
            if (analistaCompras == null)
            {
                return "N/A";
            }

            if (!this.usuarios.ContainsKey(analistaCompras))
            {
                var usuario = await this._userManager.FindByIdAsync(analistaCompras);
                if (usuario != null)
                {
                    this.usuarios.Add(usuario.Id, usuario.Nombre + " " + usuario.Apellido);
                }
                else
                {

                    this.usuarios.Add(analistaCompras, "N/A");

                }

            }
            return this.usuarios[analistaCompras];
        }

        public async Task<OrdenCompra> ObtenerPorConsecutivo(int consecutivo, Guid idProyecto, Guid idProveedor)
        {
            var filtro1 = Builders<OrdenCompra>.Filter.Eq("consecutivo", consecutivo);
            var filtro2 = Builders<OrdenCompra>.Filter.Eq("proveedor", idProveedor);
            var filtro3 = Builders<OrdenCompra>.Filter.Eq("proyecto", idProyecto);
            var respuesta = await this.GetByProterty(filtro1 & filtro2 & filtro3);

            return respuesta.FirstOrDefault();

        }


        public async Task<OrdenCompra> ObtenerPorConsecutivo(int consecutivo, Guid idProyecto)
        {
            var filtro1 = Builders<OrdenCompra>.Filter.Eq("consecutivo", consecutivo);
            var filtro3 = Builders<OrdenCompra>.Filter.Eq("proyecto", idProyecto);
            var respuesta = await this.GetByProterty(filtro1 & filtro3);

            return respuesta.FirstOrDefault();

        }

        private List<string> LineaReporte(List<OrdenCompraMaterialDetalle> detalle)
        {

            //public string referencia { get; set; }
            //public string descripcion { get; set; }
            //public string unidad { get; set; }
            //public double cantidad { get; set; }
            //public string observaciones { get; set; }
            //public bool rechazado { get; set; }
            //public double valorUnitario { get; set; }
            //referencia;descripcion;unidad;cantidad;observaciones;rechazado;valorUnitario;

            var result = new List<string>();
            if (detalle != null && detalle.FirstOrDefault() != null)
            {


                foreach (var item in detalle.FirstOrDefault().detalleMaterial)
                {

                    var sb = new StringBuilder();
                    sb.Append($"{item.referencia}|{item.descripcion}|{item.unidad}|{item.cantidad}|{item.observaciones}|{item.rechazado}|{item.valorUnitario}");
                    result.Add(sb.ToString());

                }
            }

            return result;
        }

        private List<PedidoMaterialDetalle> LineaReporteObjeto(List<OrdenCompraMaterialDetalle> detalle)
        {
            var result = new List<PedidoMaterialDetalle>();

            if (detalle != null && detalle.FirstOrDefault() != null)
            {
                foreach (var item in detalle.FirstOrDefault().detalleMaterial)
                {
                    result.Add(item);

                }
            }

            return result;
        }

        private List<string> LineaReporte(PedidoServicioDetalle servicio)
        {

            var lineaServicio = $"{servicio.objeto}|{servicio.alcance}|{servicio.plazo}|" +
        $"{servicio.montoTotal}|{servicio.anticipo}|{servicio.montoAnticipo}|{servicio.porcentajeAnticipo}|" +
        $"{servicio.fechaAnticipo}|{servicio.fechaInicio}|{servicio.utilidad}|{servicio.unidad}|" +
        $"{servicio.tipoContrato}|{servicio.formaPago}| { servicio.observaciones}";

            var result = new List<string>();


            foreach (var item in servicio.servicio)
            {

                var lineaItems = $"{lineaServicio}|{ item.actividades}| { item.unidad}| { item.cantidad}| { item.valorUnidad}| { item.valorTotal}";
                result.Add(lineaItems);


            }


            return result;



        }
        private List<ItemPedidoServicio> LineaReporteServicioObjeto(PedidoServicioDetalle servicio)
        {
            var result = new List<ItemPedidoServicio>();

            foreach (var item in servicio.servicio)
            {


                result.Add(item);


            }


            return result;



        }

        public async Task<List<EjecutadoProyecto>> TotalProyectoProveedoresMes(Guid id)
        {

            List<EjecutadoProyecto> result = new List<EjecutadoProyecto>();

            var ordenesPorProyecto = await this.GetByProterty("proyecto", id);

            if (ordenesPorProyecto != null && ordenesPorProyecto.Count() > 0)
            {


                var grouped = ordenesPorProyecto.GroupBy(x => new { proveedor = x.proveedor, fecha = x.fechaGenerado.ToString("yyyy/MM") });

                foreach (var item in grouped)
                {
                    var ejecutadoProveedor = new EjecutadoProyecto();
                    double sumaDetalleMateriales = this.SumarMateriales(item.Where(x => x.pedidoMaterial == true));
                    double sumaDetalleServicios = this.SumarServicios(item.Where(x => x.pedidoMaterial == false && x.servicio != null));

                    ejecutadoProveedor.idProveedor = item.Key.proveedor.ToString();
                    ejecutadoProveedor.Mes = item.Key.fecha;

                    ejecutadoProveedor.totalEjecutado = (sumaDetalleMateriales + sumaDetalleServicios);

                    result.Add(ejecutadoProveedor);
                }

            }

            return result;

        }

        public async Task<List<PedidoMaterialDetalleReporte>> MaterialesXFechas(FiltroFacturas filtro)
        {

            var filter = Builders<OrdenCompra>.Filter.Gte("fechaGenerado", filtro.inicio);
            var filter2 = Builders<OrdenCompra>.Filter.Lte("fechaGenerado", filtro.fin);

            var filterFecha = filter & filter2;



            FilterDefinition<OrdenCompra> filtroProyecto = null;

            if (filtro.proyectos != null && filtro.proyectos.Count > 0)
            {
                filtroProyecto = Builders<OrdenCompra>.Filter.Eq("proyecto", filtro.proyectos[0]);

                for (int i = 1; i < filtro.proyectos.Count(); i++)
                {
                    var tempFilter = Builders<OrdenCompra>.Filter.Eq("proyecto", filtro.proyectos[i]);
                    filtroProyecto = filtroProyecto | tempFilter;

                }
            }



            FilterDefinition<OrdenCompra> filtroProveedores = null;

            if (filtro.proveedor != null && filtro.proveedor.Count > 0)
            {
                filtroProveedores = Builders<OrdenCompra>.Filter.Eq("proveedor", filtro.proveedor[0]);

                for (int i = 1; i < filtro.proveedor.Count(); i++)
                {
                    var tempFilter = Builders<OrdenCompra>.Filter.Eq("proveedor", filtro.proveedor[i]);
                    filtroProveedores = filtroProveedores | tempFilter;

                }
            }


            var ordenes = new List<OrdenCompra>();

            if (filtroProyecto != null)
            {

                if (filtroProveedores != null)
                {
                    ordenes = await this.GetByProterty(filterFecha & filtroProyecto & filtroProveedores);

                }
                else
                {
                    ordenes = await this.GetByProterty(filterFecha & filtroProyecto);


                }

            }
            else if (filtroProveedores != null)
            {
                ordenes = await this.GetByProterty(filterFecha & filtroProveedores);

            }
            else
            {

                ordenes = await this.GetByProterty(filterFecha);

            }







            var materiales = new List<PedidoMaterialDetalleReporte>();




            foreach (var orden in ordenes)
            {
                if (orden.detalle != null)
                {
                    foreach (var detalle in orden.detalle)
                    {

                        foreach (var detalleMaterial in detalle.detalleMaterial)
                        {

                            var detalleMaterialReporte = new PedidoMaterialDetalleReporte(detalleMaterial);
                            detalleMaterialReporte.fecha = orden.fechaGenerado;
                            await asignarProveedor(orden, detalleMaterialReporte);
                            await asignarProyecto(orden, detalleMaterialReporte);

                            materiales.Add(detalleMaterialReporte);
                        }


                    }
                }
            }

            return materiales;

        }

        Dictionary<Guid, string> listaProveedores = new Dictionary<Guid, string>();



        private async Task asignarProveedor(OrdenCompra orden, PedidoMaterialDetalleReporte detalleMaterial)
        {
            if (listaProveedores.ContainsKey(orden.proveedor))
            {

            }
            else
            {

                var proveedor = (await this._proveedorBLL.GetById(orden.proveedor));

                listaProveedores.Add(orden.proveedor, proveedor?.nombre);



            }

            detalleMaterial.proveedor = listaProveedores[orden.proveedor];

        }


        Dictionary<Guid, string> listaProyectos = new Dictionary<Guid, string>();
        private async Task asignarProyecto(OrdenCompra orden, PedidoMaterialDetalleReporte detalleMaterial)
        {
            if (listaProyectos.ContainsKey(orden.proyecto))
            {

            }
            else
            {

                var proyecto = (await this._proyectoBll.GetById(orden.proyecto));

                listaProyectos.Add(orden.proyecto, proyecto.nombre);



            }
            detalleMaterial.proyecto = listaProyectos[orden.proyecto];

        }


        private async Task<string> nombreProyecto(OrdenCompra orden)
        {
            if (listaProyectos.ContainsKey(orden.proyecto))
            {
                return listaProyectos[orden.proyecto];
            }
            else
            {

                var proyecto = (await this._proyectoBll.GetById(orden.proyecto));

                listaProyectos.Add(orden.proyecto, proyecto.nombre);

                return listaProyectos[orden.proyecto];


            }

        }


        private async Task<string> nombreProveedor(OrdenCompra orden)
        {
            if (listaProveedores.ContainsKey(orden.proveedor))
            {

            }
            else
            {

                var proveedor = (await this._proveedorBLL.GetById(orden.proveedor));

                listaProveedores.Add(orden.proveedor, proveedor?.nombre);



            }

            return listaProveedores[orden.proveedor];

        }



        public async Task<string> MaterialesXFechasCsv(FiltroFacturas filtro)
        {

            var materiales = await MaterialesXFechas(filtro);


            return GenerarCSV(materiales.OrderBy(x => x.fecha).ToList());

        }


        private string GenerarCSV(List<PedidoMaterialDetalleReporte> materiales)
        {


            var sb = new StringBuilder("referencia|descripcion|unidad|cantidad|observaciones|valorUnitario|fecha|proyecto|proveedor");
            sb.AppendLine();
            foreach (var item in materiales)
            {

                sb.AppendLine(LineaReporte(item));
            }

            return sb.ToString();


        }

        private string LineaReporte(PedidoMaterialDetalleReporte item)
        {
            var linea = $"{item.referencia}|{item.descripcion}|{item.unidad}|{item.cantidad}|{item.observaciones}|{item.valorUnitario}|{item.fecha}|{item.proyecto}|{item.proveedor}";
            return linea;
        }




        public async Task<CupoDisponibleProyecto> CalcularCupo(Guid idProyecto, Guid idProveedor)
        {


            var result = new CupoDisponibleProyecto();
            result.idProveedor = idProveedor;
            result.idProyecto = idProyecto;

            var filter = Builders<OrdenCompra>.Filter.Eq("proyecto", idProyecto);
            var filter2 = Builders<OrdenCompra>.Filter.Eq("proveedor", idProveedor);

            var ordenesXProveedorXProyecto = await this.GetByProterty((filter & filter2));

            if (ordenesXProveedorXProyecto != null)
            {
                double sumaDetalleMateriales = this.SumarMateriales(ordenesXProveedorXProyecto.Where(x => x.pedidoMaterial == true && x.pagada == false));
                double sumaDetalleServicios = this.SumarServicios(ordenesXProveedorXProyecto.Where(x => x.pedidoMaterial == false && x.pagada == false));

                result.TotalMateriales = sumaDetalleMateriales;
                result.TotalServicios = sumaDetalleServicios;




                var proveedor = await _proveedorBLL.GetById(idProveedor);

                if (proveedor.cupoProyecto != null)
                {
                    var cupoProyecto = proveedor.cupoProyecto.FirstOrDefault(x => x.proyecto == idProyecto);

                    if (cupoProyecto != null)
                    {
                        result.CupoProveedor = cupoProyecto.cupo;
                    }
                    else
                    {

                        result.CupoProveedor = proveedor.cupoCreditoGeneral;

                    }

                    if (result.CupoProveedor.HasValue)
                    {
                        result.CupoDisponible = (result.CupoProveedor - result.TotalMateriales - result.TotalServicios);
                    }
                }



            }

            return result;
        }

        private double SumarServicios(IEnumerable<OrdenCompra> ordenesServicios)
        {
            var result = 0.0;

            if (ordenesServicios != null)
            {
                result = ordenesServicios.Where(x => x.servicio != null).Sum(x => x.servicio.montoTotal);

            }

            return result;
        }

        private double SumarMateriales(IEnumerable<OrdenCompra> ordenesServicio)
        {
            var total = 0.0;


            if (ordenesServicio != null)
            {
                var detalles = new List<OrdenCompraMaterialDetalle>();

                foreach (var orden in ordenesServicio)
                {
                    detalles.AddRange(orden.detalle);
                }

                var materialDetalle = new List<PedidoMaterialDetalle>();

                foreach (var detalle in detalles)
                {
                    materialDetalle.AddRange(detalle.detalleMaterial);
                }

                foreach (var material in materialDetalle)
                {
                    var subtotal = material.cantidad * material.valorUnitario;

                    total += subtotal;
                }

            }

            return total;

        }

        public async Task Pendiente(Guid entityIdOrden)
        {
            OrdenCompra ordenCompra = await this.GetById(entityIdOrden);

            ordenCompra.pendiente = true;

            await this.Update(ordenCompra);


        }

        public async Task<List<OrdenCompra>> OrdenesPendientes()
        {

            return await this.GetByProterty("pendiente", true);
        }

        public async Task NoPendiente(Guid entityIdOrden)
        {
            OrdenCompra ordenCompra = await this.GetById(entityIdOrden);

            ordenCompra.pendiente = false;

            await this.Update(ordenCompra);
        }

      

        public async Task<List<ResultadoReporteReferencia>> ReporteReferencias(FiltroReporteReferencias filtro)
        {

            List<ResultadoReporteReferencia> result = new List<ResultadoReporteReferencia>();

            List<FilterDefinition<OrdenCompra>> listaFiltrosOrdenCompra = new List<FilterDefinition<OrdenCompra>>();

            listaFiltrosOrdenCompra.Add(Builders<OrdenCompra>.Filter.Eq("pedidoMaterial", true));



            if (filtro.fechaInicial.HasValue)
            {
                listaFiltrosOrdenCompra.Add(Builders<OrdenCompra>.Filter.Gte("fechaGenerado", filtro.fechaInicial.Value.Date));

            }



            if (filtro.fechaFinal.HasValue)
            {
                listaFiltrosOrdenCompra.Add(Builders<OrdenCompra>.Filter.Lte("fechaGenerado", filtro.fechaFinal.Value.Date));

            }


            if (filtro.proyecto != null && filtro.proyecto.Count() > 0)
            {
                FilterDefinition<OrdenCompra> filtroProyectos = Builders<OrdenCompra>.Filter.Eq("proyecto", filtro.proyecto[0]);

                for (int i = 1; i < filtro.proyecto.Count; i++)
                {
                    filtroProyectos = filtroProyectos | Builders<OrdenCompra>.Filter.Eq("proyecto", filtro.proyecto[i]);
                }

                listaFiltrosOrdenCompra.Add(filtroProyectos);
            }

            FilterDefinition<OrdenCompra> filtroOrdenes = null;

            if (listaFiltrosOrdenCompra.Count() > 1)
            {

                FilterDefinition<OrdenCompra> filter = listaFiltrosOrdenCompra[0];

                for (int i = 1; i < listaFiltrosOrdenCompra.Count(); i++)
                {
                    filter = filter & listaFiltrosOrdenCompra[i];
                }

                filtroOrdenes = filter;
            }
            else if (listaFiltrosOrdenCompra.Count() == 1)
            {
                filtroOrdenes = listaFiltrosOrdenCompra[0];
            }

            if (filtroOrdenes != null)
            {
                var ordenes = await this.GetByProterty(filtroOrdenes);

                var detalles = from o in ordenes
                               select new
                               {
                                   detalle = o.detalle,
                                   idProveedor = o.proveedor,
                                   idProyecto = o.proyecto
                               };




                var referencias = from d in detalles
                                  from r in d.detalle
                                  from dm in r.detalleMaterial
                                  select new
                                  {
                                      referencia = dm.referencia,
                                      nombreReferencia = dm.nombre,
                                      id = dm.id,
                                      idProyecto = d.idProyecto,
                                      idCategoria = dm.idCategoria,
                                      descripcion = dm.descripcion,
                                      unidad = dm.unidad,
                                      cantidad = dm.cantidad,
                                      valorUnitario = dm.valorUnitario,
                                      idProveedor = d.idProveedor

                                  };


                referencias = referencias.Where(x => !string.IsNullOrWhiteSpace(x.referencia));
                if (filtro.categoria != null && filtro.categoria.Count > 0)
                {

                    referencias = referencias.Where(x => filtro.categoria.Contains(x.idCategoria));


                }

                if (filtro.referencia != null && filtro.referencia.Count > 0)
                {

                    referencias = referencias.Where(x => filtro.referencia.Contains(x.id));


                }

                var categorias = await this._categoriasBLL.GetAll();
                var proyectos = await _proyectoBll.GetAll();
                var proveedores = await _proveedorBLL.GetAll();

                //result = (from r in referencias
                //          select
                //              new ResultadoReporteReferencia()
                //              {
                //                  referencia = r.referencia,
                //                  cantidad = r.cantidad,
                //                  categoria = categorias.Find(x => x.id == r.idCategoria) == null ? "N/A" : categorias.Find(x => x.id == r.idCategoria).Name,
                //                  proveedor = proveedores.Find(x => x.id == r.idProveedor).nombre,
                //                  proyecto = proyectos.Find(x => x.id == r.idProyecto).nombre,
                //                  unidad = r.unidad,
                //                  valorUnitario = r.valorUnitario,
                //                  total = r.cantidad * r.valorUnitario,
                //                  idProyecto = r.idProyecto

                //              }).ToList();

                foreach (var r in referencias)
                {
                    try
                    {
                        var o = new ResultadoReporteReferencia();

                        o.referencia = r.referencia;
                        o.nombreReferencia = r.nombreReferencia;
                        o.cantidad = r.cantidad;
                        o.categoria = categorias.Find(x => x.id == r.idCategoria) == null ? "N/A" : categorias.Find(x => x.id == r.idCategoria).Name;
                        o.proveedor = proveedores.Find(x => x.id == r.idProveedor) == null ? "N/A" : proveedores.Find(x => x.id == r.idProveedor).nombre;
                        o.proyecto = proyectos.Find(x => x.id == r.idProyecto) == null ? "N/A" : proyectos.Find(x => x.id == r.idProyecto).nombre;
                        o.unidad = r.unidad;
                        o.valorUnitario = r.valorUnitario;
                        o.total = r.cantidad * r.valorUnitario;
                        o.idProyecto = r.idProyecto;

                        result.Add(o);

                    }
                    catch (Exception ex)
                    {

                        throw;
                    }
                }

            }
            else
            {

            }



            return result;

        }
    }
}
