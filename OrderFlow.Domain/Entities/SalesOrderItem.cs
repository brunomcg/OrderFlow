using OrderFlow.Domain.Common;

namespace OrderFlow.Domain.Entities
{
    public class SalesOrderItem
    {
        // Chaves estrangeiras e propriedades físicas
        public long SalesOrderId { get; private set; }
        public long ProductId { get; private set; }
        public decimal UnitPrice { get; private set; }
        public int Quantity { get; private set; }

        // Propriedades de navegação do EF
        public virtual SalesOrder SalesOrder { get; private set; } = null!;
        public virtual Product Product { get; private set; } = null!;

        // 💡 O construtor não exige mais o SalesOrderId obrigatoriamente na criação manual
        private SalesOrderItem(long productId, decimal unitPrice, int quantity)
        {
            ProductId = productId;
            UnitPrice = unitPrice;
            Quantity = quantity;
        }

        // Construtor protegido exigido pelo EF Core para materializar do banco
        protected SalesOrderItem() { }

        public static Result<SalesOrderItem> Create(long productId, decimal unitPrice, int quantity)
        {
            // 💡 Validamos apenas o que pertence ao escopo de existência do Item isolado
            if (productId <= 0)
                return Result<SalesOrderItem>.Failure(nameof(productId), "ProductId deve ser maior que zero.");

            if (unitPrice < 0) // Permitido zero caso haja uma promoção de brinde, por exemplo
                return Result<SalesOrderItem>.Failure(nameof(unitPrice), "UnitPrice não pode ser negativo.");

            if (quantity <= 0)
                return Result<SalesOrderItem>.Failure(nameof(quantity), "Quantity deve ser maior que zero.");

            var salesOrderItem = new SalesOrderItem(productId, unitPrice, quantity);

            return Result<SalesOrderItem>.Success(salesOrderItem);
        }
    }
}
