using DentalERP.Modules.Laboratory.Domain.Entities;
using FluentAssertions;

namespace DentalERP.UnitTests.Laboratory;

public class LabOrderItemTests
{
    [Fact]
    public void Create_CalculatesTotalCost()
    {
        var item = LabOrderItem.Create(Guid.NewGuid(), "Crown", 250m, 3);
        item.TotalCost.Should().Be(750m);
        item.UnitCost.Should().Be(250m);
        item.Quantity.Should().Be(3);
    }

    [Fact]
    public void Create_WithZeroQuantity_ThrowsArgumentException()
    {
        var act = () => LabOrderItem.Create(Guid.NewGuid(), "Crown", 250m, 0);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithNegativeQuantity_ThrowsArgumentException()
    {
        var act = () => LabOrderItem.Create(Guid.NewGuid(), "Crown", 250m, -1);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithNegativePrice_ThrowsArgumentException()
    {
        var act = () => LabOrderItem.Create(Guid.NewGuid(), "Crown", -1m, 1);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithZeroPrice_Succeeds()
    {
        var item = LabOrderItem.Create(Guid.NewGuid(), "Free service", 0m, 1);
        item.TotalCost.Should().Be(0m);
    }
}
