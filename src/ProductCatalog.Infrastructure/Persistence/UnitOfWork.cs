using Microsoft.EntityFrameworkCore;
using ProductCatalog.Application.Common.Exceptions;
using ProductCatalog.Application.Persistencia;

namespace ProductCatalog.Infrastructure.Persistence
{
    internal sealed class UnitOfWork(AppDbContext context) : IUnitOfWork
    {
        public async Task SaveChangesAsync(CancellationToken ct = default)
        {
            try
            {
                await context.SaveChangesAsync(ct);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Limpia las entidades obsoletas para que un reintento
                // vuelva a leer el estado actual desde la base.
                context.ChangeTracker.Clear();
                throw new ConcurrencyConflictException(
                    "El recurso fue modificado por otra operación concurrente.", ex);
            }
        }
    }
}
