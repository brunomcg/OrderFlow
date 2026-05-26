using OrderFlow.Domain.Common; // Ajustado para o seu namespace correto de Result

namespace OrderFlow.Domain.Entities;

public class SalesOrder
{
    public long Id { get; private set; }
    public long CustomerId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public string CurrencyCode { get; private set; } = null!; // Chave estrangeira física
    public virtual Currency Currency { get; private set; } = null!; // Propriedade de navegação

    public int SalesOrderStatusId { get; private set; }
    public virtual SalesOrderStatus SalesOrderStatus { get; private set; } = null!;

    public decimal Total { get; private set; }

    private readonly List<SalesOrderItem> _items = new();
    public IReadOnlyCollection<SalesOrderItem> Items => _items.AsReadOnly();

    private SalesOrder(long customerId, string currency)
    {
        CustomerId = customerId;
        CurrencyCode = currency;
        CreatedAt = DateTime.UtcNow;
        SalesOrderStatusId = SalesOrderStatus.Placed.Id; // Define o ID inicial (1)
        Total = 0m;
    }

    public static Result<SalesOrder> Create(long customerId, string currency)
    {
        if (customerId <= 0)
            return Result<SalesOrder>.Failure(nameof(customerId), "CustomerId deve ser maior que zero.");

        if (string.IsNullOrWhiteSpace(currency))
            return Result<SalesOrder>.Failure(nameof(currency), "A moeda (Currency) é obrigatória.");

        var validatedCurrency = Currency.List()
            .FirstOrDefault(c => c.Code.Equals(currency.Trim(), StringComparison.OrdinalIgnoreCase));

        if (validatedCurrency is null)
        {
            return Result<SalesOrder>.Failure(nameof(currency), $"A moeda '{currency.Trim().ToUpper()}' não é suportada pelo sistema.");
        }

        var order = new SalesOrder(customerId, validatedCurrency.Code);

        return Result<SalesOrder>.Success(order);
    }

    protected SalesOrder()
    {
        Currency = null!;
    }

    public Result AddItem(long productId, decimal unitPrice, int quantity)
    {
        var salesOrderItemResult = SalesOrderItem.Create(productId, unitPrice, quantity);

        if (!salesOrderItemResult.IsSuccess)
        {
            return Result.Failure(salesOrderItemResult.Field ?? "Items", salesOrderItemResult.Error);
        }

        var item = salesOrderItemResult.Value;

        _items.Add(item);
        Total += item.UnitPrice * item.Quantity;

        return Result.Success();
    }

    public Result Confirm()
    {
        if (SalesOrderStatusId == SalesOrderStatus.Confirmed.Id)
            return Result.Success();

        if (SalesOrderStatusId == SalesOrderStatus.Canceled.Id)
            return Result.Failure("Status", "Não é possível confirmar um pedido cancelado.");

        SalesOrderStatusId = SalesOrderStatus.Confirmed.Id;

        return Result.Success();
    }

    public Result Cancel()
    {
        if (SalesOrderStatusId == SalesOrderStatus.Canceled.Id)
            return Result.Success();

        if (SalesOrderStatusId != SalesOrderStatus.Placed.Id && SalesOrderStatusId != SalesOrderStatus.Confirmed.Id)
            return Result.Failure("Status", "Não é possível cancelar um pedido com o status atual.");

        SalesOrderStatusId = SalesOrderStatus.Canceled.Id;

        return Result.Success();
    }
}
