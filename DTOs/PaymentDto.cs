using System;

namespace EXE02_Backend_RE_CAFE.DTOs
{
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
        public string? ReferenceCode { get; set; }
        public string? PaymentChannel { get; set; }
    }
}
