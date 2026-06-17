using DentalERP.SharedKernel.Abstractions;

namespace DentalERP.Modules.IAM.Domain.Entities;

public sealed class Permission : BaseEntity
{
    public string Name { get; private set; } = default!;
    public string DisplayName { get; private set; } = default!;
    public string Module { get; private set; } = default!;

    private Permission() { }

    public static Permission Create(string name, string displayName, string module)
        => new() { Name = name, DisplayName = displayName, Module = module };
}
