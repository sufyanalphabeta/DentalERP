using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace DentalERP.IntegrationTests.Api;

public class ExpensesEndpointTests(DentalERPTestFactory factory)
    : IClassFixture<DentalERPTestFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private void ClearAuth() => _client.DefaultRequestHeaders.Authorization = null;

    // Expense Categories
    [Fact]
    public async Task GetExpenseCategories_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.GetAsync("/api/expenses/categories");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateExpenseCategory_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.PostAsJsonAsync("/api/expenses/categories", new { Name = "Test" });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // Expenses
    [Fact]
    public async Task GetExpenses_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.GetAsync("/api/expenses/");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetExpenseById_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.GetAsync($"/api/expenses/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateExpense_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.PostAsJsonAsync("/api/expenses/", new { });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateExpense_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.PutAsJsonAsync($"/api/expenses/{Guid.NewGuid()}", new { });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteExpense_WithoutToken_Returns401()
    {
        ClearAuth();
        var response = await _client.DeleteAsync($"/api/expenses/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // PDF Report
    [Fact]
    public async Task GetExpenseReportPdf_WithoutToken_Returns401()
    {
        ClearAuth();
        var from = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1));
        var to = DateOnly.FromDateTime(DateTime.Today);
        var response = await _client.GetAsync($"/api/expenses/report/pdf?dateFrom={from:yyyy-MM-dd}&dateTo={to:yyyy-MM-dd}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
