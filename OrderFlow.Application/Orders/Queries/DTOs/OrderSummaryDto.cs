namespace OrderFlow.Application.Orders.Queries.DTOs;

public record OrderSummaryDto(
    long Id,
    long CustomerId,
    string Status,
    decimal TotalAmount,
    DateTime CreatedAt);

