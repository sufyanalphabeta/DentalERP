using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace DentalERP.IntegrationTests.Api;

public class ChartEndpointTests(DentalERPTestFactory factory)
    : IClassFixture<DentalERPTestFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetChart_WithoutToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync($"/api/patients/{Guid.NewGuid()}/chart");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PostChart_WithoutToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.PostAsJsonAsync($"/api/patients/{Guid.NewGuid()}/chart", new
        {
            toothId = 16,
            condition = "Caries"
        });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

public class TreatmentPlanEndpointTests(DentalERPTestFactory factory)
    : IClassFixture<DentalERPTestFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task CreateTreatmentPlan_WithoutToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.PostAsJsonAsync($"/api/patients/{Guid.NewGuid()}/treatment-plans", new
        {
            title = "خطة التقويم",
            estimatedCost = 5000
        });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateTreatmentPlanStatus_WithoutToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.PatchAsJsonAsync(
            $"/api/treatment-plans/{Guid.NewGuid()}/status",
            new { status = "Active" });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

public class ProcedureEndpointTests(DentalERPTestFactory factory)
    : IClassFixture<DentalERPTestFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task AddProcedure_WithoutToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.PostAsJsonAsync($"/api/appointments/{Guid.NewGuid()}/procedures", new
        {
            patientId = Guid.NewGuid(),
            procedureName = "حشو"
        });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

public class MediaEndpointTests(DentalERPTestFactory factory)
    : IClassFixture<DentalERPTestFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task UploadMedia_WithoutToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.PostAsJsonAsync($"/api/patients/{Guid.NewGuid()}/media", new
        {
            mediaType = "XRay",
            fileName = "xray.jpg",
            filePath = "patient-media/1/xray.jpg"
        });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

public class AssignmentEndpointTests(DentalERPTestFactory factory)
    : IClassFixture<DentalERPTestFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task AssignDoctor_WithoutToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.PostAsJsonAsync($"/api/patients/{Guid.NewGuid()}/doctors", new
        {
            doctorId = Guid.NewGuid()
        });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TransferDoctor_WithoutToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.PatchAsJsonAsync(
            $"/api/patients/{Guid.NewGuid()}/doctors/{Guid.NewGuid()}/transfer",
            new { newDoctorId = Guid.NewGuid() });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

public class TimelineEndpointTests(DentalERPTestFactory factory)
    : IClassFixture<DentalERPTestFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetTimeline_WithoutToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync($"/api/patients/{Guid.NewGuid()}/timeline");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetTimeline_WithCategoryFilter_WithoutToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync($"/api/patients/{Guid.NewGuid()}/timeline?category=Clinical");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
