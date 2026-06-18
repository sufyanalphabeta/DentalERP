using FluentValidation;

namespace DentalERP.Modules.Inventory.Features.CreateManualIssue;

public sealed class CreateManualIssueCommandValidator : AbstractValidator<CreateManualIssueCommand>
{
    private static readonly string[] ValidDestinations = ["Clinic", "Lab", "Radiology", "Doctor", "Other"];

    public CreateManualIssueCommandValidator()
    {
        RuleFor(x => x.ItemId).NotEmpty();
        RuleFor(x => x.WarehouseId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.DestinationType).NotEmpty().Must(d => ValidDestinations.Contains(d))
            .WithMessage("Destination type must be one of: Clinic, Lab, Radiology, Doctor, Other");
    }
}
