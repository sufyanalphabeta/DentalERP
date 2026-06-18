using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace DentalERP.IntegrationTests.Api;

public class InsuranceEndpointTests(DentalERPTestFactory factory)
    : IClassFixture<DentalERPTestFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private void ClearAuth()
        => _client.DefaultRequestHeaders.Authorization = null;

    [Fact]
    public async Task GetInsuranceCompanies_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.GetAsync("/api/insurance/companies");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateInsuranceCompany_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.PostAsJsonAsync("/api/insurance/companies", new { name = "Test Insurance" });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetInsuranceClaims_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.GetAsync("/api/insurance/claims");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateInsuranceClaim_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.PostAsJsonAsync("/api/insurance/claims", new { });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SubmitInsuranceClaim_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.PostAsJsonAsync($"/api/insurance/claims/{Guid.NewGuid()}/submit", new { });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RecordInsurancePayment_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.PostAsJsonAsync($"/api/insurance/claims/{Guid.NewGuid()}/payment", new { });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RejectInsuranceClaim_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.PostAsJsonAsync($"/api/insurance/claims/{Guid.NewGuid()}/reject", new { reason = "test" });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateVaultTransfer_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.PostAsJsonAsync("/api/vaults/transfer", new { });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
