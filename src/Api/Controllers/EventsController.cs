using Api.Contracts.Events;
using Application.Events.Commands.PostEvent;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
        public EventsController(
            ILogger<EventsController> logger,
            IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> PostEvents(
            [FromBody] PostEventRequest rq)
        {
            var cmd = new PostEventCommand(
                rq.Name,
                rq.Description,
                rq.Venue,
                rq.Country,
                rq.City,
                rq.AddressLine1,
                rq.AddressLine2,
                DateTime.Parse(rq.StartDate),
                DateTime.Parse(rq.EndDate),
                rq.CoverImageUrl,
                int.Parse(rq.Capacity));
            var result= await _mediator.Send(cmd,CancellationToken.None);
            return result.Match<IActionResult>(
                onSuccess:Response=>Ok(Response),
                onFailure: error => error.Code switch
                {
                    var c when c.Contains("Notfound") => NotFound(error),
                    _ => BadRequest(new { error.Code, error.Message })
                });
        }
    }
}
