using DentalERP.SharedKernel.Abstractions;
using DentalERP.Modules.IAM.Domain.Events;

namespace DentalERP.Modules.IAM.Domain.Entities;

public sealed class User : BaseEntity
{
    public string Username { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public string FullName { get; private set; } = default!;
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public bool IsActive { get; private set; } = true;
    public bool MustChangePassword { get; private set; } = true;
    public DateTime? LastLoginAt { get; private set; }

    private readonly List<UserRole> _userRoles = [];
    public IReadOnlyList<UserRole> UserRoles => _userRoles.AsReadOnly();

    private readonly List<RefreshToken> _refreshTokens = [];
    public IReadOnlyList<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    private User() { }

    public static User Create(string username, string passwordHash, string fullName, string? email = null, string? phone = null)
    {
        var user = new User
        {
            Username = username.ToLowerInvariant(),
            PasswordHash = passwordHash,
            FullName = fullName,
            Email = email,
            Phone = phone
        };
        user.AddDomainEvent(new UserCreatedEvent(user.Id, user.Username));
        return user;
    }

    public void Update(string fullName, string? email, string? phone)
    {
        FullName = fullName;
        Email = email;
        Phone = phone;
        Touch();
    }

    public void ChangePassword(string newHash, bool clearMustChange = false)
    {
        PasswordHash = newHash;
        if (clearMustChange) MustChangePassword = false;
        Touch();
    }

    public void SetMustChangePassword(bool value)
    {
        MustChangePassword = value;
        Touch();
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
        Touch();
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        Touch();
    }

    public void AddRole(Guid roleId)
    {
        if (_userRoles.Any(ur => ur.RoleId == roleId)) return;
        _userRoles.Add(new UserRole { UserId = Id, RoleId = roleId });
        Touch();
    }

    public void ClearRoles()
    {
        _userRoles.Clear();
        Touch();
    }

    public void Delete()
    {
        SoftDelete();
    }

    public RefreshToken AddRefreshToken(string token, DateTime expiresAt)
    {
        var rt = new RefreshToken
        {
            UserId = Id,
            Token = token,
            ExpiresAt = expiresAt
        };
        _refreshTokens.Add(rt);
        return rt;
    }

    public void RevokeRefreshToken(string token)
    {
        var rt = _refreshTokens.FirstOrDefault(t => t.Token == token);
        if (rt is not null) rt.Revoke();
    }

    public void RevokeAllRefreshTokens()
    {
        foreach (var rt in _refreshTokens.Where(t => !t.IsRevoked))
            rt.Revoke();
    }
}

public sealed class UserRole
{
    public Guid UserId { get; init; }
    public Guid RoleId { get; init; }
    public User User { get; init; } = default!;
    public Role Role { get; init; } = default!;
}

public sealed class RefreshToken
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid UserId { get; init; }
    public string Token { get; init; } = default!;
    public DateTime ExpiresAt { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; private set; }
    public bool IsRevoked => RevokedAt.HasValue;
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;

    public User User { get; init; } = default!;

    internal void Revoke() => RevokedAt = DateTime.UtcNow;
}
