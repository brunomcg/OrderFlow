namespace OrderFlow.Application.Orders.Queries.DTOs;

// DTO Interno para os Itens do Pedido
public record OrderItemDto(
    long ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal TotalItemPrice);
