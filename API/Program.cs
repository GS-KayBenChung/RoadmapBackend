using API.Extensions;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Serilog;
using Serilog.Context;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Configure JWT authentication
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.Authority = "https://accounts.google.com";
        options.Audience = "36494825135-hb6snjuupfv7r5pqdupedv1u1oklvj44.apps.googleusercontent.com";
    });
// Configure JWT authentication


// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

// Moved all services to ApplicationServicesExtension file
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

// SERILOG
app.UseSerilogRequestLogging();
// SERILOG TraceID
app.Use(async (context, next) =>
{
    using (LogContext.PushProperty("TraceId", Guid.NewGuid().ToString()))
    {
        await next.Invoke();
    }
});

builder.Services.AddAuthorization();

// Use Authentication Middleware
app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();
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
    logger.LogError(ex, "An Error Has Occured");
}

app.Run();