using Carter;
using DistributedSystem.API.DependencyInjection.Extensions;
using DistributedSystem.API.Middleware;
using DistributedSystem.Application.DependencyInjection.Extensions;
// using DistributedSystem.Persistence.DependencyInjection.Extensions;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using DistributedSystem.Infrastructure.DependencyInjection.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Logging
    .ClearProviders()
    .AddSerilog();

builder.Host.UseSerilog();

builder.Services.AddInfrastructureServices();

// ======= Tạm thời comment =======
// builder.Services.AddRedisService(builder.Configuration);

// builder.Services.ConfigureSqlServerRetryOptions(
//     builder.Configuration.GetSection(nameof(SqlServerRetryOptions)));

// builder.Services.AddInterceptorDbContext();
// builder.Services.AddSqlConfiguration();
// builder.Services.AddRepositoryBaseConfiguration();
// ================================

builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddConfigureMediatR();
builder.Services.AddConfigureAutoMapper();

builder.Services.AddTransient<ExceptionHandlingMiddleware>();

builder.Services.AddCarter();

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

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapCarter();

// Để deploy IIS cũng có Swagger, tạm bỏ điều kiện môi trường
app.ConfigureSwagger();

app.Run();