using CEP.Backend.Data;
using CEP.Backend.Interfaces;
using CEP.Backend.Models;

namespace CEP.Backend.Services;

public class AuditService : IAuditService
{
    private readonly AppDbContext _context;
    private readonly ILogger<AuditService> _logger;

    public AuditService(AppDbContext context, ILogger<AuditService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task LogAsync(string userName, string action, string entityName, int entityId, string? oldValues, string? newValues)
    {
        try
        {
            var log = new AuditLog
            {
                UserName = string.IsNullOrWhiteSpace(userName) ? "System" : userName,
                Action = action,
                EntityName = entityName,
                EntityId = entityId,
                OldValues = oldValues,
                NewValues = newValues,
                CreatedAt = DateTime.UtcNow
            };

            await _context.AuditLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Logging failure should not disrupt the business transaction
            _logger.LogError(ex, "Failed to create audit log for {EntityName} id {EntityId}", entityName, entityId);
        }
    }
}
