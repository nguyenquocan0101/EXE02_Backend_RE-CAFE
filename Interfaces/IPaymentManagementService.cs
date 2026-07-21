using System;
using System.Threading.Tasks;
using EXE02_Backend_RE_CAFE.DTOs;

namespace EXE02_Backend_RE_CAFE.Interfaces
{
    public interface IPaymentManagementService
    {
        Task<AdminPaymentPageDto> GetPaymentsAsync(AdminPaymentQuery query);
        Task<AdminPaymentDto?> GetPaymentByIdAsync(Guid id);
        Task<AdminPaymentSummaryDto> GetPaymentSummaryAsync(AdminPaymentQuery query);
        Task<AdminPaymentExportResult> GetPaymentExportAsync(AdminPaymentQuery query);
    }
}
