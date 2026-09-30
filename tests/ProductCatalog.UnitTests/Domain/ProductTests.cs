using ProductCatalog.Domain.Exceptions;
using ProductCatalog.Domain.Products;

namespace ProductCatalog.UnitTests.Domain
{
    public class ProductTests
    {
        private static Product NewProduct(int stock = 10) =>
            Product.Create("Café", "Café de Santander", 25000m, stock);

        [Fact]
        public void Create_WithValidData_SetsProperties()
        {
            var product = Product.Create("  Café  ", "Tostado", 25000m, 5);

            Assert.NotEqual(Guid.Empty, product.Id);
            Assert.Equal("Café", product.Name);
            Assert.Equal(25000m, product.Price);
            Assert.Equal(5, product.Stock);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-100)]
        public void Create_WithNonPositivePrice_Throws(int price) =>
            Assert.Throws<DomainValidationException>(() => Product.Create("Café", "", price, 1));

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_WithEmptyName_Throws(string name) =>
            Assert.Throws<DomainValidationException>(() => Product.Create(name, "", 1000m, 1));

        [Fact]
        public void Create_WithNegativeStock_Throws() =>
            Assert.Throws<DomainValidationException>(() => Product.Create("Café", "", 1000m, -1));

        [Fact]
        public void AdjustStock_PositiveQuantity_IncreasesStock()
        {
            var product = NewProduct(10);
            product.AdjustStock(5);
            Assert.Equal(15, product.Stock);
        }

        [Fact]
        public void AdjustStock_ToExactlyZero_IsAllowed()
        {
            var product = NewProduct(10);
            product.AdjustStock(-10);
            Assert.Equal(0, product.Stock);
        }

        [Fact]
        public void AdjustStock_Zero_Throws() =>
            Assert.Throws<DomainValidationException>(() => NewProduct().AdjustStock(0));

        [Fact]
        public void AdjustStock_Overflow_Throws() =>
            Assert.Throws<DomainValidationException>(() => NewProduct(int.MaxValue).AdjustStock(1));
    }
}
