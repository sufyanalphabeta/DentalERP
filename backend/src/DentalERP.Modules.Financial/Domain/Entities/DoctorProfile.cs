namespace DentalERP.Modules.Financial.Domain.Entities;

public sealed class DoctorProfile
{
    public static readonly string[] ValidMethods =
        ["percentage_of_service", "fixed_amount", "percentage_of_net_service"];

    public Guid UserId { get; private set; }
    public string CommissionMethod { get; private set; } = "percentage_of_service";
    public decimal DefaultCommissionValue { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private DoctorProfile() { }

    public static DoctorProfile Create(Guid userId, string commissionMethod = "percentage_of_service", decimal commissionValue = 0)
        => new()
        {
            UserId = userId,
            CommissionMethod = commissionMethod,
            DefaultCommissionValue = commissionValue,
            UpdatedAt = DateTime.UtcNow
        };

    public void Update(string commissionMethod, decimal commissionValue)
    {
        CommissionMethod = commissionMethod;
        DefaultCommissionValue = commissionValue;
        UpdatedAt = DateTime.UtcNow;
    }
}
