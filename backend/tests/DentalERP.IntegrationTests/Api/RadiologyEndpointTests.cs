using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace DentalERP.IntegrationTests.Api;

public class RadiologyEndpointTests(DentalERPTestFactory factory)
    : IClassFixture<DentalERPTestFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private void ClearAuth()
        => _client.DefaultRequestHeaders.Authorization = null;

    [Fact]
    public async Task GetRadiologyTypes_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.GetAsync("/api/radiology/types");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetRadiologyOrders_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.GetAsync("/api/radiology/orders");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateRadiologyOrder_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.PostAsJsonAsync("/api/radiology/orders", new { });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetRadiologyOrderById_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.GetAsync($"/api/radiology/orders/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task MarkRadiologyImaged_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.PostAsJsonAsync($"/api/radiology/orders/{Guid.NewGuid()}/imaged", new { });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UploadRadiologyImage_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.PostAsJsonAsync($"/api/radiology/orders/{Guid.NewGuid()}/images", new { });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SaveRadiologyReport_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.PostAsJsonAsync($"/api/radiology/orders/{Guid.NewGuid()}/report", new { reportText = "test", reportedById = Guid.NewGuid() });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CompleteRadiologyOrder_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.PostAsJsonAsync($"/api/radiology/orders/{Guid.NewGuid()}/complete", new { });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CancelRadiologyOrder_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.PostAsJsonAsync($"/api/radiology/orders/{Guid.NewGuid()}/cancel", new { reason = "test" });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
