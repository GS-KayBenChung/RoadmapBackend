using Google.Apis.Auth;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class GoogleTokenValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _config;
    private readonly ILogger<GoogleTokenValidationMiddleware> _logger;

    public GoogleTokenValidationMiddleware(RequestDelegate next, IConfiguration config, ILogger<GoogleTokenValidationMiddleware> logger)
    {
        _next = next;
        _config = config;
        _logger = logger;
    }

    //public async Task Invoke(HttpContext context)
    //{
    //    var path = context.Request.Path.Value?.ToLower();
    //    var authorizationHeader = context.Request.Headers["Authorization"].FirstOrDefault();

    //    _logger.LogInformation($"Incoming Request: {path}");

    //    if (path == "/api/authentication/googleresponse")
    //    {
    //        _logger.LogInformation(" Skipping authentication for Google login.");
    //        await _next(context);
    //        return;
    //    }

    //    if (authorizationHeader != null && authorizationHeader.StartsWith("Bearer "))
    //    {
    //        var token = authorizationHeader.Substring("Bearer ".Length);
    //        _logger.LogInformation($" Received Token: {token}");

    //        try
    //        {
    //            var googlePayload = await GoogleJsonWebSignature.ValidateAsync(token, new GoogleJsonWebSignature.ValidationSettings
    //            {
    //                Audience = new[] { _config["Authentication:Google:ClientId"] }
    //            });

    //            _logger.LogInformation($"Google Token Verified: {googlePayload.Email}");

    //            var claims = new[]
    //            {
    //            new Claim(ClaimTypes.NameIdentifier, googlePayload.Subject),
    //            new Claim(ClaimTypes.Email, googlePayload.Email),
    //            new Claim(ClaimTypes.Name, googlePayload.Name)
    //        };

    //            var identity = new ClaimsIdentity(claims, "Google");
    //            var principal = new ClaimsPrincipal(identity);
    //            context.User = principal;
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogWarning($"Invalid Google Token: {ex.Message}");
    //            context.Response.StatusCode = 401;
    //            await context.Response.WriteAsync("Invalid Google Token");
    //            return;
    //        }
    //    }

    //    await _next(context);
    //}

    public async Task Invoke(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower();
        var authorizationHeader = context.Request.Headers["Authorization"].FirstOrDefault();

        _logger.LogInformation($"Incoming Request: {path}");

        if (path == "/api/authentication/googleresponse")
        {
            _logger.LogInformation("Skipping Google validation for login request.");
            await _next(context);
            return;
        }

        if (authorizationHeader != null && authorizationHeader.StartsWith("Bearer "))
        {
            var token = authorizationHeader.Substring("Bearer ".Length);
            _logger.LogInformation($" Received Token in Middleware: {token}");

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

                if (jsonToken != null)
                {
                    _logger.LogInformation($" Token Algorithm: {jsonToken.Header.Alg}");
                }

                if (jsonToken?.Header.Alg == SecurityAlgorithms.RsaSha256)
                {
                    var googlePayload = await GoogleJsonWebSignature.ValidateAsync(token, new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = new[] { _config["Authentication:Google:ClientId"] }
                    });

                    _logger.LogInformation($" Verified Google Token: {googlePayload.Email}");
                }
                else if (jsonToken?.Header.Alg == SecurityAlgorithms.HmacSha256)
                {
                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
                    var validationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = _config["Jwt:Issuer"],
                        ValidAudience = _config["Jwt:Audience"],
                        IssuerSigningKey = key
                    };

                    var principal = handler.ValidateToken(token, validationParameters, out _);
                    context.User = principal;

                    _logger.LogInformation(" Verified Backend JWT Token.");
                }
                else
                {
                    throw new Exception("JWT algorithm must be 'RS256' (Google) or 'HS256' (Backend).");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($" Invalid Token: {ex.Message}");
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Invalid Token");
                return;
            }
        }

        await _next(context);
    }




}
