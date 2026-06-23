using DentalERP.Modules.Clinical.Features.Media.UploadMedia;
using DentalERP.Modules.Clinical.Infrastructure;
using DentalERP.SharedKernel.Extensions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DentalERP.Modules.Clinical.Endpoints;

public static class MediaEndpoints
{
    public static IEndpointRouteBuilder MapMediaEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/patients/{patientId}/media");

        group.MapGet("", async (Guid patientId, ClinicalDbContext db, CancellationToken ct) =>
        {
            var items = await db.PatientMedia
                .Where(m => m.PatientId == patientId && m.DeletedAt == null)
                .OrderByDescending(m => m.UploadedAt)
                .Select(m => new {
                    m.Id, m.MediaType, m.FileName, m.FilePath, m.FileSizeBytes,
                    m.MimeType, m.ThumbnailPath, m.Title, m.Description,
                    m.ToothId, m.AppointmentId, m.IsRequired, m.IsApproved,
                    m.UploadedById, m.UploadedAt
                })
                .AsNoTracking()
                .ToListAsync(ct);
            return Results.Ok(items);
        })
        .RequirePermission("Clinical.Files.View")
        .WithName("GetPatientMedia")
        .Produces(200).Produces(401);

        group.MapPost("", async (Guid patientId, UploadMediaRequest req, ISender sender,
            ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await sender.Send(new UploadMediaCommand(
                patientId, userId, req.MediaType, req.FileName, req.FilePath,
                req.FileSizeBytes, req.MimeType, req.ThumbnailPath, req.Title,
                req.Description, req.ToothId, req.AppointmentId, req.IsRequired), ct);
            return result.IsSuccess
                ? Results.Created($"/api/patients/{patientId}/media/{result.Value}", new { id = result.Value })
                : Results.BadRequest(result.Error);
        })
        .RequirePermission("Clinical.Files.Upload")
        .WithName("UploadMedia")
        .Produces(201).Produces(400).Produces(401);

        return app;
    }
}

public sealed record UploadMediaRequest(
    string MediaType,
    string FileName,
    string FilePath,
    long? FileSizeBytes = null,
    string? MimeType = null,
    string? ThumbnailPath = null,
    string? Title = null,
    string? Description = null,
    short? ToothId = null,
    Guid? AppointmentId = null,
    bool IsRequired = false);
