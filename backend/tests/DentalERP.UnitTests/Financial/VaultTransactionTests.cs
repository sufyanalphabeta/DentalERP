using DentalERP.Modules.Financial.Domain.Entities;
using FluentAssertions;

namespace DentalERP.UnitTests.Financial;

public sealed class VaultTransactionTests
{
    private static VaultTransaction CreateTx(
        string type = "receipt_from_patient",
        string direction = "in",
        decimal amount = 500m)
        => VaultTransaction.Create(Guid.NewGuid(), type, amount, direction);

    // ── Create ──────────────────────────────────────────────────

    [Fact]
    public void Create_IsReversedIsFalse()
    {
        var tx = CreateTx();
        tx.IsReversed.Should().BeFalse();
    }

    [Fact]
    public void Create_IsReversalDefaultsFalse()
    {
        var tx = CreateTx();
        tx.IsReversal.Should().BeFalse();
    }

    [Fact]
    public void Create_StoresDirection()
    {
        var tx = CreateTx(direction: "out");
        tx.Direction.Should().Be("out");
    }

    [Fact]
    public void Create_StoresTransactionType()
    {
        var tx = CreateTx("payment_to_doctor", "out");
        tx.TransactionType.Should().Be("payment_to_doctor");
    }

    [Fact]
    public void Create_StoresAmount()
    {
        var tx = CreateTx(amount: 750m);
        tx.Amount.Should().Be(750m);
    }

    [Fact]
    public void Create_ReversalFlag_IsStoredCorrectly()
    {
        var tx = VaultTransaction.Create(Guid.NewGuid(), "general_receipt", 100m, "in", isReversal: true);
        tx.IsReversal.Should().BeTrue();
    }

    [Fact]
    public void Create_RelatedIds_StoredWhenProvided()
    {
        var invoiceId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var tx = VaultTransaction.Create(Guid.NewGuid(), "receipt_from_patient", 200m, "in",
            relatedInvoiceId: invoiceId, relatedPatientId: patientId);

        tx.RelatedInvoiceId.Should().Be(invoiceId);
        tx.RelatedPatientId.Should().Be(patientId);
    }

    [Fact]
    public void Create_GeneratesUniqueId()
    {
        var t1 = CreateTx();
        var t2 = CreateTx();
        t1.Id.Should().NotBe(t2.Id);
    }

    // ── MarkReversed ─────────────────────────────────────────────

    [Fact]
    public void MarkReversed_SetsIsReversedTrue()
    {
        var tx = CreateTx();
        tx.MarkReversed();
        tx.IsReversed.Should().BeTrue();
    }

    [Fact]
    public void MarkReversed_CalledTwice_RemainsTrue()
    {
        var tx = CreateTx();
        tx.MarkReversed();
        tx.MarkReversed();
        tx.IsReversed.Should().BeTrue();
    }

    // ── ValidTypes ──────────────────────────────────────────────

    [Fact]
    public void ValidTypes_ContainsFiveExpectedTypes()
    {
        VaultTransaction.ValidTypes.Should().HaveCount(5);
        VaultTransaction.ValidTypes.Should().Contain([
            "receipt_from_patient",
            "payment_to_doctor",
            "general_receipt",
            "general_payment",
            "inter_vault_transfer"
        ]);
    }
}
