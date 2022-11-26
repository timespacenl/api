using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Timespace.Api.Application.Features.Authentication.Registration.Entities;
using Timespace.Api.Infrastructure.Persistence.Common;

namespace Timespace.Api.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    // private readonly ISessionInfoProvider _sessionInfoProvider;
    private readonly IClock _clock;
    
    public AppDbContext(DbContextOptions<AppDbContext> options, IClock clock) : base(options)
    {
        _clock = clock;
    }

    // Selfservice flows
    public DbSet<RegistrationFlow> RegistrationFlows { get; init; } = null!;

    public override int SaveChanges()
    {
        throw new NotSupportedException();
    }

    public async override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is IBaseEntity && e.State is EntityState.Added or EntityState.Modified);

        foreach (var entityEntry in entries)
        {
            ((IBaseEntity)entityEntry.Entity).UpdatedAt = _clock.GetCurrentInstant();

            if (entityEntry.State == EntityState.Added)
            {
                ((IBaseEntity)entityEntry.Entity).CreatedAt = _clock.GetCurrentInstant();
            }
        }

        // var tenantEntries = ChangeTracker
        //     .Entries()
        //     .Where(e => e.Entity is ITenantEntity && e.State is EntityState.Added or EntityState.Modified);
        //
        // foreach (var entityEntry in tenantEntries)
        // {
        //     ((ITenantEntity)entityEntry.Entity).TenantId = _sessionInfoProvider.GetGuaranteedSession().Identity.TenantId;
        // }
        
        var softdeleteEntries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is ISoftDeletable && 
                e.State == EntityState.Deleted);

        foreach (var entityEntry in softdeleteEntries)
        {
            ((ISoftDeletable)entityEntry.Entity).DeletedAt = _clock.GetCurrentInstant();
            ((ISoftDeletable)entityEntry.Entity).IsDeleted = true;
        }
        
        return await base.SaveChangesAsync(cancellationToken);
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Expression<Func<ITenantEntity, bool>> tenantExpression = entity => _sessionInfoProvider.GetGuaranteedSession().Identity.Tenant.Id == Guid.Empty || entity.TenantId == _sessionInfoProvider.GetGuaranteedSession().Identity.Tenant.Id;
        Expression<Func<ISoftDeletable, bool>> softDeleteExpression = entity => entity.IsDeleted == false;
        
        foreach (var entityType in modelBuilder.Model.GetEntityTypes()) {
            // if (entityType.ClrType.IsAssignableTo(typeof(ITenantEntity))) {
            //     modelBuilder.Entity(entityType.ClrType).AppendQueryFilter(tenantExpression);
            // }
            
            if (entityType.ClrType.IsAssignableTo(typeof(ISoftDeletable))) {
                modelBuilder.Entity(entityType.ClrType).AppendQueryFilter(softDeleteExpression);
            }
        }
    }
}