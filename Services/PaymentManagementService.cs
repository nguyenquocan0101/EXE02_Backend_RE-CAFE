using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EXE02_Backend_RE_CAFE.Data;
using EXE02_Backend_RE_CAFE.DTOs;
using EXE02_Backend_RE_CAFE.Interfaces;
using EXE02_Backend_RE_CAFE.Models;

namespace EXE02_Backend_RE_CAFE.Services
{
    public class PaymentManagementService : IPaymentManagementService
    {
        private const int DefaultPageSize = 20;
        private const int MaxPageSize = 100;
        private readonly ApplicationDbContext _context;

        public PaymentManagementService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AdminPaymentPageDto> GetPaymentsAsync(AdminPaymentQuery query)
        {
            var page = Math.Max(query.Page, 1);
            var pageSize = query.PageSize <= 0
                ? DefaultPageSize
                : Math.Min(query.PageSize, MaxPageSize);

            var payments = BuildQuery(query);
            var total = await payments.CountAsync();
            var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);

            var rows = await ProjectPayments(OrderPayments(payments)
                .Skip((page - 1) * pageSize)
                .Take(pageSize))
                .ToListAsync();

            return new AdminPaymentPageDto
            {
                Items = rows.Select(ToDto).ToList(),
                Total = total,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }

        public async Task<AdminPaymentDto?> GetPaymentByIdAsync(Guid id)
        {
            var row = await ProjectPayments(_context.Payments
                .AsNoTracking()
                .Where(payment => payment.Id == id))
                .FirstOrDefaultAsync();

            return row == null ? null : ToDto(row);
        }

        public async Task<AdminPaymentSummaryDto> GetPaymentSummaryAsync(AdminPaymentQuery query)
        {
            return await BuildQuery(query)
                .GroupBy(_ => 1)
                .Select(group => new AdminPaymentSummaryDto
                {
                    PaidCount = group.Count(payment => payment.Status == PaymentStatus.Paid),
                    UnpaidCount = group.Count(payment => payment.Status == PaymentStatus.Unpaid),
                    PaidAmount = group.Sum(payment => payment.Status == PaymentStatus.Paid
                        ? payment.Amount
                        : 0m)
                })
                .SingleOrDefaultAsync()
                ?? new AdminPaymentSummaryDto();
        }

        public async Task<AdminPaymentExportResult> GetPaymentExportAsync(AdminPaymentQuery query)
        {
            var payments = BuildQuery(query);
            var total = await payments.CountAsync();
            var result = new AdminPaymentExportResult { Total = total };

            if (result.IsLimitExceeded)
            {
                return result;
            }

            var rows = await ProjectPayments(OrderPayments(payments)
                .Take(AdminPaymentExportResult.MaximumRows + 1))
                .ToListAsync();

            if (rows.Count > AdminPaymentExportResult.MaximumRows)
            {
                result.Total = Math.Max(total, rows.Count);
                return result;
            }

            result.Items = rows.Select(ToDto).ToList();
            return result;
        }

        private IQueryable<Payment> BuildQuery(AdminPaymentQuery query)
        {
            var payments = _context.Payments.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(query.Keyword))
            {
                var keyword = query.Keyword.Trim().ToLower();
                payments = payments.Where(payment =>
                    payment.Order != null && (
                        payment.Order.OrderCode.ToLower().Contains(keyword) ||
                        (payment.Order.User != null && payment.Order.User.FullName.ToLower().Contains(keyword)) ||
                        (payment.Order.ShippingAddress != null && payment.Order.ShippingAddress.ReceiverName.ToLower().Contains(keyword)) ||
                        (payment.TransactionCode != null && payment.TransactionCode.ToLower().Contains(keyword))));
            }

            if (query.Status.HasValue)
            {
                payments = payments.Where(payment => payment.Status == query.Status.Value);
            }

            if (query.Method.HasValue)
            {
                payments = payments.Where(payment => payment.Method == query.Method.Value);
            }

            if (query.From.HasValue)
            {
                payments = payments.Where(payment => payment.CreatedAt >= query.From.Value.ToUniversalTime());
            }

            if (query.To.HasValue)
            {
                payments = payments.Where(payment => payment.CreatedAt <= query.To.Value.ToUniversalTime());
            }

            return payments;
        }

        private static IOrderedQueryable<Payment> OrderPayments(IQueryable<Payment> payments)
        {
            return payments
                .OrderByDescending(payment => payment.PaidAt ?? payment.CreatedAt)
                .ThenByDescending(payment => payment.Id);
        }

        private static IQueryable<PaymentProjectionRow> ProjectPayments(IQueryable<Payment> payments)
        {
            return payments.Select(payment => new PaymentProjectionRow
            {
                Id = payment.Id,
                OrderId = payment.OrderId,
                OrderCode = payment.Order != null ? payment.Order.OrderCode : string.Empty,
                CustomerName = payment.Order != null && payment.Order.User != null
                    ? payment.Order.User.FullName
                    : payment.Order != null && payment.Order.ShippingAddress != null
                        ? payment.Order.ShippingAddress.ReceiverName
                        : string.Empty,
                PaymentMethod = payment.Method,
                PaymentStatus = payment.Status,
                OrderTotalAmount = payment.Order != null ? payment.Order.TotalAmount : 0,
                Amount = payment.Amount,
                TransactionCode = payment.TransactionCode,
                PaidAt = payment.PaidAt,
                CreatedAt = payment.CreatedAt
            });
        }

        private static AdminPaymentDto ToDto(PaymentProjectionRow row)
        {
            return new AdminPaymentDto
            {
                Id = row.Id,
                OrderId = row.OrderId,
                OrderCode = row.OrderCode,
                CustomerName = row.CustomerName,
                PaymentMethod = row.PaymentMethod.ToString(),
                PaymentStatus = row.PaymentStatus.ToString(),
                OrderTotalAmount = row.OrderTotalAmount,
                Amount = row.Amount,
                TransactionCode = row.TransactionCode,
                PaidAt = row.PaidAt,
                CreatedAt = row.CreatedAt
            };
        }

        private sealed class PaymentProjectionRow
        {
            public Guid Id { get; set; }
            public Guid OrderId { get; set; }
            public string OrderCode { get; set; } = string.Empty;
            public string CustomerName { get; set; } = string.Empty;
            public PaymentMethod PaymentMethod { get; set; }
            public PaymentStatus PaymentStatus { get; set; }
            public decimal OrderTotalAmount { get; set; }
            public decimal Amount { get; set; }
            public string? TransactionCode { get; set; }
            public DateTime? PaidAt { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}
