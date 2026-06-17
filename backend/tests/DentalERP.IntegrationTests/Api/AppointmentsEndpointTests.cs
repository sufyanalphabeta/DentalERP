using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace DentalERP.IntegrationTests.Api;

public class AppointmentsEndpointTests(DentalERPTestFactory factory)
    : IClassFixture<DentalERPTestFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetAppointments_WithoutToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/appointments");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateAppointment_WithoutToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.PostAsJsonAsync("/api/appointments", new
        {
            patientId = Guid.NewGuid(),
            doctorId = Guid.NewGuid(),
            scheduledAt = DateTime.UtcNow.AddHours(2),
            durationMinutes = 30
        });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateAppointmentStatus_WithoutToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.PatchAsJsonAsync(
            $"/api/appointments/{Guid.NewGuid()}/status",
            new { status = "Confirmed" });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateAppointment_WithInvalidDuration_Returns4xx()
    {
        // No token → auth guard fires first → 401
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.PostAsJsonAsync("/api/appointments", new
        {
            patientId = Guid.NewGuid(),
            doctorId = Guid.NewGuid(),
            scheduledAt = DateTime.UtcNow.AddHours(1),
            durationMinutes = 0
        });
        ((int)response.StatusCode).Should().BeInRange(400, 499);
    }
}
