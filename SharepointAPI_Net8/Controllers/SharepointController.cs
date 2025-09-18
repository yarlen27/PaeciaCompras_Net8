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
    }
}