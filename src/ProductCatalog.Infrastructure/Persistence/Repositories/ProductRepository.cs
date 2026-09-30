using Microsoft.EntityFrameworkCore;
using ProductCatalog.Application.Common;
using ProductCatalog.Application.Persistencia;
using ProductCatalog.Domain.Products;

namespace ProductCatalog.Infrastructure.Persistence.Repositories
{
    internal sealed class ProductRepository(AppDbContext context) : IProductRepository
    {
        public Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            context.Products.FirstOrDefaultAsync(p => p.Id == id, ct);

        public async Task<PagedResult<Product>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default)
        {
            var query = context.Products.AsNoTracking();

            var total = await query.CountAsync(ct);
            var items = await query
                .OrderBy(p => p.CreatedAt).ThenBy(p => p.Id) // orden estable para paginar
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return new PagedResult<Product>(items, page, pageSize, total);
        }

        public void Add(Product product) => context.Products.Add(product);

        public void Remove(Product product) => context.Products.Remove(product);
    }
}
