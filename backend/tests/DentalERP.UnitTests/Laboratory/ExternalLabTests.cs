using DentalERP.Modules.Laboratory.Domain.Entities;
using FluentAssertions;

namespace DentalERP.UnitTests.Laboratory;

public class ExternalLabTests
{
    [Fact]
    public void Create_SetsActiveByDefault()
    {
        var lab = ExternalLab.Create("LabCo", "Ali", "0501111111", null, null, null);
        lab.IsActive.Should().BeTrue();
        lab.Name.Should().Be("LabCo");
    }

    [Fact]
    public void Update_ChangesName()
    {
        var lab = ExternalLab.Create("LabCo", null, null, null, null, null);
        lab.Update("NewLabCo", "Ahmed", "0502222222", "lab@lab.com", "Riyadh", "VIP lab");
        lab.Name.Should().Be("NewLabCo");
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var lab = ExternalLab.Create("LabCo", null, null, null, null, null);
        lab.Deactivate();
        lab.IsActive.Should().BeFalse();
    }
}
