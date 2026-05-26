using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Domain.Entities;
using OrderFlow.Infrastructure.Data;

namespace OrderFlow.UnitTests.Infrastructure
{
    public class CurrencyConfigurationTests
    {
        [Fact]
        public void CurrencyEntity_ShouldBeMapped_Correctly()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new ApplicationDbContext(options);

            var model = context.Model;
            var entity = model.FindEntityType(typeof(Currency)) ?? model.GetEntityTypes().FirstOrDefault(e => e.ClrType.Name == nameof(Currency));

            entity.Should().NotBeNull("the Currency entity must be part of the EF model via CurrencyConfiguration");

            entity.GetTableName().Should().Be("currency");

            var propCode = entity.FindProperty(nameof(Currency.Code));
            propCode.Should().NotBeNull();
            var pk = entity.FindPrimaryKey();
            pk.Properties.Should().Contain(propCode);
            propCode.GetMaxLength().Should().Be(3);
            propCode.IsNullable.Should().BeFalse();
            var fixedAnn = propCode.GetAnnotations().FirstOrDefault(a => a.Name.EndsWith("IsFixedLength", StringComparison.OrdinalIgnoreCase));
            fixedAnn.Should().NotBeNull();
            fixedAnn.Value.Should().BeEquivalentTo(true);

            var propSymbol = entity.FindProperty(nameof(Currency.Symbol));
            propSymbol.Should().NotBeNull();
            propSymbol.GetMaxLength().Should().Be(5);
            propSymbol.IsNullable.Should().BeFalse();

            var propName = entity.FindProperty(nameof(Currency.Name));
            propName.Should().NotBeNull();
            propName.GetMaxLength().Should().Be(50);
            propName.IsNullable.Should().BeFalse();
        }
    }
}

