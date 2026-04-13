using Api.Contracts.Users;
using Application.Common.Models;
using Application.Users.Commands.UpdatePaymentAccounts;
using Application.Users.Commands.UpdateProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly ILogger<UsersController> _logger;
        private readonly IMediator _mediator;
        public UsersController(ILogger<UsersController> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }
        /// <summary>
        /// mise a jour du profile de l'utilisateur connecté. Seul les champs fournis dans la requete seront mis a jour, les autres resteront inchangés. Le champ slug doit être unique et ne peut pas être changé une fois défini. Si le champ slug est fourni dans la requete, il doit être différent de l'ancien slug de l'utilisateur.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Policy = "OrganisateurOnly")]
        [HttpPut("me/profile")]
        public async Task<IActionResult> UpdateProfile(
            [FromBody] UpdateProfileRequest request)
        {
            var cmd = new UpdateProfileCommand
            (
                request.DisplayName,
                request.Bio,
                request.LogoUrl,
                request.BannerUrl,
                request.Slug
            );
            var result = await _mediator.Send(cmd, CancellationToken.None);
            return result.Match<IActionResult>(
                onSuccess: Response => Ok(Response),
                onFailure: error => error.Code switch
                {
                    var c when c.Contains("Notfound") => NotFound(error),
                    _ => BadRequest(new { error.Code, error.Message })
                });
        }

        /// <summary>
        /// Met à jour les comptes de paiement mobile de l'organisateur connecté.
        /// </summary>
        [Authorize(Policy = "OrganisateurOnly")]
        [HttpPut("me/payment-accounts")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdatePaymentAccounts(
            [FromBody] PaymentAccountsRequest request)
        {
            var cmd = new UpdatePaymentAccountsCommand(
                request.OrangeMoney,
                request.MoovMoneyNumber);

            var result = await _mediator.Send(cmd, CancellationToken.None);

            return result.Match<IActionResult>(
                onSuccess: dto => Ok(dto),
                onFailure: error => error.Code switch
                {
                    var c when c.Contains("Notfound") => NotFound(error),
                    _ => BadRequest(new { error.Code, error.Message })
                });
        }
    }
}
