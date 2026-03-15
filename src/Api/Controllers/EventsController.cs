using Api.Contracts.Events;
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
            [FromBody]CreateEventRequest rq)
        {
            var cmd
        }
    }
}
