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
                _logger.LogInformation("🔑 CONTRASEÑA USADA: '{CertificatePassword}'", certificatePassword);
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
                
                // Si es error de certificado, incluir la contraseña en el mensaje
                if (ex.Message.Contains("certificate data cannot be read") || ex.Message.Contains("password"))
                {
                    var certificatePassword = _configuration["AzureAd:CertificatePassword"];
                    throw new Exception($"{ex.Message} - PASSWORD USADA: '{certificatePassword}'");
                }
                
                throw;
            }
        }

        public async Task<Links> GenerarContratoAsync(string base64Content)
        {
            try
            {
                _logger.LogInformation("📄 Iniciando generación de contrato...");
                
                // Convertir base64 a bytes
                byte[] fileBytes = Convert.FromBase64String(base64Content);
                _logger.LogInformation("📝 Archivo convertido desde base64, tamaño: {Size} bytes", fileBytes.Length);
                
                // Crear nombre de archivo con timestamp (igual que el original)
                string fileName = $"Contrato-{DateTime.Now:dd-MM-yyyy-hh-mm-ss}.docx";
                _logger.LogInformation("📁 Nombre de archivo: {FileName}", fileName);
                
                // Configurar autenticación
                var tenantId = _configuration["AzureAd:TenantId"] ?? throw new ArgumentException("AzureAd:TenantId no configurado");
                var clientId = _configuration["AzureAd:ClientId"] ?? throw new ArgumentException("AzureAd:ClientId no configurado");
                var certificatePath = _configuration["AzureAd:CertificatePath"] ?? throw new ArgumentException("AzureAd:CertificatePath no configurado");
                var certificatePassword = _configuration["AzureAd:CertificatePassword"] ?? throw new ArgumentException("AzureAd:CertificatePassword no configurado");
                
                var certificate = new X509Certificate2(certificatePath, certificatePassword);
                var credential = new ClientCertificateCredential(tenantId, clientId, certificate);
                var graphServiceClient = new GraphServiceClient(credential);
                
                // Obtener el sitio
                var site = await graphServiceClient.Sites["paeciaconstructora.sharepoint.com:/sites/SGD"].GetAsync();
                _logger.LogInformation("🌐 Sitio obtenido: {SiteId}", site.Id);
                
                // Buscar la biblioteca "Proyectos" 
                var library = _configuration["SharePoint:Library"] ?? "Proyectos";
                var drives = await graphServiceClient.Sites[site.Id].Drives.GetAsync();
                var targetDrive = drives?.Value?.FirstOrDefault(d => d.Name == library);
                
                if (targetDrive == null)
                {
                    throw new Exception($"No se encontró la biblioteca '{library}' en SharePoint");
                }
                
                _logger.LogInformation("📚 Biblioteca encontrada: {LibraryName} (ID: {DriveId})", targetDrive.Name, targetDrive.Id);
                
                // Buscar/crear carpeta "Contratos"
                var principalFolder = _configuration["SharePoint:PrincipalFolder"] ?? "Contratos";
                var folderPath = principalFolder;
                
                // Subir archivo usando Microsoft Graph
                using var fileStream = new MemoryStream(fileBytes);
                var uploadedFile = await graphServiceClient.Drives[targetDrive.Id]
                    .Root
                    .ItemWithPath($"{folderPath}/{fileName}")
                    .Content
                    .PutAsync(fileStream);
                
                _logger.LogInformation("✅ Archivo subido exitosamente");
                _logger.LogInformation("🔗 URL: {WebUrl}", uploadedFile?.WebUrl);
                
                // Crear objeto de respuesta similar al original
                var links = new Links
                {
                    ShareURL = uploadedFile?.WebUrl ?? "",
                    LinkingURL = uploadedFile?.WebUrl ?? ""
                };
                
                _logger.LogInformation("🎉 Contrato generado exitosamente: {FileName}", fileName);
                return links;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al generar contrato: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<Links> UploadFilesAsync(byte[] fileBytes, string fileName)
        {
            try
            {
                _logger.LogInformation("📁 Iniciando subida de archivo: {FileName}", fileName);
                
                // Configurar autenticación
                var tenantId = _configuration["AzureAd:TenantId"] ?? throw new ArgumentException("AzureAd:TenantId no configurado");
                var clientId = _configuration["AzureAd:ClientId"] ?? throw new ArgumentException("AzureAd:ClientId no configurado");
                var certificatePath = _configuration["AzureAd:CertificatePath"] ?? throw new ArgumentException("AzureAd:CertificatePath no configurado");
                var certificatePassword = _configuration["AzureAd:CertificatePassword"] ?? throw new ArgumentException("AzureAd:CertificatePassword no configurado");
                
                var certificate = new X509Certificate2(certificatePath, certificatePassword);
                var credential = new ClientCertificateCredential(tenantId, clientId, certificate);
                var graphServiceClient = new GraphServiceClient(credential);
                
                // Obtener el sitio
                var site = await graphServiceClient.Sites["paeciaconstructora.sharepoint.com:/sites/SGD"].GetAsync();
                _logger.LogInformation("🌐 Sitio obtenido: {SiteId}", site.Id);
                
                // Buscar la biblioteca "Proyectos"
                var library = _configuration["SharePoint:Library"] ?? "Proyectos";
                var drives = await graphServiceClient.Sites[site.Id].Drives.GetAsync();
                var targetDrive = drives?.Value?.FirstOrDefault(d => d.Name == library);
                
                if (targetDrive == null)
                {
                    throw new Exception($"No se encontró la biblioteca '{library}' en SharePoint");
                }
                
                _logger.LogInformation("📚 Biblioteca encontrada: {LibraryName} (ID: {DriveId})", targetDrive.Name, targetDrive.Id);
                
                // Buscar/crear carpeta "Contratos"
                var principalFolder = _configuration["SharePoint:PrincipalFolder"] ?? "Contratos";
                var folderPath = principalFolder;
                
                // Subir archivo usando Microsoft Graph
                using var fileStream = new MemoryStream(fileBytes);
                var uploadedFile = await graphServiceClient.Drives[targetDrive.Id]
                    .Root
                    .ItemWithPath($"{folderPath}/{fileName}")
                    .Content
                    .PutAsync(fileStream);
                
                _logger.LogInformation("✅ Archivo subido exitosamente");
                _logger.LogInformation("🔗 URL: {WebUrl}", uploadedFile?.WebUrl);
                
                // Crear objeto de respuesta
                var links = new Links
                {
                    ShareURL = uploadedFile?.WebUrl ?? "",
                    LinkingURL = uploadedFile?.WebUrl ?? ""
                };
                
                _logger.LogInformation("🎉 Archivo subido exitosamente: {FileName}", fileName);
                return links;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al subir archivo: {Message}", ex.Message);
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
