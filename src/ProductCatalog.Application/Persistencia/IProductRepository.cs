using ProductCatalog.Application.Common;
using ProductCatalog.Domain.Products;

namespace ProductCatalog.Application.Persistencia
{
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<PagedResult<Product>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
        void Add(Product product);
        void Remove(Product product);
    }
}
