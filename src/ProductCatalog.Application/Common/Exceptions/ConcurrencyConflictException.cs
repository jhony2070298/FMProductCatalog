namespace ProductCatalog.Application.Common.Exceptions
{
    public sealed class ConcurrencyConflictException(string message, Exception? inner = null)
    : Exception(message, inner);
}
