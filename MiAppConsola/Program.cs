using Azure.Identity;
using Microsoft.Graph;
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        try
        {
            Console.WriteLine("🚀 Iniciando conexión a SharePoint con Microsoft Graph API...");
            
            // Configuración
            var tenantId = "c841d196-d1a6-4064-a0e0-9878c15cb140";
            var clientId = "b5326f32-839e-4730-b094-52a71f3d5f37";
            var certificatePath = "graphapi-certificate.pfx";
            var certificatePassword = "GraphAPI2024";
            
            Console.WriteLine("🔐 Cargando certificado...");
            var certificate = new X509Certificate2(certificatePath, certificatePassword);
            
            Console.WriteLine("🔑 Creando ClientCertificateCredential...");
            var credential = new ClientCertificateCredential(tenantId, clientId, certificate);
            
            Console.WriteLine("📡 Creando GraphServiceClient...");
            var graphServiceClient = new GraphServiceClient(credential);
            
            Console.WriteLine("🌐 Conectando a SharePoint...");
            var site = await graphServiceClient.Sites["paeciaconstructora.sharepoint.com:/sites/SGD"].GetAsync();
            
            Console.WriteLine("✅ ¡CONECTADO EXITOSAMENTE!");
            Console.WriteLine($"📝 Título del sitio: {site.DisplayName}");
            Console.WriteLine($"🔗 URL: {site.WebUrl}");
            Console.WriteLine($"🆔 ID: {site.Id}");
            Console.WriteLine($"📄 Descripción: {site.Description}");
            Console.WriteLine($"📅 Creado: {site.CreatedDateTime}");
            Console.WriteLine($"🔄 Modificado: {site.LastModifiedDateTime}");
            
            Console.WriteLine("\n📋 === LISTAS DE SHAREPOINT ===");
            var lists = await graphServiceClient.Sites[site.Id].Lists.GetAsync();
            if (lists?.Value != null)
            {
                foreach (var list in lists.Value)
                {
                    Console.WriteLine($"📝 {list.DisplayName} - {list.Description}");
                    Console.WriteLine($"   🆔 ID: {list.Id}");
                    Console.WriteLine();
                }
            }
            
            Console.WriteLine("\n💾 === DRIVES/BIBLIOTECAS ===");
            var drives = await graphServiceClient.Sites[site.Id].Drives.GetAsync();
            if (drives?.Value != null)
            {
                foreach (var drive in drives.Value)
                {
                    Console.WriteLine($"📁 {drive.Name} - {drive.Description}");
                    Console.WriteLine($"   🆔 ID: {drive.Id}");
                    Console.WriteLine($"   💿 Tipo: {drive.DriveType}");
                    Console.WriteLine($"   📊 Usado: {drive.Quota?.Used}/{drive.Quota?.Total} bytes");
                    Console.WriteLine();
                }
            }
            
            Console.WriteLine("\n🌳 === SUBSITIOS ===");
            var subsites = await graphServiceClient.Sites[site.Id].Sites.GetAsync();
            if (subsites?.Value != null && subsites.Value.Any())
            {
                foreach (var subsite in subsites.Value)
                {
                    Console.WriteLine($"🏠 {subsite.DisplayName}");
                    Console.WriteLine($"   🔗 URL: {subsite.WebUrl}");
                    Console.WriteLine();
                }
            }
            else
            {
                Console.WriteLine("No hay subsitios");
            }
            
            Console.WriteLine("\n📁 === ARCHIVOS EN DRIVE PRINCIPAL ===");
            var mainDrive = drives?.Value?.FirstOrDefault();
            if (mainDrive != null)
            {
                try
                {
                    var items = await graphServiceClient.Drives[mainDrive.Id].Items["root"].Children.GetAsync();
                    if (items?.Value != null)
                    {
                        foreach (var item in items.Value.Take(10)) // Solo primeros 10
                        {
                            var type = item.Folder != null ? "📁" : "📄";
                            Console.WriteLine($"{type} {item.Name}");
                            Console.WriteLine($"   📅 Modificado: {item.LastModifiedDateTime}");
                            if (item.Size.HasValue)
                                Console.WriteLine($"   📏 Tamaño: {item.Size} bytes");
                            Console.WriteLine();
                        }
                    }
                }
                catch (Exception driveEx)
                {
                    Console.WriteLine($"❌ Error accediendo a archivos: {driveEx.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            Console.WriteLine($"📋 Tipo: {ex.GetType().Name}");
            Console.WriteLine($"📍 Stack: {ex.StackTrace}");
            
            if (ex.InnerException != null)
            {
                Console.WriteLine($"🔍 Inner Exception: {ex.InnerException.Message}");
            }
        }
        
        Console.WriteLine("\n🏁 Presiona cualquier tecla para salir...");
        Console.ReadKey();
    }
}