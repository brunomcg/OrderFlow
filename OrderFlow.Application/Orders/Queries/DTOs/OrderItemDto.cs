namespace OrderFlow.Application.Orders.Queries.DTOs;

public record OrderItemDto(
    long ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal TotalItemPrice);