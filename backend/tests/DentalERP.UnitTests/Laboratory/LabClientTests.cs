using DentalERP.Modules.Laboratory.Domain.Entities;
using FluentAssertions;

namespace DentalERP.UnitTests.Laboratory;

public class LabClientTests
{
    [Theory]
    [InlineData("Doctor")]
    [InlineData("Clinic")]
    [InlineData("ExternalClient")]
    public void Create_WithValidClientType_Succeeds(string clientType)
    {
        var client = LabClient.Create("Test Client", clientType, null, null, null);
        client.ClientType.Should().Be(clientType);
        client.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WithInvalidClientType_ThrowsArgumentException()
    {
        var act = () => LabClient.Create("Test", "Unknown", null, null, null);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var client = LabClient.Create("Test", "Doctor", null, null, null);
        client.Deactivate();
        client.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_SetsIsActiveTrue()
    {
        var client = LabClient.Create("Test", "Doctor", null, null, null);
        client.Deactivate();
        client.Activate();
        client.IsActive.Should().BeTrue();
    }
}
