using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace DentalERP.IntegrationTests.Api;

public class FinancialEndpointTests(DentalERPTestFactory factory)
    : IClassFixture<DentalERPTestFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private void ClearAuth()
        => _client.DefaultRequestHeaders.Authorization = null;

    // ── Services Catalog ─────────────────────────────────────────

    [Fact]
    public async Task GetServices_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.GetAsync("/api/services");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateService_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.PostAsJsonAsync("/api/services", new
        {
            name = "حشوة",
            price = 150.0
        });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateService_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.PutAsJsonAsync($"/api/services/{Guid.NewGuid()}", new
        {
            name = "Updated",
            price = 200.0
        });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Invoices ─────────────────────────────────────────────────

    [Fact]
    public async Task GetInvoices_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.GetAsync("/api/invoices");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetInvoiceById_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.GetAsync($"/api/invoices/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateInvoice_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.PostAsJsonAsync("/api/invoices", new
        {
            patientId = Guid.NewGuid(),
            doctorId = Guid.NewGuid(),
            items = new[]
            {
                new { serviceName = "Filling", unitPrice = 100.0, quantity = 1 }
            }
        });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CancelInvoice_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.PostAsJsonAsync($"/api/invoices/{Guid.NewGuid()}/cancel", new
        {
            reason = "Mistake"
        });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Payments ─────────────────────────────────────────────────

    [Fact]
    public async Task AddPayment_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.PostAsJsonAsync($"/api/invoices/{Guid.NewGuid()}/payments", new
        {
            vaultId = Guid.NewGuid(),
            amount = 100.0,
            paymentMethod = "cash"
        });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Treasury ─────────────────────────────────────────────────

    [Fact]
    public async Task GetVaultBalances_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.GetAsync("/api/treasury/vaults/balances");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetDoctorAccount_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.GetAsync($"/api/treasury/doctors/{Guid.NewGuid()}/account");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PayCommission_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.PostAsJsonAsync($"/api/treasury/commissions/{Guid.NewGuid()}/pay", new
        {
            vaultId = Guid.NewGuid()
        });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Installments ─────────────────────────────────────────────

    [Fact]
    public async Task CreateInstallmentPlan_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.PostAsJsonAsync("/api/installments/plans", new
        {
            invoiceId = Guid.NewGuid(),
            patientId = Guid.NewGuid(),
            totalAmount = 600.0,
            installmentsCount = 3,
            startDate = DateTime.UtcNow.ToString("o")
        });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PayInstallment_WithoutToken_Returns401()
    {
        ClearAuth();
        var planId = Guid.NewGuid();
        var response = await _client.PostAsJsonAsync($"/api/installments/{planId}/pay/1", new
        {
            vaultId = Guid.NewGuid(),
            paymentMethod = "cash"
        });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateAdvancePayment_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.PostAsJsonAsync("/api/advance-payments", new
        {
            patientId = Guid.NewGuid(),
            vaultId = Guid.NewGuid(),
            amount = 500.0
        });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
