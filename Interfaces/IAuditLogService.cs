using System;
using System.Threading.Tasks;

namespace EXE02_Backend_RE_CAFE.Interfaces
{
    public interface IAuditLogService
    {
        Task WriteAsync(
            Guid userId,
            string action,
            string entityName,
            Guid entityId,
            string? oldValue = null,
            string? newValue = null);
    }
}
