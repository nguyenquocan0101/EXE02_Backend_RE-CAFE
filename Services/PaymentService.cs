using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using EXE02_Backend_RE_CAFE.Data;
using EXE02_Backend_RE_CAFE.DTOs;
using EXE02_Backend_RE_CAFE.Interfaces;
using EXE02_Backend_RE_CAFE.Models;

namespace EXE02_Backend_RE_CAFE.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public PaymentService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public string GetPaymentQrUrl(string orderCode, decimal totalAmount)
        {
            var bankAccount = _configuration["Sepay:BankAccount"] ?? "123456789";
            var bankName = _configuration["Sepay:BankName"] ?? "MBBank";
            var prefix = _configuration["Sepay:QrPrefix"] ?? "RECAFE";
            var amount = (int)totalAmount;
            var description = $"{prefix} {orderCode}";

            return $"https://qr.sepay.vn/img?acc={bankAccount}&bank={bankName}&amount={amount}&des={Uri.EscapeDataString(description)}";
        }

        public async Task<(bool Success, string Message)> ProcessSepayWebhookAsync(SepayWebhookRequest request)
        {
            // We only process incoming transfers ("in")
            if (request.TransferType != null && !request.TransferType.Equals("in", StringComparison.OrdinalIgnoreCase))
            {
                return (true, "Ignored non-incoming transaction");
            }

            // Parse Transaction Date (SePay timezone is Vietnam UTC+7, convert to UTC and specify Utc kind for PostgreSQL)
            DateTime paidAt = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(request.TransactionDate))
            {
                if (DateTime.TryParseExact(request.TransactionDate, 
                    new[] { "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd'T'HH:mm:ss", "yyyy-MM-dd'T'HH:mm:ss.fffZ", "yyyy-MM-dd'T'HH:mm:ssZ" }, 
                    System.Globalization.CultureInfo.InvariantCulture, 
                    System.Globalization.DateTimeStyles.None, 
                    out var parsedDate))
                {
                    paidAt = DateTime.SpecifyKind(parsedDate.AddHours(-7), DateTimeKind.Utc);
                }
            }

            // Parse Order Code from Code or Content using regex
            // Order Code format: ORD-YYYYMMDD-NNNN (e.g. ORD-20260525-1234)
            var regex = new Regex(@"(ORD)-?(\d{8})-?(\d{4})", RegexOptions.IgnoreCase);
            var searchTarget = (request.Code ?? "") + " " + (request.Content ?? "");
            var match = regex.Match(searchTarget);

            if (!match.Success)
            {
                return (false, "Could not find a valid order code in transfer code or content");
            }

            var orderCode = $"ORD-{match.Groups[2].Value}-{match.Groups[3].Value}";

            // Query the Order from Database
            var order = await _context.Orders
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.OrderCode.ToLower() == orderCode.ToLower());

            if (order == null)
            {
                return (false, $"Order with code {orderCode} was not found");
            }

            if (order.PaymentStatus == PaymentStatus.Paid)
            {
                return (true, $"Order {orderCode} has already been paid");
            }

            // Validate the paid amount
            if (request.TransferAmount < order.TotalAmount)
            {
                return (false, $"Received amount ({request.TransferAmount:N0} VND) is less than order total ({order.TotalAmount:N0} VND)");
            }

            // Update order status and record payment
            order.PaymentStatus = PaymentStatus.Paid;
            order.Status = OrderStatus.Confirmed;

            if (order.Payment != null)
            {
                order.Payment.Status = PaymentStatus.Paid;
                order.Payment.Amount = request.TransferAmount;
                order.Payment.TransactionCode = request.ReferenceCode ?? request.Id.ToString();
                order.Payment.PaidAt = paidAt;
            }
            else
            {
                var payment = new Payment
                {
                    OrderId = order.Id,
                    Method = PaymentMethod.BankTransfer,
                    Status = PaymentStatus.Paid,
                    Amount = request.TransferAmount,
                    TransactionCode = request.ReferenceCode ?? request.Id.ToString(),
                    PaidAt = paidAt
                };
                _context.Payments.Add(payment);
            }

            await _context.SaveChangesAsync();

            return (true, $"Successfully processed payment for order {orderCode}");
        }
    }
}
