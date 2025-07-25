using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
    public class InformacionContable
    {

        public double APagar { get; set; }
        public double Anticipo { get; set; }
        public bool? esAnticipo { get; set; }
        public double Base { get; set; }
        public double Iva { get; set; }
        public double OtrosDescuentos { get; set; }
        public double RetFte { get; set; }
        public double RetGarantia { get; set; }
        public double RetGtia { get; set; }
        public double Tarifa { get; set; }


        public double PorcentajeICA { get; set; }
        public double ValorICA { get; set; }
        public double Base2 { get; set; }
        public double RetFte2 { get; set; }
        public double Tarifa2 { get; set; }

        public string usuario { get; set; }


        public bool EsDocumentoSoporte { get; set; }
        public DatosContables[] ToDatosContables()
        {

            var result = new List<DatosContables>();


            result.Add(new DatosContables() { codigo = "Base", valor = this.Base.ToString("0.0") });
            result.Add(new DatosContables() { codigo = "Iva", valor = this.Iva.ToString("0.0") });
            result.Add(new DatosContables() { codigo = "Tarifa ", valor = this.Tarifa.ToString("0.0") });
            result.Add(new DatosContables() { codigo = "Ret. Fte", valor = this.RetFte.ToString("0.0") });

            result.Add(new DatosContables() { codigo = "Base 2", valor = this.Base2.ToString("0.0") });
            result.Add(new DatosContables() { codigo = "Ret. Fte 2", valor = this.RetFte2.ToString("0.0") });
            result.Add(new DatosContables() { codigo = "Tarifa 2", valor = this.Tarifa2.ToString("0.0") });

            result.Add(new DatosContables() { codigo = "% Ret garantía", valor = this.RetGarantia.ToString("0.0") });
            result.Add(new DatosContables() { codigo = "Ret. Gtia", valor = this.RetGtia.ToString("0.0") });
            result.Add(new DatosContables() { codigo = "Anticipo ", valor = this.Anticipo.ToString("0.0") });
            result.Add(new DatosContables() { codigo = "Otros Descuentos", valor = this.OtrosDescuentos.ToString("0.0") });
            result.Add(new DatosContables() { codigo = "A Pagar", valor = this.APagar.ToString("0.0") });


            result.Add(new DatosContables() { codigo = "Porcentaje ICA", valor = this.PorcentajeICA.ToString("0.0") });
            result.Add(new DatosContables() { codigo = "Valor ICA", valor = this.ValorICA.ToString("0.0") });

           


            return result.ToArray();
        }

        internal List<DatosContables> ToDatosContablesFormat()
        {
            var result = new List<DatosContables>();

            result.Add(new DatosContables() { codigo = "Base", valor = this.Base.ToString("#,##0.00") });
            result.Add(new DatosContables() { codigo = "Iva", valor = this.Iva.ToString("#,##0.00") });
            result.Add(new DatosContables() { codigo = "Tarifa ", valor = this.Tarifa.ToString("0.0") + "%" });
            result.Add(new DatosContables() { codigo = "Ret. Fte", valor = this.RetFte.ToString("#,##0.00") });

            result.Add(new DatosContables() { codigo = "Base 2", valor = this.Base2.ToString("#,##0.00") });
            result.Add(new DatosContables() { codigo = "Ret. Fte 2", valor = this.RetFte2.ToString("#,##0.00") });
            result.Add(new DatosContables() { codigo = "Tarifa 2", valor = this.Tarifa2.ToString("0.0") + "%" });

            result.Add(new DatosContables() { codigo = "Porcentaje ICA", valor = this.PorcentajeICA.ToString("0.0") + "%" });
            result.Add(new DatosContables() { codigo = "Valor ICA", valor = this.ValorICA.ToString("#,##0.00") });

            result.Add(new DatosContables() { codigo = "% Ret garantía", valor = this.RetGarantia.ToString("0.0") + "%" });
            result.Add(new DatosContables() { codigo = "Ret. Gtia", valor = this.RetGtia.ToString("#,##0.00") });
            result.Add(new DatosContables() { codigo = "Anticipo ", valor = this.Anticipo.ToString("#,##0.00") });
            result.Add(new DatosContables() { codigo = "Otros Descuentos", valor = this.OtrosDescuentos.ToString("#,##0.00") });
            result.Add(new DatosContables() { codigo = "A Pagar", valor = this.APagar.ToString("#,##0.00") });

           
           

            return result;

        }
    }
}
