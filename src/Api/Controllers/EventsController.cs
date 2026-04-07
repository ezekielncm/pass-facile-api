using Api.Contracts.Events;
using Application.Events.Commands.DuplicateEvent;
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
        /// 
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
        /// 
        /// </summary>
        /// <param name="id"></param>
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
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="id"></param>
        ///// <param name="rq"></param>
        ///// <returns></returns>
        //[HttpPost("{id:guid}/categories")]
        //public async Task<IActionResult> AddCategoryToEvent(
        //    Guid id,
        //    [FromBody] AddCategoryRequest rq)
        //{
        //}
    }
}