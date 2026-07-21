using System;
using System.Globalization;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EXE02_Backend_RE_CAFE.DTOs;
using EXE02_Backend_RE_CAFE.Interfaces;

namespace EXE02_Backend_RE_CAFE.Controllers
{
    [ApiController]
    [Route("api/admin/payments")]
    [Authorize(Roles = "Admin,Staff")]
    public class AdminPaymentsController : BaseApiController
    {
        private readonly IPaymentManagementService _paymentManagementService;
        private readonly IAuditLogService _auditLogService;

        public AdminPaymentsController(
            IPaymentManagementService paymentManagementService,
            IAuditLogService auditLogService)
        {
            _paymentManagementService = paymentManagementService;
            _auditLogService = auditLogService;
        }

        [HttpGet]
        public async Task<IActionResult> GetPayments([FromQuery] AdminPaymentQuery query)
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(ErrorResponse<object>(
                    message: "Unauthorized request. User identifier is missing or invalid.",
                    action: "GetAdminPayments",
                    statusCode: StatusCodes.Status401Unauthorized));
            }

            var payments = await _paymentManagementService.GetPaymentsAsync(query);
            await _auditLogService.WriteAsync(
                userId,
                action: "view_list",
                entityName: "Payment",
                entityId: Guid.Empty,
                newValue: JsonSerializer.Serialize(new
                {
                    query.Keyword,
                    Status = query.Status?.ToString(),
                    Method = query.Method?.ToString(),
                    query.From,
                    query.To,
                    RowCount = payments.Items.Count,
                    payments.Total
                }));

            return Ok(SuccessResponse(
                message: "Payment transactions retrieved successfully.",
                action: "GetAdminPayments",
                data: payments,
                statusCode: StatusCodes.Status200OK));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetPaymentById(Guid id)
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(ErrorResponse<object>(
                    message: "Unauthorized request. User identifier is missing or invalid.",
                    action: "GetAdminPaymentById",
                    statusCode: StatusCodes.Status401Unauthorized));
            }

            var payment = await _paymentManagementService.GetPaymentByIdAsync(id);
            if (payment == null)
            {
                return NotFound(ErrorResponse<object>(
                    message: $"Payment with ID {id} not found.",
                    action: "GetAdminPaymentById",
                    statusCode: StatusCodes.Status404NotFound));
            }

            await _auditLogService.WriteAsync(
                userId,
                action: "view_detail",
                entityName: "Payment",
                entityId: id);

            return Ok(SuccessResponse(
                message: "Payment transaction retrieved successfully.",
                action: "GetAdminPaymentById",
                data: payment,
                statusCode: StatusCodes.Status200OK));
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetPaymentSummary([FromQuery] AdminPaymentQuery query)
        {
            if (!TryGetUserId(out _))
            {
                return Unauthorized(ErrorResponse<object>(
                    message: "Unauthorized request. User identifier is missing or invalid.",
                    action: "GetAdminPaymentSummary",
                    statusCode: StatusCodes.Status401Unauthorized));
            }

            var summary = await _paymentManagementService.GetPaymentSummaryAsync(query);
            return Ok(SuccessResponse(
                message: "Payment summary retrieved successfully.",
                action: "GetAdminPaymentSummary",
                data: summary,
                statusCode: StatusCodes.Status200OK));
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportPayments([FromQuery] AdminPaymentQuery query)
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(ErrorResponse<object>(
                    message: "Unauthorized request. User identifier is missing or invalid.",
                    action: "ExportAdminPayments",
                    statusCode: StatusCodes.Status401Unauthorized));
            }

            var export = await _paymentManagementService.GetPaymentExportAsync(query);
            await _auditLogService.WriteAsync(
                userId,
                action: "export",
                entityName: "Payment",
                entityId: Guid.Empty,
                newValue: JsonSerializer.Serialize(new
                {
                    query.Keyword,
                    Status = query.Status?.ToString(),
                    Method = query.Method?.ToString(),
                    query.From,
                    query.To,
                    RowCount = export.IsLimitExceeded ? export.Total : export.Items.Count,
                    export.Total,
                    Limit = AdminPaymentExportResult.MaximumRows,
                    Rejected = export.IsLimitExceeded
                }));

            if (export.IsLimitExceeded)
            {
                return BadRequest(ErrorResponse<object>(
                    message: $"CSV export is limited to {AdminPaymentExportResult.MaximumRows:N0} rows. The current filters match {export.Total:N0} rows; narrow the filters and try again.",
                    action: "ExportAdminPayments",
                    statusCode: StatusCodes.Status400BadRequest,
                    data: new
                    {
                        export.Total,
                        Limit = AdminPaymentExportResult.MaximumRows
                    }));
            }

            Response.Headers.CacheControl = "no-store";
            Response.Headers.Append("X-Content-Type-Options", "nosniff");
            var fileName = $"payment-transactions-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv";
            return File(CreateCsv(export.Items), "text/csv; charset=utf-8", fileName);
        }

        private static byte[] CreateCsv(IReadOnlyList<AdminPaymentDto> payments)
        {
            var csv = new StringBuilder();
            csv.Append('\uFEFF');
            AppendCsvRow(csv,
                "Payment ID",
                "Order ID",
                "Order Code",
                "Customer",
                "Payment Method",
                "Payment Status",
                "Order Total Amount",
                "Payment Amount",
                "Transaction Code",
                "Paid At (UTC)",
                "Created At (UTC)");

            foreach (var payment in payments)
            {
                AppendCsvRow(csv,
                    payment.Id.ToString(),
                    payment.OrderId.ToString(),
                    payment.OrderCode,
                    payment.CustomerName,
                    payment.PaymentMethod,
                    payment.PaymentStatus,
                    payment.OrderTotalAmount.ToString("0.##", CultureInfo.InvariantCulture),
                    payment.Amount.ToString("0.##", CultureInfo.InvariantCulture),
                    payment.TransactionCode,
                    FormatUtc(payment.PaidAt),
                    FormatUtc(payment.CreatedAt));
            }

            return Encoding.UTF8.GetBytes(csv.ToString());
        }

        private static void AppendCsvRow(StringBuilder csv, params string?[] values)
        {
            csv.AppendLine(string.Join(',', values.Select(EscapeCsvCell)));
        }

        private static string EscapeCsvCell(string? value)
        {
            var safeValue = value ?? string.Empty;
            var trimmedValue = safeValue.TrimStart();
            if ((trimmedValue.Length > 0 && "=+-@".Contains(trimmedValue[0]))
                || (safeValue.Length > 0 && safeValue[0] is '\t' or '\r' or '\n'))
            {
                safeValue = $"'{safeValue}";
            }

            return $"\"{safeValue.Replace("\"", "\"\"")}\"";
        }

        private static string FormatUtc(DateTime? value)
        {
            return value.HasValue
                ? value.Value.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture)
                : string.Empty;
        }

        private bool TryGetUserId(out Guid userId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdClaim?.Value, out userId);
        }
    }
}
