using System.Security.Claims;
using AwesomeAssertions;
using Commons.Services;

namespace Commons.Test.Services;

public class CurrentUserTests
{
    [Fact]
    public void From_Reads_Subject_And_Manager_Role()
    {
        var identity = new ClaimsIdentity(
        [
            new Claim("sub", "user-123"),
            new Claim(ClaimTypes.Role, CurrentUser.ManagerRole)
        ], authenticationType: "Test");
        var principal = new ClaimsPrincipal(identity);

        var user = CurrentUser.From(principal);

        user.userId.Should().Be("user-123");
        user.isManager.Should().BeTrue();
    }

    [Fact]
    public void From_Treats_Customer_As_Not_Manager()
    {
        var identity = new ClaimsIdentity(
        [
            new Claim("sub", "user-456"),
            new Claim(ClaimTypes.Role, CurrentUser.CustomerRole)
        ], authenticationType: "Test");

        var user = CurrentUser.From(new ClaimsPrincipal(identity));

        user.isManager.Should().BeFalse();
    }

    [Fact]
    public void From_Throws_When_Subject_Claim_Is_Missing()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(authenticationType: "Test"));

        var act = () => CurrentUser.From(principal);

        act.Should().Throw<UnauthorizedAccessException>();
    }

    [Fact]
    public void EnsureManager_Throws_For_Customers()
    {
        var customer = new CurrentUser("user-1", false);

        var act = customer.EnsureManager;

        act.Should().Throw<UnauthorizedAccessException>();
    }

    [Fact]
    public void EnsureManager_Does_Not_Throw_For_Managers()
    {
        var manager = new CurrentUser("user-1", true);

        var act = manager.EnsureManager;

        act.Should().NotThrow();
    }
}
