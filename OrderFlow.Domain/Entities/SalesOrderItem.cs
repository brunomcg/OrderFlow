using OrderFlow.Domain.Common;

namespace OrderFlow.Domain.Entities
{
    public class SalesOrderItem
    {
        public long SalesOrderId { get; private set; }
        public long ProductId { get; private set; }
        public decimal UnitPrice { get; private set; }
        public int Quantity { get; private set; }

        public virtual SalesOrder SalesOrder { get; private set; } = null!;
        public virtual Product Product { get; private set; } = null!;

        private SalesOrderItem(long productId, decimal unitPrice, int quantity)
        {
            ProductId = productId;
            UnitPrice = unitPrice;
            Quantity = quantity;
        }

        protected SalesOrderItem() { }

        public static Result<SalesOrderItem> Create(long productId, decimal unitPrice, int quantity)
        {
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

