using Application.Tickets.Queries.GetTicketQr;
using Application.Tickets.Queries.GetTicketsByPhone;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TicketsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TicketsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Récupère le QR code d'un ticket.
        /// </summary>
        [HttpGet("{id:guid}/qr")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTicketQr(
            Guid id,
            [FromQuery] string format = "png")
        {
            var query = new GetTicketQrQuery(id, format);
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
        /// Récupère les tickets d'un utilisateur via son numéro de téléphone.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetTicketsByPhone(
            [FromQuery] string phone)
        {
            var query = new GetTicketsByPhoneQuery(phone);
            var result = await _mediator.Send(query, CancellationToken.None);

            return result.Match<IActionResult>(
                onSuccess: dto => Ok(dto),
                onFailure: error => error.Code switch
                {
                    var c when c.Contains("NotFound") => NotFound(error),
                    _ => BadRequest(new { error.Code, error.Message })
                });
        }
    }
}
