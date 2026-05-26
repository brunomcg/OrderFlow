using System;
using System.Linq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using OrderFlow.Domain.Entities;
using OrderFlow.Infrastructure.Data;
using Xunit;

namespace OrderFlow.UnitTests.Infrastructure
{
    public class CurrencyConfigurationTests
    {
        [Fact]
        public void CurrencyEntity_ShouldBeMapped_Correctly()
        {
            // Arrange: create in-memory options and build model from ApplicationDbContext
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new ApplicationDbContext(options);

            // Act: obtain IModel and IEntityType for Currency
            var model = context.Model;
            var entity = model.FindEntityType(typeof(Currency)) ?? model.GetEntityTypes().FirstOrDefault(e => e.ClrType.Name == nameof(Currency));

            // Assert: entity discovered
            entity.Should().NotBeNull("the Currency entity must be part of the EF model via CurrencyConfiguration");

            // Table name
            entity.GetTableName().Should().Be("currency");

            // Property: Code
            var propCode = entity.FindProperty(nameof(Currency.Code));
            propCode.Should().NotBeNull();
            // Primary key
            var pk = entity.FindPrimaryKey();
            pk.Properties.Should().Contain(propCode);
            // MaxLength and Required
            propCode.GetMaxLength().Should().Be(3);
            propCode.IsNullable.Should().BeFalse();
            // IsFixedLength: check relational annotation if present
            var fixedAnn = propCode.GetAnnotations().FirstOrDefault(a => a.Name.EndsWith("IsFixedLength", StringComparison.OrdinalIgnoreCase));
            fixedAnn.Should().NotBeNull();
            fixedAnn.Value.Should().BeEquivalentTo(true);

            // Property: Symbol
            var propSymbol = entity.FindProperty(nameof(Currency.Symbol));
            propSymbol.Should().NotBeNull();
            propSymbol.GetMaxLength().Should().Be(5);
            propSymbol.IsNullable.Should().BeFalse();

            // Property: Name
            var propName = entity.FindProperty(nameof(Currency.Name));
            propName.Should().NotBeNull();
            propName.GetMaxLength().Should().Be(50);
            propName.IsNullable.Should().BeFalse();
        }
    }
}
