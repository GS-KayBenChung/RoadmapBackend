using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;
using Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<DataContext>(opt => 
{
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddCors(opt => {
    opt.AddPolicy("CorsPolicy", policy => 
    {
        policy.AllowAnyHeader().AllowAnyMethod().WithOrigins(builder.Configuration["CorsSettings:BaseUrl"]);
    });
});

var app = builder.Build();

    // Add Google 
// builder.Services.AddAuthentication(options =>
// {
//     options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//     options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
// })
// .AddCookie()
// .AddGoogle(googleOptions =>
// {
//     googleOptions.ClientId = "YOUR_GOOGLE_CLIENT_ID";
//     googleOptions.ClientSecret = "YOUR_GOOGLE_CLIENT_SECRET";
//     googleOptions.CallbackPath = "/signin-google"; // Match the redirect URI in Google Console
// });

builder.Services.AddAuthorization();

// Use Authentication Middleware
app.UseAuthentication();
app.UseAuthorization();

// app.MapGet("/", () => "Hello World!");
// app.MapGet("/signin-google", (HttpContext context) => "Google Sign-In Callback"); // Example callback handler

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

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