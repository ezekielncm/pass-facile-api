using Api.Contracts.Finance;
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
    public class FinanceController : ControllerBase
    {
        private readonly IMediator _mediator;

        public FinanceController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Récupère le portefeuille de l'organisateur connecté (solde, transactions).
        /// </summary>
        [HttpGet("wallet")]
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
        [HttpPost("withdrawals")]
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
                onSuccess: dto => Created($"api/finance/withdrawals/{dto.Id}", dto),
                onFailure: error => error.Code switch
                {
                    var c when c.Contains("NotFound") => NotFound(error),
                    _ => BadRequest(new { error.Code, error.Message })
                });
        }
    }
}
