using System.Security.Claims;
using AwesomeAssertions;
using Commons.Services;

namespace Commons.Test.Services;

public class KeycloakRealmRolesClaimsTransformationTests
{
    private readonly KeycloakRealmRolesClaimsTransformation transformation = new();

    [Fact]
    public async Task TransformAsync_Maps_Realm_Roles_To_Role_Claims()
    {
        var identity = new ClaimsIdentity(
        [
            new Claim("realm_access", """{"roles":["customer","offline_access"]}""")
        ], authenticationType: "Test");
        var principal = new ClaimsPrincipal(identity);

        var result = await transformation.TransformAsync(principal);

        result.IsInRole(CurrentUser.CustomerRole).Should().BeTrue();
        result.IsInRole("offline_access").Should().BeTrue();
    }

    [Fact]
    public async Task TransformAsync_Is_NoOp_Without_Realm_Access_Claim()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(authenticationType: "Test"));

        var result = await transformation.TransformAsync(principal);

        result.Claims.Should().BeEmpty();
    }

    [Fact]
    public async Task TransformAsync_Does_Not_Duplicate_Existing_Role_Claims()
    {
        var identity = new ClaimsIdentity(
        [
            new Claim("realm_access", """{"roles":["manager"]}"""),
            new Claim(ClaimTypes.Role, CurrentUser.ManagerRole)
        ], authenticationType: "Test");

        var result = await transformation.TransformAsync(new ClaimsPrincipal(identity));

        result.FindAll(ClaimTypes.Role).Should().ContainSingle();
    }
}
