using OrderFlow.Domain.Common; // Ajustado para o seu namespace correto de Result

namespace OrderFlow.Domain.Entities;

public class SalesOrder
{
    public long Id { get; private set; }
    public long CustomerId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // 💡 NOVA PROPRIEDADE: Armazena o código da moeda (ex: BRL, USD)
    public string CurrencyCode { get; private set; } = null!; // Chave estrangeira física
    public virtual Currency Currency { get; private set; } = null!; // Propriedade de navegação

    // Propriedades separadas para o Smart Enum funcionar com o EF Core
    public int SalesOrderStatusId { get; private set; }
    public virtual SalesOrderStatus SalesOrderStatus { get; private set; } = null!;

    public decimal Total { get; private set; }

    private readonly List<SalesOrderItem> _items = new();
    public IReadOnlyCollection<SalesOrderItem> Items => _items.AsReadOnly();

    // 💡 Construtor privado atualizado para receber a moeda
    private SalesOrder(long customerId, string currency)
    {
        CustomerId = customerId;
        CurrencyCode = currency;
        CreatedAt = DateTime.UtcNow;
        SalesOrderStatusId = SalesOrderStatus.Placed.Id; // Define o ID inicial (1)
        Total = 0m;
    }

    // 💡 Factory Method atualizado com a validação da moeda
    public static Result<SalesOrder> Create(long customerId, string currency)
    {
        if (customerId <= 0)
            return Result<SalesOrder>.Failure(nameof(customerId), "CustomerId deve ser maior que zero.");

        if (string.IsNullOrWhiteSpace(currency))
            return Result<SalesOrder>.Failure(nameof(currency), "A moeda (Currency) é obrigatória.");

        // 💡 Busca a moeda no Smart Enum. Se não existir, o método retorna null em vez de estourar exception
        var validatedCurrency = Currency.List()
            .FirstOrDefault(c => c.Code.Equals(currency.Trim(), StringComparison.OrdinalIgnoreCase));

        if (validatedCurrency is null)
        {
            return Result<SalesOrder>.Failure(nameof(currency), $"A moeda '{currency.Trim().ToUpper()}' não é suportada pelo sistema.");
        }

        // Cria o pedido passando o código rigidamente correto e formatado pelo Smart Enum (ex: "BRL")
        var order = new SalesOrder(customerId, validatedCurrency.Code);

        return Result<SalesOrder>.Success(order);
    }

    // Construtor protegido exigido pelo EF Core
    protected SalesOrder()
    {
        Currency = null!;
    }

    public Result AddItem(long productId, decimal unitPrice, int quantity)
    {
        // SOLID (SRP): Delega inteiramente a validação de campos para o OrderItem
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
        // REGRA DE IDEMPOTÊNCIA: Se já estiver confirmado, ignora e retorna sucesso
        if (SalesOrderStatusId == SalesOrderStatus.Confirmed.Id)
            return Result.Success();

        // REGRA DE TRANSIÇÃO: Só confirma se estiver em "Placed"
        if (SalesOrderStatusId == SalesOrderStatus.Canceled.Id)
            return Result.Failure("Status", "Não é possível confirmar um pedido cancelado.");

        // MUTAÇÃO DE ESTADO SEGURO
        SalesOrderStatusId = SalesOrderStatus.Confirmed.Id;

        return Result.Success();
    }

    public Result Cancel()
    {
        // REGRA DE IDEMPOTÊNCIA: Se já estiver cancelado, ignora e retorna sucesso
        if (SalesOrderStatusId == SalesOrderStatus.Canceled.Id)
            return Result.Success();

        // REGRA DE TRANSIÇÃO: Só pode cancelar se estiver em Placed ou Confirmed
        if (SalesOrderStatusId != SalesOrderStatus.Placed.Id && SalesOrderStatusId != SalesOrderStatus.Confirmed.Id)
            return Result.Failure("Status", "Não é possível cancelar um pedido com o status atual.");

        // MUTAÇÃO DE ESTADO
        SalesOrderStatusId = SalesOrderStatus.Canceled.Id;

        return Result.Success();
    }
}