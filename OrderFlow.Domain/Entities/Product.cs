using OrderFlow.Domain.Common;

namespace OrderFlow.Domain.Entities;

public class Product
{
    public long Id { get; private set; }
    public string Name { get; private set; } = null!;
    public decimal UnitPrice { get; private set; }
    public int AvailableQuantity { get; private set; }

    private Product(string name, decimal unitPrice, int availableQuantity)
    {
        Name = name;
        UnitPrice = unitPrice;
        AvailableQuantity = availableQuantity;
    }

    protected Product() { }

    public static Result<Product> Create(string name, decimal unitPrice, int availableQuantity)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<Product>.Failure(nameof(name), "Nome do produto é obrigatório.");

        if (unitPrice < 0)
            return Result<Product>.Failure(nameof(unitPrice), "Preço unitário não pode ser negativo.");

        if (availableQuantity < 0)
            return Result<Product>.Failure(nameof(availableQuantity), "Quantidade disponível não pode ser negativa.");

        var product = new Product(name.Trim(), unitPrice, availableQuantity);

        return Result<Product>.Success(product);
    }

    public Result DeductStock(int quantity)
    {
        if (quantity <= 0)
            return Result.Failure(nameof(quantity), "Quantidade a deduzir deve ser maior que zero.");

        if (quantity > AvailableQuantity)
            return Result.Failure(nameof(AvailableQuantity), "Estoque insuficiente para deduzir a quantidade solicitada.");

        AvailableQuantity -= quantity;

        return Result.Success();
    }

    public Result ReleaseStock(int quantity)
    {
        if (quantity <= 0)
            return Result.Failure(nameof(quantity), "Quantidade a liberar deve ser maior que zero.");

        AvailableQuantity += quantity;

        return Result.Success();
    }
}
