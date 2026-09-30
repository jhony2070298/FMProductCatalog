using ProductCatalog.Domain.Products;

namespace ProductCatalog.Application.Products.Dtos
{
    internal static class ProductMappings
    {
        public static ProductResponse ToResponse(this Product p) =>
            new(p.Id, p.Name, p.Description, p.Price, p.Stock, p.CreatedAt, p.UpdatedAt);
    }
}
