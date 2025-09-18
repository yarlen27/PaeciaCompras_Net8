namespace SharepointAPI_Net8.Models
{
    public class PedidoServicio 
    {
        public string base64 { get; set; } = string.Empty;
    }

    public class ResultadoPDF
    {
        public string Link { get; set; } = string.Empty;
    }

    public class PdfContrato
    {
        public string Name { get; set; } = string.Empty;
        public string Base64 { get; set; } = string.Empty;
    }
}