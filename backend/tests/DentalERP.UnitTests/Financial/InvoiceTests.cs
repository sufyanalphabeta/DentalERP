using DentalERP.Modules.Financial.Domain.Entities;
using FluentAssertions;

namespace DentalERP.UnitTests.Financial;

public sealed class InvoiceTests
{
    private static Invoice CreateInvoice() =>
        Invoice.Create("INV-2024-000001", Guid.NewGuid(), Guid.NewGuid());

    private static InvoiceItem MakeItem(decimal price = 100m, short qty = 1, decimal discount = 0m) =>
        InvoiceItem.Create(Guid.NewGuid(), "Filling", price, qty, discount);

    // ── Create ──────────────────────────────────────────────────

    [Fact]
    public void Create_StatusIsDraft()
    {
        var inv = CreateInvoice();
        inv.Status.Should().Be("Draft");
    }

    [Fact]
    public void Create_TotalsAreZero()
    {
        var inv = CreateInvoice();
        inv.TotalAmount.Should().Be(0m);
        inv.PaidAmount.Should().Be(0m);
        inv.Remaining.Should().Be(0m);
    }

    [Fact]
    public void Create_ItemsListIsEmpty()
    {
        var inv = CreateInvoice();
        inv.Items.Should().BeEmpty();
    }

    [Fact]
    public void Create_StoresInvoiceNumber()
    {
        var inv = Invoice.Create("INV-2024-000099", Guid.NewGuid(), Guid.NewGuid());
        inv.InvoiceNumber.Should().Be("INV-2024-000099");
    }

    // ── AddItem / RecalculateTotals ──────────────────────────────

    [Fact]
    public void AddItem_RecalculatesTotals()
    {
        var inv = CreateInvoice();
        inv.AddItem(MakeItem(200m, 2, 50m)); // subtotal=400, discount=50, total=350

        inv.Subtotal.Should().Be(400m);
        inv.DiscountTotal.Should().Be(50m);
        inv.TotalAmount.Should().Be(350m);
    }

    [Fact]
    public void AddItem_MultipleItems_SumsCorrectly()
    {
        var inv = CreateInvoice();
        inv.AddItem(MakeItem(100m));
        inv.AddItem(MakeItem(200m));
        inv.AddItem(MakeItem(300m));

        inv.TotalAmount.Should().Be(600m);
    }

    [Fact]
    public void AddItem_SetsItemsCollection()
    {
        var inv = CreateInvoice();
        inv.AddItem(MakeItem(100m));
        inv.AddItem(MakeItem(150m));

        inv.Items.Should().HaveCount(2);
    }

    // ── Confirm ──────────────────────────────────────────────────

    [Fact]
    public void Confirm_FromDraft_WithItems_Succeeds()
    {
        var inv = CreateInvoice();
        inv.AddItem(MakeItem(100m));

        var result = inv.Confirm();

        result.IsSuccess.Should().BeTrue();
        inv.Status.Should().Be("Confirmed");
    }

    [Fact]
    public void Confirm_FromDraft_WithoutItems_Fails()
    {
        var inv = CreateInvoice();

        var result = inv.Confirm();

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Invoice.NoItems");
    }

    [Fact]
    public void Confirm_AlreadyConfirmed_Fails()
    {
        var inv = CreateInvoice();
        inv.AddItem(MakeItem());
        inv.Confirm();

        var result = inv.Confirm();

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Invoice.NotDraft");
    }

    [Fact]
    public void Confirm_FromCancelled_Fails()
    {
        var inv = CreateInvoice();
        inv.AddItem(MakeItem());
        inv.Cancel("test");

        var result = inv.Confirm();

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Invoice.NotDraft");
    }

    // ── Cancel ──────────────────────────────────────────────────

    [Fact]
    public void Cancel_FromDraft_Succeeds()
    {
        var inv = CreateInvoice();

        var result = inv.Cancel("Mistake");

        result.IsSuccess.Should().BeTrue();
        inv.Status.Should().Be("Cancelled");
        inv.CancelledReason.Should().Be("Mistake");
    }

    [Fact]
    public void Cancel_FromConfirmed_Succeeds()
    {
        var inv = CreateInvoice();
        inv.AddItem(MakeItem());
        inv.Confirm();

        var result = inv.Cancel("Patient changed mind");

        result.IsSuccess.Should().BeTrue();
        inv.Status.Should().Be("Cancelled");
    }

    [Fact]
    public void Cancel_FromPaid_Fails()
    {
        var inv = CreateInvoice();
        inv.AddItem(MakeItem(100m));
        inv.Confirm();
        inv.ApplyPayment(100m);

        var result = inv.Cancel("Oops");

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Invoice.CannotCancel");
    }

    [Fact]
    public void Cancel_AlreadyCancelled_Fails()
    {
        var inv = CreateInvoice();
        inv.Cancel("First cancel");

        var result = inv.Cancel("Second cancel");

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Invoice.CannotCancel");
    }

    // ── ApplyPayment ─────────────────────────────────────────────

    [Fact]
    public void ApplyPayment_PartialPayment_StatusIsPartiallyPaid()
    {
        var inv = CreateInvoice();
        inv.AddItem(MakeItem(200m));
        inv.Confirm();

        var result = inv.ApplyPayment(100m);

        result.IsSuccess.Should().BeTrue();
        inv.Status.Should().Be("PartiallyPaid");
        inv.PaidAmount.Should().Be(100m);
        inv.Remaining.Should().Be(100m);
    }

    [Fact]
    public void ApplyPayment_FullPayment_StatusIsPaid()
    {
        var inv = CreateInvoice();
        inv.AddItem(MakeItem(300m));
        inv.Confirm();

        inv.ApplyPayment(300m);

        inv.Status.Should().Be("Paid");
        inv.PaidAmount.Should().Be(300m);
        inv.Remaining.Should().Be(0m);
    }

    [Fact]
    public void ApplyPayment_MultipleParts_AccumulatesCorrectly()
    {
        var inv = CreateInvoice();
        inv.AddItem(MakeItem(600m));
        inv.Confirm();

        inv.ApplyPayment(200m);
        inv.ApplyPayment(200m);

        inv.PaidAmount.Should().Be(400m);
        inv.Remaining.Should().Be(200m);
        inv.Status.Should().Be("PartiallyPaid");
    }

    [Fact]
    public void ApplyPayment_FromDraft_Fails()
    {
        var inv = CreateInvoice();
        inv.AddItem(MakeItem(100m));

        var result = inv.ApplyPayment(50m);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Invoice.Draft");
    }

    [Fact]
    public void ApplyPayment_ToCancelled_Fails()
    {
        var inv = CreateInvoice();
        inv.Cancel("Reason");

        var result = inv.ApplyPayment(50m);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Invoice.Cancelled");
    }

    [Fact]
    public void ApplyPayment_ZeroAmount_Fails()
    {
        var inv = CreateInvoice();
        inv.AddItem(MakeItem(100m));
        inv.Confirm();

        var result = inv.ApplyPayment(0m);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Payment.InvalidAmount");
    }

    [Fact]
    public void ApplyPayment_NegativeAmount_Fails()
    {
        var inv = CreateInvoice();
        inv.AddItem(MakeItem(100m));
        inv.Confirm();

        var result = inv.ApplyPayment(-10m);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Payment.InvalidAmount");
    }

    [Fact]
    public void ApplyPayment_ExceedsRemaining_Fails()
    {
        var inv = CreateInvoice();
        inv.AddItem(MakeItem(100m));
        inv.Confirm();

        var result = inv.ApplyPayment(150m);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Payment.ExceedsRemaining");
    }

    // ── Remaining computed property ───────────────────────────────

    [Fact]
    public void Remaining_IsComputedFromTotalMinusPaid()
    {
        var inv = CreateInvoice();
        inv.AddItem(MakeItem(500m));
        inv.Confirm();
        inv.ApplyPayment(150m);

        inv.Remaining.Should().Be(350m);
    }
}
