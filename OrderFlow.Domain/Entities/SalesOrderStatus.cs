namespace OrderFlow.Domain.Entities;

public class SalesOrderStatus
{
    public static readonly SalesOrderStatus Placed = new(1, "Placed", "Pedido de venda recebido pelo sistema e aguardando processamento.");
    public static readonly SalesOrderStatus Confirmed = new(2, "Confirmed", "Pagamento aprovado e pedido de venda confirmado.");
    public static readonly SalesOrderStatus Canceled = new(3, "Canceled", "Pedido de venda cancelado.");

    public int Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;

    private SalesOrderStatus(int id, string name, string description)
    {
        Id = id;
        Name = name;
        Description = description;
    }

    protected SalesOrderStatus()
    {
    }

    public static IEnumerable<SalesOrderStatus> List() => new[] { Placed, Confirmed, Canceled };

    public static SalesOrderStatus FromId(int id)
    {
        var state = List().FirstOrDefault(s => s.Id == id);

        if (state is null)
            throw new ArgumentException($"ID de status inválido: {id}");

        return state;
    }

    public override string ToString() => Name;
}
