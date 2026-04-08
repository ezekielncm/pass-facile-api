using Api.Contracts.Auth;
using Application.Auth.Commands.RefreshToken;
using Application.Auth.Commands.RequestOtp;
using Application.Auth.Commands.VerifyOtp;
using Application.Common.Models;
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
        /// <summary>
        /// send otp to the user phone number for authentication or registration. If the user does not exist, it will be created.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("send-otp")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [AllowAnonymous]
        public async Task<IActionResult> RequestOtp(
            [FromBody] SendOtpRequest request)
        {
            var commad = new RequestOtpCommand(request.PhoneNumber);
            var result = await _mediator.Send(commad,CancellationToken.None);
            return result.Match<IActionResult>(
                onSuccess: Response => Ok(Response),
                onFailure: error => error.Code switch
                {
                    var c when c.Contains("Notfound") => NotFound(error),
                    _ => BadRequest(new { error.Code, error.Message })
                });
        }
        /// <summary>
        /// verify otp sent to the user phone number for authentication or registration. If the user does not exist, it will be created.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("verify-otp")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyOtp(
            [FromBody] VerifyOtpRequest request)
        {
            var command = new VerifyOtpCommand(request.PhoneNumber, request.Code, request.DeviceId);
            var result = await _mediator.Send(command);
            return result.Match<IActionResult>(
                onSuccess: Response => Ok(Response),
                onFailure: error => error.Code switch
                {
                    var c when c.Contains("Notfound") => NotFound(error),
                    _ => BadRequest(new { error.Code, error.Message })
                });
        }

        /// <summary>
        /// refresh otp sent to the user phone number for authentication or registration. If the user does not exist, it will be created.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("refresh")]
        [Authorize]
        public async Task<IActionResult> refresh(
            [FromBody] RefreshRequest request)
        {
            var command = new RefreshTokenCommand(request.RefreshToken);
            var result = await _mediator.Send(command);
            return result.Match<IActionResult>(
                onSuccess: Response => Ok(Response),
                onFailure: Error => Error.Code switch
                {
                    var c when c.Contains("Notfound") => NotFound(Error),
                    _ => BadRequest(new { Error.Code, Error.Message })
                });
        }
    }
}
