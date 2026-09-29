namespace ProductCatalog.Domain.Exceptions
{
    public sealed class InsufficientStockException(Guid productId, int currentStock, int requestedAdjustment)
    : DomainException(
        $"Stock insuficiente para el producto {productId}. " +
        $"Stock actual: {currentStock}, ajuste solicitado: {requestedAdjustment}.")
    {
        public Guid ProductId { get; } = productId;
        public int CurrentStock { get; } = currentStock;
        public int RequestedAdjustment { get; } = requestedAdjustment;
    }
}
