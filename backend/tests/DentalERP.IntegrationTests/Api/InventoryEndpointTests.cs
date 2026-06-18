using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace DentalERP.IntegrationTests.Api;

public class InventoryEndpointTests(DentalERPTestFactory factory)
    : IClassFixture<DentalERPTestFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private void ClearAuth() => _client.DefaultRequestHeaders.Authorization = null;

    [Fact]
    public async Task GetItems_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.GetAsync("/api/inventory/items");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateItem_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.PostAsJsonAsync("/api/inventory/items", new { });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetItemById_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.GetAsync($"/api/inventory/items/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetItemByBarcode_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.GetAsync("/api/inventory/items/by-barcode/BC-TEST");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AdjustStock_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.PostAsJsonAsync($"/api/inventory/items/{Guid.NewGuid()}/adjust", new { });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetStockAlerts_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.GetAsync("/api/inventory/stock/alerts");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task IssueStock_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.PostAsJsonAsync("/api/inventory/stock/issue", new { });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMovements_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.GetAsync("/api/inventory/movements");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetWarehouses_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.GetAsync("/api/inventory/warehouses");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateWarehouse_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.PostAsJsonAsync("/api/inventory/warehouses", new { });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetItemCategories_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.GetAsync("/api/inventory/item-categories");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUnitsOfMeasure_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.GetAsync("/api/inventory/units-of-measure");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
