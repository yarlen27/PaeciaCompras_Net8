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

            var proveedores = await this.PorNITs(nit);

            if (proveedores.Count == 0)
            {
                throw new Exception("Proveedor no encontrado");
            }
            //creamos un codigo de 6 digitos aleatorio
            Random random = new Random();
            string plainText = random.Next(100000, 999999).ToString();

            this.EnviarOtpAlCorreo(plainText, proveedores.First().email.Split(";")[0]);

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
                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }

        }

        private void EnviarOtpAlCorreo(string otp, string email)
        {

           this.EnviarOTP(otp, email);

        }
        internal async Task EnviarOTP(string otp, string email)
        {
            SendGridClient client = BuildSendGridClient();
            var from = new EmailAddress("compras@paecia.com", "Notificaciones");


            var to = new EmailAddress(email);


            var emailText = $"SU CLAVE DE ACCESO TEMPORAL COMO PROVEEDOR A PAECIA ES: {otp}";
            //


            var plainTextContent = Regex.Replace(emailText, "<[^>]*>", "");
            var msg = MailHelper.CreateSingleEmail(from, to, "NOTIFICACIÓN - CLAVE DE ACCESO A PORTAL PROVEEDORES", plainTextContent, emailText);
            var response = await client.SendEmailAsync(msg);


        }

        private static SendGridClient BuildSendGridClient()
        {
            //var apiKey = "YOUR_SENDGRID_API_KEY_HERE";
            var apiKey = "YOUR_SENDGRID_API_KEY_HERE";
            //var apiKey = "YOUR_SENDGRID_API_KEY_HERE";
            var client = new SendGridClient(apiKey);
            return client;
        }

    }
}
