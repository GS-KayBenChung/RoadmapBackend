using Application.Users;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.Extensions.Logging;
using Application.Dtos;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers
{
    //[Route("api/roadmaps")]
    [AllowAnonymous]
    public class AuthenticationController : BaseApiController
    {
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public AuthenticationController(IMediator mediator, ILogger<AuthenticationController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost("googleresponse")]
        public async Task<IActionResult> GoogleResponse([FromBody] CredentialRequest request)
        {
            try
            {
                var response = await _mediator.Send(new Login.Command { Credential = request.Credential });
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in Google Response: " + ex);
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}