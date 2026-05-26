using OrderFlow.Domain.Common;

namespace OrderFlow.Domain.Entities;

public class Product
{
    public long Id { get; private set; }
    public string Name { get; private set; } = null!;
    public decimal UnitPrice { get; private set; }
    public int AvailableQuantity { get; private set; }

    // Construtor privado para garantir que a criação passe apenas pelo Factory Method
    private Product(string name, decimal unitPrice, int availableQuantity)
    {
        Name = name;
        UnitPrice = unitPrice;
        AvailableQuantity = availableQuantity;
    }

    // Construtor protegido exigido pelo Entity Framework Core para a Infraestrutura
    protected Product() { }

    // Factory Method: Única porta de entrada pública para criar um produto de forma segura
    public static Result<Product> Create(string name, decimal unitPrice, int availableQuantity)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<Product>.Failure(nameof(name), "Nome do produto é obrigatório.");

        if (unitPrice < 0)
            return Result<Product>.Failure(nameof(unitPrice), "Preço unitário não pode ser negativo.");

        if (availableQuantity < 0)
            return Result<Product>.Failure(nameof(availableQuantity), "Quantidade disponível não pode ser negativa.");

        // Se passou em todas as validações, cria e encapsula a instância no Result de sucesso
        var product = new Product(name.Trim(), unitPrice, availableQuantity);

        return Result<Product>.Success(product);
    }

    // 💡 Ajustado o comentário: Retorna Result.Failure se o estoque for insuficiente.
    public Result DeductStock(int quantity)
    {
        if (quantity <= 0)
            return Result.Failure(nameof(quantity), "Quantidade a deduzir deve ser maior que zero.");

        if (quantity > AvailableQuantity)
            return Result.Failure(nameof(AvailableQuantity), "Estoque insuficiente para deduzir a quantidade solicitada.");

        AvailableQuantity -= quantity;

        return Result.Success();
    }

    // Libera (adiciona) quantidade ao estoque (útil se o pedido for Cancelado)
    public Result ReleaseStock(int quantity)
    {
        if (quantity <= 0)
            return Result.Failure(nameof(quantity), "Quantidade a liberar deve ser maior que zero.");

        AvailableQuantity += quantity;

        return Result.Success();
    }
}