using Api.Contracts.Events;
using Application.Events.Commands.PatchEvent;
using Application.Events.Commands.PostEvent;
using Application.Events.Commands.UpdateEvent;
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
                rq.City,
                rq.Address,
                rq.GpsCoordinates,
                rq.SalesStartDate,
                rq.SalesEndDate,
                rq.StartDate,
                rq.EndDate);

            var result = await _mediator.Send(cmd, CancellationToken.None);

            return result.Match<IActionResult>(
                onSuccess: dto => CreatedAtAction(nameof(PostEvent), new { id = dto.Id }, dto),
                onFailure: error => error.Code switch
                {
                    var c when c.Contains("NotFound") => NotFound(error),
                    _ => BadRequest(new { error.Code, error.Message })
                });
        }
        /// <summary>
        /// retrouver un event publié par son slug. Le slug est une chaîne de caractères unique qui identifie un événement de manière lisible. Par exemple, si un événement s'appelle "Concert de rock à Paris", son slug pourrait être "concert-de-rock-a-paris". Cette méthode permet de récupérer les détails d'un événement publié en utilisant son slug dans l'URL. Si l'événement n'est pas trouvé, elle retourne une réponse 404 Not Found.
        /// </summary>
        /// <param name="slug"></param>
        /// <returns></returns>
        [HttpGet("{slug}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [AllowAnonymous]
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
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateEvent(
        Guid id,
        [FromBody] EventRequest rq)
        {
            var cmd = new UpdateEventCommand(
                id,
                rq.Name,
                rq.Description!,
                rq.VenueName,
                rq.City,
                rq.Address,
                rq.GpsCoordinates,
                rq.SalesStartDate,
                rq.SalesEndDate,
                rq.StartDate,
                rq.EndDate);
            var result = await _mediator.Send(cmd, CancellationToken.None);
            return result.Match<IActionResult>(
                onSuccess: dto => Ok(dto),
                onFailure: error => error.Code switch
                {
                    var c when c.Contains("NotFound") => NotFound(error),
                    _ => BadRequest(new { error.Code, error.Message })
                });
        }
        [HttpPatch("{id:guid}/status")]
        public async Task<IActionResult> EventStatus(
            Guid id,
            [FromBody]EventStatusRequest rq)
        {
            var cmd = new PatchEventCommand(id, rq.Status);
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