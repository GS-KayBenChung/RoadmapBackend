using API.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Persistence;
using Serilog;
using Serilog.Context;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

//NEW GOOGLEAUTH
builder.Services.AddAuthentication("Google")
    .AddJwtBearer(options =>
    {
        options.Authority = "https://accounts.google.com";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "accounts.google.com",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Authentication:Google:ClientId"]
        };
    });

builder.Services.AddAuthorization();
//NEW GOOGLEAUTH

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