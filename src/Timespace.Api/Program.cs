using Asp.Versioning.ApiExplorer;
using Destructurama;
using Hellang.Middleware.ProblemDetails;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerUI;
using Timespace.Api;
using Timespace.Api.Infrastructure.Middleware;
using Timespace.Api.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddConfiguration(builder.Configuration);

builder.Host
    .UseSerilog((_, lc) => lc
        .ReadFrom.Configuration(builder.Configuration)
        .Destructure.UsingAttributes()
    );

// Add services to the container.
builder.Services.AddAspnetServices();
builder.Services.AddServices(builder.Configuration);
builder.Services.AddApiExplorerServices();

var app = builder.Build();

if (builder.Environment.IsDevelopment() && !builder.Configuration.GetValue<bool>("IntegrationTestingMode"))
{
    app.Services.GetRequiredService<ApiDetailsExtractor>().Execute();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(opt =>
    {
        var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

        foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
        {
            opt.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                $"Timespace - {description.GroupName.ToUpper()}");
            // opt.DefaultModelsExpandDepth(-1);
            opt.DocExpansion(DocExpansion.List);
        }
    });
}

app.UseProblemDetails();

app.UseHttpsRedirection();
app.UseCors();

app.UseMiddleware<AuthenticationTokenExtractor>();

app.UseAuthorization();

app.MapControllers();
// app.MapIdentityApi<ApplicationUser>();

app.Run();

namespace Timespace.Api
{
    public class Program;
}