namespace ProductCatalog.Application.Common.Exceptions
{
    public sealed class NotFoundException(string resource, object key)
    : Exception($"{resource} con id '{key}' no fue encontrado.");
}
