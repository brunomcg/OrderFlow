namespace OrderFlow.Domain.Entities;

public class Currency
{
    public static readonly Currency BRL = new("BRL", "R$", "Real Brasileiro");
    public static readonly Currency USD = new("USD", "$", "United States Dollar");
    public static readonly Currency EUR = new("EUR", "€", "Euro");

    // 💡 O 'Code' (BRL, USD) será a Chave Primária física no banco de dados
    public string Code { get; private set; }
    public string Symbol { get; private set; }
    public string Name { get; private set; }

    private Currency(string code, string symbol, string name)
    {
        Code = code;
        Symbol = symbol;
        Name = name;
    }

    protected Currency()
    {
        Code = null!;
        Symbol = null!;
        Name = null!;
    }

    public static IEnumerable<Currency> List() => new[] { BRL, USD, EUR };

    public static Currency FromCode(string code)
    {
        var currency = List().FirstOrDefault(c => c.Code.Equals(code.Trim(), StringComparison.OrdinalIgnoreCase));

        if (currency is null)
            throw new ArgumentException($"Moeda não suportada: {code}");

        return currency;
    }

    public override string ToString() => Code;
}