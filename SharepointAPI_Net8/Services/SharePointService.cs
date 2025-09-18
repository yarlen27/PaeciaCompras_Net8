using Azure.Identity;
using Microsoft.Graph;
using System.Security.Cryptography.X509Certificates;
using ConvertApiDotNet;
using SharepointAPI_Net8.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SharepointAPI_Net8.Services
{
    public class SharePointService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SharePointService> _logger;

        public SharePointService(IConfiguration configuration, ILogger<SharePointService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> GetSiteNameAsync()
        {
            try
            {
                _logger.LogInformation("🚀 Iniciando conexión a SharePoint con Microsoft Graph API...");
                
                // Usar EXACTAMENTE la misma configuración que funciona
                var tenantId = _configuration["AzureAd:TenantId"] ?? throw new ArgumentException("AzureAd:TenantId no configurado");
                var clientId = _configuration["AzureAd:ClientId"] ?? throw new ArgumentException("AzureAd:ClientId no configurado");
                var certificatePath = _configuration["AzureAd:CertificatePath"] ?? throw new ArgumentException("AzureAd:CertificatePath no configurado");
                var certificatePassword = _configuration["AzureAd:CertificatePassword"] ?? throw new ArgumentException("AzureAd:CertificatePassword no configurado");
                
                _logger.LogInformation("🔐 Cargando certificado: {CertificatePath}", certificatePath);
                var certificate = new X509Certificate2(certificatePath, certificatePassword);
                
                _logger.LogInformation("🔑 Creando ClientCertificateCredential...");
                var credential = new ClientCertificateCredential(tenantId, clientId, certificate);
                
                _logger.LogInformation("📡 Creando GraphServiceClient...");
                var graphServiceClient = new GraphServiceClient(credential);
                
                _logger.LogInformation("🌐 Conectando a SharePoint...");
                var site = await graphServiceClient.Sites["paeciaconstructora.sharepoint.com:/sites/SGD"].GetAsync();
                
                _logger.LogInformation("✅ ¡CONECTADO EXITOSAMENTE!");
                _logger.LogInformation("📝 Título del sitio: {DisplayName}", site.DisplayName);
                _logger.LogInformation("🔗 URL: {WebUrl}", site.WebUrl);
                _logger.LogInformation("🆔 ID: {Id}", site.Id);
                
                return site.DisplayName ?? "Sitio sin nombre";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al conectar con SharePoint: {Message}", ex.Message);
                throw;
            }
        }
    }

    public class Links
    {
        public string ShareURL { get; set; } = string.Empty;
        public string LinkingURL { get; set; } = string.Empty;
    }
}

//Application (client) ID: a9926836-1318-4207-8e5a-db00e6bc0266
//Directory (tenant) ID: c841d196-d1a6-4064-a0e0-9878c15cb140
