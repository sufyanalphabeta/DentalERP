using System.Net;
using System.Net.Http.Json;
using DentalERP.Modules.IAM.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DentalERP.IntegrationTests.Api;

public class DentalERPTestFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"] = "test-secret-key-that-is-long-enough-32chars!!",
                ["Jwt:Issuer"] = "DentalERP",
                ["Jwt:Audience"] = "DentalERP-Clients",
                ["Jwt:AccessTokenExpiryMinutes"] = "60",
                ["Jwt:RefreshTokenExpiryDays"] = "30",
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=dentalerp_test;Username=test;Password=test"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Replace real DB with in-memory for fast integration tests
            services.RemoveAll<DbContextOptions<IAMDbContext>>();
            services.RemoveAll<IAMDbContext>();
            services.AddDbContext<IAMDbContext>(opts =>
                opts.UseInMemoryDatabase("dentalerp_integration_test"));

            // Ensure DB is created
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            scope.ServiceProvider.GetRequiredService<IAMDbContext>().Database.EnsureCreated();
        });
    }
}

public class AuthEndpointTests(DentalERPTestFactory factory)
    : IClassFixture<DentalERPTestFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task HealthCheck_ReturnsOk()
    {
        var response = await _client.GetAsync("/health");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            username = "nonexistent",
            password = "wrongpass123"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithEmptyUsername_Returns4xx()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            username = "",
            password = "password123"
        });

        ((int)response.StatusCode).Should().BeInRange(400, 499);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/api/users");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
