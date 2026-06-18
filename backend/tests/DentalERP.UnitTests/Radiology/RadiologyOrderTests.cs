using DentalERP.Modules.Radiology.Domain.Entities;
using FluentAssertions;

namespace DentalERP.UnitTests.Radiology;

public class RadiologyOrderTests
{
    private static RadiologyOrder CreateOrder(bool isExternal = false)
    {
        if (isExternal)
            return RadiologyOrder.Create("RAD-2026-000001", Guid.NewGuid(), 200m,
                null, true, "External Patient", "0501234567",
                null, null, null);
        return RadiologyOrder.Create("RAD-2026-000001", Guid.NewGuid(), 200m,
            Guid.NewGuid(), false, null, null, Guid.NewGuid(), null, null);
    }

    [Fact]
    public void Create_InternalPatient_SetsOrderedStatus()
    {
        var order = CreateOrder();
        order.Status.Should().Be("Ordered");
        order.IsExternalPatient.Should().BeFalse();
    }

    [Fact]
    public void Create_ExternalPatient_SetsFields()
    {
        var order = CreateOrder(isExternal: true);
        order.IsExternalPatient.Should().BeTrue();
        order.ExternalPatientName.Should().Be("External Patient");
    }

    [Fact]
    public void Create_InternalWithoutPatientId_ThrowsArgumentException()
    {
        var act = () => RadiologyOrder.Create("RAD-2026-000001", Guid.NewGuid(), 200m,
            null, false, null, null, null, null, null);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_ExternalWithoutName_ThrowsArgumentException()
    {
        var act = () => RadiologyOrder.Create("RAD-2026-000001", Guid.NewGuid(), 200m,
            null, true, null, null, null, null, null);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void MarkImaged_FromOrdered_Succeeds()
    {
        var order = CreateOrder();
        var result = order.MarkImaged();
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be("Imaged");
    }

    [Fact]
    public void MarkImaged_FromImaged_Fails()
    {
        var order = CreateOrder();
        order.MarkImaged();
        var result = order.MarkImaged();
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void SaveReport_FromImaged_Succeeds()
    {
        var order = CreateOrder();
        order.MarkImaged();
        var result = order.SaveReport("Normal findings", Guid.NewGuid());
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be("ReportSaved");
        order.Report.Should().NotBeNull();
    }

    [Fact]
    public void SaveReport_FromOrdered_Fails()
    {
        var order = CreateOrder();
        var result = order.SaveReport("Report text", Guid.NewGuid());
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void UpdateReport_WhenReportExists_Succeeds()
    {
        var order = CreateOrder();
        order.MarkImaged();
        order.SaveReport("Initial", Guid.NewGuid());
        var result = order.UpdateReport("Updated text");
        result.IsSuccess.Should().BeTrue();
        order.Report!.ReportText.Should().Be("Updated text");
    }

    [Fact]
    public void UpdateReport_WhenNoReport_Fails()
    {
        var order = CreateOrder();
        var result = order.UpdateReport("No report yet");
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Complete_FromReportSaved_Succeeds()
    {
        var order = CreateOrder();
        order.MarkImaged();
        order.SaveReport("Report", Guid.NewGuid());
        var result = order.Complete();
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be("Completed");
    }

    [Fact]
    public void Complete_FromImaged_Fails()
    {
        var order = CreateOrder();
        order.MarkImaged();
        var result = order.Complete();
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Cancel_FromOrdered_Succeeds()
    {
        var order = CreateOrder();
        var result = order.Cancel("Cancelled by patient");
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be("Cancelled");
        order.CancellationReason.Should().Be("Cancelled by patient");
    }

    [Fact]
    public void Cancel_FromCompleted_Fails()
    {
        var order = CreateOrder();
        order.MarkImaged();
        order.SaveReport("Report", Guid.NewGuid());
        order.Complete();
        var result = order.Cancel("Too late");
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Cancel_FromAlreadyCancelled_Fails()
    {
        var order = CreateOrder();
        order.Cancel("First cancel");
        var result = order.Cancel("Second cancel");
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void SetCommissions_StoresValues()
    {
        var order = CreateOrder();
        order.SetCommissions(30m, 10m);
        order.DoctorCommissionAmount.Should().Be(30m);
        order.TechCommissionAmount.Should().Be(10m);
    }

    [Fact]
    public void SetInvoice_StoresInvoiceId()
    {
        var order = CreateOrder();
        var invoiceId = Guid.NewGuid();
        order.SetInvoice(invoiceId);
        order.InvoiceId.Should().Be(invoiceId);
    }
}
