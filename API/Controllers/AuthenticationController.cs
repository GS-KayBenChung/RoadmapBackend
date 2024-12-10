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
            //tokenRequest.OauthToken = "ya29.a0AeDClZB18emBhgWYbz_0WupNgoPahSpSaDOCqua2DapKSNKe5bMwpV7M1VSo9wJat-KpeD65vs9zyOXLNquOdN45iy1PFb7yLJyffR-oTsUocIFtfgMj-PE6zs5hGMUftknAAcPDaHC0SEV3KyTOuElb4JHSS1GdbmMaCgYKAdUSARISFQHGX2MiUeyoEVlfuAkHt0zUxN7jvQ0170";
            Console.WriteLine("Oauth: " + tokenRequest.OauthToken);


            if (string.IsNullOrEmpty(tokenRequest.OauthToken))
            {
                return BadRequest("OAuth token is required.");
            }

            using var client = new HttpClient();
            //var response = await client.GetStringAsync($"https://www.googleapis.com/oauth2/v3/tokeninfo?id_token={tokenRequest.OauthToken}");
            var response = await GoogleJsonWebSignature.ValidateAsync(tokenRequest.OauthToken);
            Console.WriteLine("email: " + response.Email);
               


            // var userInfo = JsonConvert.DeserializeObject<GoogleUserInfo>(response);

            var userInfo = "123";

            if (userInfo == null)
            {
                return Unauthorized("Invalid token");
            }

            var jwt = "";
            //var jwt = GenerateJwtToken(userInfo);
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

            //var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("your-secret-key"));
            //var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                //issuer: "Roadmap",
                //audience: "GoSaas"
                claims: claims,
                expires: DateTime.Now.AddHours(1)
               // signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
