using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        private readonly ILogger<HealthController> _logger;

        public HealthController(ILogger<HealthController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Basic health check endpoint
        /// </summary>
        /// <returns>200 OK with basic status</returns>
        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                _logger.LogInformation("Health check requested");
                
                var response = new
                {
                    status = "healthy",
                    timestamp = DateTime.UtcNow,
                    service = "PaeciaCompras API",
                    version = "1.0.0"
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                
                var response = new
                {
                    status = "unhealthy",
                    timestamp = DateTime.UtcNow,
                    service = "PaeciaCompras API",
                    error = ex.Message
                };

                return StatusCode(503, response);
            }
        }

        /// <summary>
        /// Simple ping endpoint
        /// </summary>
        /// <returns>200 OK with pong response</returns>
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            _logger.LogInformation("Ping requested");
            
            return Ok(new { 
                message = "pong", 
                timestamp = DateTime.UtcNow 
            });
        }

        /// <summary>
        /// Basic status endpoint with uptime
        /// </summary>
        /// <returns>200 OK with status information</returns>
        [HttpGet("status")]
        public IActionResult Status()
        {
            try
            {
                _logger.LogInformation("Status check requested");
                
                var response = new
                {
                    status = "running",
                    timestamp = DateTime.UtcNow,
                    service = "PaeciaCompras API",
                    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                    machine = Environment.MachineName,
                    uptime = DateTime.UtcNow.Subtract(System.Diagnostics.Process.GetCurrentProcess().StartTime.ToUniversalTime()),
                    dotnet_version = Environment.Version.ToString()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Status check failed");
                return StatusCode(500, new { 
                    status = "error", 
                    message = ex.Message,
                    timestamp = DateTime.UtcNow 
                });
            }
        }
    }
}