namespace OrderFlow.Application.Orders.Queries.DTOs;
    public record OrderDetailsDto(
        long Id,
        long CustomerId,
        string Status,
        decimal TotalAmount,
        DateTime CreatedAt,
        IReadOnlyCollection<OrderItemDto> Items);
