using Carter;
using DistributedSystem.API.DependencyInjection.Extensions;
using DistributedSystem.API.Middleware;
using DistributedSystem.Application.DependencyInjection.Extensions;
using DistributedSystem.Persistence.DependencyInjection.Extensions;
using DistributedSystem.Persistence.DependencyInjection.Options;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using DistributedSystem.Infrastructure.DependencyInjection.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add configuration

Log.Logger = new LoggerConfiguration().ReadFrom
    .Configuration(builder.Configuration)
    .CreateLogger();

builder.Logging
    .ClearProviders()
    .AddSerilog();

builder.Host.UseSerilog();

builder.Services.AddInfrastructureServices();
builder.Services.AddRedisService(builder.Configuration);

builder.Services.AddJwtAuthentication(builder.Configuration);

//builder
//    .Services
//    .AddControllers()
//    .AddApplicationPart(DistributedSystem.Presentation.AssemblyReference.Assembly);

builder.Services.AddConfigureMediatR();
builder.Services.AddConfigureAutoMapper();

// Add Middleware => Remember using middleware
builder.Services.AddTransient<ExceptionHandlingMiddleware>();

// Configure Options and SQL => Remember mapcarter
builder.Services.AddInterceptorDbContext();
builder.Services.ConfigureSqlServerRetryOptions(builder.Configuration.GetSection(nameof(SqlServerRetryOptions)));
builder.Services.AddSqlConfiguration();
builder.Services.AddRepositoryBaseConfiguration();

// Add Carter module
builder.Services.AddCarter();

// Add Swagger
builder.Services
        .AddSwaggerGenNewtonsoftSupport()
        .AddFluentValidationRulesToSwagger()
        .AddEndpointsApiExplorer()
        .AddSwagger();

builder.Services
    .AddApiVersioning(options => options.ReportApiVersions = true)
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

var app = builder.Build();

// Using middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

//app.UseHttpsRedirection();

app.UseAuthentication(); // This need to be added before UseAuthorization
app.UseAuthorization();

//app.MapControllers();

// Add API Endpoint with carter module
app.MapCarter();

// Configure the HTTP request pipeline. 
if (builder.Environment.IsDevelopment() || builder.Environment.IsStaging())
    app.ConfigureSwagger(); // => After MapCarter => Show Version

try
{
await app.RunAsync();
Log.Information("Stopped cleanly");
}
catch (Exception ex)
{
Log.Fatal(ex, "An unhandled exception occured during bootstrapping");
await app.StopAsync();
}
finally
{
Log.CloseAndFlush();
await app.DisposeAsync();
}

public partial class Program { }
