using Api.Contracts.Users;
using Application.Common.Models;
using Application.User.Commands.UpdateProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : Controller
    {
        private readonly ILogger<UsersController> _logger;
        private readonly IMediator _mediator;
        public UsersController(ILogger<UsersController> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }
        //[Authorize]
        //[HttpPut("me/profile")]
        //public async Task<IActionResult> UpdateProfile(
        //    [FromBody]UpdateProfileRequest request)
        //{
        //    var cmd = new UpdateProfileCommand
        //    (
        //        request.DisplayName,
        //        request.Bio,
        //        request.LogoUrl,
        //        request.BannerUrl,
        //        request.Slug
        //    );
        //    var result = _mediator.Send(cmd, CancellationToken.None);
        //    return result.Match<IActionResult>(
        //        onSuccess: Response=>Ok(Response),
        //        onFailure: error => error.Code switch
        //        {
        //            var c when c.Contains("Notfound") => NotFound(error),
        //            _ => BadRequest(new { error.Code, error.Message })
        //        });
        //}
    }
}
