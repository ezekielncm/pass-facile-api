using Api.Contracts.Orders;
using Application.Orders.Commands.CreateOrder;
using Application.Orders.Commands.InitiatePayment;
using Application.Orders.Commands.PaymentWebhook;
using Application.Orders.Commands.RefundOrder;
using Application.Orders.Queries.GetOrderStatus;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrdersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Crée une nouvelle commande pour acheter des tickets.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateOrder(
            [FromBody] CreateOrderRequest request)
        {
            var cmd = new CreateOrderCommand(
                request.CategoryId,
                request.Quantity,
                request.BuyerPhone,
                request.PromoCode);

            var result = await _mediator.Send(cmd, CancellationToken.None);

            return result.Match<IActionResult>(
                onSuccess: dto => CreatedAtAction(nameof(GetOrderStatus), new { id = dto.Id }, dto),
                onFailure: error => error.Code switch
                {
                    var c when c.Contains("NotFound") => NotFound(error),
                    _ => BadRequest(new { error.Code, error.Message })
                });
        }

        /// <summary>
        /// Récupère le statut d'une commande avec les tickets associés.
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetOrderStatus(Guid id)
        {
            var query = new GetOrderStatusQuery(id);
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
        /// Initie un paiement mobile pour une commande existante.
        /// </summary>
        [HttpPost("{id:guid}/payments")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> InitiatePayment(
            Guid id,
            [FromBody] InitiatePaymentRequest request)
        {
            var cmd = new InitiatePaymentCommand(
                id,
                request.PaymentPhone,
                request.Provider);

            var result = await _mediator.Send(cmd, CancellationToken.None);

            return result.Match<IActionResult>(
                onSuccess: dto => CreatedAtAction(nameof(GetOrderStatus), new { id }, dto),
                onFailure: error => error.Code switch
                {
                    var c when c.Contains("NotFound") => NotFound(error),
                    _ => BadRequest(new { error.Code, error.Message })
                });
        }

        /// <summary>
        /// Webhook appelé par le fournisseur de paiement pour confirmer le statut.
        /// </summary>
        [HttpPost("webhooks/payment")]
        [AllowAnonymous]
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

        /// <summary>
        /// Demande un remboursement pour une commande.
        /// </summary>
        [Authorize(Policy = "OrganisateurOnly")]
        [HttpPost("{id:guid}/refunds")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RefundOrder(
            Guid id,
            [FromBody] RefundOrderRequest request)
        {
            var cmd = new RefundOrderCommand(
                id,
                request.Amount,
                request.Reason);

            var result = await _mediator.Send(cmd, CancellationToken.None);

            return result.Match<IActionResult>(
                onSuccess: dto => CreatedAtAction(nameof(GetOrderStatus), new { id }, dto),
                onFailure: error => error.Code switch
                {
                    var c when c.Contains("NotFound") => NotFound(error),
                    _ => BadRequest(new { error.Code, error.Message })
                });
        }
    }
}
