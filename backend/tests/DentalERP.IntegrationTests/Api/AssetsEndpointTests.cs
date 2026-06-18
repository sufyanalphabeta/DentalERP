using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace DentalERP.IntegrationTests.Api;

public class AssetsEndpointTests(DentalERPTestFactory factory)
    : IClassFixture<DentalERPTestFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private void ClearAuth() => _client.DefaultRequestHeaders.Authorization = null;

    // Asset Categories
    [Fact]
    public async Task GetAssetCategories_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.GetAsync("/api/assets/categories");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateAssetCategory_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.PostAsJsonAsync("/api/assets/categories", new { Name = "IT" });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // Assets
    [Fact]
    public async Task GetAssets_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.GetAsync("/api/assets/");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAssetById_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.GetAsync($"/api/assets/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAssetByTag_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.GetAsync("/api/assets/by-tag/AST-000001");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateAsset_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.PostAsJsonAsync("/api/assets/", new { Name = "Test" });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateAsset_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.PutAsJsonAsync($"/api/assets/{Guid.NewGuid()}", new { });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DisposeAsset_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.PostAsJsonAsync($"/api/assets/{Guid.NewGuid()}/dispose", new { });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // Documents
    [Fact]
    public async Task GetAssetDocuments_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.GetAsync($"/api/assets/{Guid.NewGuid()}/documents");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // Maintenances
    [Fact]
    public async Task GetAssetMaintenances_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.GetAsync($"/api/assets/{Guid.NewGuid()}/maintenances");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateAssetMaintenance_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.PostAsJsonAsync($"/api/assets/{Guid.NewGuid()}/maintenances", new { });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // PDF Register
    [Fact]
    public async Task GetAssetRegisterPdf_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.GetAsync("/api/assets/register/pdf");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
