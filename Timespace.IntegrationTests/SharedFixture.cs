using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NodaTime;
using NodaTime.Testing;
using Npgsql;
using Respawn;
using Respawn.Graph;
using Timespace.Api.Infrastructure.Persistence;

namespace Timespace.IntegrationTests;

public sealed class SharedFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private Respawner? _respawner;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(configurationBuilder =>
        {
            configurationBuilder.Sources.Clear();
            configurationBuilder.SetBasePath(Directory.GetCurrentDirectory());
            configurationBuilder.AddJsonFile("appsettings.json");
            configurationBuilder.AddEnvironmentVariables();
            configurationBuilder.Build();
        });

        builder.ConfigureServices(services =>
        {
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(x => x.HttpContext).Returns(new DefaultHttpContext());
                
            services.Remove<IHttpContextAccessor>()
                .AddSingleton(httpContextAccessorMock.Object);
            
            services
                .Remove<IClock>()
                .AddSingleton<IClock>(_ => FakeClock.FromUtc(2021, 1, 1, 1, 1, 1));

            services
                .Remove<DbContextOptions<AppDbContext>>()
                .AddDbContext<AppDbContext>((sp, options) =>
                    options.UseNpgsql(sp.GetRequiredService<IConfiguration>().GetConnectionString("DefaultConnection"),
                        npgsqlDbContextOptionsBuilder =>
                        {
                            npgsqlDbContextOptionsBuilder.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                            npgsqlDbContextOptionsBuilder.UseNodaTime();
                        }));
        });
    }
    
    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.ExecuteSqlRawAsync("drop schema public cascade;");
        await dbContext.Database.ExecuteSqlRawAsync("create schema public;");
        await dbContext.Database.MigrateAsync();
        
    }

    public async Task DisposeAsync()
    {
    }

    public async Task ResetDatabase()
    {
        var connectionString = Services.GetRequiredService<IConfiguration>().GetConnectionString("DefaultConnection")!;

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        _respawner ??= await Respawner.CreateAsync(
            connection,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                TablesToIgnore = new Table[] { new("__EFMigrationsHistory") },
                SchemasToInclude = new[] { "public" },
            }
        );

        await _respawner.ResetAsync(connection);
    }
}