using DentalERP.Modules.IAM.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.IAM.Features.Settings;

public sealed class GetSettingsQueryHandler(IAMDbContext db)
    : IRequestHandler<GetSettingsQuery, Result<IReadOnlyList<SettingItem>>>
{
    public async Task<Result<IReadOnlyList<SettingItem>>> Handle(GetSettingsQuery request, CancellationToken ct)
    {
        var query = db.SystemSettings.AsNoTracking();
        if (!string.IsNullOrEmpty(request.Group))
            query = query.Where(s => s.Group == request.Group);

        var settings = await query.OrderBy(s => s.Group).ThenBy(s => s.Key)
            .Select(s => new SettingItem(s.Id, s.Key, s.Value, s.Description, s.Group))
            .ToListAsync(ct);

        return Result.Success<IReadOnlyList<SettingItem>>(settings);
    }
}

public sealed class UpdateSettingCommandHandler(IAMDbContext db)
    : IRequestHandler<UpdateSettingCommand, Result>
{
    public async Task<Result> Handle(UpdateSettingCommand request, CancellationToken ct)
    {
        var setting = await db.SystemSettings.FirstOrDefaultAsync(s => s.Key == request.Key, ct);
        if (setting is null) return Result.Failure(Error.NotFound("Setting"));

        setting.SetValue(request.Value);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
