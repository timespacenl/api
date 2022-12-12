using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using NodaTime.Testing;
using Npgsql;
using Respawn;
using Respawn.Graph;
using Timespace.Api.Infrastructure.Persistence;
using Timespace.Api.Infrastructure.Persistence.Common;

namespace Timespace.IntegrationTests;

[SetUpFixture]
public partial class Testing
{
    private static WebApplicationFactory<Program> _factory = null!;
    private static IConfiguration _configuration = null!;
    private static IServiceScopeFactory _scopeFactory = null!;
    private static Respawner _respawner = null!;
    private static string? _currentSessionToken = null;
    private static IClock _clock;
    
    [OneTimeSetUp]
    public async Task RunBeforeAnyTests()
    {
        _factory = new CustomWebApplicationFactory();
        _scopeFactory = _factory.Services.GetRequiredService<IServiceScopeFactory>();
        _configuration = _factory.Services.GetRequiredService<IConfiguration>();

        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await dbContext.Database.MigrateAsync();

        var clock = scope.ServiceProvider.GetRequiredService<IClock>();

        if(clock is FakeClock fakeClock)
            fakeClock.Reset(SystemClock.Instance.GetCurrentInstant());

        var httpContextAccessor = _factory.Services.GetRequiredService<IHttpContextAccessor>();
        httpContextAccessor.HttpContext = new DefaultHttpContext();
        
        using(var connection = new NpgsqlConnection(CustomWebApplicationFactory.IntegrationConfig.GetConnectionString("DefaultConnection")))
        {
            await connection.OpenAsync();
            _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions()
            {
                SchemasToInclude = new []
                {
                    "public"
                },
                DbAdapter = DbAdapter.Postgres,
                TablesToIgnore = new Table[] { "__EFMigrationsHistory" }
            });
        }
    }

    public static async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        using var scope = _scopeFactory.CreateScope();

        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();

        return await mediator.Send(request);
    }
    
    public static async Task ResetState()
    {
        using(var connection = new NpgsqlConnection(CustomWebApplicationFactory.IntegrationConfig.GetConnectionString("DefaultConnection")))
        {
            await connection.OpenAsync();

            await _respawner.ResetAsync(connection);
        }
        
        var httpContextAccessor = _factory.Services.GetRequiredService<IHttpContextAccessor>();
        httpContextAccessor.HttpContext = new DefaultHttpContext();
        
        _currentSessionToken = null;
        
        var clock = _factory.Services.GetRequiredService<IClock>();

        if(clock is FakeClock fakeClock)
            fakeClock.Reset(Instant.FromUtc(2021, 1, 1, 0, 0));
    }

    public static void AdvanceTime(Duration duration)
    {
        var clock = _factory.Services.GetRequiredService<IClock>();

        if(clock is FakeClock fakeClock)
            fakeClock.Advance(duration);
    }

    [OneTimeTearDown]
    public void RunAfterAnyTests()
    {
    }
}