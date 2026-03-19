using Api.Contracts.Events;
using Application.Events.Commands.PostEvent;
using Application.Events.Queries.GetEventPublish;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class EventsController : ControllerBase
    {
        private readonly ILogger<EventsController> _logger;
        private readonly IMediator _mediator;

        public EventsController(ILogger<EventsController> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        /// <summary>Crée un nouvel événement (brouillon).</summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PostEvent([FromBody] CreateEventRequest rq)
        {
            var cmd = new PostEventCommand(
                rq.Name,
                rq.VenueName,
                rq.Country,
                rq.City,
                rq.AddressLine1,
                rq.AddressLine2,
                rq.SalesStartDate,
                rq.SalesEndDate,
                rq.EventDate);

            var result = await _mediator.Send(cmd, CancellationToken.None);

            return result.Match<IActionResult>(
                onSuccess: dto => CreatedAtAction(nameof(PostEvent), new { id = dto.Id }, dto),
                onFailure: error => error.Code switch
                {
                    var c when c.Contains("NotFound") => NotFound(error),
                    _ => BadRequest(new { error.Code, error.Message })
                });
        }
        [HttpGet("{slug}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEventPublish(
            string slug)
        {
            var cmd = new GetEventPublishQuery(slug);
            var result = await _mediator.Send(cmd, CancellationToken.None);
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
