using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Dto;
using Domain;
using Microsoft.Extensions.Logging;

namespace Application.Users
{
    public class Login
    {
        public class Command : IRequest<LoginResponseDto>
        {
            public string Credential { get; set; } 
        }

        public class Handler : IRequestHandler<Command, LoginResponseDto>
        {
            private readonly DataContext _context;
            private readonly IConfiguration _config;
            private readonly ILogger<Handler> _logger; 

            public Handler(DataContext context, IConfiguration config, ILogger<Handler> logger)
            {
                _context = context;
                _config = config;
                _logger = logger;
            }

            public async Task<LoginResponseDto> Handle(Command request, CancellationToken cancellationToken)
            {

                try
                {
                    var payload = await GoogleJsonWebSignature.ValidateAsync(request.Credential);
                    _logger.LogInformation("Google Payload: Name: {Name}, Email: {Email}, GoogleId: {GoogleId}", payload.Name, payload.Email, payload.JwtId);
               
                    var user = await _context.UserRoadmap.FirstOrDefaultAsync(u => u.Email == payload.Email, cancellationToken);
                    if (user == null)
                    {
          
                        user = new Domain.UserRoadmap
                        {
                            UserId = Guid.NewGuid(),
                            Name = payload.Name,
                            Email = payload.Email,
                            GoogleId = payload.JwtId,
                            CreatedAt = DateTime.UtcNow,                          
                        };

                        _context.UserRoadmap.Add(user);
                        try
                        {
                            await _context.SaveChangesAsync(cancellationToken);
                            _logger.LogInformation("User saved to the database.");
                        }
                        catch (Exception dbEx)
                        {
                            _logger.LogError(dbEx, "Error while saving the user to the database.");
                            throw;
                        }

                    }

                    var token = GenerateJwtToken(user);

                    return new LoginResponseDto
                    {
                        Id = user.UserId,
                        Username = user.Name,
                        Email = user.Email,                 
                        CreatedAt = user.CreatedAt,          
                        Token = token
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while processing Google login.");
                    throw new Exception("Invalid token", ex);
                }
            }

            private string GenerateJwtToken(Domain.UserRoadmap user)
            {
                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()) 
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: _config["Jwt:Issuer"],
                    audience: _config["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:ExpiryMinutes"])),
                    signingCredentials: creds
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
        }
    }
}