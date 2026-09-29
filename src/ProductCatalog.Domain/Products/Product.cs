using ProductCatalog.Domain.Exceptions;

namespace ProductCatalog.Domain.Products
{
    public sealed class Product
    {
        public const int NameMaxLength = 150;
        public const int DescriptionMaxLength = 1000;

        public Guid Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public decimal Price { get; private set; }
        public int Stock { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        // Requerido por EF Core
        private Product() { }

        public static Product Create(string name, string description, decimal price, int initialStock)
        {
            if (initialStock < 0)
                throw new DomainValidationException("El stock inicial no puede ser negativo.");

            var now = DateTime.UtcNow;
            var product = new Product
            {
                Id = Guid.CreateVersion7(),
                Stock = initialStock,
                CreatedAt = now,
                UpdatedAt = now
            };
            product.SetDetails(name, description, price);
            return product;
        }

        public void UpdateDetails(string name, string description, decimal price)
        {
            SetDetails(name, description, price);
            Touch();
        }

        /// <summary>
        /// Suma (cantidad positiva) o resta (cantidad negativa) unidades al stock actual.
        /// </summary>
        public void AdjustStock(int quantity)
        {
            if (quantity == 0)
                throw new DomainValidationException("La cantidad a ajustar debe ser distinta de cero.");

            long newStock = (long)Stock + quantity; // evita overflow de int

            if (newStock < 0)
                throw new InsufficientStockException(Id, Stock, quantity);

            if (newStock > int.MaxValue)
                throw new DomainValidationException("El stock resultante excede el máximo permitido.");

            Stock = (int)newStock;
            Touch();
        }

        private void SetDetails(string name, string description, decimal price)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainValidationException("El nombre es obligatorio.");

            name = name.Trim();
            description = description?.Trim() ?? string.Empty;

            if (name.Length > NameMaxLength)
                throw new DomainValidationException($"El nombre no puede superar {NameMaxLength} caracteres.");

            if (description.Length > DescriptionMaxLength)
                throw new DomainValidationException($"La descripción no puede superar {DescriptionMaxLength} caracteres.");

            if (price <= 0)
                throw new DomainValidationException("El precio debe ser mayor que cero.");

            Name = name;
            Description = description;
            Price = price;
        }

        private void Touch() => UpdatedAt = DateTime.UtcNow;
    }
}
