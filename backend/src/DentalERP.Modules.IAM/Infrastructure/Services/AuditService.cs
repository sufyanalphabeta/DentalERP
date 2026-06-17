using System.Text.Json;
using DentalERP.Modules.IAM.Domain.Entities;
using DentalERP.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace DentalERP.Modules.IAM.Infrastructure.Services;

public sealed class AuditService(ICurrentUser currentUser, IHttpContextAccessor httpContextAccessor)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = false };

    public IReadOnlyList<AuditLog> GetAuditLogs(IEnumerable<EntityEntry> entries)
    {
        var logs = new List<AuditLog>();

        foreach (var entry in entries.Where(e =>
            e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted))
        {
            var entityName = entry.Entity.GetType().Name;
            var entityId = GetEntityId(entry);
            var action = entry.State switch
            {
                EntityState.Added => "Created",
                EntityState.Modified => "Updated",
                EntityState.Deleted => "Deleted",
                _ => "Unknown"
            };

            string? oldValues = null;
            string? newValues = null;

            if (entry.State == EntityState.Modified)
            {
                var original = entry.Properties
                    .Where(p => p.IsModified)
                    .ToDictionary(p => p.Metadata.Name, p => p.OriginalValue);
                var current = entry.Properties
                    .Where(p => p.IsModified)
                    .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);

                oldValues = JsonSerializer.Serialize(original, JsonOptions);
                newValues = JsonSerializer.Serialize(current, JsonOptions);
            }
            else if (entry.State == EntityState.Added)
            {
                var current = entry.Properties
                    .Where(p => p.Metadata.Name != "PasswordHash")
                    .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);
                newValues = JsonSerializer.Serialize(current, JsonOptions);
            }
            else if (entry.State == EntityState.Deleted)
            {
                var original = entry.Properties
                    .ToDictionary(p => p.Metadata.Name, p => p.OriginalValue);
                oldValues = JsonSerializer.Serialize(original, JsonOptions);
            }

            logs.Add(new AuditLog
            {
                UserId = currentUser.IsAuthenticated ? currentUser.UserId : null,
                Username = currentUser.IsAuthenticated ? currentUser.Username : "system",
                EntityName = entityName,
                EntityId = entityId,
                Action = action,
                OldValues = oldValues,
                NewValues = newValues,
                IpAddress = GetIpAddress(),
                UserAgent = GetUserAgent()
            });
        }

        return logs;
    }

    public AuditLog CreateActionLog(string action, string entityName, string entityId, string? details = null)
        => new()
        {
            UserId = currentUser.IsAuthenticated ? currentUser.UserId : null,
            Username = currentUser.IsAuthenticated ? currentUser.Username : "system",
            EntityName = entityName,
            EntityId = entityId,
            Action = action,
            NewValues = details,
            IpAddress = GetIpAddress(),
            UserAgent = GetUserAgent()
        };

    private string GetIpAddress()
        => httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    private string? GetUserAgent()
        => httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString();

    private static string GetEntityId(EntityEntry entry)
    {
        var keyProps = entry.Metadata.FindPrimaryKey()?.Properties;
        if (keyProps is null) return "unknown";
        var values = keyProps.Select(p => entry.Property(p.Name).CurrentValue?.ToString() ?? "null");
        return string.Join("|", values);
    }
}
