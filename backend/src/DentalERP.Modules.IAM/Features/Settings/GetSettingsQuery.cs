using DentalERP.SharedKernel.Behaviors;
using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.IAM.Features.Settings;

[RequirePermission("Settings.View")]
public sealed record GetSettingsQuery(string? Group = null) : IRequest<Result<IReadOnlyList<SettingItem>>>;
public sealed record SettingItem(Guid Id, string Key, string Value, string? Description, string Group);

[RequirePermission("Settings.Edit")]
public sealed record UpdateSettingCommand(string Key, string Value) : IRequest<Result>;
