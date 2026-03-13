using Api.Contracts.Users;
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
        [Authorize]
        [HttpPut("me/profile")]
        public IActionResult me(
            [FromBody]UpdateProfileRequest request)
        {
            var cmd = new UpdateProfileCommand
            {
                DisplayName = request.DisplayName,
                Bio = request.Bio,
                LogoUrl = request.LogoUrl,
                BannerUrl = request.BannerUrl,
                Slug = request.Slug
            };
            var result = _mediator.Send(cmd, CancellationToken.None);
            return result.Match<IActionResult>(
                onSuccess: Response=>Ok(Response),
                onFailure: error => error.Code switch
                {
                    var c when c.Contains("Notfound") => NotFound(error),
                    _ => BadRequest(new { error.Code, error.Message })
                });
        }
    }
}
