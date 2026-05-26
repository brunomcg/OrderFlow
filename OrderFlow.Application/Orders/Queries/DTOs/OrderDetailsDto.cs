namespace OrderFlow.Application.Orders.Queries.DTOs;
    // DTO Principal do Pedido
    public record OrderDetailsDto(
        long Id,
        long CustomerId,
        string Status,
        decimal TotalAmount,
        DateTime CreatedAt,
        IReadOnlyCollection<OrderItemDto> Items);