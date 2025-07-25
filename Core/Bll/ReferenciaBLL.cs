using ClosedXML.Excel;
using Cobalto.Mongo.Core.BLL;
using Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Core.Bll
{
    public class ReferenciaBLL : BaseBLL<Referencia>
    {
        private CategoriaBLL categoriaBLL;

        public ReferenciaBLL(IConfiguration configuration, IHttpContextAccessor httpContext, CategoriaBLL categoriaBLL) : base(configuration, httpContext)
        {
            this.categoriaBLL = categoriaBLL;
        }


        public async Task<IEnumerable<Referencia>> PorCategoria(Guid idCategoria)
        {

            var referencias = await this.GetAll();


            var query = from r in referencias
                        from c in r.categorias
                        where c.id == idCategoria
                        select r;

            return query;

        }


        public async Task< List<string>> CrearItems(MemoryStream ms)
        {
            List<string> logsCargaItem = new List<string>();
            var registrosCarga = await ConvertirArchivoExcelADataTable(ms, logsCargaItem);

            await InsertarItemsMasivamente(registrosCarga);
            //ActualizarProyectoGestionCompras(idProyecto);
            return logsCargaItem;
        }

        private async Task InsertarItemsMasivamente(List<Referencia> registrosCarga)
        {
            var referencias = await this.GetAll();

            foreach (var item in registrosCarga)
            {
                var itemExistente = referencias.FirstOrDefault(x => x.codigo.ToLower() == item.codigo.ToLower());

                if (itemExistente == null)
                {
                    await this.Insert(item);
                }
                else
                {
                    itemExistente.nombre = item.nombre;
                    itemExistente.descripcion = item.descripcion;
                    itemExistente.valorUnitario = item.valorUnitario;
                    itemExistente.unidad = item.unidad;
                    itemExistente.ProcentajeIva = item.ProcentajeIva;

                    await this.Update(itemExistente);

                }
            }
        }

        public async Task<List<Referencia>> ConvertirArchivoExcelADataTable(Stream archivoStream, List<string> logs)
        {

            List<Referencia> dataTable = new List<Referencia>();
            var categorias = await this.categoriaBLL.GetAll();

            //agregarColumnas(dataTable);

            using (XLWorkbook libroExcel = new XLWorkbook(archivoStream))
            {

                IXLWorksheet hojaExcel = libroExcel.Worksheet(1);

                //celda.IsEmpty

                foreach (IXLRow fila in hojaExcel.Rows(2, hojaExcel.RangeUsed().RowCount()))
                {
                    AgregarFila(fila, dataTable, logs, categorias);
                }
            }


            return dataTable;

        }


        private async void AgregarFila(IXLRow fila, List<Referencia> table, List<string> logs, List<Categoria> categorias)
        {
            List<string> logsLocales = new List<string>();

            var numFila = fila.RowNumber();
            double cantidad = 0;


            var nuevaFila = new Referencia();


            double valorUnitario = 0;
            var valorUnitarioExcel = fila.Cell(5).Value.ToString().Trim();
            nuevaFila.valorUnitario = valorUnitarioExcel;

            nuevaFila.codigo = fila.Cell(1).Value.ToString().Trim();
            nuevaFila.nombre = fila.Cell(2).Value.ToString().Trim();
            nuevaFila.descripcion = fila.Cell(3).Value.ToString().Trim();
            nuevaFila.unidad = fila.Cell(4).Value.ToString().Trim();
            nuevaFila.ProcentajeIva = double.Parse(fila.Cell(7).Value.ToString().Trim());

            var splitedCat = fila.Cell(6).Value.ToString().Trim().Split(",");


            var listCategorias = new List<Categoria>();

            foreach (var item in splitedCat)
            {
                var cat = categorias.FirstOrDefault(x => x.Name.ToLower().Trim() == item.ToLower().Trim());

                if (cat != null)
                {
                    listCategorias.Add(cat);
                }
                else
                {
                    cat = await this.categoriaBLL.Insert(new Categoria()
                    {
                        erased = false,
                        Name = item
                    });
                    listCategorias.Add(cat);

                }
            }





            nuevaFila.categorias = listCategorias;




            if (!logsLocales.Any())
            {
                table.Add(nuevaFila);
            }
            else
            {
                if (logsLocales.Count() < 7)
                {
                    logs.AddRange(logsLocales);
                }
            }






        }
    }
}
