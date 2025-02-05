using API.Extensions;
using Application.Dtos;
using Application.Validator;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Persistence;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using System.Text;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

//NEW GOOGLEAUTH
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
//NEW GOOGLEAUTH

// Add services to the container.
builder.Services.AddControllers();

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

builder.Services.AddHostedService<GracefulShutdownService>();
builder.Services.AddHostedService<PostgresMonitorService>();



var app = builder.Build();

// SERILOG
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


app.UseMiddleware<ErrorHandlingMiddleware>();

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

app.UseCors("CorsPolicy");

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