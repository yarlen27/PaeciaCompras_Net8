using System;
using System.IO;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;

class Program
{
    static void Main()
    {
        string inputPdf = "original.pdf";
        string outputPdf = "pdf_con_tabla.pdf";

        // 1️⃣ Abrir PDF original
        PdfDocument pdfDoc = new PdfDocument(new PdfReader(inputPdf), new PdfWriter(outputPdf));

        int numPages = pdfDoc.GetNumberOfPages();

        // 2️⃣ Crear tabla (iText Layout)
        Table table = new Table(UnitValue.CreatePercentArray(new float[] { 2, 1, 1 }))
            .UseAllAvailableWidth();

        // Encabezado
        table.AddHeaderCell("Producto");
        table.AddHeaderCell("Cantidad");
        table.AddHeaderCell("Precio");

        // Filas
        table.AddCell("Manzanas");
        table.AddCell("5");
        table.AddCell("$10");

        table.AddCell("Naranjas");
        table.AddCell("8");
        table.AddCell("$15");

        // 3️⃣ Superponer la tabla en cada página
        for (int i = 1; i <= numPages; i++)
        {
            var pdfPage = pdfDoc.GetPage(i);
            var canvas = new iText.Layout.Canvas(new PdfCanvas(pdfPage), pdfPage.GetPageSize());

            // Posicionar la tabla en la parte superior
            canvas.ShowTextAligned(new Paragraph("Encabezado opcional"), 
                297.5f, pdfPage.GetPageSize().GetTop() - 20, i, 
                iText.Layout.Properties.TextAlignment.CENTER, 
                iText.Layout.Properties.VerticalAlignment.TOP, 0);

            canvas.Add(table);
            canvas.Close();
        }

        pdfDoc.Close();
        Console.WriteLine("✅ PDF generado: " + outputPdf);
    }
}
