using Api.Contracts.Orders;
using Application.Orders.Commands.PaymentWebhook;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// Endpoints dédiés aux callbacks des fournisseurs de paiement.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PaymentsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Webhook appelé par le fournisseur de paiement pour confirmer le statut d'une transaction.
        /// </summary>
        [HttpPost("webhook")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PaymentWebhook(
            [FromBody] PaymentWebhookRequest request)
        {
            var cmd = new PaymentWebhookCommand(
                request.TransactionId,
                request.Status,
                request.FailureReason,
                request.Signature);

            var result = await _mediator.Send(cmd, CancellationToken.None);

            return result.Match<IActionResult>(
                onSuccess: _ => Ok(),
                onFailure: error => BadRequest(new { error.Code, error.Message }));
        }
    }
}
