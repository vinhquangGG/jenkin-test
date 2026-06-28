using Carter;
using DistributedSystem.API.DependencyInjection.Extensions;
using DistributedSystem.API.Middleware;
using DistributedSystem.Application.DependencyInjection.Extensions;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Logging
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog();
builder.Host.UseSerilog();

// ===== Infrastructure =====

// builder.Services.AddInfrastructureServices();
// builder.Services.AddRedisService(builder.Configuration);

// Nếu không dùng JWT thì comment luôn
// builder.Services.AddJwtAuthentication(builder.Configuration);

// ===== Application =====

// Nếu Handler của bạn phụ thuộc Repository thì comment luôn
// builder.Services.AddConfigureMediatR();

builder.Services.AddConfigureAutoMapper();

// ===== Middleware =====

builder.Services.AddTransient<ExceptionHandlingMiddleware>();

// ===== Database =====

// builder.Services.AddInterceptorDbContext();

// builder.Services.ConfigureSqlServerRetryOptions(
//     builder.Configuration.GetSection(nameof(SqlServerRetryOptions)));

// builder.Services.AddSqlConfiguration();

// builder.Services.AddRepositoryBaseConfiguration();

// ===== Carter =====

builder.Services.AddCarter();

// ===== Swagger =====

builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGenNewtonsoftSupport()
    .AddFluentValidationRulesToSwagger()
    .AddSwagger();

builder.Services
    .AddApiVersioning(options =>
    {
        options.ReportApiVersions = true;
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

// app.UseAuthentication();
// app.UseAuthorization();

app.MapCarter();

app.ConfigureSwagger();

await app.RunAsync();