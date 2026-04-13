using Api.Contracts.Finance;
using Application.Events.Queries.GetEvents;
using Application.Finance.Commands.RequestWithdrawal;
using Application.Finance.Queries.GetWallet;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "OrganisateurOnly")]
    public class OrganizersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrganizersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>Récupère la liste des événements de l'organisateur connecté.</summary>
        [HttpGet("me/events")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetEvents(
            [FromQuery] string? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var query = new GetEventsQuery(status, page, pageSize);
            var result = await _mediator.Send(query, CancellationToken.None);

            return result.Match<IActionResult>(
                onSuccess: dto => Ok(dto),
                onFailure: error => BadRequest(new { error.Code, error.Message }));
        }

        /// <summary>
        /// Récupère le portefeuille de l'organisateur connecté (solde, transactions).
        /// </summary>
        [HttpGet("me/wallet")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetWallet()
        {
            var query = new GetWalletQuery();
            var result = await _mediator.Send(query, CancellationToken.None);

            return result.Match<IActionResult>(
                onSuccess: dto => Ok(dto),
                onFailure: error => error.Code switch
                {
                    var c when c.Contains("NotFound") => NotFound(error),
                    _ => BadRequest(new { error.Code, error.Message })
                });
        }

        /// <summary>
        /// Demande un retrait de fonds vers un compte de paiement mobile.
        /// </summary>
        [HttpPost("me/wallet/withdraw")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RequestWithdrawal(
            [FromBody] RequestWithdrawalRequest request)
        {
            var cmd = new RequestWithdrawalCommand(
                request.Amount,
                request.AccountId);

            var result = await _mediator.Send(cmd, CancellationToken.None);

            return result.Match<IActionResult>(
                onSuccess: dto => Created($"api/organizers/me/wallet/withdrawals/{dto.Id}", dto),
                onFailure: error => error.Code switch
                {
                    var c when c.Contains("NotFound") => NotFound(error),
                    _ => BadRequest(new { error.Code, error.Message })
                });
        }
    }
}
