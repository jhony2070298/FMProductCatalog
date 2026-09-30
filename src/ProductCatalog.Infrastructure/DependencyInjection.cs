using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProductCatalog.Application.Persistencia;
using ProductCatalog.Infrastructure.Persistence;
using ProductCatalog.Infrastructure.Persistence.Repositories;

namespace ProductCatalog.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString, npgsql =>
                    npgsql.EnableRetryOnFailure(maxRetryCount: 3))); // errores transitorios de red

            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }

        public static async Task ApplyMigrationsAsync(this IServiceProvider services, CancellationToken ct = default)
        {
            await using var scope = services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.MigrateAsync(ct);
        }
    }
}
