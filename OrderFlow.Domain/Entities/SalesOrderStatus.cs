namespace OrderFlow.Domain.Entities;

public class SalesOrderStatus
{
    // 1. As Opções Estáticas (Atuam exatamente como os itens do Enum antigo no C#)
    public static readonly SalesOrderStatus Placed = new(1, "Placed", "Pedido de venda recebido pelo sistema e aguardando processamento.");
    public static readonly SalesOrderStatus Confirmed = new(2, "Confirmed", "Pagamento aprovado e pedido de venda confirmado.");
    public static readonly SalesOrderStatus Canceled = new(3, "Canceled", "Pedido de venda cancelado.");

    // 2. As Propriedades que darão origem às colunas da Tabela de Domínio no Postgres
    public int Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;

    // Construtor privado para garantir que ninguém crie status dinâmicos/inválidos por fora
    private SalesOrderStatus(int id, string name, string description)
    {
        Id = id;
        Name = name;
        Description = description;
    }

    // Construtor protegido exigido pelo EF Core para materializar as linhas do banco
    protected SalesOrderStatus()
    {
    }

    // 💡 Retorna todos os status disponíveis usando um array estático (evita switch-case manual)
    public static IEnumerable<SalesOrderStatus> List() => new[] { Placed, Confirmed, Canceled };

    // 💡 Método auxiliar dinâmico: Converte de ID (int) para o objeto de forma limpa
    public static SalesOrderStatus FromId(int id)
    {
        var state = List().FirstOrDefault(s => s.Id == id);

        if (state is null)
            throw new ArgumentException($"ID de status inválido: {id}");

        return state;
    }

    // 💡 Opcional: Sobrescrever o ToString facilita muito o log da aplicação
    public override string ToString() => Name;
}