using AutoMapper;
using Cobalto.SQL.Core.Models;
using Core.Bll;
using Core.BLL;
using Core.Models;
using Dapper;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Presentation;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Cobalto.SQL.Core.BLL
{
    public class MovimientoInventarioBLL : BaseBLL<MovimientoInventario>
    {

        public OrdenCompraBLL OcMongoBLL { get; }

        private OrdenCompraSQLBLL OcSQLBLL;
        private ItemOrdenCompraBLL itemOrdenCompraBLL;
        private MaterialBLL materialBll;
        private ReferenciaBLL referenciaBLL;
        private FrenteBLL frenteBLL;
        private ResponsableBLL responsableBLL;
        private AprobacionBLL aprobacionBLL;
        private ProveedorBLL proveedorBLL;
        private readonly FacturaBLL facturaBLL;
        private RemisionBLL remisionBLL;
        private readonly ItemObraBLL itemObraBLL;

        public MovimientoInventarioBLL(IConfiguration configuration,
            OrdenCompraBLL OcMongoBLL,
            OrdenCompraSQLBLL OcSQLBLL,
            ItemOrdenCompraBLL itemOrdenCompraBLL,
            ReferenciaBLL referenciaBLL,
            ResponsableBLL responsableBLL,
            FrenteBLL frenteBLL,
            AprobacionBLL aprobacionBLL,
            ProveedorBLL proveedorBLL,
            FacturaBLL facturaBLL,
            RemisionBLL remisionBLL,
            ItemObraBLL itemObraBLL,
            MaterialBLL materialBll) : base(configuration)
        {
            this.OcMongoBLL = OcMongoBLL;
            this.OcSQLBLL = OcSQLBLL;

            this.itemOrdenCompraBLL = itemOrdenCompraBLL;
            this.materialBll = materialBll;
            this.referenciaBLL = referenciaBLL;
            this.frenteBLL = frenteBLL;
            this.responsableBLL = responsableBLL;
            this.aprobacionBLL = aprobacionBLL;

            this.proveedorBLL = proveedorBLL;
            this.facturaBLL = facturaBLL;
            this.remisionBLL = remisionBLL;
            this.itemObraBLL = itemObraBLL;
        }


        public async Task<OrdenCompraSQLDTO> RegistrarOC(Guid ocId, string usuario)
        {


            OrdenCompraSQL ordenCompra = this.OcSQLBLL.PorIdCompras(ocId);



            if (ordenCompra == null)
            {

                var ocMOngo = await this.OcMongoBLL.GetById(ocId);

                var id = this.OcSQLBLL.Insertar(ocMOngo, usuario);


                ordenCompra = this.OcSQLBLL.PorIdCompras(ocId);

            }

            OrdenCompraSQLDTO dest = ConvertToSQl(ordenCompra);

            dest.Items = this.itemOrdenCompraBLL.PorOrden(ordenCompra.Id);

            return dest;



        }




        public IEnumerable<MovimientoInventario> SalidaDesdeFrente(SalidaDesdeFrente salida)
        {
            var frente = this.frenteBLL.PorId(salida.IdFrente);
            //var responsable = this.responsableBLL.PorId(frente.IdResponsable);

            var result = new List<MovimientoInventario>();


            var fecha = DateTime.Now;
            foreach (Salida itemSalida in salida.Items)
            {


                MovimientoInventario movimientoInventario = new MovimientoInventario();

                var material = this.materialBll.PorIdInsumoYProyecto(itemSalida.IdInsumo, salida.IdProyecto);



                movimientoInventario.IdMaterial = material.Id;
                movimientoInventario.Referencia = material.Referencia;
                movimientoInventario.Nombre = material.Nombre;
                movimientoInventario.Descripcion = material.Descripcion;
                movimientoInventario.Unidad = material.Unidad;
                // movimientoInventario.Observaciones = material.Observaciones;
                //movimientoInventario.ValorUnitario = material.ValorUnitario;
                movimientoInventario.Ingreso = -1.0;
                movimientoInventario.TipoMovimiento = TipoMovimientoENUM.SalidaDesdeFrente;
                movimientoInventario.Fecha = fecha;
                movimientoInventario.Usuario = salida.Usuario;
                movimientoInventario.IdFrente = salida.IdFrente;
                movimientoInventario.IdProyecto = salida.IdProyecto;
                movimientoInventario.NumeroPedido = string.Empty;
                movimientoInventario.IdProveedor = Guid.Empty;
                movimientoInventario.Aprobado = true;
                movimientoInventario.IdResponsable = frente.IdResponsable;
                movimientoInventario.IdInsumo = material.IdInsumo;

                movimientoInventario.CantidadOriginal = ObtenerCantidadActual(itemSalida.IdInsumo, salida.IdProyecto);


                movimientoInventario.CantidadMovimiento = itemSalida.Cantidad;
                movimientoInventario.IdItemContrato = itemSalida.IdItemContrato;

                movimientoInventario.Id = this.Insertar(movimientoInventario).Value;


                result.Add(movimientoInventario);

            }


            return result;

        }

        public IEnumerable<CantidadActual> CantidadesPorProyectoYFrente(Guid idProyecto, int idFrente)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var result = new List<CantidadActual>();

                var movimientos = connection.Query<MovimientoInventario>(@"select [Id]
                          ,[IdMaterial]
                          ,[IdOrden]
                          ,[IdInsumo]
                          ,[Referencia]
                          ,[Nombre]
                          ,[Descripcion]
                          ,[Unidad]
                          ,[CantidadOriginal]
                          ,[CantidadMovimiento]
                          ,[Observaciones]
                          ,[ValorUnitario]
                          ,[Ingreso]
                          ,[TipoMovimiento]
                          ,[Fecha]
                          ,[Usuario]
                          ,[IdFrente]
                          ,[IdProyecto]
                          ,[NumeroPedido]
                          ,[IdProveedor]
                          ,[Aprobado]
                          ,[Borrado]
                     from (select *,
                                  row_number() over (partition by [IdInsumo] order by fecha desc,  [Id] desc) as seqnum
                           from [dbo].[MovimientoInventario]
	                       where [IdProyecto] = '" + idProyecto + @"' and [IdFrente] is not null and ingreso = -1 and [IdFrente] = " + idFrente + @"
                          ) t
                    where seqnum = 1;");



                foreach (var movimiento in movimientos)
                {

                    double cantidad = 0.0;


                    var material = this.materialBll.PorIdInsumoYProyecto(movimiento.IdInsumo, movimiento.IdProyecto);

                    var cantidadActual = MaterialACantidadActual(material);



                    cantidad = movimiento.CantidadOriginal + (movimiento.CantidadMovimiento * movimiento.Ingreso);

                    cantidadActual.Cantidad = cantidad;

                    result.Add(cantidadActual);


                }


                return result;

            }
        }


        public async Task<IEnumerable<CantidadActual>> Inventario(Guid idProyecto, int? idFrente)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var result = new List<CantidadActual>();


                var sql = $"select [IdMaterial] ," +
                    $"SUM([CantidadMovimiento] * Ingreso ) as CantidadOriginal," +
                    $"MIN([Referencia]) as  [Referencia]," +

                     $"MIN([IdProyecto]) as  [IdProyecto]," +
                     $"MIN([IdFrente]) as  [IdFrente]," +
                    $" MIN([Descripcion] ) as [Descripcion]," +
                    $" min ([Nombre]) as [Nombre]   , min([IdInsumo] ) as [IdInsumo] from [dbo].[MovimientoInventario]	 where [IdProyecto] =" +
                    $" '{idProyecto}' group by IdMaterial";

                if (idFrente.HasValue)
                {

                    sql = $"select [IdMaterial] ," +
                        $"SUM([CantidadMovimiento] * Ingreso ) as CantidadOriginal," +
                        $"MIN([Referencia]) as  [Referencia]," +
                          $"MIN([IdProyecto]) as  [IdProyecto]," +
                     $"MIN([IdFrente]) as  [IdFrente]," +
                        $" MIN([Descripcion] ) as [Descripcion]," +
                        $" min ([Nombre]) as [Nombre]   , min([IdInsumo] ) as [IdInsumo] from [dbo].[MovimientoInventario]	 where [IdProyecto] =" +
                        $" '{idProyecto}' and idFrente = '{idFrente.Value}' group by IdMaterial";
                }


                var movimientos = connection.Query<MovimientoInventario>(sql);



                foreach (var movimiento in movimientos)
                {

                    double cantidad = 0.0;


                    var material = this.materialBll.PorIdInsumoYProyecto(movimiento.IdInsumo, movimiento.IdProyecto);

                    var cantidadActual = MaterialACantidadActual(material);




                    cantidadActual.Cantidad = movimiento.CantidadOriginal;
                    cantidadActual.Fecha = DateTime.Now;

                    //public double? ValorUnitario { get; set; }
                    //public double? ValorTotal { get; set; }
                    //public double? Iva { get; set; }
                    //public double? ValorTotalConIva { get; set; }


                    var ultimoMovimiento = this.Filtrar(new { IdInsumo = movimiento.IdInsumo, Ingreso = 1 }).OrderByDescending(x => x.Fecha).FirstOrDefault(x=> x.ValorUnitario != 0);


                   

                    if (ultimoMovimiento != null)
                    {
                        cantidadActual.ValorUnitario = ultimoMovimiento.ValorUnitario;

                    }
                    else
                    {
                        cantidadActual.ValorUnitario = 0;

                    }


                    var referencia = await this.referenciaBLL.GetById(material.IdInsumo);

                    cantidadActual.ValorTotal = cantidadActual.ValorUnitario * cantidadActual.Cantidad;



                    if (referencia != null)
                    {

                        cantidadActual.Unidad = referencia.unidad;

                        cantidadActual.Iva = referencia.ProcentajeIva;

                        if (cantidadActual.Iva.HasValue)
                        {
                            cantidadActual.ValorTotalConIva = cantidadActual.ValorUnitario * cantidadActual.Cantidad * (1.0 + cantidadActual.Iva.Value / 100);
                        }
                        else
                        {

                            cantidadActual.ValorTotalConIva = cantidadActual.ValorUnitario * cantidadActual.Cantidad;
                        }


                    }





                    result.Add(cantidadActual);


                }


                return result;

            }
        }

        public MovimientoInventario Aprobar(AprobacionMovimiento aprobacionMovimiento)
        {


            MovimientoInventario movimiento = this.PorId(aprobacionMovimiento.IdMovimiento);

            if (movimiento != null)
            {
                movimiento.Aprobado = true;

                this.Actualizar(movimiento);

                var aprobacion = new Aprobacion();

                aprobacion.Fecha = DateTime.Now;
                aprobacion.IdMovimiento = aprobacionMovimiento.IdMovimiento;
                aprobacion.Usuario = aprobacionMovimiento.Usuario;

                this.aprobacionBLL.Insertar(aprobacion);

            }

            return movimiento;

        }

        public IEnumerable<MovimientoInventario> SinAprobar(Guid idProyecto)
        {
            using (var connection = new SqlConnection(_connectionString))
            {

                // var result = new List<MovimientoInventario>();


                var movimientos = connection.GetListPaged<MovimientoInventario>(1, 1, "where [IdProyecto] = @idProyecto and aprobado = 0", "Fecha asc", new { idProyecto }, null, null);


                return movimientos.ToList();

            }
        }


        public async Task< IEnumerable<MovimientoInventario>> Kardex(FiltroKardex value)
        {
            using (var connection = new SqlConnection(_connectionString))
            {






                /****************/

                var where = "select * from MovimientoInventario where ";

                var filtros = new List<string>();

                if (value.FechaFinal.HasValue)
                {
                    filtros.Add($" [dbo].[MovimientoInventario].fecha <= '{value.FechaFinal.Value.ToString("yyyy-MM-dd")}'");
                }

                if (value.FechaInicial.HasValue)
                {
                    filtros.Add($" [dbo].[MovimientoInventario].fecha >= '{value.FechaInicial.Value.ToString("yyyy-MM-dd")}'");
                }

                //********************************
                if (value.IdProyecto.HasValue)
                {
                    filtros.Add($" IdProyecto = '{value.IdProyecto.Value}'");
                }

                if (value.IdFrente.HasValue)
                {
                    filtros.Add($" IdFrente = {value.IdFrente.Value}");

                }


                var k = 0;
                foreach (var item in filtros)
                {
                    if (k == 0)
                    {
                        where = where += $" {item} ";

                    }
                    else
                    {
                        where = where += $" AND {item} ";

                    }

                    k++;
                }

                /******************/






                var movimientos = connection.Query<MovimientoInventario>(where);


                var proveedores = await this.proveedorBLL.GetAll();

                foreach (var item in movimientos)
                {
                    var proveedor = proveedores.FirstOrDefault(x => x.id == item.IdProveedor);

                    if (proveedor != null)
                    {
                        item.Proveedor = proveedor.nombre;

                    }

                    if (item.IdOrden.HasValue)
                    {

                        var orden = this.OcSQLBLL.PorId(item.IdOrden.Value);




                        if (orden != null)
                        {
                            var remision = (await this.remisionBLL.GetByProterty("idOrdenCompra", orden.IdCompras)).FirstOrDefault();

                            if (remision != null)
                            {
                                item.Remision = remision.numeroRemision;
                                
                            }
                        }
                    }

                }


                //return movimientos.ToList();

                return movimientos;

            }
        }



        public async Task<IEnumerable<MovimientoInventario>> ReporteEntradas(FiltroKardex value)
        {
            using (var connection = new SqlConnection(_connectionString))
            {






                /****************/

                var where = "select * from MovimientoInventario where ";

                var filtros = new List<string>();

                if (value.FechaFinal.HasValue)
                {
                    filtros.Add($" [dbo].[MovimientoInventario].fecha <= '{value.FechaFinal.Value.ToString("yyyy-MM-dd")}'");
                }

                if (value.FechaInicial.HasValue)
                {
                    filtros.Add($" [dbo].[MovimientoInventario].fecha >= '{value.FechaInicial.Value.ToString("yyyy-MM-dd")}'");
                }

                //********************************
                if (value.IdProyecto.HasValue)
                {
                    filtros.Add($" IdProyecto = '{value.IdProyecto.Value}'");
                }

                if (value.IdFrente.HasValue)
                {
                    filtros.Add($" IdFrente = {value.IdFrente.Value}");

                }

                filtros.Add($" (TipoMovimiento not like 'SALIDA%')");

                

                var k = 0;
                foreach (var item in filtros)
                {
                    if (k == 0)
                    {
                        where = where += $" {item} ";

                    }
                    else
                    {
                        where = where += $" AND {item} ";

                    }

                    k++;
                }

                /******************/






                var movimientos = connection.Query<MovimientoInventario>(where);


                var proveedores = await this.proveedorBLL.GetAll();

                foreach (var item in movimientos)
                {
                    var proveedor = proveedores.FirstOrDefault(x => x.id == item.IdProveedor);


                    var material = this.materialBll.PorIdInsumoYProyecto(item.IdInsumo, item.IdProyecto);
                    var referencia = await this.referenciaBLL.GetById(material.IdInsumo);

                    item.ValorTotal = item.ValorUnitario * item.CantidadMovimiento;

                    if (referencia != null)
                    {
                        item.Iva = referencia.ProcentajeIva;

                        if (item.Iva.HasValue)
                        {
                            item.ValorTotalConIva = item.ValorUnitario * item.CantidadMovimiento * (1.0 + item.Iva.Value/100);
                        }
                        else
                        {

                            item.ValorTotalConIva = item.ValorUnitario * item.CantidadMovimiento;
                        }


                    }


                    if (proveedor != null)
                    {
                        item.Proveedor = proveedor.nombre;

                    }

                    if (item.IdOrden.HasValue)
                    {

                        var orden = this.OcSQLBLL.PorId(item.IdOrden.Value);




                        if (orden != null)
                        {

                            item.OrdenCompra = orden.Consecutivo.ToString();
                            var remision = (await this.remisionBLL.GetByProterty("idOrdenCompra", orden.IdCompras)).FirstOrDefault();

                            var facturas = await this.facturaBLL.GetByProterty("idOrdenCompra", orden.IdCompras);

                            var factura = facturas.OrderBy(x => x.aprobada).ThenByDescending(x => x.fecha).FirstOrDefault();

                            if (factura != null)
                            {
                                item.numeroFactura = factura.numeroFactura;
                                item.FechaFactura = factura.fecha;
                            }

                            if (remision != null)
                            {
                                item.Remision = remision.numeroRemision;

                            }
                        }
                    }

                }


                //return movimientos.ToList();

                return movimientos;

            }
        }

        public async Task<IEnumerable<MovimientoInventario>> ReporteSalidas(FiltroKardex value)
        {
            using (var connection = new SqlConnection(_connectionString))
            {






                /****************/

                var where = "select * from MovimientoInventario where ";

                var filtros = new List<string>();

                if (value.FechaFinal.HasValue)
                {
                    filtros.Add($" [dbo].[MovimientoInventario].fecha <= '{value.FechaFinal.Value.ToString("yyyy-MM-dd")}'");
                }

                if (value.FechaInicial.HasValue)
                {
                    filtros.Add($" [dbo].[MovimientoInventario].fecha >= '{value.FechaInicial.Value.ToString("yyyy-MM-dd")}'");
                }

                //********************************
                if (value.IdProyecto.HasValue)
                {
                    filtros.Add($" IdProyecto = '{value.IdProyecto.Value}'");
                }

                if (value.IdFrente.HasValue)
                {
                    filtros.Add($" IdFrente = {value.IdFrente.Value}");

                }

                filtros.Add($" (TipoMovimiento like 'SALIDA%')");



                var k = 0;
                foreach (var item in filtros)
                {
                    if (k == 0)
                    {
                        where = where += $" {item} ";

                    }
                    else
                    {
                        where = where += $" AND {item} ";

                    }

                    k++;
                }

                /******************/






                var movimientos = connection.Query<MovimientoInventario>(where);


                var proveedores = await this.proveedorBLL.GetAll();

                foreach (var item in movimientos)
                {
                    var proveedor = proveedores.FirstOrDefault(x => x.id == item.IdProveedor);
                    
                    var ultimoMovimiento = this.Filtrar(new { IdInsumo = item.IdInsumo, TipoMovimiento = "INGRESO_DESDE_OC" }).OrderByDescending(x => x.Fecha).FirstOrDefault();


                    if (item.IdItemContrato > 0)
                    {
                        var itemContrato = this.itemObraBLL.PorId(item.IdItemContrato);

                        item.CodigoItemObra = itemContrato.Item;
                        item.ItemContrato = itemContrato.Descripcion;
                    }

                    if (ultimoMovimiento!= null)
                    {
                        item.ValorUnitario = ultimoMovimiento.ValorUnitario;

                    }


                    var material = this.materialBll.PorIdInsumoYProyecto(item.IdInsumo, item.IdProyecto);
                    var referencia = await this.referenciaBLL.GetById(material.IdInsumo);

                    item.ValorTotal = item.ValorUnitario * item.CantidadMovimiento;



                    if (referencia != null)
                    {

                        item.Unidad = referencia.unidad;

                        item.Iva = referencia.ProcentajeIva;

                        if (item.Iva.HasValue)
                        {
                            item.ValorTotalConIva = item.ValorUnitario * item.CantidadMovimiento * (1.0 + item.Iva.Value / 100);
                        }
                        else
                        {

                            item.ValorTotalConIva = item.ValorUnitario * item.CantidadMovimiento;
                        }


                    }


                    if (proveedor != null)
                    {
                        item.Proveedor = proveedor.nombre;

                    }

                    if (item.IdOrden.HasValue)
                    {

                        var orden = this.OcSQLBLL.PorId(item.IdOrden.Value);




                        if (orden != null)
                        {

                            item.OrdenCompra = orden.Consecutivo.ToString();
                            var remision = (await this.remisionBLL.GetByProterty("idOrdenCompra", orden.IdCompras)).FirstOrDefault();

                            var facturas = await this.facturaBLL.GetByProterty("idOrdenCompra", orden.IdCompras);

                            var factura = facturas.OrderBy(x => x.aprobada).ThenByDescending(x => x.fecha).FirstOrDefault();

                            if (factura != null)
                            {
                                item.numeroFactura = factura.numeroFactura;
                                item.FechaFactura = factura.fecha;
                            }

                            if (remision != null)
                            {
                                item.Remision = remision.numeroRemision;

                            }
                        }
                    }

                }


                //return movimientos.ToList();

                return movimientos;

            }
        }



        public async Task<IEnumerable<CantidadActualFrente>> PorFrente(FiltroKardex value)
        {
            var idFrente = value.IdFrente;
            var idProyecto = value.IdProyecto;

            var frentes = this.frenteBLL.Filtrar(new { IdProyecto = value.IdProyecto });
            
            using (var connection = new SqlConnection(_connectionString))
            {
                var result = new List<CantidadActualFrente>();


                var sql = $"select IdFrente, [IdMaterial] ," +
                    $"SUM([CantidadMovimiento] * Ingreso ) as CantidadOriginal," +
                    $"MIN([Referencia]) as  [Referencia]," +

                     $"MIN([IdProyecto]) as  [IdProyecto]," +
                     $"MIN([IdFrente]) as  [IdFrente]," +
                    $" MIN([Descripcion] ) as [Descripcion]," +
                    $" min ([Nombre]) as [Nombre]   , min([IdInsumo] ) as [IdInsumo] from [dbo].[MovimientoInventario]	 where [IdProyecto] =" +
                    $" '{idProyecto}' group by IdMaterial, IdFrente";

                if (idFrente.HasValue)
                {

                    sql = $"select  IdFrente, [IdMaterial] ," +
                        $"SUM([CantidadMovimiento] * Ingreso ) as CantidadOriginal," +
                        $"MIN([Referencia]) as  [Referencia]," +
                          $"MIN([IdProyecto]) as  [IdProyecto]," +
                     $"MIN([IdFrente]) as  [IdFrente]," +
                        $" MIN([Descripcion] ) as [Descripcion]," +
                        $" min ([Nombre]) as [Nombre]   , min([IdInsumo] ) as [IdInsumo] from [dbo].[MovimientoInventario]	 where [IdProyecto] =" +
                        $" '{idProyecto}' and idFrente = '{idFrente.Value}' group by IdMaterial, IdFrente";
                }


                var movimientos = connection.Query<MovimientoInventario>(sql);



                foreach (var movimiento in movimientos)
                {

                    double cantidad = 0.0;


                    var material = this.materialBll.PorIdInsumoYProyecto(movimiento.IdInsumo, movimiento.IdProyecto);

                    var cantidadActual = MaterialACantidadActualFrente(material);




                    cantidadActual.Cantidad = -1.0*movimiento.CantidadOriginal;
                    cantidadActual.IdFrente = movimiento.IdFrente ;


                    cantidadActual.Fecha = DateTime.Now;

                    //public double? ValorUnitario { get; set; }
                    //public double? ValorTotal { get; set; }
                    //public double? Iva { get; set; }
                    //public double? ValorTotalConIva { get; set; }


                    var ultimoMovimiento = this.Filtrar(new { IdInsumo = movimiento.IdInsumo, Ingreso = 1 }).OrderByDescending(x => x.Fecha).FirstOrDefault(x => x.ValorUnitario != 0);




                    if (ultimoMovimiento != null)
                    {
                        cantidadActual.ValorUnitario = ultimoMovimiento.ValorUnitario;

                    }
                    else
                    {
                        cantidadActual.ValorUnitario = 0;

                    }


                    var referencia = await this.referenciaBLL.GetById(material.IdInsumo);

                    cantidadActual.ValorTotal = cantidadActual.ValorUnitario * cantidadActual.Cantidad;



                    if (referencia != null)
                    {

                        cantidadActual.Unidad = referencia.unidad;

                        cantidadActual.Iva = referencia.ProcentajeIva;

                        if (cantidadActual.Iva.HasValue)
                        {
                            cantidadActual.ValorTotalConIva = cantidadActual.ValorUnitario * cantidadActual.Cantidad * (1.0 + cantidadActual.Iva.Value / 100);
                        }
                        else
                        {

                            cantidadActual.ValorTotalConIva = cantidadActual.ValorUnitario * cantidadActual.Cantidad;
                        }


                    }


                    if (cantidadActual.IdFrente.HasValue &&   frentes.FirstOrDefault(x => x.Id == cantidadActual.IdFrente) != null)
                    {
                        cantidadActual.Frente = frentes.FirstOrDefault(x => x.Id == cantidadActual.IdFrente).Nombre;
                        result.Add(cantidadActual);

                    }



                }


                return result;

            }
        }


        public MovimientoInventario Salida(SalidaSinFrente salida)
        {

            MovimientoInventario result = new MovimientoInventario();




            MovimientoInventario movimientoInventario = new MovimientoInventario();

            var material = this.materialBll.PorIdInsumoYProyecto(salida.IdInsumo, salida.IdProyecto);



            movimientoInventario.IdMaterial = material.Id;
            movimientoInventario.Referencia = material.Referencia;
            movimientoInventario.Nombre = material.Nombre;
            movimientoInventario.Descripcion = material.Descripcion;
            movimientoInventario.Unidad = material.Unidad;
            movimientoInventario.Observaciones = salida.Observaciones;
            // movimientoInventario.ValorUnitario = material.ValorUnitario;
            movimientoInventario.Ingreso = -1.0;
            movimientoInventario.TipoMovimiento = TipoMovimientoENUM.SalidaSinFrente;


            if (salida.IdFrente.HasValue)
            {
                var frente = this.frenteBLL.PorId(salida.IdFrente.Value);
                movimientoInventario.IdFrente = salida.IdFrente;
                movimientoInventario.IdResponsable = frente.IdResponsable;
            }


            movimientoInventario.Fecha = DateTime.Now;
            movimientoInventario.Usuario = salida.Usuario;
            movimientoInventario.IdProyecto = salida.IdProyecto;
            movimientoInventario.NumeroPedido = string.Empty;
            movimientoInventario.IdProveedor = Guid.Empty;
            movimientoInventario.Aprobado = false;
            movimientoInventario.IdInsumo = material.IdInsumo;
            movimientoInventario.Causa = salida.Causa;

            movimientoInventario.CantidadOriginal = ObtenerCantidadActual(salida.IdInsumo, salida.IdProyecto);


            movimientoInventario.CantidadMovimiento = salida.Cantidad;

            movimientoInventario.Id = this.Insertar(movimientoInventario).Value;


            result = movimientoInventario;




            return result;

        }

        public IEnumerable<CantidadActual> CantidadesPorProyecto(Guid idProyecto)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var result = new List<CantidadActual>();

                var movimientos = connection.Query<MovimientoInventario>(@"select [Id]
                          ,[IdMaterial]
                          ,[IdOrden]
                          ,[IdInsumo]
                          ,[Referencia]
                          ,[Nombre]
                          ,[Descripcion]
                          ,[Unidad]
                          ,[CantidadOriginal]
                          ,[CantidadMovimiento]
                          ,[Observaciones]
                          ,[ValorUnitario]
                          ,[Ingreso]
                          ,[TipoMovimiento]
                          ,[Fecha]
                          ,[Usuario]
                          ,[IdFrente]
                          ,[IdProyecto]
                          ,[NumeroPedido]
                          ,[IdProveedor]
                          ,[Aprobado]
                          ,[Borrado]
                     from (select *,
                                  row_number() over (partition by [IdInsumo] order by fecha desc,  [Id] desc) as seqnum
                           from [dbo].[MovimientoInventario]
	                       where [IdProyecto] = '" + idProyecto + @"'
                          ) t
                    where seqnum = 1;");



                foreach (var movimiento in movimientos)
                {

                    double cantidad = 0.0;


                    var material = this.materialBll.PorIdInsumoYProyecto(movimiento.IdInsumo, movimiento.IdProyecto);

                    var cantidadActual = MaterialACantidadActual(material);



                    cantidad = movimiento.CantidadOriginal + (movimiento.CantidadMovimiento * movimiento.Ingreso);

                    cantidadActual.Cantidad = cantidad;

                    result.Add(cantidadActual);


                }


                return result;

            }


        }

        public IEnumerable<UltimaCatindadItem> UltimasCantidadesOC(int idOrden)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var result = new List<UltimaCatindadItem>();

                var movimientos = connection.Query<MovimientoInventario>(@"select [Id]
      ,[IdMaterial]
      ,[IdOrden]
      ,[IdInsumo]
      ,[Referencia]
      ,[Nombre]
      ,[Descripcion]
      ,[Unidad]
      ,[CantidadOriginal]
      ,[CantidadMovimiento]
      ,[Observaciones]
      ,[ValorUnitario]
      ,[Ingreso]
      ,[TipoMovimiento]
      ,[Fecha]
      ,[Usuario]
      ,[IdFrente]
      ,[IdProyecto]
      ,[NumeroPedido]
      ,[IdProveedor]
      ,[Aprobado]
      ,[Borrado]
                     from (select *,
                                  row_number() over (partition by [IdInsumo] order by fecha desc,  [Id] desc) as seqnum
                           from [dbo].[MovimientoInventario]
	                       where [IdOrden] = " + idOrden + @" and idOrden is not null
                          ) t
                    where seqnum = 1;");



                foreach (var movimiento in movimientos)
                {

                    double cantidad = 0.0;

                    cantidad = movimiento.CantidadOriginal + (movimiento.CantidadMovimiento * movimiento.Ingreso);

                    result.Add(new UltimaCatindadItem() { Id = movimiento.IdMaterial, Cantidad = cantidad });


                }


                return result;

            }
        }


        private static CantidadActual MaterialACantidadActual(Material material)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Material, CantidadActual>();
            });
            var mapper = config.CreateMapper();

            var dest = mapper.Map<Material, CantidadActual>(material);
            return dest;
        }

        private static CantidadActualFrente MaterialACantidadActualFrente(Material material)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Material, CantidadActualFrente>();
            });
            var mapper = config.CreateMapper();

            var dest = mapper.Map<Material, CantidadActualFrente>(material);
            return dest;
        }

        private static OrdenCompraSQLDTO ConvertToSQl(OrdenCompraSQL ordenCompra)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<OrdenCompraSQL, OrdenCompraSQLDTO>();
            });
            var mapper = config.CreateMapper();

            var dest = mapper.Map<OrdenCompraSQL, OrdenCompraSQLDTO>(ordenCompra);
            return dest;
        }

        public IEnumerable<MovimientoInventario> IngresarDesdeOC(IngresoDesdeOC ingresoInventario)
        {

            var result = new List<MovimientoInventario>();

            var oc = this.OcSQLBLL.PorId(ingresoInventario.IdOrdenCompra);

            var fecha = DateTime.Now;
            foreach (IngresoInventario ingreso in ingresoInventario.Items)
            {

                var itemOrdenCompra = this.itemOrdenCompraBLL.PorId(ingreso.IdItemCompra);


                MovimientoInventario movimientoInventario = new MovimientoInventario();

                var material = this.materialBll.PorId(itemOrdenCompra.IdMaterial);



                movimientoInventario.IdMaterial = itemOrdenCompra.IdMaterial;
                movimientoInventario.IdOrden = ingreso.IdOrden;
                movimientoInventario.Referencia = material.Referencia;
                movimientoInventario.Nombre = material.Nombre;
                movimientoInventario.Descripcion = material.Descripcion;
                movimientoInventario.Unidad = itemOrdenCompra.Unidad;
                movimientoInventario.Observaciones = itemOrdenCompra.Observaciones;
                movimientoInventario.ValorUnitario = itemOrdenCompra.ValorUnitario;
                movimientoInventario.Ingreso = 1.0;
                movimientoInventario.TipoMovimiento = TipoMovimientoENUM.IngresoDesdeOC;
                movimientoInventario.Fecha = fecha;
                movimientoInventario.Usuario = ingresoInventario.Usuario;
                movimientoInventario.IdFrente = null;
                movimientoInventario.IdProyecto = ingresoInventario.IdProyecto;
                movimientoInventario.NumeroPedido = string.Empty;
                movimientoInventario.IdProveedor = oc.IdProveedor;
                movimientoInventario.Aprobado = true;
                movimientoInventario.IdInsumo = material.IdInsumo;

                movimientoInventario.Remision = ingresoInventario.Remision;

                movimientoInventario.CantidadOriginal = ObtenerCantidadActual(itemOrdenCompra.IdInsumo, ingresoInventario.IdProyecto);


                movimientoInventario.CantidadMovimiento = ingreso.Cantidad;

                movimientoInventario.Id = this.Insertar(movimientoInventario).Value;


                result.Add(movimientoInventario);

            }


            return result;




        }


        public async Task<MovimientoInventario> IngresarSinOC(IngresoItemSinOC ingreso)
        {
            var material = this.materialBll.PorIdInsumoYProyecto(ingreso.IdInsumo, ingreso.IdProyecto);
            var referencia = await this.referenciaBLL.GetById(ingreso.IdInsumo);
            if (material == null)
            {

                material = new Material()
                {
                    IdInsumo = ingreso.IdInsumo,
                    IdProyecto = ingreso.IdProyecto,
                    IdCategoria = ingreso.IdCategoria,
                    Referencia = referencia.nombre,
                    Nombre = referencia.nombre,
                    Descripcion = referencia.descripcion,
                    Unidad = referencia.unidad
                };

                material.Id = this.materialBll.Insertar(material).Value;
            }



            MovimientoInventario movimientoInventario = new MovimientoInventario();





            movimientoInventario.IdMaterial = material.Id;

            movimientoInventario.Referencia = material.Referencia;
            movimientoInventario.Nombre = material.Nombre;
            movimientoInventario.Descripcion = material.Descripcion;
            movimientoInventario.Unidad = material.Unidad;
            movimientoInventario.Observaciones = ingreso.Observaciones;
            movimientoInventario.ValorUnitario = ingreso.ValorUnitario;
            movimientoInventario.Ingreso = 1.0;
            movimientoInventario.TipoMovimiento = TipoMovimientoENUM.IngresoSinOC;
            movimientoInventario.Fecha = DateTime.Now;
            movimientoInventario.Usuario = ingreso.Usuario;
            movimientoInventario.IdFrente = null;
            movimientoInventario.IdProyecto = ingreso.IdProyecto;
            movimientoInventario.NumeroPedido = ingreso.NumeroPedido;
            //movimientoInventario.IdProveedor = oc.IdProveedor;
            movimientoInventario.Aprobado = true;
            movimientoInventario.IdInsumo = ingreso.IdInsumo;

            movimientoInventario.CantidadOriginal = ObtenerCantidadActual(material.IdInsumo, ingreso.IdProyecto);


            movimientoInventario.NumeroPedido = ingreso.NumeroPedido;



            movimientoInventario.CantidadMovimiento = ingreso.Cantidad;

            movimientoInventario.Id = this.Insertar(movimientoInventario).Value;


            return movimientoInventario;




        }

        private double ObtenerCantidadActual(Guid idInsumo, Guid? idProyecto = null)
        {
            using (var connection = new SqlConnection(_connectionString))
            {


                var movimientos = connection.GetListPaged<MovimientoInventario>(1, 1, "where [IdInsumo] = @idInsumo and [IdProyecto] = @idProyecto", "Fecha desc", new { idInsumo, idProyecto }, null, null);

                if (idProyecto.HasValue == false)
                {
                    movimientos = connection.GetListPaged<MovimientoInventario>(1, 1, "where [IdInsumo] = @idInsumo", "Fecha desc", new { idInsumo, idProyecto }, null, null);

                }
                var movimiento = movimientos.FirstOrDefault();

                double cantidad = 0.0;

                if (movimiento != null)
                {
                    cantidad = movimiento.CantidadOriginal + (movimiento.CantidadMovimiento * movimiento.Ingreso);

                }

                return cantidad;

            }
        }

        public async Task<bool> ReingresoDevolucion(ReingresoDevolucion reingreso)
        {


            foreach (var ingreso in reingreso.Items)
            {

                var material = this.materialBll.PorIdInsumoYProyecto(ingreso.IdInsumo, reingreso.IdProyecto);
                var referencia = await this.referenciaBLL.GetById(ingreso.IdInsumo);
                if (material == null)
                {

                    material = new Material()
                    {
                        IdInsumo = ingreso.IdInsumo,
                        IdProyecto = reingreso.IdProyecto,
                        IdCategoria = material.IdCategoria,
                        Referencia = referencia.nombre,
                        Nombre = referencia.nombre,
                        Descripcion = referencia.descripcion,
                        Unidad = referencia.unidad
                    };

                    material.Id = this.materialBll.Insertar(material).Value;
                }



                MovimientoInventario movimientoInventario = new MovimientoInventario();


                var ultimoMovimiento = this.Filtrar(new { IdInsumo = ingreso.IdInsumo, Ingreso = 1 }).OrderByDescending(x => x.Fecha).FirstOrDefault(x => x.ValorUnitario != 0);



                movimientoInventario.IdMaterial = material.Id;

                movimientoInventario.Referencia = material.Referencia;
                movimientoInventario.Nombre = material.Nombre;
                movimientoInventario.Descripcion = material.Descripcion;
                movimientoInventario.Unidad = material.Unidad;
                //movimientoInventario.Observaciones = ingreso.Observaciones;
                if (ultimoMovimiento != null)
                {
                    movimientoInventario.ValorUnitario = ultimoMovimiento.ValorUnitario;//ingreso.ValorUnitario;

                }
                movimientoInventario.Ingreso = 1.0;
                movimientoInventario.TipoMovimiento = TipoMovimientoENUM.ReingresoDevolucion;
                movimientoInventario.Fecha = DateTime.Now;
                movimientoInventario.Usuario = reingreso.Usuario;
                movimientoInventario.IdFrente = reingreso.IdFrente;
                movimientoInventario.IdProyecto = reingreso.IdProyecto;
                //movimientoInventario.NumeroPedido = ingreso.NumeroPedido;
                //movimientoInventario.IdProveedor = oc.IdProveedor;
                movimientoInventario.Aprobado = true;
                movimientoInventario.IdInsumo = ingreso.IdInsumo;

                movimientoInventario.CantidadOriginal = ObtenerCantidadActual(material.IdInsumo, reingreso.IdProyecto);


                //movimientoInventario.NumeroPedido = ingreso.NumeroPedido;



                movimientoInventario.CantidadMovimiento = ingreso.Cantidad;

                movimientoInventario.Id = this.Insertar(movimientoInventario).Value;



            }

           return true;

        }
    }



}
