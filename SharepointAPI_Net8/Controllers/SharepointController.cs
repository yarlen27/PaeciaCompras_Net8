using Microsoft.AspNetCore.Mvc;
using SharepointAPI_Net8.Models;
using SharepointAPI_Net8.Services;

namespace SharepointAPI_Net8.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SharepointController : ControllerBase
    {
        private readonly SharePointService _sharePointService;

        public SharepointController(SharePointService sharePointService)
        {
            _sharePointService = sharePointService;
        }

        [HttpGet("get-site-name")]
        public async Task<IActionResult> GetSiteName()
        {
            try
            {
                string siteName = await _sharePointService.GetSiteNameAsync();
                return Ok(new { 
                    success = true,
                    siteName = siteName,
                    message = "Conexión exitosa con Microsoft Graph API",
                    authType = "Certificate-based Authentication"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { 
                    success = false, 
                    siteName = "",
                    message = ex.Message,
                    authType = "Certificate-based Authentication"
                });
            }
        }

        [HttpPost("GenerarContrato")]
        public async Task<IActionResult> GenerarContrato([FromBody] PedidoServicio pedidoServicio)
        {
            try
            {
                var links = await _sharePointService.GenerarContratoAsync(pedidoServicio.base64);
                return Ok(links);
            }
            catch (Exception ex)
            {
                return BadRequest(new { 
                    success = false, 
                    message = ex.Message
                });
            }
        }

        [HttpPost("GenerarPDF")]
        public async Task<IActionResult> GenerarPDF([FromBody] GenerarPDFRequest request)
        {
            try
            {
                var pdfUrl = await _sharePointService.GenerarPDFAsync(request.Link);
                return Ok(new { 
                    success = true, 
                    pdfUrl = pdfUrl,
                    message = "PDF generado exitosamente"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { 
                    success = false, 
                    message = ex.Message
                });
            }
        }

        [HttpPost("DescargarPDF")]
        public async Task<IActionResult> DescargarPDF([FromBody] ResultadoPDF request)
        {
            try
            {
                var pdfContrato = await _sharePointService.DescargarPDFAsync(request.Link);
                return Ok(pdfContrato);
            }
            catch (Exception ex)
            {
                return BadRequest(new { 
                    success = false, 
                    message = ex.Message
                });
            }
        }
    }
}