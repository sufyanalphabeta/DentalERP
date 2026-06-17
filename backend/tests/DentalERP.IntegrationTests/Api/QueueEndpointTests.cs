using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace DentalERP.IntegrationTests.Api;

public class QueueEndpointTests(DentalERPTestFactory factory)
    : IClassFixture<DentalERPTestFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetQueue_WithoutToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/queue");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetQueue_WithDate_WithoutToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var today = DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd");
        var response = await _client.GetAsync($"/api/queue?date={today}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CheckIn_WithoutToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.PostAsJsonAsync("/api/queue/check-in", new
        {
            patientId = Guid.NewGuid()
        });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateQueueStatus_WithoutToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.PatchAsJsonAsync(
            $"/api/queue/{Guid.NewGuid()}/status",
            new { status = "Called" });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
