using DentalERP.SharedKernel.Abstractions;

namespace DentalERP.Modules.IAM.Domain.Entities;

public sealed class SystemSetting : BaseEntity
{
    public string Key { get; private set; } = default!;
    public string Value { get; private set; } = default!;
    public string? Description { get; private set; }
    public string Group { get; private set; } = "General";

    private SystemSetting() { }

    public static SystemSetting Create(string key, string value, string? description = null, string group = "General")
        => new() { Key = key, Value = value, Description = description, Group = group };

    public void SetValue(string value)
    {
        Value = value;
        Touch();
    }
}
