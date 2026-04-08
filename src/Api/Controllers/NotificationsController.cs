using Api.Contracts.Notifications;
using Application.Notifications.Commands.SendReminder;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "OrganisateurOnly")]
    public class NotificationsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public NotificationsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Envoie un rappel aux participants d'un événement.
        /// </summary>
        [HttpPost("reminders")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SendReminder(
            [FromBody] SendReminderRequest request)
        {
            var cmd = new SendReminderCommand(
                request.EventId,
                request.Message,
                request.ScheduledAt);

            var result = await _mediator.Send(cmd, CancellationToken.None);

            return result.Match<IActionResult>(
                onSuccess: dto => Created($"api/notifications/reminders/{dto.JobId}", dto),
                onFailure: error => error.Code switch
                {
                    var c when c.Contains("NotFound") => NotFound(error),
                    _ => BadRequest(new { error.Code, error.Message })
                });
        }
    }
}
