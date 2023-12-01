using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Timespace.Api.Infrastructure.Persistence;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql("User ID=postgres;Password=root;Server=localhost;Port=5432;Database=timespace;Include Error Detail=true;", opt =>
        {
            opt.UseNodaTime();
        });

        return new AppDbContext(optionsBuilder.Options);
    }
}
