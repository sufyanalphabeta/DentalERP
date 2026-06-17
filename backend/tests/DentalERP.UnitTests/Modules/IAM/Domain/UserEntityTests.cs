using DentalERP.Modules.IAM.Domain.Entities;
using DentalERP.Modules.IAM.Domain.Events;
using FluentAssertions;

namespace DentalERP.UnitTests.Modules.IAM.Domain;

public class UserEntityTests
{
    [Fact]
    public void Create_ShouldSetProperties_AndRaiseDomainEvent()
    {
        var user = User.Create("TestUser", "hash123", "Test User", "test@test.com");

        user.Username.Should().Be("testuser");
        user.FullName.Should().Be("Test User");
        user.Email.Should().Be("test@test.com");
        user.IsActive.Should().BeTrue();
        user.IsDeleted.Should().BeFalse();
        user.DomainEvents.Should().ContainSingle(e => e is UserCreatedEvent);
    }

    [Fact]
    public void Create_ShouldLowercaseUsername()
    {
        var user = User.Create("AdminUser", "hash", "Admin");
        user.Username.Should().Be("adminuser");
    }

    [Fact]
    public void SetActive_False_ShouldDeactivateUser()
    {
        var user = User.Create("user1", "hash", "User One");
        user.SetActive(false);
        user.IsActive.Should().BeFalse();
        user.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Delete_ShouldSetDeletedAt()
    {
        var user = User.Create("user1", "hash", "User One");
        user.Delete();
        user.IsDeleted.Should().BeTrue();
        user.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public void AddRole_ShouldNotAddDuplicate()
    {
        var user = User.Create("user1", "hash", "User One");
        var roleId = Guid.NewGuid();

        user.AddRole(roleId);
        user.AddRole(roleId);

        user.UserRoles.Should().HaveCount(1);
    }

    [Fact]
    public void AddRefreshToken_ShouldAddToken()
    {
        var user = User.Create("user1", "hash", "User One");
        var token = user.AddRefreshToken("my-token", DateTime.UtcNow.AddDays(30));

        token.IsActive.Should().BeTrue();
        user.RefreshTokens.Should().ContainSingle();
    }

    [Fact]
    public void RevokeRefreshToken_ShouldDeactivateToken()
    {
        var user = User.Create("user1", "hash", "User One");
        user.AddRefreshToken("my-token", DateTime.UtcNow.AddDays(30));

        user.RevokeRefreshToken("my-token");

        user.RefreshTokens.First().IsRevoked.Should().BeTrue();
        user.RefreshTokens.First().IsActive.Should().BeFalse();
    }

    [Fact]
    public void RevokeAllRefreshTokens_ShouldRevokeAll()
    {
        var user = User.Create("user1", "hash", "User One");
        user.AddRefreshToken("token1", DateTime.UtcNow.AddDays(30));
        user.AddRefreshToken("token2", DateTime.UtcNow.AddDays(30));

        user.RevokeAllRefreshTokens();

        user.RefreshTokens.All(t => t.IsRevoked).Should().BeTrue();
    }

    [Fact]
    public void RefreshToken_IsExpired_WhenPastExpiryDate()
    {
        var user = User.Create("user1", "hash", "User One");
        var token = user.AddRefreshToken("expired-token", DateTime.UtcNow.AddDays(-1));

        token.IsExpired.Should().BeTrue();
        token.IsActive.Should().BeFalse();
    }
}
