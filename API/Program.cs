using API.ErrorHandling;
using API.Extensions;
using Application.Dtos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
//app.Use(async (context, next) =>
//{
//    using (LogContext.PushProperty("TraceId", Guid.NewGuid().ToString()))
//    {
//        await next.Invoke();
//    }
//});

//app.Use(async (context, next) =>
//{
//    var traceId = Guid.NewGuid().ToString();
//    context.Items["TraceId"] = traceId;
//    LogContext.PushProperty("TraceId", traceId);

//    // Log Request Body
//    context.Request.EnableBuffering();
//    var requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
//    context.Request.Body.Position = 0;

//    Log.Information("Request Body: {RequestBody}", requestBody);

//    // Capture Response
//    var originalResponseBodyStream = context.Response.Body;
//    using var responseBody = new MemoryStream();
//    context.Response.Body = responseBody;

//    await next();

//    // Log Response Body
//    context.Response.Body.Seek(0, SeekOrigin.Begin);
//    var responseBodyText = await new StreamReader(context.Response.Body).ReadToEndAsync();
//    context.Response.Body.Seek(0, SeekOrigin.Begin);

//    Log.Information("Response Body: {ResponseBody}", responseBodyText);
//    await responseBody.CopyToAsync(originalResponseBodyStream);
//});
app.Use(async (context, next) =>
{
    var traceId = Guid.NewGuid().ToString();
    context.Items["TraceId"] = traceId;
    LogContext.PushProperty("TraceId", traceId);

    // Log Request Body
    context.Request.EnableBuffering();
    var requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
    context.Request.Body.Position = 0;

    Log.Information("Request Body: {RequestBody}", requestBody);

    // Capture Response
    var originalResponseBodyStream = context.Response.Body;
    using var responseBody = new MemoryStream();
    context.Response.Body = responseBody;

    await next();

    // Log Response Body (Formatted JSON)
    context.Response.Body.Seek(0, SeekOrigin.Begin);
    var responseBodyString = await new StreamReader(context.Response.Body).ReadToEndAsync();
    context.Response.Body.Seek(0, SeekOrigin.Begin);

    // Pretty-print the JSON response body
    var responseBodyText = JsonSerializer.Serialize(
        JsonSerializer.Deserialize<object>(responseBodyString),
        new JsonSerializerOptions { WriteIndented = true }
    );

    Log.Information("Response Body: {ResponseBody}", responseBodyText);

    await responseBody.CopyToAsync(originalResponseBodyStream);
});





//app.UseMiddleware<ErrorHandlingMiddleware>();

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