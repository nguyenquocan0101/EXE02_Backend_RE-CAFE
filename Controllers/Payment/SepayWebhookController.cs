using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using EXE02_Backend_RE_CAFE.DTOs;
using EXE02_Backend_RE_CAFE.Interfaces;

namespace EXE02_Backend_RE_CAFE.Controllers
{
    [ApiController]
    [Route("api/sepay-webhook")]
    public class SepayWebhookController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IConfiguration _configuration;

        public SepayWebhookController(IPaymentService paymentService, IConfiguration configuration)
        {
            _paymentService = paymentService;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> HandleWebhook([FromBody] SepayWebhookRequest request)
        {
            // 1. Verify SePay authorization
            var authHeader = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Apikey ", StringComparison.OrdinalIgnoreCase))
            {
                return Unauthorized(new { success = false, message = "Missing or invalid authorization header" });
            }

            var token = authHeader.Substring("Apikey ".Length).Trim();
            var expectedToken = _configuration["Sepay:ApiKey"];
            if (expectedToken != token)
            {
                return Unauthorized(new { success = false, message = "Invalid API Key" });
            }

            // 2. Delegate payment processing to PaymentService
            var (success, message) = await _paymentService.ProcessSepayWebhookAsync(request);

            if (!success)
            {
                return Ok(new { success = false, message });
            }

            return Ok(new { success = true, message });
        }
    }
}
