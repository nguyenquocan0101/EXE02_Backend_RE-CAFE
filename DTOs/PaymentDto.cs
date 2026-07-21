using System;
using System.ComponentModel.DataAnnotations;
using EXE02_Backend_RE_CAFE.Models;

namespace EXE02_Backend_RE_CAFE.DTOs
{
    public class AdminPaymentQuery
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? Keyword { get; set; }
        public PaymentStatus? Status { get; set; }
        public PaymentMethod? Method { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }

    public class AdminPaymentDto
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public decimal OrderTotalAmount { get; set; }
        public decimal Amount { get; set; }
        public string? TransactionCode { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AdminPaymentPageDto
    {
        public List<AdminPaymentDto> Items { get; set; } = new();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class AdminPaymentSummaryDto
    {
        public int PaidCount { get; set; }
        public int UnpaidCount { get; set; }
        public decimal PaidAmount { get; set; }
    }

    public class AdminPaymentExportResult
    {
        public const int MaximumRows = 10_000;

        public List<AdminPaymentDto> Items { get; set; } = new();
        public int Total { get; set; }
        public bool IsLimitExceeded => Total > MaximumRows;
    }

    public class SepayWebhookRequest
    {
        public long Id { get; set; }
        public string? Gateway { get; set; }
        public string? TransactionDate { get; set; }
        public string? AccountNumber { get; set; }
        public string? SubAccount { get; set; }
        public string? TransferType { get; set; } // "in" or "out"
        public decimal TransferAmount { get; set; }
        public decimal Accumulated { get; set; }
        public string? Code { get; set; } // The transaction code matched by SePay
        public string? Content { get; set; } // Full transaction content
        [StringLength(100)]
        public string? ReferenceCode { get; set; }
        public string? PaymentChannel { get; set; }
    }
}
