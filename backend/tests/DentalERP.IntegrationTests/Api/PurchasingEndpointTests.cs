using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace DentalERP.IntegrationTests.Api;

public class PurchasingEndpointTests(DentalERPTestFactory factory)
    : IClassFixture<DentalERPTestFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private void ClearAuth() => _client.DefaultRequestHeaders.Authorization = null;

    // Supplier endpoints
    [Fact]
    public async Task GetSuppliers_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.GetAsync("/api/suppliers/");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateSupplier_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.PostAsJsonAsync("/api/suppliers/", new { });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetSupplierById_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.GetAsync($"/api/suppliers/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateSupplier_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.PutAsJsonAsync($"/api/suppliers/{Guid.NewGuid()}", new { });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetSupplierBalance_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.GetAsync($"/api/suppliers/{Guid.NewGuid()}/balance");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetSupplierStatement_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.GetAsync($"/api/suppliers/{Guid.NewGuid()}/statement");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetSupplierCatalog_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.GetAsync($"/api/suppliers/{Guid.NewGuid()}/catalog");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // PO endpoints
    [Fact]
    public async Task GetPurchaseOrders_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.GetAsync("/api/purchasing/purchase-orders");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreatePurchaseOrder_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.PostAsJsonAsync("/api/purchasing/purchase-orders", new { });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ApprovePurchaseOrder_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.PostAsJsonAsync($"/api/purchasing/purchase-orders/{Guid.NewGuid()}/approve", new { });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SendPurchaseOrder_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.PostAsJsonAsync($"/api/purchasing/purchase-orders/{Guid.NewGuid()}/send", new { });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CancelPurchaseOrder_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.PostAsJsonAsync($"/api/purchasing/purchase-orders/{Guid.NewGuid()}/cancel", new { });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // GR endpoints
    [Fact]
    public async Task CreateGoodsReceipt_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.PostAsJsonAsync("/api/purchasing/goods-receipts", new { });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetGoodsReceiptById_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.GetAsync($"/api/purchasing/goods-receipts/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // Payment endpoints
    [Fact]
    public async Task RecordSupplierPayment_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.PostAsJsonAsync("/api/purchasing/supplier-payments", new { });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // Return endpoints
    [Fact]
    public async Task GetPurchaseReturns_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.GetAsync("/api/purchasing/purchase-returns");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreatePurchaseReturn_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.PostAsJsonAsync("/api/purchasing/purchase-returns", new { });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ConfirmPurchaseReturn_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.PostAsJsonAsync($"/api/purchasing/purchase-returns/{Guid.NewGuid()}/confirm", new { });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
