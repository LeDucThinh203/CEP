namespace CEP.Backend.Interfaces;

public interface IAuditService
{
    Task LogAsync(string userName, string action, string entityName, int entityId, string? oldValues, string? newValues);
}
