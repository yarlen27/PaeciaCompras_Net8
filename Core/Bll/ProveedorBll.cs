using Cobalto.Mongo.Core.BLL;
using Core.Models;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using RestSharp;
using SendGrid.Helpers.Mail;
using SendGrid;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Core.BLL
{
    public partial class ProveedorBLL : BaseBLL<Proveedor>
    {
        

        public ProveedorBLL(IConfiguration configuration, IHttpContextAccessor httpContext) : base(configuration, httpContext)
        {
          
        }

        public async Task<Guid> UploadFile(Guid id, Adjunto archivo)
        {
            var proveedor = await this.GetById(id);

            if (proveedor.adjuntos == null)
            {
                proveedor.adjuntos = new List<Adjunto>();

            }
            proveedor.adjuntos.Add(archivo);


            await this.Update(proveedor);
            return id;

        }


        public override Task<Proveedor> Insert(Proveedor item)
        {


            return base.Insert(item);
        }


        public async Task<List<Proveedor>> PorNITs(string nit)
        {

            var filter = Builders<Proveedor>.Filter.Eq("nit", nit);
            var r = await this.GetByProterty(filter);


            var result = r.FirstOrDefault(x => x.erased == false);
            if (r.Count > 1)
            {
                for (int i = 1; i < r.Count(); i++)
                {
                    if (r[i].adjuntos != null)
                    {
                        if (result.adjuntos == null)
                        {
                            result.adjuntos = new List<Adjunto>();
                        }

                        result.adjuntos.AddRange(r[i].adjuntos);
                    }
                }
            }


            return r;
        }


        public async Task<Proveedor> PorNIT(string nit)
        {

            var filter = Builders<Proveedor>.Filter.Eq("nit", nit);
            var r = await this.GetByProterty(filter);


            var result = r.FirstOrDefault(x => x.erased == false);
            if (r.Count > 1)
            {
                for (int i = 1; i < r.Count(); i++)
                {
                    if (r[i].adjuntos != null)
                    {
                        if (result.adjuntos == null)
                        {
                            result.adjuntos = new List<Adjunto>();
                        }

                        result.adjuntos.AddRange(r[i].adjuntos);
                    }
                }
            }


            return result;
        }

        public async Task<bool> NIT(string nit)
        {

            var filter = Builders<Proveedor>.Filter.Eq("nit", nit);
            var r = await this.GetByProterty(filter);


            var result = r.FirstOrDefault(x => x.erased == false);

            return result != null;
        }

        public async Task<List<Proveedor>> PorLista(IEnumerable<Guid> lista)
        {

            var filter = Builders<Proveedor>.Filter.In("id", lista);
            var r = await this.GetByProterty(filter);


            return r;
        }


        public async Task<Proveedor> CrearDesdeMtto(int id)
        {
            var urlMtto = this.configuration.GetValue(typeof(string), "urlMtto");


            var client = new RestClient($"{urlMtto}");
            var request = new RestRequest($"api/proveedores/{id}", Method.Get);
            var response = client.Execute<ProveedorMtto>(request);



            var proveedorMtto = JsonConvert.DeserializeObject<ProveedorMtto>(response.Content);

            var proveedor = await this.PorNIT(proveedorMtto.NIT);

            if (proveedor == null)
            {
                proveedor = await this.Insert(new Proveedor()
                {
                    erased = false,
                    email = proveedorMtto.Email,
                    nit = proveedorMtto.NIT,
                    nombre = proveedorMtto.Nombre,
                    telefono = proveedorMtto.Telefono,
                    contacto = proveedorMtto.Contacto
                });

            }

            return proveedor;

        }



        public async void EvaluarSeleccion(Guid idCompras)
        {
            var proveedor = await this.GetById(idCompras);
            proveedor.evaluacionSeleccion = true;
            await this.Update(proveedor);
        }
        private static readonly string key = "6gmulF3xcSLA3mDfsYZU1PNMwamaecx5"; // Debe ser de 32 caracteres para AES-256

        public async Task<string> GenerarOTP(string nit)
        {
            var logPath = "C:\\tmp\\proveedor_otp_bll_debug.log";
            try
            {
                await File.AppendAllTextAsync(logPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] INICIO GenerarOTP BLL - NIT: {nit}\n");

                var proveedores = await this.PorNITs(nit);
                await File.AppendAllTextAsync(logPath, $"DEBUG: Proveedores encontrados: {proveedores.Count}\n");

                if (proveedores.Count == 0)
                {
                    await File.AppendAllTextAsync(logPath, $"ERROR: Proveedor no encontrado para NIT: {nit}\n");
                    throw new Exception("Proveedor no encontrado");
                }
                
                var email = proveedores.First().email?.Split(";")[0];
                await File.AppendAllTextAsync(logPath, $"DEBUG: Email del proveedor: {email}\n");
                
                //creamos un codigo de 6 digitos aleatorio
                Random random = new Random();
                string plainText = random.Next(100000, 999999).ToString();
                await File.AppendAllTextAsync(logPath, $"DEBUG: OTP generado: {plainText}\n");

                await File.AppendAllTextAsync(logPath, $"DEBUG: Llamando a EnviarOtpAlCorreo con email: {email}\n");
                await this.EnviarOtpAlCorreo(plainText, email);
                await File.AppendAllTextAsync(logPath, $"DEBUG: EnviarOtpAlCorreo completado\n");

                plainText += $",{proveedores.First().email.Split(";")[0]}";
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = Encoding.UTF8.GetBytes(key);
                    aesAlg.IV = new byte[16]; // Inicialización vacía para simplificar

                    ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                            {
                                swEncrypt.Write(plainText);
                            }
                        }
                        var result = Convert.ToBase64String(msEncrypt.ToArray());
                        await File.AppendAllTextAsync(logPath, $"DEBUG: GenerarOTP BLL completado exitosamente\n");
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                await File.AppendAllTextAsync(logPath, $"ERROR: Error en GenerarOTP BLL - NIT: {nit}\n");
                await File.AppendAllTextAsync(logPath, $"ERROR: {ex.Message}\n");
                await File.AppendAllTextAsync(logPath, $"StackTrace: {ex.StackTrace}\n");
                throw;
            }
        }

        private async Task EnviarOtpAlCorreo(string otp, string email)
        {

           await this.EnviarOTP(otp, email);

        }
        internal async Task EnviarOTP(string otp, string email)
        {
            var logPath = "C:\\tmp\\proveedor_otp_email_debug.log";
            try
            {
                await File.AppendAllTextAsync(logPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] INICIO EnviarOTP - Email: {email}, OTP: {otp}\n");
                
                await File.AppendAllTextAsync(logPath, $"DEBUG: Creando cliente SendGrid\n");
                SendGridClient client = BuildSendGridClient();
                await File.AppendAllTextAsync(logPath, $"DEBUG: SendGrid client creado\n");
                var from = new EmailAddress("compras@paecia.com", "Notificaciones");

                await File.AppendAllTextAsync(logPath, $"DEBUG: From configurado: {from.Email}\n");
                var to = new EmailAddress(email);
                await File.AppendAllTextAsync(logPath, $"DEBUG: To configurado: {to.Email}\n");

                var emailText = $"SU CLAVE DE ACCESO TEMPORAL COMO PROVEEDOR A PAECIA ES: {otp}";
                await File.AppendAllTextAsync(logPath, $"DEBUG: Mensaje creado: {emailText}\n");

                var plainTextContent = Regex.Replace(emailText, "<[^>]*>", "");
                var msg = MailHelper.CreateSingleEmail(from, to, "NOTIFICACIÓN - CLAVE DE ACCESO A PORTAL PROVEEDORES", plainTextContent, emailText);
                
                await File.AppendAllTextAsync(logPath, $"DEBUG: Enviando email...\n");
                var response = await client.SendEmailAsync(msg);
                await File.AppendAllTextAsync(logPath, $"DEBUG: Email enviado - Status Code: {response.StatusCode}\n");
                
                if (response.IsSuccessStatusCode)
                {
                    await File.AppendAllTextAsync(logPath, $"SUCCESS: Email enviado exitosamente\n");
                }
                else
                {
                    await File.AppendAllTextAsync(logPath, $"WARNING: Email enviado con código no exitoso: {response.StatusCode}\n");
                    var responseBody = await response.Body.ReadAsStringAsync();
                    await File.AppendAllTextAsync(logPath, $"Response Body: {responseBody}\n");
                }
            }
            catch (Exception ex)
            {
                await File.AppendAllTextAsync(logPath, $"ERROR: Error enviando OTP email - Email: {email}\n");
                await File.AppendAllTextAsync(logPath, $"ERROR: {ex.Message}\n");
                await File.AppendAllTextAsync(logPath, $"StackTrace: {ex.StackTrace}\n");
                throw;
            }
        }

        private SendGridClient BuildSendGridClient()
        {
            var apiKey = this.configuration.GetSection("SendGrid:ApiKey").Value ?? "YOUR_SENDGRID_API_KEY_HERE";
            var logPath = "C:\\tmp\\proveedor_otp_email_debug.log";
            File.AppendAllTextAsync(logPath, $"DEBUG: SendGrid API Key encontrado: {(apiKey.StartsWith("SG.") ? "SG.***" : "NO VÁLIDO")}\n");
            var client = new SendGridClient(apiKey);
            return client;
        }

    }
}
