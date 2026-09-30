using ProductCatalog.Application.Common;
using ProductCatalog.Application.Products.Dtos;

namespace ProductCatalog.Application.Products
{
    public interface IProductService
    {
        Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken ct = default);
        Task<ProductResponse> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<PagedResult<ProductResponse>> GetPagedAsync(GetProductsQuery query, CancellationToken ct = default);
        Task<ProductResponse> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken ct = default);
        Task<ProductResponse> AdjustStockAsync(Guid id, AdjustStockRequest request, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
