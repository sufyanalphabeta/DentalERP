namespace DentalERP.SharedKernel.Interfaces;

public interface ICurrentUser
{
    Guid UserId { get; }
    string Username { get; }
    bool IsAuthenticated { get; }
    IReadOnlyList<string> Permissions { get; }
    bool HasPermission(string permission);
}
