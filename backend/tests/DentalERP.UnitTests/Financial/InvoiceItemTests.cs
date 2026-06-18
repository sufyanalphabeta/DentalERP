using DentalERP.Modules.Financial.Domain.Entities;
using FluentAssertions;

namespace DentalERP.UnitTests.Financial;

public sealed class InvoiceItemTests
{
    private static Guid InvoiceId => Guid.NewGuid();

    [Fact]
    public void Create_SetsFieldsCorrectly()
    {
        var item = InvoiceItem.Create(InvoiceId, "Filling", 100m, quantity: 2, discount: 10m);

        item.ServiceName.Should().Be("Filling");
        item.UnitPrice.Should().Be(100m);
        item.Quantity.Should().Be(2);
        item.Discount.Should().Be(10m);
        item.Total.Should().Be(190m); // (100*2) - 10
    }

    [Fact]
    public void Create_DefaultQuantityIsOne()
    {
        var item = InvoiceItem.Create(InvoiceId, "Extraction", 200m);

        item.Quantity.Should().Be(1);
        item.Total.Should().Be(200m);
    }

    [Fact]
    public void Create_ZeroDiscount_TotalEqualsUnitPriceTimesQuantity()
    {
        var item = InvoiceItem.Create(InvoiceId, "Cleaning", 150m, quantity: 3);

        item.Total.Should().Be(450m);
    }

    [Fact]
    public void Create_TotalFloorIsZero_WhenDiscountExceedsPrice()
    {
        var item = InvoiceItem.Create(InvoiceId, "Consultation", 50m, quantity: 1, discount: 200m);

        item.Total.Should().Be(0m);
    }

    [Fact]
    public void Create_StoresProcedureId_WhenProvided()
    {
        var procedureId = Guid.NewGuid();
        var item = InvoiceItem.Create(InvoiceId, "Root Canal", 500m, procedureId: procedureId);

        item.ProcedureId.Should().Be(procedureId);
    }

    [Fact]
    public void Create_ProcedureIdIsNull_WhenNotProvided()
    {
        var item = InvoiceItem.Create(InvoiceId, "Scaling", 300m);

        item.ProcedureId.Should().BeNull();
    }

    [Fact]
    public void Create_StoresServiceCode_WhenProvided()
    {
        var item = InvoiceItem.Create(InvoiceId, "Crown", 1200m, serviceCode: "CR-001");

        item.ServiceCode.Should().Be("CR-001");
    }

    [Fact]
    public void Create_GeneratesUniqueId()
    {
        var item1 = InvoiceItem.Create(InvoiceId, "A", 100m);
        var item2 = InvoiceItem.Create(InvoiceId, "B", 200m);

        item1.Id.Should().NotBe(item2.Id);
    }
}
