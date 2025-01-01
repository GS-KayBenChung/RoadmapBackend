//using API.Controllers;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.IdentityModel.JsonWebTokens;
//using System.IdentityModel.Tokens.Jwt;

//public class AuthenticationController : BaseApiController
//{
//    [HttpPost("googlelogin")]
//    public IActionResult GoogleLogin([FromBody] GoogleLoginDto dto)
//    {
//        Console.WriteLine(dto.Token);
//        var handler = new JwtSecurityTokenHandler();
//        var jwt = handler.ReadJwtToken(dto.Token);

//        return Ok(new { Message = "Login Successful", UserId = jwt.Subject });
//    }
//}
//public class GoogleLoginDto
//{
//    public string Token { get; set; }
//}


//using Application.Users;
//using Microsoft.AspNetCore.Mvc;
//using MediatR;
//using Microsoft.Extensions.Logging;
//using Application.Dto;
//using Serilog;

//namespace API.Controllers
//{
//    public class AuthenticationController : BaseApiController
//    {
//        private readonly IMediator _mediator;

//        public AuthenticationController(IMediator mediator, ILogger<AuthenticationController> logger)
//        {
//            _mediator = mediator;
//        }

//        [HttpPost("googleresponse")]
//        public async Task<IActionResult> GoogleResponse([FromBody] CredentialRequest request)
//        {
//            var response = await _mediator.Send(new Login.Command { Credential = request.Credential });
//            return Ok(response);
//        }
//    }
//}