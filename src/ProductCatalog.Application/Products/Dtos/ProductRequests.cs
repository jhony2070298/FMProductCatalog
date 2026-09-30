namespace ProductCatalog.Application.Products.Dtos
{
    public sealed record CreateProductRequest(string Name, string? Description, decimal Price, int InitialStock);

    public sealed record UpdateProductRequest(string Name, string? Description, decimal Price);

    /// <param name="Quantity">Unidades a ajustar: positivo suma, negativo resta. No puede ser 0.</param>
    public sealed record AdjustStockRequest(int Quantity);

    public sealed record GetProductsQuery(int Page = 1, int PageSize = 20);
}
