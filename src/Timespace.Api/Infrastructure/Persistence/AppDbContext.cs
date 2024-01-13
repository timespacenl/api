using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Timespace.Api.Infrastructure.Persistence.Common;
using Timespace.Api.Infrastructure.Persistence.Entities.Users;
using Timespace.Api.Infrastructure.Services;

namespace Timespace.Api.Infrastructure.Persistence;

internal sealed class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
{
	private readonly IClock _clock = null!;
	private readonly IUsageContext _usageContext = null!;

	public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
	{
	}

	public AppDbContext(DbContextOptions<AppDbContext> options, IClock clock, IUsageContext usageContext) : base(options)
	{
		_clock = clock;
		_usageContext = usageContext;
	}

	// Tenant
	// public DbSet<Tenant> Tenants { get; init; } = null!;

	public override int SaveChanges()
	{
		throw new NotSupportedException();
	}

	public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
	{
		var entries = ChangeTracker
			.Entries()
			.Where(e => e is { Entity: IEntity, State: EntityState.Added or EntityState.Modified });

		foreach (var entityEntry in entries)
		{
			((ITimestamped)entityEntry.Entity).UpdatedAt = _clock.GetCurrentInstant();

			if (entityEntry.State == EntityState.Added)
			{
				((ITimestamped)entityEntry.Entity).CreatedAt = _clock.GetCurrentInstant();
			}
		}

		var softdeleteEntries = ChangeTracker
			.Entries()
			.Where(e => e is { Entity: ISoftDeletable, State: EntityState.Deleted });

		foreach (var entityEntry in softdeleteEntries)
		{
			((ISoftDeletable)entityEntry.Entity).DeletedAt = _clock.GetCurrentInstant();
		}

		return await base.SaveChangesAsync(cancellationToken);
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		_ = modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

		Expression<Func<ITenantEntity, bool>> tenantExpression =
			entity => entity.TenantId == _usageContext.CurrentTenantId;
		Expression<Func<ISoftDeletable, bool>> softDeleteExpression = entity => entity.DeletedAt == null;

		foreach (var entityType in modelBuilder.Model.GetEntityTypes())
		{
			if (entityType.ClrType.IsAssignableTo(typeof(ITenantEntity)))
			{
				modelBuilder.Entity(entityType.ClrType).AppendQueryFilter(tenantExpression);
			}

			if (entityType.ClrType.IsAssignableTo(typeof(ISoftDeletable)))
			{
				modelBuilder.Entity(entityType.ClrType).AppendQueryFilter(softDeleteExpression);
			}
		}
	}
}
