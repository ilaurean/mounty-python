using AwesomeAssertions;
using FluentValidation;
using Inventory.Services.Features.Catalogs.Domain;

namespace Inventory.Tests.Domain;

public class BoardDomainTests
{
    [Fact]
    public void Build_Rejects_An_Empty_Name()
    {
        AssertValidationError(() => ValidBoardBuilder().WithName(string.Empty).Build(), "Name is required.");
    }

    [Fact]
    public void Build_Rejects_An_Overlong_Description()
    {
        AssertValidationError(() => ValidBoardBuilder().WithDescription(new string('x', 1025)).Build(), "Description cannot exceed 1024 characters long.");
    }

    [Fact]
    public void Build_Rejects_A_Nonpositive_Length()
    {
        AssertValidationError(() => ValidBoardBuilder().WithDimension(0m, 5m).Build(), "Length must be greater than 0.");
    }

    [Fact]
    public void Build_Rejects_A_Nonpositive_Width()
    {
        AssertValidationError(() => ValidBoardBuilder().WithDimension(10m, -1m).Build(), "Width must be greater than 0.");
    }

    [Fact]
    public void Build_Validates_Components()
    {
        AssertValidationError(
            () => ValidBoardBuilder().WithComponents([Component.Create.WithName("Resistor").WithQuantity(0).Build()]).Build(),
            "Quantity must be greater than 0.");
    }

    [Fact]
    public void Update_Rejects_A_Nonpositive_Dimension()
    {
        var board = ValidBoardBuilder().Build();

        AssertValidationError(() => board.Update("Updated", null, 0m, null), "Length must be greater than 0.");
    }

    [Fact]
    public void Update_Rejects_A_Deleted_Board()
    {
        var board = ValidBoardBuilder().Build();
        board.Delete();

        AssertValidationError(() => board.Update("Updated", null, null, null), "Only boards that are not deleted can be modified.");
    }

    [Fact]
    public void Delete_Rejects_An_Already_Deleted_Board()
    {
        var board = ValidBoardBuilder().Build();
        board.Delete();

        AssertValidationError(() => board.Delete(), "Board already deleted.");
    }

    private static Board.BoardBuilder ValidBoardBuilder() => Board.Create
        .WithName("Valid board")
        .WithDescription("Description")
        .WithDimension(10m, 5m)
        .WithComponents([Component.Create.WithName("Resistor").WithDescription("10k resistor").WithQuantity(1).Build()]);

    private static void AssertValidationError(Action action, string expectedMessage)
    {
        var exception = Assert.Throws<ValidationException>(action);
        exception.Errors.Select(error => error.ErrorMessage).Should().Contain(expectedMessage);
    }
}