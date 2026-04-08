using Application.Dashboard.Queries.ExportParticipants;
using Application.Dashboard.Queries.GetEventDashboard;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/events/{eventId:guid}/dashboard")]
    [ApiController]
    [Authorize(Policy = "OrganisateurOnly")]
    public class DashboardController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Récupère le tableau de bord d'un événement (ventes, revenus, statistiques par catégorie).
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEventDashboard(Guid eventId)
        {
            var query = new GetEventDashboardQuery(eventId);
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
        /// Exporte la liste des participants d'un événement (CSV ou PDF).
        /// </summary>
        [HttpGet("export")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ExportParticipants(
            Guid eventId,
            [FromQuery] string format = "csv",
            [FromQuery] Guid? categoryId = null)
        {
            var query = new ExportParticipantsQuery(eventId, format, categoryId);
            var result = await _mediator.Send(query, CancellationToken.None);

            return result.Match<IActionResult>(
                onSuccess: dto => File(dto.Content, dto.ContentType, dto.FileName),
                onFailure: error => error.Code switch
                {
                    var c when c.Contains("NotFound") => NotFound(error),
                    _ => BadRequest(new { error.Code, error.Message })
                });
        }
    }
}
