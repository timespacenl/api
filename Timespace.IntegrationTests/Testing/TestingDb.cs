using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Timespace.Api.Infrastructure.Persistence;
using Timespace.Api.Infrastructure.Persistence.Common;

namespace Timespace.IntegrationTests.Testing;

public partial class Testing
{
    public static async Task<TEntity?> FindAsync<TEntity>(params object[] keyValues)
        where TEntity : class
    {
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        return await context.FindAsync<TEntity>(keyValues);
    }

    public static async Task<TEntity?> FirstOrDefault<TEntity>(Guid id)
        where TEntity : class, IEntity
    {
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        return await context.Set<TEntity>().FirstOrDefaultAsync(x => x.Id == id);
    }
    
    public static async Task AddAsync<TEntity>(TEntity entity)
        where TEntity : class
    {
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        context.Add(entity);

        await context.SaveChangesAsync();
    }

    public static async Task<int> CountAsync<TEntity>() where TEntity : class
    {
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        return await context.Set<TEntity>().CountAsync();
    }
}