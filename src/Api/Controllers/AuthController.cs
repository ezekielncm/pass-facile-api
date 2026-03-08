using Api.Contracts.Auth;
using Application.Auth.Commands.Otp;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AuthController : ControllerBase
    {
        private IMediator _mediator;
        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }
        [HttpPost("send-otp")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [AllowAnonymous]
        public async Task<IActionResult> Sendotp(
            [FromBody] OtpRequest request)
        {
            var commad = new RequestOtpCommand(request.PhoneNumber);
            var result = await _mediator.Send(commad);
            return result.Match<IActionResult>(
                onSuccess: Response => Ok(Response),
                onFailure: error => error.Code switch
                {
                    var c when c.Contains("Notfound") => NotFound(error),
                    _ => BadRequest(new { error.Code, error.Message })
                });
        }
    }
}
