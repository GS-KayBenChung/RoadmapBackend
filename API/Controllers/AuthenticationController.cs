using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        [HttpPost("verify-token")]
        public async Task<IActionResult> VerifyToken([FromBody] TokenRequest tokenRequest)
        {
          
            Console.WriteLine("Oauth: " + tokenRequest.OauthToken);


            if (string.IsNullOrEmpty(tokenRequest.OauthToken))
            {
                return BadRequest("OAuth token is required.");
            }

            using var client = new HttpClient();
            var response = await GoogleJsonWebSignature.ValidateAsync(tokenRequest.OauthToken);
            Console.WriteLine("email: " + response.Email);

            var userInfo = "123";

            if (userInfo == null)
            {
                return Unauthorized("Invalid token");
            }

            var jwt = "";
            return Ok(new { token = jwt, user = userInfo });
        }

        public class TokenRequest
        {
            public string OauthToken { get; set; }
        }

        private string GenerateJwtToken(GoogleUserInfo userInfo)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, userInfo.Name),
                new Claim(ClaimTypes.Email, userInfo.Email),
            };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(1)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
