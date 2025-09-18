using Azure.Identity;
using Microsoft.Graph;
using System.Security.Cryptography.X509Certificates;
using ConvertApiDotNet;
using SharepointAPI_Net8.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

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

        public async Task<string> GenerarPDFAsync(string sharePointLink)
        {
            try
            {
                _logger.LogInformation("📄 Iniciando conversión DOCX → PDF para: {Link}", sharePointLink);
                
                // 1. Descargar archivo DOCX desde SharePoint
                byte[] docxBytes = await DescargarArchivoAsync(sharePointLink);
                _logger.LogInformation("📥 Archivo DOCX descargado: {Size} bytes", docxBytes.Length);
                
                // 2. Convertir DOCX a PDF usando ConvertAPI
                byte[] pdfBytes = await ConvertirDocxAPdfAsync(docxBytes);
                _logger.LogInformation("🔄 Conversión completada: {Size} bytes PDF", pdfBytes.Length);
                
                // 3. Generar nombre de archivo PDF con timestamp
                string pdfFileName = $"Contrato{DateTime.Now:dd-MM-yyyy-HH_mm_ss}.pdf";
                
                // 4. Subir PDF a SharePoint
                var links = await UploadFilesAsync(pdfBytes, pdfFileName);
                _logger.LogInformation("✅ PDF subido exitosamente: {FileName}", pdfFileName);
                
                return links.ShareURL;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al generar PDF: {Message}", ex.Message);
                throw;
            }
        }

        private async Task<byte[]> DescargarArchivoAsync(string sharePointLink)
        {
            // Configurar autenticación
            var tenantId = _configuration["AzureAd:TenantId"] ?? throw new ArgumentException("AzureAd:TenantId no configurado");
            var clientId = _configuration["AzureAd:ClientId"] ?? throw new ArgumentException("AzureAd:ClientId no configurado");
            var certificatePath = _configuration["AzureAd:CertificatePath"] ?? throw new ArgumentException("AzureAd:CertificatePath no configurado");
            var certificatePassword = _configuration["AzureAd:CertificatePassword"] ?? throw new ArgumentException("AzureAd:CertificatePassword no configurado");
            
            var certificate = new X509Certificate2(certificatePath, certificatePassword);
            var credential = new ClientCertificateCredential(tenantId, clientId, certificate);
            var graphServiceClient = new GraphServiceClient(credential);
            
            // Extraer información del link de SharePoint
            // Link ejemplo: https://paeciaconstructora.sharepoint.com/sites/SGD/_layouts/15/Doc.aspx?sourcedoc={GUID}&file=Contrato.docx
            var uri = new Uri(sharePointLink);
            var fileName = System.Web.HttpUtility.ParseQueryString(uri.Query)["file"];
            
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("No se pudo extraer el nombre del archivo del link de SharePoint");
            }
            
            // Obtener el sitio
            var site = await graphServiceClient.Sites["paeciaconstructora.sharepoint.com:/sites/SGD"].GetAsync();
            
            // Buscar la biblioteca "Proyectos"
            var library = _configuration["SharePoint:Library"] ?? "Proyectos";
            var drives = await graphServiceClient.Sites[site.Id].Drives.GetAsync();
            var targetDrive = drives?.Value?.FirstOrDefault(d => d.Name == library);
            
            if (targetDrive == null)
            {
                throw new Exception($"No se encontró la biblioteca '{library}' en SharePoint");
            }
            
            // Buscar el archivo en la carpeta "Contratos"
            var folderPath = _configuration["SharePoint:PrincipalFolder"] ?? "Contratos";
            var fileItem = await graphServiceClient.Drives[targetDrive.Id]
                .Root
                .ItemWithPath($"{folderPath}/{fileName}")
                .GetAsync();
                
            // Descargar contenido del archivo
            var fileStream = await graphServiceClient.Drives[targetDrive.Id]
                .Items[fileItem.Id]
                .Content
                .GetAsync();
                
            using var memoryStream = new MemoryStream();
            await fileStream.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }

        private async Task<byte[]> ConvertirDocxAPdfAsync(byte[] docxBytes)
        {
            _logger.LogInformation("🔄 Iniciando conversión DOCX → PDF con LibreOffice");
            
            // 1. Crear archivos temporales
            string tempDir = Path.GetTempPath();
            string docxFile = Path.Combine(tempDir, $"temp_{Guid.NewGuid()}.docx");
            string pdfFile = Path.ChangeExtension(docxFile, ".pdf");
            
            try
            {
                // 2. Escribir DOCX a archivo temporal
                await File.WriteAllBytesAsync(docxFile, docxBytes);
                _logger.LogInformation("📝 Archivo DOCX temporal creado: {DocxFile}", docxFile);
                
                // 3. Ejecutar LibreOffice para conversión
                var processInfo = new ProcessStartInfo
                {
                    FileName = "libreoffice",
                    Arguments = $"--headless --convert-to pdf \"{docxFile}\" --outdir \"{tempDir}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                
                _logger.LogInformation("🚀 Ejecutando LibreOffice: {Command} {Arguments}", processInfo.FileName, processInfo.Arguments);
                
                using var process = Process.Start(processInfo);
                if (process == null)
                {
                    throw new Exception("No se pudo iniciar el proceso LibreOffice");
                }
                
                await process.WaitForExitAsync();
                
                var stdout = await process.StandardOutput.ReadToEndAsync();
                var stderr = await process.StandardError.ReadToEndAsync();
                
                _logger.LogInformation("📋 LibreOffice stdout: {Stdout}", stdout);
                if (!string.IsNullOrEmpty(stderr))
                {
                    _logger.LogWarning("⚠️ LibreOffice stderr: {Stderr}", stderr);
                }
                
                if (process.ExitCode != 0)
                {
                    throw new Exception($"LibreOffice falló con código {process.ExitCode}. Error: {stderr}");
                }
                
                // 4. Verificar que el PDF fue creado
                if (!File.Exists(pdfFile))
                {
                    throw new Exception($"LibreOffice no generó el archivo PDF esperado: {pdfFile}");
                }
                
                // 5. Leer PDF generado
                byte[] pdfBytes = await File.ReadAllBytesAsync(pdfFile);
                _logger.LogInformation("✅ PDF generado exitosamente: {Size} bytes", pdfBytes.Length);
                
                return pdfBytes;
            }
            finally
            {
                // 6. Limpiar archivos temporales
                try
                {
                    if (File.Exists(docxFile)) 
                    {
                        File.Delete(docxFile);
                        _logger.LogInformation("🗑️ Archivo DOCX temporal eliminado");
                    }
                    if (File.Exists(pdfFile)) 
                    {
                        File.Delete(pdfFile);
                        _logger.LogInformation("🗑️ Archivo PDF temporal eliminado");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "⚠️ Error al limpiar archivos temporales");
                }
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
