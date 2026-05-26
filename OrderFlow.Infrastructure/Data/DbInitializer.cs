using Microsoft.EntityFrameworkCore;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.Migrate();

            if (context.Products.Any())
            return; 

            var products = new List<Product>
            {
               Product.Create("Mouse Gamer Wireless", 250.00m, 100).Value,
               Product.Create("Teclado Mecânico RGB", 450.90m, 200).Value,
               Product.Create("Monitor 24' IPS 144Hz", 1199.00m, 50).Value
            };

            context.Products.AddRange(products);
            context.SaveChanges();

        }
    }
}

