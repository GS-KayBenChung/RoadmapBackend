using API.Extensions;
using Application.Validator;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Persistence;
using Serilog;
using Serilog.Events;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JsonOptions>(options =>
{
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never;
});

builder.Services.AddScoped<IValidationService, ValidationService>();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

builder.Services.AddControllers();

builder.Host.UseSerilog((context, services, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddCors(opt =>
{
    opt.AddPolicy("CorsPolicy", policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .WithOrigins("http://localhost:3000")
              .AllowCredentials();
    });
});

// Moved all services to ApplicationServicesExtension file
builder.Services.AddApplicationServices(builder.Configuration);


builder.Services.AddSingleton<PostgresHealthCheck>();
builder.Services.AddHostedService<GracefulShutdownService>();
builder.Services.AddHttpClient<MicroserviceHealthCheck>();
builder.Services.AddHostedService<MicroserviceHealthCheck>();

builder.Services.AddValidatorsFromAssemblyContaining<CreateRoadmapValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateLogsValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<DeleteRoadmapValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<GetDetailsQueryValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<GetLogsValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<ListQueryValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<PatchCompletionStatusValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<PatchRoadmapValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<PublishRoadmapValidator>();


builder.Services.AddFluentValidationAutoValidation();
var app = builder.Build();


var logger2 = app.Services.GetRequiredService<ILogger<Program>>();
var dbHealthCheck = app.Services.GetRequiredService<PostgresHealthCheck>();

if (!await dbHealthCheck.CheckDatabaseConnection())
{
    logger2.LogError("PostgreSQL is unreachable. Shutting down application.");
    return;
}

app.Lifetime.ApplicationStarted.Register(() =>
{
    Task.Run(async () =>
    {
        while (!app.Lifetime.ApplicationStopping.IsCancellationRequested)
        {
            if (!await dbHealthCheck.CheckDatabaseConnection())
            {
                logger2.LogError("PostgreSQL connection lost! Shutting down...");
                Environment.Exit(1); 
            }
            await Task.Delay(5000); 
        }
    });
});

app.UseMiddleware<ExceptionMiddleware>();

app.UseSerilogRequestLogging(opts =>
{
    opts.GetLevel = (httpContext, elapsed, ex) =>
    {
        if (httpContext.Response.StatusCode >= 500)
            return LogEventLevel.Error;

        if (httpContext.Response.StatusCode >= 400)
            return LogEventLevel.Warning;

        return LogEventLevel.Information;
    };
});

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("CorsPolicy");
app.MapControllers();

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;

try
{
    var context = services.GetRequiredService<DataContext>();
    context.Database.Migrate();
}
catch (Exception ex)
{
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An Error Has Occurred");
}

app.Run();
