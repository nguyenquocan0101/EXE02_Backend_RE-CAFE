using System.Threading.Tasks;
using EXE02_Backend_RE_CAFE.DTOs;

namespace EXE02_Backend_RE_CAFE.Interfaces
{
    public interface IPaymentService
    {
        string GetPaymentQrUrl(string orderCode, decimal totalAmount);
        Task<(bool Success, string Message)> ProcessSepayWebhookAsync(SepayWebhookRequest request);
    }
}
