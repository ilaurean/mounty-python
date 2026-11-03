using AwesomeAssertions;
using FluentValidation;
using Operative.Contracts;
using Operative.Services.Features.Operations.Domain;

namespace Operative.Tests.Domain;

public class OrderDomainTests
{
    [Fact]
    public void Build_Rejects_An_Empty_Name()
    {
        AssertValidationError(() => ValidOrderBuilder().WithName(string.Empty).Build(), "Name is required.");
    }

    [Fact]
    public void Build_Rejects_An_Order_Without_Items()
    {
        AssertValidationError(() => ValidOrderBuilder().WithItems([]).Build(), "Items is required.");
    }

    [Fact]
    public void Build_Rejects_An_Order_Date_In_The_Past()
    {
        AssertValidationError(() => ValidOrderBuilder().WithOrderDate(DateTime.UtcNow.Date.AddDays(-1)).Build(), "OrderDate cannot be in the past days.");
    }

    [Fact]
    public void UpdateNameOrDescription_Rejects_An_Order_Not_In_Created_Status()
    {
        var order = ValidOrderBuilder().WithStatus(OrderStates.Processing).Build();

        AssertValidationError(() => order.UpdateNameOrDescription("Updated", null), "Only orders with status Created can be processed.");
    }

    [Fact]
    public void UpdateNameOrDescription_Validates_Updated_Values()
    {
        var order = ValidOrderBuilder().Build();

        AssertValidationError(() => order.UpdateNameOrDescription(string.Empty, null), "Name is required.");
    }

    [Fact]
    public void UpdateItems_Rejects_An_Order_Not_In_Created_Status()
    {
        var order = ValidOrderBuilder().WithStatus(OrderStates.Processing).Build();

        AssertValidationError(() => order.UpdateItems([ValidItem()]), "Only orders with status Created can be processed.");
    }

    [Fact]
    public void StartProcessing_Rejects_An_Order_Not_In_Created_Status()
    {
        var order = ValidOrderBuilder().WithStatus(OrderStates.Processing).Build();

        AssertValidationError(order.StartProcessing, "Only orders with status Created can be processed.");
    }

    [Fact]
    public void Complete_Rejects_An_Order_Not_In_Processing_Status()
    {
        var order = ValidOrderBuilder().Build();

        AssertValidationError(order.Complete, "Only orders with status Processing can be completed.");
    }

    [Fact]
    public void Cancel_Rejects_An_Order_Not_In_Created_Status()
    {
        var order = ValidOrderBuilder().WithStatus(OrderStates.Processing).Build();

        AssertValidationError(order.Cancel, "Only orders with status Created can be cancelled.");
    }

    [Fact]
    public void Delete_Rejects_An_Order_Not_In_Created_Status()
    {
        var order = ValidOrderBuilder().WithStatus(OrderStates.Completed).Build();

        AssertValidationError(() => order.Delete(), "Only orders with status Created can be deleted.");
    }

    private static Order.OrderBuilder ValidOrderBuilder() => Order.Create
        .WithName("Valid order")
        .WithDescription("Description")
        .WithCustomerID("customer-1")
        .WithOrderDate(DateTime.UtcNow.Date.AddDays(1))
        .WithItems([ValidItem()]);

    private static OrderItem ValidItem() => OrderItem.Create
        .WithPosition(1)
        .WithBoard(Board.Create
            .WithID("BRD-001")
            .WithName("Board")
            .WithComponents([Component.Create.WithName("Resistor").WithQuantity(1).Build()])
            .Build())
        .WithQuantity(1)
        .Build();

    private static void AssertValidationError(Action action, string expectedMessage)
    {
        var exception = Assert.Throws<ValidationException>(action);
        exception.Errors.Select(error => error.ErrorMessage).Should().Contain(expectedMessage);
    }
}