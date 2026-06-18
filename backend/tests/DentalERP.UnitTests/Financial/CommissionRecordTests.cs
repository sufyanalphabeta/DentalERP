using DentalERP.Modules.Financial.Domain.Entities;
using FluentAssertions;

namespace DentalERP.UnitTests.Financial;

public sealed class CommissionRecordTests
{
    private static CommissionRecord CreateRecord(
        string method = "percentage_of_service",
        decimal baseAmount = 500m,
        decimal rate = 10m,
        decimal commissionAmount = 50m)
        => CommissionRecord.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            method, baseAmount, rate, commissionAmount);

    // ── Create ──────────────────────────────────────────────────

    [Fact]
    public void Create_IsPaidIsFalse()
    {
        var record = CreateRecord();
        record.IsPaid.Should().BeFalse();
    }

    [Fact]
    public void Create_PaidAtIsNull()
    {
        var record = CreateRecord();
        record.PaidAt.Should().BeNull();
    }

    [Fact]
    public void Create_VaultTransactionIdIsNull()
    {
        var record = CreateRecord();
        record.VaultTransactionId.Should().BeNull();
    }

    [Fact]
    public void Create_StoresCommissionMethod()
    {
        var record = CreateRecord("fixed_amount");
        record.CommissionMethod.Should().Be("fixed_amount");
    }

    [Fact]
    public void Create_StoresBaseAmountRateAndAmount()
    {
        var record = CreateRecord(baseAmount: 1000m, rate: 15m, commissionAmount: 150m);
        record.BaseAmount.Should().Be(1000m);
        record.CommissionRate.Should().Be(15m);
        record.CommissionAmount.Should().Be(150m);
    }

    [Fact]
    public void Create_ProcedureIdIsNullByDefault()
    {
        var record = CreateRecord();
        record.ProcedureId.Should().BeNull();
    }

    [Fact]
    public void Create_StoresProcedureId_WhenProvided()
    {
        var procedureId = Guid.NewGuid();
        var record = CommissionRecord.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "percentage_of_service", 500m, 10m, 50m, procedureId);

        record.ProcedureId.Should().Be(procedureId);
    }

    [Fact]
    public void Create_GeneratesUniqueId()
    {
        var r1 = CreateRecord();
        var r2 = CreateRecord();
        r1.Id.Should().NotBe(r2.Id);
    }

    // ── MarkPaid ─────────────────────────────────────────────────

    [Fact]
    public void MarkPaid_SetsIsPaidTrue()
    {
        var record = CreateRecord();
        record.MarkPaid(Guid.NewGuid());
        record.IsPaid.Should().BeTrue();
    }

    [Fact]
    public void MarkPaid_SetsPaidAt()
    {
        var record = CreateRecord();
        var before = DateTime.UtcNow;
        record.MarkPaid(Guid.NewGuid());
        record.PaidAt.Should().NotBeNull();
        record.PaidAt!.Value.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void MarkPaid_SetsVaultTransactionId()
    {
        var txId = Guid.NewGuid();
        var record = CreateRecord();
        record.MarkPaid(txId);
        record.VaultTransactionId.Should().Be(txId);
    }

    [Fact]
    public void MarkPaid_CalledTwice_OverwritesVaultTransactionId()
    {
        var record = CreateRecord();
        record.MarkPaid(Guid.NewGuid());
        var secondTx = Guid.NewGuid();
        record.MarkPaid(secondTx);
        record.VaultTransactionId.Should().Be(secondTx);
    }
}
