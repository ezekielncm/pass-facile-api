using Api.Contracts.Events;
using Application.Events.Commands.CreatePromoCode;
using Application.Events.Commands.DuplicateEvent;
using Application.Events.Commands.PatchEvent;
using Application.Events.Commands.PostEvent;
using Application.Events.Commands.PostTicketCategories;
using Application.Events.Commands.UpdateEvent;
using Application.Events.Queries.GetEventPublish;
using Application.Events.Queries.GetEvents;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EventsController : ControllerBase
    {
        private readonly ILogger<EventsController> _logger;
        private readonly IMediator _mediator;

        public EventsController(ILogger<EventsController> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        /// <summary>Récupère la liste des événements de l'organisateur connecté.</summary>
        [Authorize(Policy = "OrganisateurOnly")]
        [HttpGet]
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

        /// <summary>Crée un nouvel événement (brouillon).</summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PostEvent([FromBody] PostEventRequest rq)
        {
            var cmd = new PostEventCommand(
                rq.Name,
                rq.Description,
                rq.Venue,
                rq.City,
                rq.AddressLine1,
                null,
                DateTimeOffset.Parse(rq.StartDate),
                DateTimeOffset.Parse(rq.EndDate),
                //rq.StartDate,
                rq.CoverImageUrl,
                int.Parse(rq.Capacity));

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
        /// retrouver un event publié par son slug.
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
                null,
                rq.SalesStartDate,
                rq.SalesEndDate,
                null);
            var result = await _mediator.Send(cmd, CancellationToken.None);
            return result.Match<IActionResult>(
                onSuccess: dto => Ok(dto),
                onFailure: error => error.Code switch
                {
                    var c when c.Contains("NotFound") => NotFound(error),
                    _ => BadRequest(new { error.Code, error.Message })
                });
        }
        /// <summary>
        /// Met à jour le statut d'un événement (publier, annuler, etc.).
        /// </summary>
        /// <param name="id"></param>
        /// <param name="rq"></param>
        /// <returns></returns>
        [HttpPatch("{id:guid}/status")]
        public async Task<IActionResult> EventStatus(
            Guid id,
            [FromBody] EventStatusRequest rq)
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
        /// <summary>
        /// Duplique un événement existant avec de nouvelles dates.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="newStartDate"></param>
        /// <param name="newEndDate"></param>
        /// <returns></returns>
        [HttpPost("{id:guid}/duplicate")]
        public async Task<IActionResult> DuplicateEvent(
            Guid id,
            [FromQuery] DateTimeOffset newStartDate,
            [FromQuery] DateTimeOffset newEndDate)
        {
            var cmd = new DuplicateEventCommand(
                id,
                newStartDate,
                newEndDate);
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
        /// Ajoute une catégorie de ticket à un événement.
        /// </summary>
        [Authorize(Policy = "OrganisateurOnly")]
        [HttpPost("{id:guid}/categories")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddCategoryToEvent(
            Guid id,
            [FromBody] AddCategoryRequest rq)
        {
            var cmd = new PostTicketCategoryCommand(
                id,
                rq.Name,
                rq.Price,
                rq.Quota,
                rq.Description,
                rq.FeePolicy);

            var result = await _mediator.Send(cmd, CancellationToken.None);

            return result.Match<IActionResult>(
                onSuccess: dto => Created($"api/events/{id}/categories/{dto.Id}", dto),
                onFailure: error => error.Code switch
                {
                    var c when c.Contains("NotFound") => NotFound(error),
                    _ => BadRequest(new { error.Code, error.Message })
                });
        }

        /// <summary>
        /// Crée un code promo pour un événement.
        /// </summary>
        [Authorize(Policy = "OrganisateurOnly")]
        [HttpPost("{id:guid}/promo-codes")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreatePromoCode(
            Guid id,
            [FromBody] CreatePromoCodeRequest rq)
        {
            var cmd = new CreatePromoCodeCommand(
                id,
                rq.Code,
                rq.DiscountType,
                rq.Value,
                rq.MaxUses,
                rq.ExpiresAt);

            var result = await _mediator.Send(cmd, CancellationToken.None);

            return result.Match<IActionResult>(
                onSuccess: dto => Created($"api/events/{id}/promo-codes/{dto.Id}", dto),
                onFailure: error => error.Code switch
                {
                    var c when c.Contains("NotFound") => NotFound(error),
                    _ => BadRequest(new { error.Code, error.Message })
                });
        }
    }
}