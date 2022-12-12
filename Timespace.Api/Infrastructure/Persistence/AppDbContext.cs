using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Timespace.Api.Application.Features.Authentication.Login.Common.Entities;
using Timespace.Api.Application.Features.Authentication.Registration.Common.Entities;
using Timespace.Api.Application.Features.Authentication.Sessions.Common.Entities;
using Timespace.Api.Application.Features.Tenants.Common.Entities;
using Timespace.Api.Application.Features.Users.Common.Entities;
using Timespace.Api.Application.Features.Users.Common.Entities.Credentials;
using Timespace.Api.Application.Features.Users.Settings.Mfa.Entities;
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
    public DbSet<LoginFlow> LoginFlows { get; init; } = null!;
    public DbSet<MfaSetupFlow> MfaSetupFlows { get; init; } = null!;

    // Identity
    public DbSet<Identity> Identities { get; init; } = null!;
    public DbSet<IdentityIdentifier> IdentityIdentifiers { get; init; } = null!;
    public DbSet<IdentityCredential> IdentityCredentials { get; init; } = null!;
    public DbSet<Session> Sessions { get; init; } = null!;

    // Tenant
    public DbSet<Tenant> Tenants { get; init; } = null!;

    public override int SaveChanges()
    {
        throw new NotSupportedException();
    }

    public async override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e is { Entity: IEntity, State: EntityState.Added or EntityState.Modified });

        foreach (var entityEntry in entries)
        {
            ((IEntity)entityEntry.Entity).UpdatedAt = _clock.GetCurrentInstant();

            if (entityEntry.State == EntityState.Added)
            {
                ((IEntity)entityEntry.Entity).CreatedAt = _clock.GetCurrentInstant();
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
        }
        
        return await base.SaveChangesAsync(cancellationToken);
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Expression<Func<ITenantEntity, bool>> tenantExpression = entity => _sessionInfoProvider.GetGuaranteedSession().Identity.Tenant.Id == Guid.Empty || entity.TenantId == _sessionInfoProvider.GetGuaranteedSession().Identity.Tenant.Id;
        Expression<Func<ISoftDeletable, bool>> softDeleteExpression = entity => entity.DeletedAt == null;
        
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