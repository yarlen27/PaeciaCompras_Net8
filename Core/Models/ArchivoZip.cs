using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
    public class ArchivoZip
    {
        public string Base64Archivo { get; set; }
        public string NombreArchivo { get; set; }
    }

    public class ArchivoZipSeparado
    {
        public string NombreArchivoPdf { get; set; }
        public string Base64StringPdf { get; set; }
        public string NombreArchivoXml { get; set; }
        public string Base64StringXml { get; set; }

    }
}
