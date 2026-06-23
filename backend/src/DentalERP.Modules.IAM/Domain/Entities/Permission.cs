using DentalERP.SharedKernel.Abstractions;

namespace DentalERP.Modules.IAM.Domain.Entities;

public sealed class Permission : BaseEntity
{
    public string Name { get; private set; } = default!;
    public string DisplayName { get; private set; } = default!;
    public string Module { get; private set; } = default!;
    public string? Screen { get; private set; }
    public int SortOrder { get; private set; }

    private Permission() { }

    public static Permission Create(string name, string displayName, string module, string? screen = null, int sortOrder = 0)
        => new() { Name = name, DisplayName = displayName, Module = module, Screen = screen, SortOrder = sortOrder };

    public string Action => Name.Split('.') is { Length: >= 3 } parts ? parts[2] : Name;
}
