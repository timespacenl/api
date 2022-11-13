using Destructurama;
using Serilog;
using Timespace.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddConfiguration(builder.Configuration);

builder.Host
    .UseSerilog((_, lc) => lc
        .ReadFrom.Configuration(builder.Configuration)
        .Destructure.UsingAttributes()
    );

// Add services to the container.
builder.Services.AddServices();
builder.Services.AddAspnetServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();