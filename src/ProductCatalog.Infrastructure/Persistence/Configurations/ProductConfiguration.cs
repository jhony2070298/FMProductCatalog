using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductCatalog.Domain.Products;

namespace ProductCatalog.Infrastructure.Persistence.Configurations
{
    internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("products", t =>
            {
                // Defensa en profundidad: aunque el dominio lo impide,
                // la base de datos tampoco acepta estados inválidos.
                t.HasCheckConstraint("ck_products_stock_non_negative", "stock >= 0");
                t.HasCheckConstraint("ck_products_price_positive", "price > 0");
            });

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .HasColumnName("id")
                .ValueGeneratedNever(); // el dominio genera el Guid v7

            builder.Property(p => p.Name)
                .HasColumnName("name")
                .HasMaxLength(Product.NameMaxLength)
                .IsRequired();

            builder.Property(p => p.Description)
                .HasColumnName("description")
                .HasMaxLength(Product.DescriptionMaxLength)
                .IsRequired();

            builder.Property(p => p.Price)
                .HasColumnName("price")
                .HasPrecision(18, 2);

            builder.Property(p => p.Stock).HasColumnName("stock");
            builder.Property(p => p.CreatedAt).HasColumnName("created_at");
            builder.Property(p => p.UpdatedAt).HasColumnName("updated_at");

            // Concurrencia optimista con la columna de sistema xmin de PostgreSQL.
            // Shadow property: el dominio no sabe que existe.
            builder.Property<uint>("Version").IsRowVersion();
        }
    }
}
