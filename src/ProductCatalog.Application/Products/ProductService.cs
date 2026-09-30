using FluentValidation;
using Microsoft.Extensions.Logging;
using ProductCatalog.Application.Common;
using ProductCatalog.Application.Common.Exceptions;
using ProductCatalog.Application.Persistencia;
using ProductCatalog.Application.Products.Dtos;
using ProductCatalog.Domain.Products;

namespace ProductCatalog.Application.Products
{
    public sealed class ProductService(
    IProductRepository repository,
    IUnitOfWork unitOfWork,
    IValidator<CreateProductRequest> createValidator,
    IValidator<UpdateProductRequest> updateValidator,
    IValidator<AdjustStockRequest> adjustStockValidator,
    IValidator<GetProductsQuery> queryValidator,
    ILogger<ProductService> logger) : IProductService
    {
        private const int MaxConcurrencyAttempts = 3;

        public async Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken ct = default)
        {
            await createValidator.ValidateAndThrowAsync(request, ct);

            var product = Product.Create(request.Name, request.Description ?? string.Empty, request.Price, request.InitialStock);

            repository.Add(product);
            await unitOfWork.SaveChangesAsync(ct);

            return product.ToResponse();
        }

        public async Task<ProductResponse> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var product = await GetExistingAsync(id, ct);
            return product.ToResponse();
        }

        public async Task<PagedResult<ProductResponse>> GetPagedAsync(GetProductsQuery query, CancellationToken ct = default)
        {
            await queryValidator.ValidateAndThrowAsync(query, ct);

            var page = await repository.GetPagedAsync(query.Page, query.PageSize, ct);

            return new PagedResult<ProductResponse>(
                page.Items.Select(p => p.ToResponse()).ToList(),
                page.Page, page.PageSize, page.TotalCount);
        }

        public async Task<ProductResponse> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken ct = default)
        {
            await updateValidator.ValidateAndThrowAsync(request, ct);

            return await ExecuteWithConcurrencyRetryAsync(id, async () =>
            {
                var product = await GetExistingAsync(id, ct);
                product.UpdateDetails(request.Name, request.Description ?? string.Empty, request.Price);
                await unitOfWork.SaveChangesAsync(ct);
                return product.ToResponse();
            }, ct);
        }

        public async Task<ProductResponse> AdjustStockAsync(Guid id, AdjustStockRequest request, CancellationToken ct = default)
        {
            await adjustStockValidator.ValidateAndThrowAsync(request, ct);

            return await ExecuteWithConcurrencyRetryAsync(id, async () =>
            {
                // Cada intento relee el producto: la regla del dominio
                // se evalúa siempre contra el stock más reciente.
                var product = await GetExistingAsync(id, ct);
                product.AdjustStock(request.Quantity);
                await unitOfWork.SaveChangesAsync(ct);
                return product.ToResponse();
            }, ct);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            await ExecuteWithConcurrencyRetryAsync(id, async () =>
            {
                var product = await GetExistingAsync(id, ct);
                repository.Remove(product);
                await unitOfWork.SaveChangesAsync(ct);
                return true;
            }, ct);
        }

        private async Task<Product> GetExistingAsync(Guid id, CancellationToken ct) =>
            await repository.GetByIdAsync(id, ct) ?? throw new NotFoundException("Producto", id);

        /// <summary>
        /// Reintenta la operación completa (leer → aplicar regla → guardar) si otra
        /// petición modificó el producto entre la lectura y la escritura.
        /// Si se agotan los intentos, la ConcurrencyConflictException sube y la API responde 409.
        /// </summary>
        private async Task<T> ExecuteWithConcurrencyRetryAsync<T>(Guid id, Func<Task<T>> operation, CancellationToken ct)
        {
            for (var attempt = 1; ; attempt++)
            {
                try
                {
                    return await operation();
                }
                catch (ConcurrencyConflictException) when (attempt < MaxConcurrencyAttempts)
                {
                    logger.LogWarning(
                        "Conflicto de concurrencia en producto {ProductId}. Reintento {Attempt}/{Max}.",
                        id, attempt, MaxConcurrencyAttempts - 1);

                    // Pequeña espera aleatoria para que las peticiones en conflicto no choquen de nuevo al mismo tiempo
                    await Task.Delay(Random.Shared.Next(10, 50) * attempt, ct);
                }
            }
        }
    }
}
